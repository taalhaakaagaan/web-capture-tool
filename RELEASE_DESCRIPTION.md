# 🎉 Initial Release v1.0.0

First stable release of the Web Capture Tool - a self-contained web page capture tool with GUI and CLI modes!

## ✨ Features

### 🖼️ High-Quality Screenshots
- Full-page screenshot capture using headful browser
- Intelligent waiting for page load completion  
- 1280x960 default resolution

### 📝 Structured Content Extraction
- Organized output with title, headers, and content sections
- Turkish language support for labels
- Clean HTML parsing with paragraph grouping

### 🖥️ GUI Mode
- Interactive WinForms interface
- Live browser navigation
- Visual content and screenshot preview
- Browser stays open for manual exploration

### ⌨️ CLI Mode
- Command-line interface for automation
- JSON output for easy integration
- Python coordinator for scripting
- Reliable file-based output

### 📦 Self-Contained Package
- **No .NET installation required**
- **Embedded Python runtime included**
- **All dependencies bundled**
- **Portable - works from any directory**

## 📥 Installation

1. Download `web-capture-tool-v1.0.0.zip` below
2. Extract to any directory
3. Run `CSharpApp.exe` - that's it!

## 🚀 Usage

### GUI Mode (Interactive)
```powershell
.\CSharpApp.exe
```

### CLI Mode (Automation)
```powershell
# Using Python coordinator (recommended)
python coordinator.py "https://example.com"

# Using C# directly
.\CSharpApp.exe --capture-url "https://example.com"
```

### Output Files
- `screenshots/screenshot_[timestamp].png` - Screenshot image
- `screenshots/screenshot_[timestamp].json` - Structured content data

## 📊 JSON Output Format

```json
{
    "success": true,
    "data": "Başlık (Title): Example Domain\n\nSayfa Başlıkları (Headers):\n  - Example Domain\n\nSayfa İçeriği (Content):\n  This domain is for use in documentation examples...",
    "screenshot": "screenshot_20251017_123456789.png"
}
```

## 🔧 Technical Details

- **Framework:** .NET 6.0 (self-contained)
- **Browser Engine:** WebBrowser control (IE11 mode)  
- **Python:** 3.11 embedded runtime
- **Platform:** Windows x64
- **Package Size:** ~71 MB

## 🐛 Known Issues

- Uses IE11 engine - some modern websites may not render perfectly
- Limited to Windows platform
- JavaScript-heavy SPAs may require longer wait times

## 🔮 Planned Improvements

- [ ] WebView2 (Edge Chromium) support
- [ ] Linux/macOS compatibility  
- [ ] PDF export option
- [ ] Configurable resolution and timeouts
- [ ] Better error handling and logging

## 📝 License

MIT License - Free for personal and commercial use

## 🙏 Feedback & Contributions

Issues and pull requests are welcome!

---

**Full Documentation:** [README.md](https://github.com/taalhaakaagaan/web-capture-tool#readme)  
**Report Issues:** [GitHub Issues](https://github.com/taalhaakaagaan/web-capture-tool/issues)  
**Full Changelog:** https://github.com/taalhaakaagaan/web-capture-tool/compare/...v1.0.0
