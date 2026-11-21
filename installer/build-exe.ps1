param(
    [string]$Configuration = "Release",
    [string]$Version = "0.1.0",
    [string[]]$Platforms = @("x64", "arm64")
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CmdPalPomodoro\CmdPalPomodoro.csproj"
$inno = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

foreach ($arch in $Platforms) {
    $publish = Join-Path $root "publish"
    if (Test-Path $publish) {
        Remove-Item $publish -Recurse -Force
    }

    Write-Host "Publishing win-$arch"
    dotnet publish $project `
        -c $Configuration `
        -r "win-$arch" `
        -p:Platform=$arch `
        -p:WindowsPackageType=None `
        -p:EnableMsixTooling=false `
        -p:AppxPackageSigningEnabled=false `
        -p:PublishSingleFile=true `
        -p:SelfContained=true `
        -o $publish
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $arch"
    }

    if (-not (Test-Path $inno)) {
        Write-Warning "Inno Setup was not found. Publish output is in $publish."
        continue
    }

    $archFlag = if ($arch -eq "arm64") { "arm64" } else { "x64compatible" }
    & $inno `
        "/DMyAppVersion=$Version" `
        "/DArchName=$arch" `
        "/DArchitecturesAllowed=$archFlag" `
        "/DArchitecturesInstallIn64BitMode=$archFlag" `
        (Join-Path $PSScriptRoot "setup-template.iss")
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed for $arch"
    }
}
