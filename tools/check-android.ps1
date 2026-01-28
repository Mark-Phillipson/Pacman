<#
Checks Android development environment on Windows.
Usage: Open PowerShell and run `.	ools\check-android.ps1` from the repo root.
#>

Write-Host "=== Android Environment Check ===" -ForegroundColor Cyan

function Check-Command($cmd) {
    $path = (Get-Command $cmd -ErrorAction SilentlyContinue)?.Path
    if ($null -ne $path) {
        Write-Host "Found $cmd at: $path" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host "$cmd not found on PATH." -ForegroundColor Yellow
        return $false
    }
}

$adb = Check-Command adb
$sdkmanager = Check-Command sdkmanager
$ndkbuild = Check-Command ndk-build

Write-Host "`nEnvironment variables:" -ForegroundColor Cyan
Get-Item Env:ANDROID_SDK_ROOT -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "ANDROID_SDK_ROOT=$($_.Value)" }
Get-Item Env:ANDROID_HOME -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "ANDROID_HOME=$($_.Value)" }
Get-Item Env:ANDROID_NDK_HOME -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "ANDROID_NDK_HOME=$($_.Value)" }

Write-Host "`nADB devices:" -ForegroundColor Cyan
try {
    & adb devices | ForEach-Object { Write-Host $_ }
} catch {
    Write-Host "Failed to run adb. Ensure adb is on PATH and device is connected with USB debugging enabled." -ForegroundColor Red
}

Write-Host "`nDotnet workloads (android)" -ForegroundColor Cyan
try {
    & dotnet workload list | Select-String -Pattern "android|maui" -CaseSensitive:$false | ForEach-Object { Write-Host $_ }
} catch {
    Write-Host "Could not query dotnet workloads (is dotnet installed?)" -ForegroundColor Yellow
}

Write-Host "`nRecommendations:" -ForegroundColor Cyan
if (-not $sdkmanager) {
    Write-Host "- Install Android SDK command-line tools or install 'Mobile development with .NET' via Visual Studio Installer." -ForegroundColor Yellow
    Write-Host "  * Visual Studio Installer -> Workloads -> Mobile development with .NET" -ForegroundColor Gray
    Write-Host "  * Or install Android SDK tools from: https://developer.android.com/studio#downloads" -ForegroundColor Gray
}
if (-not $ndkbuild) {
    Write-Host "- Install Android NDK (required for native Vosk libraries) via SDK Manager or Visual Studio Installer." -ForegroundColor Yellow
}

if ($adb -and $sdkmanager -and $ndkbuild) {
    Write-Host "All required Android CLI tools appear present (adb/sdkmanager/ndk-build)." -ForegroundColor Green
} else {
    Write-Host "Some tools are missing. See recommendations above." -ForegroundColor Yellow
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1) If needed, install SDK/NDK or add their tools to PATH."
Write-Host "2) Re-run this script to confirm."
Write-Host "3) Then run .\tools\build-and-deploy-android.ps1 to attempt a device build/deploy." -ForegroundColor Gray
