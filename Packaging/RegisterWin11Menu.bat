@echo off
:: Windows 11 Main Menu Registration
:: This enables the "Convert Image..." option in Windows 11's main context menu

echo ========================================
echo   Windows 11 Context Menu Setup
echo ========================================
echo.
echo This will register Image Converter in Windows 11's MAIN context menu
echo (not just "Show more options").
echo.
echo REQUIREMENTS:
echo   1. Windows 10 version 2004+ or Windows 11
echo   2. Developer Mode enabled (see below)
echo.
echo To enable Developer Mode:
echo   Settings ^> Privacy ^& Security ^> For developers ^> Developer Mode
echo.
echo Press any key to continue (or close to cancel)...
pause > nul

:: Check for admin
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

powershell -ExecutionPolicy Bypass -File "%~dp0RegisterWin11Menu.ps1"
