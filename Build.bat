@echo off
:: Build script for ConvertMe

echo ========================================
echo   Building ConvertMe
echo ========================================
echo.

:: Check for dotnet
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found!
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

cd /d "%~dp0"

echo [1/3] Restoring packages...
dotnet restore ImageConverter.App\ImageConverter.App.csproj
if %errorlevel% neq 0 (
    echo Error: Package restore failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Building application...
dotnet build ImageConverter.App\ImageConverter.App.csproj -c Release
if %errorlevel% neq 0 (
    echo Error: Build failed!
    pause
    exit /b 1
)

echo.
echo [3/3] Publishing self-contained executable...
dotnet publish ImageConverter.App\ImageConverter.App.csproj -c Release -r win-x64 --self-contained true
if %errorlevel% neq 0 (
    echo Error: Publish failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Build Complete!
echo ========================================
echo.
echo Output location:
echo   ImageConverter.App\bin\Release\net8.0-windows\win-x64\publish\
echo.
echo To install, run:
echo   Installer\Install.bat
echo.
pause
