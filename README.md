# MultiLanguage Self-Contained Project

This project demonstrates a self-contained application using C++, Python, and C#. Each component is designed to run without requiring external dependencies or installations.

## Project Structure

```
.
├── bin/           # Compiled outputs
├── lib/           # Libraries and dependencies
├── src/
│   ├── cpp/       # C++ source files
│   ├── cs/        # C# source files
│   └── python/    # Python source files
└── CMakeLists.txt # C++ build configuration
```

## Building the Project

### C++ Component
```powershell
cmake -B build
cmake --build build
```
The output will be in `bin/cpp_app.exe`

### C# Component
```powershell
dotnet publish src/cs/CSharpApp.csproj -c Release -o bin/cs
```
The output will be a self-contained executable in `bin/cs/CSharpApp.exe`

### Python Component
The Python script is designed to run with embedded Python - no installation required.

## Running the Applications

- C++: `.\bin\cpp_app.exe`
- C#: `.\bin\cs\CSharpApp.exe`
- Python: The Python script will be packaged with an embedded Python runtime

All executables are self-contained and don't require any external dependencies or installations.