using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Svg.Skia;
using SkiaSharp;

var outputDir = args.Length > 0 ? args[0] : @"..\..\ConvertMe.Package\Images";
var appAssetsDir = @"..\..\ConvertMe.App\Assets";
var svgPath = @"..\..\new-logo.svg";

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
Directory.CreateDirectory(appAssetsDir);

// Load and process the SVG - replace currentColor with blue
var svgContent = File.ReadAllText(svgPath);
// Use a nice blue color (similar to Tailwind blue-600 / the sidebar-primary from the CSS)
var blueColor = "#2563eb";
svgContent = svgContent.Replace("currentColor", blueColor);
svgContent = svgContent.Replace("class=\"text-blue-600\"", ""); // Remove class reference

// Generate master icon at high resolution (512x512)
using var masterIcon = RenderSvgToImage(svgContent, 512, 512);

// Save master icon for the app
masterIcon.Save(System.IO.Path.Combine(appAssetsDir, "app.png"));
Console.WriteLine("Created: app.png (512x512) - master icon");

// Generate ICO file for the app (Windows icon with multiple sizes)
GenerateIcoFile(svgContent, System.IO.Path.Combine(appAssetsDir, "app.ico"));
Console.WriteLine("Created: app.ico - Windows icon");

foreach (var (name, width, height) in assets)
{
    var outputPath = System.IO.Path.Combine(outputDir, name);
    
    if (width == height)
    {
        // Square - render SVG at this size for best quality
        using var icon = RenderSvgToImage(svgContent, width, height);
        icon.Save(outputPath);
    }
    else
    {
        // Non-square (wide/splash) - center the icon on dark background
        using var canvas = new Image<Rgba32>(width, height, new Rgba32(26, 26, 26, 255)); // #1a1a1a background
        
        var iconSize = Math.Min(width, height) - 40; // Leave padding
        using var icon = RenderSvgToImage(svgContent, iconSize, iconSize);
        
        var x = (width - iconSize) / 2;
        var y = (height - iconSize) / 2;
        
        canvas.Mutate(ctx => ctx.DrawImage(icon, new Point(x, y), 1f));
        canvas.Save(outputPath);
    }
    
    Console.WriteLine($"Created: {name} ({width}x{height})");
}

Console.WriteLine($"\nGenerated {assets.Length + 2} assets from SVG");

// Render SVG to ImageSharp image
static Image<Rgba32> RenderSvgToImage(string svgContent, int width, int height)
{
    using var svg = new SKSvg();
    svg.FromSvg(svgContent);
    
    if (svg.Picture == null)
        throw new Exception("Failed to load SVG");
    
    var bounds = svg.Picture.CullRect;
    float scaleX = width / bounds.Width;
    float scaleY = height / bounds.Height;
    float scale = Math.Min(scaleX, scaleY);
    
    using var bitmap = new SKBitmap(width, height);
    using var canvas = new SKCanvas(bitmap);
    
    canvas.Clear(SKColors.Transparent);
    
    // Center the SVG
    float offsetX = (width - bounds.Width * scale) / 2;
    float offsetY = (height - bounds.Height * scale) / 2;
    
    canvas.Translate(offsetX, offsetY);
    canvas.Scale(scale);
    canvas.DrawPicture(svg.Picture);
    
    // Convert SkiaSharp bitmap to ImageSharp
    var image = new Image<Rgba32>(width, height);
    var pixels = bitmap.Pixels;
    
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = pixels[y * width + x];
            image[x, y] = new Rgba32(pixel.Red, pixel.Green, pixel.Blue, pixel.Alpha);
        }
    }
    
    return image;
}

// Generate Windows ICO file with multiple sizes
static void GenerateIcoFile(string svgContent, string path)
{
    // ICO sizes needed for Windows (16, 32, 48, 256 are the key sizes)
    int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };
    
    using var fs = new FileStream(path, FileMode.Create);
    using var writer = new BinaryWriter(fs);
    
    // ICO Header
    writer.Write((short)0);           // Reserved
    writer.Write((short)1);           // Type: 1 = ICO
    writer.Write((short)sizes.Length); // Number of images
    
    // Calculate offsets - header is 6 bytes, each directory entry is 16 bytes
    int dataOffset = 6 + (sizes.Length * 16);
    var imageData = new List<byte[]>();
    
    // Generate PNG data for each size
    foreach (var size in sizes)
    {
        using var icon = RenderSvgToImage(svgContent, size, size);
        using var ms = new MemoryStream();
        icon.SaveAsPng(ms);
        imageData.Add(ms.ToArray());
    }
    
    // Write directory entries
    for (int i = 0; i < sizes.Length; i++)
    {
        var size = sizes[i];
        var data = imageData[i];
        
        writer.Write((byte)(size >= 256 ? 0 : size)); // Width (0 = 256)
        writer.Write((byte)(size >= 256 ? 0 : size)); // Height (0 = 256)
        writer.Write((byte)0);    // Color palette
        writer.Write((byte)0);    // Reserved
        writer.Write((short)1);   // Color planes
        writer.Write((short)32);  // Bits per pixel
        writer.Write(data.Length); // Image data size
        writer.Write(dataOffset);  // Offset to image data
        
        dataOffset += data.Length;
    }
    
    // Write image data
    foreach (var data in imageData)
    {
        writer.Write(data);
    }
}
