@echo off
:: ConvertMe Installer
:: This batch file runs the PowerShell installer with administrator privileges

echo ========================================
echo   ConvertMe Installer
echo ========================================
echo.
echo This will install ConvertMe and add it to your right-click menu.
echo.
echo Press any key to continue, or close this window to cancel...
pause > nul

:: Check for admin rights and elevate if needed
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process -FilePath '%~f0' -Verb RunAs -ArgumentList '%~dp0'"
    exit /b
)

:: Change to script directory (important when elevated)
cd /d "%~dp0"

:: Run the PowerShell installer
powershell -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
