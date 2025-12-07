# Microsoft Store Submission Guide

This document outlines the steps to publish ConvertMe to the Microsoft Store.

## Prerequisites

1. **Microsoft Partner Center Account** - Sign up at https://partner.microsoft.com/dashboard
   - One-time $19 registration fee for individuals
   - Or free with Visual Studio subscription

2. **Visual Studio 2022** with:
   - .NET Desktop Development workload
   - Windows App SDK / UWP development tools

## Setup Steps

### 1. Reserve App Name in Partner Center

1. Go to Partner Center → Apps and Games → New product → MSIX or PWA app
2. Reserve the name "ConvertMe" (or your preferred name)
3. Note down your **Publisher ID** (format: `CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`)

### 2. Update Package Identity

Edit `ImageConverter.Package/Package.appxmanifest`:

```xml
<Identity Name="YOUR_RESERVED_NAME"
          Publisher="CN=YOUR_PUBLISHER_ID"
          Version="1.0.0.0"
          ProcessorArchitecture="x64" />
```

Replace:
- `YOUR_RESERVED_NAME` with the name from Partner Center (e.g., `12345BurkeHolland.ImageConverter`)
- `YOUR_PUBLISHER_ID` with your Publisher ID from Partner Center

### 3. Build the MSIX Package

#### Option A: Using Visual Studio
1. Open `ImageConverter.sln` in Visual Studio 2022
2. Right-click `ImageConverter.Package` → Publish → Create App Packages
3. Select "Microsoft Store as package type" and sign in
4. Follow the wizard to create the package

#### Option B: Using Command Line
```powershell
# Build the package
msbuild ImageConverter.Package\ImageConverter.Package.wapproj /p:Configuration=Release /p:Platform=x64 /p:UapAppxPackageBuildMode=StoreUpload /p:AppxBundle=Never
```

### 4. Submit to Store

1. Go to Partner Center → Your App → Submissions → Start new submission
2. Fill in:
   - **Packages**: Upload the `.msix` or `.msixupload` file
   - **Store listing**: Description, screenshots, etc.
   - **Properties**: Category (Utilities), age rating, etc.
   - **Pricing**: Free or set a price

3. Submit for certification

## Store Listing Content

### Description (suggested)
```
ConvertMe is a fast, modern Windows app for converting images between formats.

Features:
• Convert between JPEG, PNG, WebP, GIF, BMP, TIFF, and ICO
• Convert SVG files to any raster format
• Adjust quality for lossy formats
• Target a specific file size
• Resize images while maintaining aspect ratio
• Right-click context menu integration
• Modern Windows 11 Fluent Design
• Dark theme support

Simply right-click any image in File Explorer and select "Convert Image" to get started!
```

### Keywords
image converter, format converter, png to jpg, webp converter, image resize, photo converter

### Category
Utilities & Tools → Photo & Media

## Important Notes

### runFullTrust Capability

This app uses `runFullTrust` capability for shell integration (context menu). This means:
- The app goes through additional review
- You may need to justify why this capability is needed
- Justification: "Required to register Windows Explorer context menu handlers for direct image conversion from File Explorer"

### Testing Before Submission

1. Build the MSIX package locally
2. Install it using the `.msix` file (may need to trust the certificate)
3. Verify:
   - App launches correctly
   - Context menu integration works (requires re-registration)
   - All conversion features work

## Regenerating Store Assets

If you update the app icon, regenerate assets:

```powershell
cd Tools\AssetGenerator
dotnet run
```

This creates all required icon sizes in `ImageConverter.Package\Images\`.
