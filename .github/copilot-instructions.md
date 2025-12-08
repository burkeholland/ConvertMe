# ConvertMe - Copilot Instructions

## Project Overview
Windows WPF image converter with Explorer shell integration. Converts between JPEG, PNG, WebP, GIF, BMP, TIFF, ICO formats. SVG is read-only (can convert FROM but not TO).

**Tech Stack:** .NET 8, WPF, WPF-UI (Fluent Design), SixLabors.ImageSharp, CommunityToolkit.Mvvm, Svg.Skia

## Architecture

### MVVM Pattern
- **ViewModels** use `CommunityToolkit.Mvvm` source generators: `[ObservableProperty]` for bindable props, `[RelayCommand]` for commands
- Property change handlers via `partial void On{PropertyName}Changed(T value)`
- View binds to ViewModel in XAML: `<Window.DataContext><vm:MainViewModel /></Window.DataContext>`

### Key Services
- [ImageConversionService](ConvertMe.App/Services/ImageConversionService.cs) - Core conversion logic using ImageSharp; SVG handled separately via Svg.Skia/SkiaSharp
- [ShellIntegrationService](ConvertMe.App/Services/ShellIntegrationService.cs) - Registry-based context menu (requires admin)

### Command-Line Modes (App.xaml.cs)
```
ConvertMe.exe --register           # Install shell integration (admin)
ConvertMe.exe --unregister         # Remove shell integration (admin)
ConvertMe.exe --convert <fmt> <file>  # Silent conversion (from context menu)
ConvertMe.exe --custom <file>      # Open UI with file pre-loaded
ConvertMe.exe <file>               # Legacy: open UI with file
```

## Build & Run
```powershell
# Quick build (uses Build.bat)
.\Build.bat

# Manual build
dotnet build ConvertMe.App -c Release

# Self-contained publish (creates single executable)
dotnet publish ConvertMe.App -c Release -r win-x64 --self-contained true
# Output: ConvertMe.App\bin\Release\net8.0-windows\win-x64\publish\

# Install shell integration (run from Installer folder)
.\Install.bat  # Prompts for admin
```

## Code Conventions

### Image Format Handling
- Formats defined in [ImageFormat.cs](ConvertMe.App/Models/ImageFormat.cs) with extension methods
- Check `CanBeConversionTarget()` before allowing format as output (SVG returns false)
- Quality slider only for formats where `SupportsQuality()` returns true (JPEG, WebP)

### XAML/UI
- Uses `ui:` namespace for WPF-UI controls (`ui:FluentWindow`, `ui:Card`, `ui:Button`)
- Theme follows system via `ApplicationThemeManager.ApplySystemTheme()`
- Value converters in [Converters/ValueConverters.cs](ConvertMe.App/Converters/ValueConverters.cs)
- All UI binds to `DynamicResource` for theme support

### Shell Integration Pattern
Registry keys created under `HKCR\SystemFileAssociations\{ext}\shell\ConvertMe` with cascading submenu structure. Format options use numbered prefixes (1_jpeg, 2_png) to control menu order.

## GitHub Releases & Deployment
Automated via [.github/workflows/release.yml](.github/workflows/release.yml) on push to `master`:

1. **Version bump** - Auto-increments patch version (or manually trigger with major/minor/patch choice)
2. **Build** - Publishes self-contained win-x64 executable
3. **Package** - Creates ZIP with `ConvertMe.exe` + installer scripts
4. **Release** - Creates GitHub Release with the ZIP attached
5. **Landing page** - Deploys `docs/` folder to GitHub Pages

**Manual release:** Go to Actions → "Build and Release" → Run workflow → Select version bump type

**Version source:** Tags (e.g., `v1.2.3`) - the workflow reads the latest tag and increments it

## Testing Considerations
- Shell integration requires admin elevation and Explorer restart to see changes
- SVG conversion uses different code path (Svg.Skia → SkiaSharp → ImageSharp)
- Target file size feature uses binary search for optimal quality (lossy formats only)
- Windows 11: context menu appears in "Show more options" submenu

## Feature Plans
Active plans in [plans/](plans/) directory follow structured format with implementation steps.
