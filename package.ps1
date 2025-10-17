# Create the final package directory
$packageDir = "bin/package"
Remove-Item -Path $packageDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

Write-Host "Building C++ component..."
if (-not (Test-Path "build")) {
    cmake -B build
}
cmake --build build --config Release

Write-Host "Building C# component..."
dotnet publish src/cs/CSharpApp.csproj -c Release -o $packageDir

Write-Host "Setting up Python component..."
$pythonDir = "$packageDir/python"
New-Item -ItemType Directory -Force -Path $pythonDir | Out-Null
Copy-Item "src/python/*" -Destination $pythonDir -Recurse -Force
Copy-Item "build/bin/Release/cpp_app.exe" -Destination $packageDir -Force

# Download embedded Python if not exists
$pythonVersion = "3.11.4"
$pythonEmbedded = "python-$pythonVersion-embed-amd64.zip"
if (-not (Test-Path "$pythonDir/python.exe")) {
    Write-Host "Downloading embedded Python..."
    $downloadUrl = "https://www.python.org/ftp/python/$pythonVersion/$pythonEmbedded"
    Invoke-WebRequest -Uri $downloadUrl -OutFile "$pythonDir/$pythonEmbedded"
    Expand-Archive -Path "$pythonDir/$pythonEmbedded" -DestinationPath $pythonDir -Force
    Remove-Item "$pythonDir/$pythonEmbedded"
}

Write-Host "Package created in bin/package"
Write-Host "Run CSharpApp.exe to start the application"