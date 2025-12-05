# Image Converter

A modern Windows application for converting images between formats directly from the right-click context menu.

![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11-blue)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

### Core Functionality
- **Format Conversion**: Convert between JPEG, PNG, WebP, GIF, BMP, TIFF, and ICO formats
- **Quality Control**: Adjustable quality slider for lossy formats (JPEG, WebP)
- **Target File Size**: Automatically adjust quality to meet a specific file size target
- **Image Resizing**: Resize images by exact dimensions, percentage, or maximum width/height
- **Aspect Ratio Lock**: Maintain proportions when resizing

### User Experience
- **Modern Fluent Design**: Native Windows 11 styling with dark theme support
- **Drag & Drop**: Simply drag images onto the window to load them
- **Shell Integration**: Right-click any image in Windows Explorer to convert
- **One-Click Installation**: Easy setup with automatic administrator elevation

### Additional Features
- **Batch Information**: View original file size, dimensions, and format before converting
- **Compression Stats**: See the resulting file size and compression ratio after conversion
- **Quick Access**: Open output folder directly after conversion
- **Metadata Handling**: Option to preserve or strip image metadata

## Installation

### Quick Install (Recommended)

1. Download or build the application
2. Navigate to the `Installer` folder
3. Double-click `Install.bat`
4. Follow the prompts (will request administrator access)

That's it! You can now right-click any image file and select "Convert Image..."

### Manual Installation

1. Build the application:
   ```powershell
   cd ImageConverter.App
   dotnet publish -c Release -r win-x64 --self-contained true
   ```

2. Copy the contents of `bin\Release\net8.0-windows\win-x64\publish` to your desired location

3. Run the application with `--register` flag as administrator to enable shell integration:
   ```powershell
   .\ImageConverter.exe --register
   ```

## Usage

### From Context Menu
1. Right-click on any image file (JPG, PNG, GIF, WebP, etc.)
2. Select "Convert Image..." from the menu
   - **Windows 10**: Option appears in the main context menu
   - **Windows 11**: See below for options to show in main menu
3. Choose your desired output format and options
4. Click "Convert Image"

### Windows 11 Context Menu Options

By default on Windows 11, "Convert Image..." appears under "Show more options". To fix this:

**Option A: Enable Classic Menu (Easiest)**
1. Run `Installer\EnableClassicMenu.bat`
2. Choose option 1 to enable classic menu
3. This shows all context menu items directly (affects all apps)

**Option B: Hold Shift**
- Hold Shift while right-clicking to show the full menu instantly

**Option C: Sparse Package (Advanced)**
1. Enable Developer Mode in Windows Settings
2. Run `Packaging\RegisterWin11Menu.bat` as Administrator

### From the Application
1. Launch Image Converter from the Start Menu
2. Drag & drop an image onto the window, or click "Browse..." to select a file
3. Configure your conversion options:
   - Select output format
   - Adjust quality (for JPEG/WebP)
   - Optionally set a target file size
   - Optionally resize the image
4. Click "Convert Image"

## Supported Formats

| Format | Read | Write | Quality Control | Transparency |
|--------|------|-------|-----------------|--------------|
| JPEG   | ✅   | ✅    | ✅              | ❌           |
| PNG    | ✅   | ✅    | ❌              | ✅           |
| WebP   | ✅   | ✅    | ✅              | ✅           |
| GIF    | ✅   | ✅    | ❌              | ✅           |
| BMP    | ✅   | ✅    | ❌              | ❌           |
| TIFF   | ✅   | ✅    | ❌              | ❌           |
| ICO    | ✅   | ✅    | ❌              | ✅           |

## Configuration Options

### Quality Settings
- **Quality Slider**: 1-100% for JPEG and WebP formats
  - Higher values = better quality, larger file size
  - Lower values = more compression, smaller file size

### Target File Size
- Enable to automatically find the optimal quality for a target file size
- Uses binary search to find the best quality setting
- Works with JPEG and WebP formats

### Resize Options
- **Exact Size**: Specify exact width and height
- **Maintain Aspect Ratio**: Keep proportions when one dimension changes
- **Percentage**: Scale image by a percentage of original size

## Uninstallation

### Quick Uninstall
1. Navigate to the `Installer` folder
2. Double-click `Uninstall.bat`
3. Follow the prompts

### Manual Uninstall
1. Run the application with `--unregister` flag as administrator:
   ```powershell
   .\ImageConverter.exe --unregister
   ```
2. Delete the application folder

## System Requirements

- **Operating System**: Windows 10 (1903+) or Windows 11
- **Runtime**: Self-contained (no .NET installation required)
- **Architecture**: x64
- **Disk Space**: ~100 MB

## Building from Source

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (optional, for development)

### Build Commands

```powershell
# Clone or download the source code
cd ImageConverter

# Restore dependencies
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true
```

## Technical Details

### Architecture
- **Framework**: .NET 8 with WPF
- **UI Library**: WPF-UI (Fluent Design)
- **Image Processing**: SixLabors.ImageSharp
- **MVVM**: CommunityToolkit.Mvvm
- **Shell Integration**: Windows Registry (SystemFileAssociations)

### Project Structure
```
ImageConverter/
├── ImageConverter.App/
│   ├── Assets/              # Icons and resources
│   ├── Converters/          # XAML value converters
│   ├── Models/              # Data models
│   ├── Services/            # Business logic
│   ├── ViewModels/          # MVVM ViewModels
│   ├── Views/               # XAML views
│   ├── App.xaml             # Application resources
│   └── MainWindow.xaml      # Main window
├── Installer/
│   ├── Install.bat          # Easy install script
│   ├── Install.ps1          # PowerShell installer
│   ├── Uninstall.bat        # Easy uninstall script
│   └── Uninstall.ps1        # PowerShell uninstaller
└── README.md
```

## Known Limitations

1. **Windows 11 Context Menu**: Due to Windows 11's new context menu design, the "Convert Image..." option appears in the classic context menu. Access it by:
   - Clicking "Show more options" after right-clicking
   - Holding Shift while right-clicking

2. **ICO Output**: ICO files are limited to single-resolution output (256x256 max)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [WPF-UI](https://github.com/lepoco/wpfui) - Modern Fluent Design for WPF
- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Cross-platform image processing
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
