@echo off
:: Image Converter Uninstaller
:: This batch file runs the PowerShell uninstaller with administrator privileges

echo ========================================
echo   Image Converter Uninstaller
echo ========================================
echo.
echo This will remove Image Converter from your system.
echo.
echo Press any key to continue, or close this window to cancel...
pause > nul

:: Check for admin rights and elevate if needed
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

:: Run the PowerShell uninstaller
powershell -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1"
