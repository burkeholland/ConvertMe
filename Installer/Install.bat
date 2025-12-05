@echo off
:: Image Converter Installer
:: This batch file runs the PowerShell installer with administrator privileges

echo ========================================
echo   Image Converter Installer
echo ========================================
echo.
echo This will install Image Converter and add it to your right-click menu.
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

:: Run the PowerShell installer
powershell -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
