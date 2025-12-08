using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

var sourceIcon = args.Length > 0 ? args[0] : @"..\..\ConvertMe.App\Assets\app.png";
var outputDir = args.Length > 1 ? args[1] : @"..\..\ConvertMe.Package\Images";

// Store asset requirements
var assets = new (string Name, int Width, int Height)[]
{
    // Square logos
    ("Square44x44Logo.png", 44, 44),
    ("Square44x44Logo.scale-100.png", 44, 44),
    ("Square44x44Logo.scale-125.png", 55, 55),
    ("Square44x44Logo.scale-150.png", 66, 66),
    ("Square44x44Logo.scale-200.png", 88, 88),
    ("Square44x44Logo.scale-400.png", 176, 176),
    
    ("Square71x71Logo.png", 71, 71),
    ("SmallTile.png", 71, 71),
    
    ("Square150x150Logo.png", 150, 150),
    ("Square150x150Logo.scale-100.png", 150, 150),
    ("Square150x150Logo.scale-125.png", 188, 188),
    ("Square150x150Logo.scale-150.png", 225, 225),
    ("Square150x150Logo.scale-200.png", 300, 300),
    ("Square150x150Logo.scale-400.png", 600, 600),
    
    ("Square310x310Logo.png", 310, 310),
    ("LargeTile.png", 310, 310),
    
    // Wide logo (centered icon on transparent/colored background)
    ("Wide310x150Logo.png", 310, 150),
    
    // Store logo
    ("StoreLogo.png", 50, 50),
    ("StoreLogo.scale-100.png", 50, 50),
    ("StoreLogo.scale-125.png", 63, 63),
    ("StoreLogo.scale-150.png", 75, 75),
    ("StoreLogo.scale-200.png", 100, 100),
    ("StoreLogo.scale-400.png", 200, 200),
    
    // Splash screen (centered icon)
    ("SplashScreen.png", 620, 300),
    ("SplashScreen.scale-100.png", 620, 300),
    ("SplashScreen.scale-125.png", 775, 375),
    ("SplashScreen.scale-150.png", 930, 450),
    ("SplashScreen.scale-200.png", 1240, 600),
};

Directory.CreateDirectory(outputDir);

using var source = Image.Load<Rgba32>(sourceIcon);

foreach (var (name, width, height) in assets)
{
    var outputPath = Path.Combine(outputDir, name);
    
    if (width == height)
    {
        // Square - just resize
        using var resized = source.Clone(x => x.Resize(width, height));
        resized.Save(outputPath);
    }
    else
    {
        // Non-square (wide/splash) - center the icon
        using var canvas = new Image<Rgba32>(width, height, new Rgba32(26, 26, 26, 255)); // #1a1a1a background
        
        var iconSize = Math.Min(width, height) - 40; // Leave padding
        using var resizedIcon = source.Clone(x => x.Resize(iconSize, iconSize));
        
        var x = (width - iconSize) / 2;
        var y = (height - iconSize) / 2;
        
        canvas.Mutate(ctx => ctx.DrawImage(resizedIcon, new Point(x, y), 1f));
        canvas.Save(outputPath);
    }
    
    Console.WriteLine($"Created: {name} ({width}x{height})");
}

Console.WriteLine($"\nGenerated {assets.Length} assets in {Path.GetFullPath(outputDir)}");
