import sys
import os
import subprocess
import json
import re
from html.parser import HTMLParser

class SimpleHTMLParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.text = []
        self.title = ""
        self.in_title = False
        self.current_tag = None
        self.skip_tags = {'script', 'style', 'meta', 'link', 'iframe'}

    def handle_starttag(self, tag, attrs):
        self.current_tag = tag
        if tag == 'title':
            self.in_title = True

    def handle_endtag(self, tag):
        self.current_tag = None
        if tag == 'title':
            self.in_title = False

    def handle_data(self, data):
        if self.current_tag not in self.skip_tags:
            text = data.strip()
            if text:
                if self.in_title:
                    self.title = text
                else:
                    self.text.append(text)

def clean_html_content(html_content):
    parser = SimpleHTMLParser()
    parser.feed(html_content)
    
    # Format the extracted content
    result = []
    if parser.title:
        result.append(f"Title: {parser.title}\n")
    
    result.append("Content:")
    result.extend(parser.text)
    
    return "\n".join(result)

def run_scraper(url):
    # Path discovery: try C# headful CLI first (CSharpApp.exe), then C++ scraper (cpp_app.exe)
    # Discover repository root by walking up until we find 'src' or solution file
    cur = os.path.abspath(os.path.dirname(__file__))
    repo_root = cur
    while True:
        if os.path.exists(os.path.join(repo_root, 'src')) or any(fname.endswith('.sln') for fname in os.listdir(repo_root)):
            break
        parent = os.path.dirname(repo_root)
        if parent == repo_root:
            break
        repo_root = parent

    # C# CLI candidates relative to repo root
    csharp_candidates = [
        os.path.join(repo_root, 'bin', 'cs', 'CSharpApp.exe'),
        os.path.join(repo_root, 'bin', 'CSharpApp.exe'),
        os.path.join(repo_root, 'src', 'cs', 'bin', 'Release', 'CSharpApp.exe'),
    ]

    csharp_path = next((c for c in csharp_candidates if os.path.exists(c)), None)

    # C++ scraper candidates (fallback)
    cpp_candidates = [
        os.path.join(repo_root, 'bin', 'package', 'cpp_app.exe'),
        os.path.join(repo_root, 'build', 'bin', 'Release', 'cpp_app.exe'),
        os.path.join(repo_root, 'src', 'cpp', 'cpp_app.exe'),
        os.path.join(repo_root, 'bin', 'package', 'cpp_app.exe')
    ]

    cpp_path = next((c for c in cpp_candidates if os.path.exists(c)), None)

    # Final fallback: working dir and package dir
    if csharp_path is None:
        for root in [os.getcwd(), os.path.join(repo_root, 'bin', 'package')]:
            p = os.path.join(root, 'CSharpApp.exe')
            if os.path.exists(p):
                csharp_path = p
                break

    if cpp_path is None:
        for root in [os.getcwd(), os.path.join(repo_root, 'bin', 'package')]:
            p = os.path.join(root, 'cpp_app.exe')
            if os.path.exists(p):
                cpp_path = p
                break

    print(f"C# candidates: {csharp_candidates}", file=sys.stderr)
    print(f"Selected C# path: {csharp_path}", file=sys.stderr)
    print(f"C++ candidates: {cpp_candidates}", file=sys.stderr)
    print(f"Selected C++ path: {cpp_path}", file=sys.stderr)
    
    try:
        # Use previously discovered csharp_path / cpp_path
        output = ""
        error = ""

        if csharp_path:
            print(f"Using C# headful capture: {csharp_path}", file=sys.stderr)
            process = subprocess.Popen([csharp_path, "--capture-url", url],
                                       stdout=subprocess.PIPE,
                                       stderr=subprocess.PIPE,
                                       text=True,
                                       cwd=os.path.dirname(csharp_path))
            out_text, err_text = process.communicate()
            output = out_text or ""
            error = err_text or ""
            if process.returncode != 0:
                return {
                    "success": False,
                    "error": error or "C# capture failed"
                }
        else:
            # Run the C++ scraper
            if cpp_path is None:
                raise FileNotFoundError("No C++ scraper found to run")
            print(f"Invoking C++ scraper: {cpp_path}", file=sys.stderr)
            # Use binary mode to avoid UnicodeDecodeError when scraper emits binary data
            process = subprocess.Popen([cpp_path, url],
                                       stdout=subprocess.PIPE,
                                       stderr=subprocess.PIPE,
                                       text=False,
                                       cwd=os.path.dirname(cpp_path))  # Set working directory to exe location

            out_bytes, err_bytes = process.communicate()
            # Decode bytes defensively
            if out_bytes is None:
                output = ""
            else:
                try:
                    output = out_bytes.decode('utf-8')
                except Exception:
                    output = out_bytes.decode('utf-8', errors='replace')

            if err_bytes is None:
                error = ""
            else:
                try:
                    error = err_bytes.decode('utf-8')
                except Exception:
                    error = err_bytes.decode('utf-8', errors='replace')

            # Debug prints
            print(f"Scraper stdout (truncated): {output[:2000]!r}", file=sys.stderr)  # Debug output (truncated)
            print(f"Scraper stderr (truncated): {error[:2000]!r}", file=sys.stderr)    # Debug output (truncated)

            if process.returncode != 0:
                return {
                    "success": False,
                    "error": error or "Failed to scrape the URL"
                }

        if not output.strip():
            return {
                "success": False,
                "error": "No output received from scraper"
            }

        # Try to parse JSON output from the scraper. Be defensive about missing keys and None values.
        try:
            output = output.strip()
            # Primary parse attempt
            result = None
            try:
                result = json.loads(output)
            except json.JSONDecodeError:
                # Attempt to salvage: find the last JSON object in the output by finding the final '{' and matching '}'
                last_open = output.rfind('{')
                if last_open != -1:
                    candidate = output[last_open:]
                    try:
                        result = json.loads(candidate)
                        print("Recovered JSON from trailing output", file=sys.stderr)
                    except Exception:
                        result = None
                if result is None:
                    raise

            if not isinstance(result, dict):
                raise ValueError("Scraper returned non-object JSON")

            if not result.get("success", False):
                return {
                    "success": False,
                    "error": result.get("error", "Unknown error occurred")
                }

            raw_content = result.get("content") or ""
            cleaned_content = clean_html_content(raw_content)

            screenshot_name = result.get("screenshot") or ""

            return {
                "success": True,
                "data": cleaned_content,
                "screenshot": screenshot_name
            }

        except json.JSONDecodeError as e:
            # JSON parse failed: return a helpful error with stderr and raw output for debugging
            print(f"JSON decode error: {str(e)}", file=sys.stderr)
            print(f"Raw scraper output (first 2000 chars): {output[:2000]!r}", file=sys.stderr)
            # Fallback: treat the raw output as HTML and return cleaned text
            cleaned_content = clean_html_content(output)
            return {
                "success": True,
                "data": cleaned_content
            }
        except Exception as e:
            # Any other parsing/format errors
            print(f"Parsing error: {str(e)}", file=sys.stderr)
            return {
                "success": False,
                "error": str(e)
            }
        
    except Exception as e:
        return {
            "success": False,
            "error": str(e)
        }

def main():
    if len(sys.argv) != 2:
        print(json.dumps({
            "success": False,
            "error": "URL not provided"
        }))
        return
    
    url = sys.argv[1]
    result = run_scraper(url)
    print(json.dumps(result))

if __name__ == "__main__":
    main()