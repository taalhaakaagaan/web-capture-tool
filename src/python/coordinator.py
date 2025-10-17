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
        self.headers = []
        self.in_header = False

    def handle_starttag(self, tag, attrs):
        self.current_tag = tag
        if tag == 'title':
            self.in_title = True
        elif tag in {'h1', 'h2', 'h3', 'h4', 'h5', 'h6'}:
            self.in_header = True

    def handle_endtag(self, tag):
        self.current_tag = None
        if tag == 'title':
            self.in_title = False
        elif tag in {'h1', 'h2', 'h3', 'h4', 'h5', 'h6'}:
            self.in_header = False

    def handle_data(self, data):
        if self.current_tag not in self.skip_tags:
            text = data.strip()
            if text:
                if self.in_title:
                    self.title = text
                elif self.in_header:
                    self.headers.append(text)
                else:
                    self.text.append(text)

def clean_html_content(html_content):
    parser = SimpleHTMLParser()
    parser.feed(html_content)
    
    # Format the extracted content in a clear, structured way
    result = []
    
    if parser.title:
        result.append(f"Başlık (Title): {parser.title}\n")
    
    if parser.headers:
        result.append("Sayfa Başlıkları (Headers):")
        for header in parser.headers:
            result.append(f"  - {header}")
        result.append("")
    
    result.append("Sayfa İçeriği (Content):")
    # Group text into meaningful paragraphs
    current_paragraph = []
    for text in parser.text:
        if len(text) < 3:  # Skip very short fragments
            continue
        if text.endswith(('.', '!', '?', ':', '"')) or len(text) > 100:
            current_paragraph.append(text)
            if current_paragraph:
                result.append("  " + " ".join(current_paragraph))
                current_paragraph = []
        else:
            current_paragraph.append(text)
    
    if current_paragraph:
        result.append("  " + " ".join(current_paragraph))
    
    return "\n".join(result)

def run_scraper(url):
    # Only look in the same directory as the coordinator
    package_dir = os.path.abspath(os.path.dirname(__file__))
    
    # Look for executables in the same directory
    csharp_path = os.path.join(package_dir, 'CSharpApp.exe')
    cpp_path = os.path.join(package_dir, 'cpp_app.exe')

    print(f"Looking for executables in: {package_dir}", file=sys.stderr)
    print(f"C# path: {csharp_path} (exists: {os.path.exists(csharp_path)})", file=sys.stderr)
    print(f"C++ path: {cpp_path} (exists: {os.path.exists(cpp_path)})", file=sys.stderr)
    
    try:
        # Decide which tool to run
        if csharp_path is not None:
            print(f"Invoking C# headful capture: {csharp_path}", file=sys.stderr)
            cmd = [csharp_path, '--capture-url', url]
            cwd = os.path.dirname(csharp_path)
        elif cpp_path is not None:
            print(f"Invoking C++ scraper: {cpp_path}", file=sys.stderr)
            cmd = [cpp_path, url]
            cwd = os.path.dirname(cpp_path)
        else:
            raise FileNotFoundError("No scraper or headful capture executable found")

        # Use binary mode to avoid UnicodeDecodeError when scraper emits binary data
        process = subprocess.Popen(cmd,
                                   stdout=subprocess.PIPE,
                                   stderr=subprocess.PIPE,
                                   text=False,
                                   cwd=cwd)  # Set working directory to exe location

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

        print(f"Scraper stdout (truncated): {output[:1000]!r}", file=sys.stderr)  # Debug output (truncated)
        print(f"Scraper stderr (truncated): {error[:1000]!r}", file=sys.stderr)    # Debug output (truncated)

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