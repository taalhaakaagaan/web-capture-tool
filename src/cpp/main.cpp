#include <iostream>
#include <string>
#include <windows.h>
#include <wininet.h>
#include <gdiplus.h>
#include <shlwapi.h>
#include <codecvt>
#include <locale>
#include <chrono>
#include <thread>
#include <random>
#include <vector>
#include <fstream>

#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib, "shlwapi.lib")

std::wstring utf8_to_wide(const std::string& str) {
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.from_bytes(str);
}

int random_range(int min, int max) {
    static std::random_device rd;
    static std::mt19937 gen(rd());
    std::uniform_int_distribution<> dis(min, max);
    return dis(gen);
}

class BrowserEmulator {
private:
    HWND hwnd;
    HDC memDC;
    HBITMAP hBitmap;
    int width;
    int height;
    std::wstring currentUrl;
    std::wstring screenshotPath;
    
    void InitializeWindow() {
        WNDCLASSEXW wc = {};
        wc.cbSize = sizeof(WNDCLASSEXW);
        wc.lpfnWndProc = DefWindowProcW;
        wc.lpszClassName = L"BrowserEmulatorClass";
        RegisterClassExW(&wc);
        
        hwnd = CreateWindowExW(
            0, L"BrowserEmulatorClass", L"Web Browser",
            WS_OVERLAPPEDWINDOW | WS_VISIBLE,
            CW_USEDEFAULT, CW_USEDEFAULT, width, height,
            NULL, NULL, GetModuleHandle(NULL), NULL
        );
        
        HDC hdc = GetDC(hwnd);
        memDC = CreateCompatibleDC(hdc);
        hBitmap = CreateCompatibleBitmap(hdc, width, height);
        SelectObject(memDC, hBitmap);
        ReleaseDC(hwnd, hdc);
    }
    
    void SaveScreenshot(const std::wstring& filename) {
        // Get the executable path
        wchar_t exePath[MAX_PATH];
        GetModuleFileNameW(NULL, exePath, MAX_PATH);
            std::wstring exeDir = std::wstring(exePath);
        exeDir = exeDir.substr(0, exeDir.find_last_of(L"\\/"));
        
        // Create screenshots directory if it doesn't exist
        std::wstring screenshotsDir = exeDir + L"\\screenshots";
            if (!CreateDirectoryW(screenshotsDir.c_str(), NULL) && GetLastError() != ERROR_ALREADY_EXISTS) {
                // If screenshots dir creation failed, try current directory
                screenshotsDir = L"screenshots";
                CreateDirectoryW(screenshotsDir.c_str(), NULL);
            }
        
        // Full path for the screenshot
        std::wstring fullPath = screenshotsDir + L"\\" + filename;
        
        Gdiplus::GdiplusStartupInput gdiplusStartupInput;
        ULONG_PTR gdiplusToken;
        Gdiplus::GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);
        
        HDC screenDC = GetDC(hwnd);
        HDC memoryDC = CreateCompatibleDC(screenDC);
        HBITMAP hBitmap = CreateCompatibleBitmap(screenDC, width, height);
        HBITMAP hOldBitmap = (HBITMAP)SelectObject(memoryDC, hBitmap);
        
        BitBlt(memoryDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY);
        
        Gdiplus::Bitmap* bmp = new Gdiplus::Bitmap(hBitmap, NULL);
        CLSID pngClsid;
        GetEncoderClsid(L"image/png", &pngClsid);
        bmp->Save(fullPath.c_str(), &pngClsid);
        
        delete bmp;
        SelectObject(memoryDC, hOldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memoryDC);
        ReleaseDC(hwnd, screenDC);
        
        Gdiplus::GdiplusShutdown(gdiplusToken);
    }
    
    int GetEncoderClsid(const WCHAR* format, CLSID* pClsid) {
        UINT num = 0;
        UINT size = 0;
        Gdiplus::GetImageEncodersSize(&num, &size);
        if (size == 0) return -1;
        
        Gdiplus::ImageCodecInfo* pImageCodecInfo = (Gdiplus::ImageCodecInfo*)(malloc(size));
        if (pImageCodecInfo == NULL) return -1;
        
        Gdiplus::GetImageEncoders(num, size, pImageCodecInfo);
        for (UINT j = 0; j < num; ++j) {
            if (wcscmp(pImageCodecInfo[j].MimeType, format) == 0) {
                *pClsid = pImageCodecInfo[j].Clsid;
                free(pImageCodecInfo);
                return j;
            }
        }
        free(pImageCodecInfo);
        return -1;
    }
    
public:
    BrowserEmulator(int w = 1024, int h = 768) : width(w), height(h) {
        InitializeWindow();
    }
    
    ~BrowserEmulator() {
        DeleteObject(hBitmap);
        DeleteDC(memDC);
        DestroyWindow(hwnd);
    }
    
    std::pair<std::string, std::wstring> DownloadWebpage(const char* url) {
        std::string result;
        std::wstring wurl = utf8_to_wide(url);
        currentUrl = wurl;
        
        SetWindowTextW(hwnd, (L"Loading: " + wurl).c_str());
        
        // Random delay between 1-3 seconds to mimic human behavior
        std::this_thread::sleep_for(std::chrono::milliseconds(random_range(1000, 3000)));
        
        HINTERNET hInternet = InternetOpenW(L"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36", 
            INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
    
    if (hInternet) {
    LPCWSTR headers = L"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7\r\n"
             L"Accept-Language: en-US,en;q=0.9\r\n"
             L"Accept-Encoding: identity\r\n"
                         L"Cache-Control: max-age=0\r\n"
                         L"User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36\r\n"
                         L"Sec-Ch-Ua: \"Chromium\";v=\"118\", \"Google Chrome\";v=\"118\"\r\n"
                         L"Sec-Ch-Ua-Mobile: ?0\r\n"
                         L"Sec-Ch-Ua-Platform: \"Windows\"\r\n"
                         L"Sec-Fetch-Dest: document\r\n"
                         L"Sec-Fetch-Mode: navigate\r\n"
                         L"Sec-Fetch-Site: none\r\n"
                         L"Sec-Fetch-User: ?1\r\n"
                         L"Upgrade-Insecure-Requests: 1\r\n"
                         L"Connection: keep-alive\r\n";

        DWORD flags = INTERNET_FLAG_RELOAD | 
                     INTERNET_FLAG_NO_CACHE_WRITE | 
                     INTERNET_FLAG_NO_COOKIES | 
                     INTERNET_FLAG_NO_UI |
                     INTERNET_FLAG_SECURE |
                     INTERNET_FLAG_IGNORE_CERT_CN_INVALID |
                     INTERNET_FLAG_IGNORE_CERT_DATE_INVALID;

        HINTERNET hConnect = InternetOpenUrlW(hInternet, wurl.c_str(), headers, -1L, flags, 0);
        
        if (hConnect) {
            // Query and print response headers (useful for debugging)
            WCHAR szHeaders[4096];
            DWORD dwHeadersLength = sizeof(szHeaders);
            HttpQueryInfoW(hConnect, HTTP_QUERY_RAW_HEADERS_CRLF, szHeaders, &dwHeadersLength, NULL);
            std::wcerr << L"Response Headers:\n" << szHeaders << std::endl;

            char buffer[4096];
            DWORD bytesRead;
            
            while (InternetReadFile(hConnect, buffer, sizeof(buffer), &bytesRead) && bytesRead > 0) {
                // Random delay between reads to mimic human reading speed
                std::this_thread::sleep_for(std::chrono::milliseconds(random_range(50, 150)));
                result.append(buffer, bytesRead);
            }
            
            InternetCloseHandle(hConnect);
            
            // Take screenshot after content is loaded
            std::wstring timestamp = std::to_wstring(std::chrono::system_clock::now().time_since_epoch().count());
            screenshotPath = L"screenshot_" + timestamp + L".png";
            SaveScreenshot(screenshotPath);
            
            SetWindowTextW(hwnd, (L"Done: " + wurl).c_str());
        }
        else {
            DWORD error = GetLastError();
            std::cerr << "Failed to connect. Error code: " << error << std::endl;
            SetWindowTextW(hwnd, L"Error: Failed to connect");
        }
        
        InternetCloseHandle(hInternet);
    }
    else {
        std::cerr << "Failed to initialize WinINet" << std::endl;
        SetWindowTextW(hwnd, L"Error: Failed to initialize");
    }
    
    // Process Windows messages to keep UI responsive
    MSG msg;
    while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    
    return std::make_pair(result, screenshotPath);
}
};

struct BrowserResult {
    std::string content;
    std::string screenshot;
    bool success;
    std::string error;
};

int main(int argc, char* argv[]) {
    if (argc != 2) {
        std::cerr << "Usage: " << argv[0] << " <url>" << std::endl;
        return 1;
    }

    try {
        BrowserEmulator browser(1280, 960);
        auto [content, screenshotPath] = browser.DownloadWebpage(argv[1]);
        
        // Escape JSON special characters in content
        std::string escaped_content;
        for (char c : content) {
            switch (c) {
                case '"': escaped_content += "\\\""; break;
                case '\\': escaped_content += "\\\\"; break;
                case '\b': escaped_content += "\\b"; break;
                case '\f': escaped_content += "\\f"; break;
                case '\n': escaped_content += "\\n"; break;
                case '\r': escaped_content += "\\r"; break;
                case '\t': escaped_content += "\\t"; break;
                default:
                    if (static_cast<unsigned char>(c) < 0x20) {
                        char buf[8];
                        snprintf(buf, sizeof(buf), "\\u%04x", c);
                        escaped_content += buf;
                    } else {
                        escaped_content += c;
                    }
            }
        }

        // Get just the filename from the full path
        std::wstring filename = screenshotPath.substr(screenshotPath.find_last_of(L"\\/") + 1);
        
        // Convert filename to string
        std::string screenshot_name(filename.begin(), filename.end());

        // Format JSON manually
        std::string json = "{\"success\":true,\"content\":\"" + escaped_content + "\",\"screenshot\":\"" + screenshot_name + "\"}";
        std::cout << json << std::endl;
    }
    catch (const std::exception& e) {
        std::cerr << "{\"success\":false, \"error\":\"" << e.what() << "\"}" << std::endl;
        return 1;
    }

    return 0;
}