# Web Page Capture Tool# MultiLanguage Self-Contained Project



A self-contained web page capture tool that takes screenshots and extracts structured content from web pages. Supports both GUI and CLI modes for interactive use and automation.This project demonstrates a self-contained application using C++, Python, and C#. Each component is designed to run without requiring external dependencies or installations.



## ✨ Features## Project Structure



- 🖼️ **High-quality Screenshots** - Captures full-page screenshots using a headful browser```

- 📝 **Structured Content Extraction** - Extracts page content with sections (title, headers, content).

- 🖥️ **GUI Mode** - Interactive interface for manual web browsing├── bin/           # Compiled outputs

- ⌨️ **CLI Mode** - Command-line interface for automation and scripting├── lib/           # Libraries and dependencies

- 🔄 **Python Integration** - Coordinator script for easy integration with other tools├── src/

- 📦 **Self-contained** - No external dependencies or installation required│   ├── cpp/       # C++ source files

- 🌐 **Portable** - Works on any Windows machine without configuration│   ├── cs/        # C# source files

│   └── python/    # Python source files

## 🚀 Quick Start└── CMakeLists.txt # C++ build configuration

```

### Download & Run

## Building the Project

1. Download the latest release from the [Releases](https://github.com/taalhaakaagaan/web-capture-tool/releases) page

2. Extract to any directory### C++ Component

3. Run `CSharpApp.exe` for GUI mode or use `coordinator.py` for CLI mode```powershell

cmake -B build

### GUI Modecmake --build build

```

Simply run the executable:The output will be in `bin/cpp_app.exe`



```powershell### C# Component

.\CSharpApp.exe```powershell

```dotnet publish src/cs/CSharpApp.csproj -c Release -o bin/cs

```

Then:The output will be a self-contained executable in `bin/cs/CSharpApp.exe`

1. Enter a URL in the text box

2. Click "Scrape"### Python Component

3. View the screenshot and contentThe Python script is designed to run with embedded Python - no installation required.

4. The browser stays open for manual navigation

## Running the Applications

### CLI Mode

- C++: `.\bin\cpp_app.exe`

Use the Python coordinator for automated captures:- C#: `.\bin\cs\CSharpApp.exe`

- Python: The Python script will be packaged with an embedded Python runtime

```powershell

python coordinator.py "https://example.com"All executables are self-contained and don't require any external dependencies or installations.
```

Or use the C# app directly:

```powershell
.\CSharpApp.exe --capture-url "https://example.com"
```

**Output:**
- Screenshot: `screenshots/screenshot_[timestamp].png`
- JSON data: `screenshots/screenshot_[timestamp].json`

## 📊 Output Format

The tool outputs JSON with structured content:

```json
{
    "success": true,
    "data": "Başlık (Title): Example Domain\n\nSayfa Başlıkları (Headers):\n  - Example Domain\n\nSayfa İçeriği (Content):\n  This domain is for use in documentation examples...",
    "screenshot": "screenshot_20251017_123456789.png"
}
```

Content is organized into:
- **Başlık (Title)** - Page title
- **Sayfa Başlıkları (Headers)** - All H1-H6 headings
- **Sayfa İçeriği (Content)** - Main page content in paragraphs

## 🛠️ Development

### Prerequisites

- .NET 6.0 SDK or later
- Python 3.8 or later
- Visual Studio 2019/2022 (optional)
- CMake 3.15+ (for C++ component, optional)

### Building from Source

1. **Clone the repository:**
   ```powershell
   git clone https://github.com/taalhaakaagaan/web-capture-tool.git
   cd web-capture-tool
   ```

2. **Build the C# application:**
   ```powershell
   dotnet publish src/cs/CSharpApp.csproj -o bin/package/python -c Release --self-contained true -r win-x64
   ```

3. **Copy Python coordinator:**
   ```powershell
   Copy-Item src/python/coordinator.py bin/package/python/
   ```

4. **Run:**
   ```powershell
   cd bin/package/python
   .\CSharpApp.exe
   ```

### Project Structure

```
web-capture-tool/
├── src/
│   ├── cs/                    # C# GUI and capture implementation
│   │   ├── Program.cs         # Main application code
│   │   └── CSharpApp.csproj   # Project file
│   ├── python/                # Python coordinator
│   │   ├── coordinator.py     # CLI wrapper and integration
│   │   └── main.py           # Additional utilities
│   └── cpp/                   # C++ scraper (optional)
│       └── main.cpp
├── bin/
│   └── package/               # Build output directory
│       └── python/            # Self-contained package
├── .gitignore
├── README.md
└── test1.0.sln                # Visual Studio solution
```

## 🔧 How It Works

1. **C# Application** uses the built-in `WebBrowser` control (IE11 engine) to render pages
2. **Headful Capture** waits for page to fully load using multiple heuristics:
   - Document ready state
   - Body text length
   - Page title presence
   - Loading indicators detection
3. **Screenshot** taken using `DrawToBitmap` method
4. **Content Extraction** uses HTML parser to extract structured text
5. **Python Coordinator** manages the process and provides clean JSON output

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. Fork the repository
2. Create a feature branch: `git checkout -b my-new-feature`
3. Make your changes
4. Test thoroughly
5. Commit: `git commit -am 'Add some feature'`
6. Push: `git push origin my-new-feature`
7. Submit a Pull Request

### Ideas for Contributions

- [ ] Add WebView2 (Edge Chromium) support as alternative to IE11
- [ ] Support for Linux/macOS using Chromium
- [ ] Additional output formats (PDF, MHTML, etc.)
- [ ] Configurable timeouts and retry logic
- [ ] Better error handling and logging
- [ ] Unit tests and CI/CD pipeline

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details

## 🙏 Acknowledgments

- Built with .NET 6.0 and Windows Forms
- Python integration for cross-platform scripting
- Self-contained packaging for easy distribution

## 📞 Support

If you encounter any issues or have questions:
- Open an [Issue](https://github.com/taalhaakaagaan/web-capture-tool/issues)
- Check existing issues for solutions
- Contributions and suggestions are always welcome!

---

Made with ❤️ by [Talha Kagan](https://github.com/taalhaakaagaan)
