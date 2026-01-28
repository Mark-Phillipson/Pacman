<#
Attempts to publish the Android build and deploy to the first connected device.
Notes:
 - This script assumes the project `PacmanVoice\PacmanVoice.csproj` supports `net10.0-android` target.
 - The script makes a best-effort default to `android-arm64` runtime identifier. Adjust `-RuntimeIdentifier` if your device uses a different ABI (use `adb shell getprop ro.product.cpu.abi`).
Usage: Run from repo root: `.	ools\build-and-deploy-android.ps1`
#>

$project = "PacmanVoice\PacmanVoice.csproj"
$configuration = "Release"
$rid = "android-arm64"    # Default; change as needed to match device ABI (android-arm, android-x86, etc.)
$out = Join-Path -Path (Split-Path $project) -ChildPath "publish"

Write-Host "=== Build & Deploy ===" -ForegroundColor Cyan
Write-Host "Project: $project`nConfiguration: $configuration`nRID: $rid`nOutput: $out`n" -ForegroundColor Gray

# Confirm device present
$devices = & adb devices | Select-Object -Skip 1 | Where-Object { $_ -match '\S' } | ForEach-Object { ($_ -split '\s+')[0] }
if (-not $devices) {
    Write-Host "No devices found via adb. Ensure a phone is connected and USB debugging is enabled." -ForegroundColor Red
    exit 1
}
$deviceId = $devices[0]
Write-Host "Using device: $deviceId" -ForegroundColor Green

# Try to detect ABI on device for RID guidance
try {
    $abi = (& adb -s $deviceId shell getprop ro.product.cpu.abi).Trim()
    if ($abi) { Write-Host "Device ABI reported: $abi" -ForegroundColor Green }
} catch { }





























nWrite-Host "Install complete. Launch the app on device manually or use 'adb shell monkey -p com.example.pacmanvoice -c android.intent.category.LAUNCHER 1' to start." -ForegroundColor Greenif ($LASTEXITCODE -ne 0) { Write-Host "adb install failed." -ForegroundColor Red; exit $LASTEXITCODE }& adb -s $deviceId install -r $apk.FullName | ForEach-Object { Write-Host $_ }
nWrite-Host "Installing APK: $($apk.FullName)" -ForegroundColor Cyan}    exit 1    Write-Host "You can inspect $out for artifacts or adjust /p:AndroidPackageFormat and RuntimeIdentifier." -ForegroundColor Gray    Write-Host "No APK found in publish output. The Android packaging settings may need adjustment." -ForegroundColor Yellow$apk = Get-ChildItem -Path $out -Recurse -Include *.apk -ErrorAction SilentlyContinue | Select-Object -First 1
nif (-not $apk) {
nWrite-Host "Publish succeeded. Searching for APK..." -ForegroundColor Green}    exit $LASTEXITCODE    $pub | ForEach-Object { Write-Host $_ }    Write-Host "dotnet publish failed. Inspect the output below to resolve build errors:" -ForegroundColor Redif ($LASTEXITCODE -ne 0) {$pub = & dotnet @publishArgs 2>&1)    "/p:RuntimeIdentifier=$rid"    '/p:AndroidPackageFormat=apk',    '-o', $out,    '-c', $configuration,    '-f', 'net10.0-android',    $project,    'publish',$publishArgs = @(Write-Host "Publishing..." -ForegroundColor Cyann# Publish step (best-effort)