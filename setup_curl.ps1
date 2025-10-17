# Download and setup libcurl
$curlVersion = "8.4.0"
$curlBuild = "3"
$curlZip = "curl-${curlVersion}_${curlBuild}-win64-msvc.zip"
$curlUrl = "https://curl.se/windows/dl-${curlVersion}_${curlBuild}/$curlZip"
$libPath = "lib/curl"

# Create directories
New-Item -ItemType Directory -Force -Path $libPath | Out-Null

# Download curl
Write-Host "Downloading libcurl..."
Invoke-WebRequest -Uri $curlUrl -OutFile "$libPath/$curlZip"

# Extract
Write-Host "Extracting libcurl..."
Expand-Archive -Path "$libPath/$curlZip" -DestinationPath "$libPath" -Force

# Clean up
Remove-Item "$libPath/$curlZip"

Write-Host "libcurl setup complete"