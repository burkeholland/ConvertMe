@echo off
:: Quick fix - Force Windows 11 to show classic context menu
:: This makes "Convert Image..." visible without clicking "Show more options"

echo ========================================
echo   Enable Classic Context Menu
echo ========================================
echo.
echo This will configure Windows 11 to always show the full classic
echo context menu (like Windows 10) instead of the simplified menu.
echo.
echo This affects ALL right-click menus system-wide.
echo.
echo Do you want to continue?
echo   [1] Enable classic menu (shows all options directly)
echo   [2] Restore Windows 11 modern menu
echo   [3] Cancel
echo.
set /p choice="Enter choice (1/2/3): "

if "%choice%"=="1" goto enable
if "%choice%"=="2" goto disable
if "%choice%"=="3" goto cancel
goto cancel

:enable
echo.
echo Enabling classic context menu...
reg add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
if %errorlevel% equ 0 (
    echo.
    echo Success! Restarting Explorer to apply changes...
    taskkill /f /im explorer.exe >nul 2>&1
    start explorer.exe
    echo.
    echo Classic context menu is now enabled!
    echo "Convert Image..." will appear in the main menu.
) else (
    echo Failed to modify registry.
)
goto end

:disable
echo.
echo Restoring Windows 11 modern menu...
reg delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f >nul 2>&1
echo.
echo Restarting Explorer to apply changes...
taskkill /f /im explorer.exe >nul 2>&1
start explorer.exe
echo.
echo Windows 11 modern menu restored!
goto end

:cancel
echo Cancelled.
goto end

:end
echo.
pause
