#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs Image Converter with shell integration.

.DESCRIPTION
    This script installs the Image Converter application to Program Files and 
    registers it in the Windows shell context menu for image files.

.EXAMPLE
    .\Install.ps1
#>

param(
    [string]$InstallPath = "$env:ProgramFiles\ImageConverter",
    [switch]$NoShellIntegration
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Image Converter Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Error: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Find the published application
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = Join-Path $scriptDir "..\ImageConverter.App\bin\Release\net8.0-windows\win-x64\publish"

if (-not (Test-Path $publishDir)) {
    # Try debug folder
    $publishDir = Join-Path $scriptDir "..\ImageConverter.App\bin\Debug\net8.0-windows\win-x64\publish"
}

if (-not (Test-Path $publishDir)) {
    # Try to build it
    Write-Host "Building application..." -ForegroundColor Yellow
    $projectPath = Join-Path $scriptDir "..\ImageConverter.App\ImageConverter.App.csproj"
    
    if (Test-Path $projectPath) {
        Push-Location (Split-Path -Parent $projectPath)
        dotnet publish -c Release -r win-x64 --self-contained true
        Pop-Location
        $publishDir = Join-Path $scriptDir "..\ImageConverter.App\bin\Release\net8.0-windows\win-x64\publish"
    }
}

if (-not (Test-Path $publishDir)) {
    Write-Host "Error: Could not find published application!" -ForegroundColor Red
    Write-Host "Please run 'dotnet publish -c Release -r win-x64 --self-contained true' first." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Source: $publishDir" -ForegroundColor Gray
Write-Host "Target: $InstallPath" -ForegroundColor Gray
Write-Host ""

# Create installation directory
Write-Host "[1/4] Creating installation directory..." -ForegroundColor Cyan
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Copy files
Write-Host "[2/4] Copying application files..." -ForegroundColor Cyan
Copy-Item -Path "$publishDir\*" -Destination $InstallPath -Recurse -Force

$exePath = Join-Path $InstallPath "ImageConverter.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Error: Application executable not found after copy!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Register shell extension
if (-not $NoShellIntegration) {
    Write-Host "[3/4] Registering shell context menu..." -ForegroundColor Cyan
    
    $imageExtensions = @(".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".ico")
    $menuName = "Convert Image..."
    $registryKeyName = "ImageConverter"
    
    foreach ($ext in $imageExtensions) {
        $keyPath = "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\$ext\shell\$registryKeyName"
        
        try {
            # Create shell key
            if (-not (Test-Path $keyPath)) {
                New-Item -Path $keyPath -Force | Out-Null
            }
            
            Set-ItemProperty -Path $keyPath -Name "(Default)" -Value $menuName
            Set-ItemProperty -Path $keyPath -Name "Icon" -Value "`"$exePath`",0"
            Set-ItemProperty -Path $keyPath -Name "Position" -Value "Top"
            
            # Create command key
            $commandPath = "$keyPath\command"
            if (-not (Test-Path $commandPath)) {
                New-Item -Path $commandPath -Force | Out-Null
            }
            Set-ItemProperty -Path $commandPath -Name "(Default)" -Value "`"$exePath`" `"%1`""
            
            Write-Host "  Registered for $ext" -ForegroundColor Green
        }
        catch {
            Write-Host "  Failed to register for $ext : $_" -ForegroundColor Yellow
        }
    }
    
    # Also register for generic "image" type
    $keyPath = "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\image\shell\$registryKeyName"
    try {
        if (-not (Test-Path $keyPath)) {
            New-Item -Path $keyPath -Force | Out-Null
        }
        Set-ItemProperty -Path $keyPath -Name "(Default)" -Value $menuName
        Set-ItemProperty -Path $keyPath -Name "Icon" -Value "`"$exePath`",0"
        Set-ItemProperty -Path $keyPath -Name "Position" -Value "Top"
        
        $commandPath = "$keyPath\command"
        if (-not (Test-Path $commandPath)) {
            New-Item -Path $commandPath -Force | Out-Null
        }
        Set-ItemProperty -Path $commandPath -Name "(Default)" -Value "`"$exePath`" `"%1`""
        
        Write-Host "  Registered for all image types" -ForegroundColor Green
    }
    catch {
        Write-Host "  Failed to register for image type: $_" -ForegroundColor Yellow
    }
}
else {
    Write-Host "[3/4] Skipping shell integration (--NoShellIntegration)" -ForegroundColor Yellow
}

# Create Start Menu shortcut
Write-Host "[4/4] Creating Start Menu shortcut..." -ForegroundColor Cyan
$startMenuPath = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Image Converter.lnk"

$WScriptShell = New-Object -ComObject WScript.Shell
$shortcut = $WScriptShell.CreateShortcut($startMenuPath)
$shortcut.TargetPath = $exePath
$shortcut.Description = "Convert images between formats"
$shortcut.WorkingDirectory = $InstallPath
$shortcut.Save()

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Image Converter has been installed to:" -ForegroundColor White
Write-Host "  $InstallPath" -ForegroundColor Gray
Write-Host ""
Write-Host "You can now:" -ForegroundColor White
Write-Host "  - Right-click any image and select 'Convert Image...'" -ForegroundColor Gray
Write-Host "  - Launch from Start Menu: 'Image Converter'" -ForegroundColor Gray
Write-Host ""

# Note about Windows 11
$osVersion = [System.Environment]::OSVersion.Version
if ($osVersion.Build -ge 22000) {
    Write-Host "Note for Windows 11:" -ForegroundColor Yellow
    Write-Host "  The 'Convert Image...' option appears in the classic context menu." -ForegroundColor Gray
    Write-Host "  Right-click an image, then click 'Show more options' to see it," -ForegroundColor Gray
    Write-Host "  or hold Shift while right-clicking to show the classic menu." -ForegroundColor Gray
    Write-Host ""
}

Read-Host "Press Enter to exit"
