using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Svg.Skia;
using SkiaSharp;

var outputDir = args.Length > 0 ? args[0] : @"..\..\ConvertMe.Package\Images";
var appAssetsDir = @"..\..\ConvertMe.App\Assets";
var storeAssetsDir = @"..\..\ConvertMe.Package\StoreAssets";
var svgPath = @"..\..\new-logo.svg";

// Package asset requirements (for app manifest)
var packageAssets = new (string Name, int Width, int Height)[]
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

// Microsoft Store listing assets (uploaded to Partner Center)
var storeAssets = new (string Name, int Width, int Height, string Description)[]
{
    // App icon for Store listing (required)
    ("AppIcon-300x300.png", 300, 300, "App icon (required)"),
    
    // Store logos - various sizes for different display contexts
    ("StoreLogo-720x1080.png", 720, 1080, "Store logo 2:3 poster"),
    ("StoreLogo-1080x1080.png", 1080, 1080, "Store logo 1:1 square"),
    
    // Screenshots placeholders - These need actual app screenshots
    // ("Screenshot-1366x768.png", 1366, 768, "Screenshot 16:9"),
    
    // Promotional images - Hero images for Store features
    ("Hero-1920x1080.png", 1920, 1080, "Hero image 16:9 (Xbox/Desktop)"),
    ("Hero-2400x1200.png", 2400, 1200, "Superhero art 2:1"),
    
    // Promotional tiles for Store
    ("Tile-414x180.png", 414, 180, "Small promo tile"),
    ("Tile-414x468.png", 414, 468, "Poster promo tile"),
    ("Tile-558x558.png", 558, 558, "Square promo tile"),
    ("Tile-558x756.png", 558, 756, "Tall promo tile"),
    ("Tile-846x468.png", 846, 468, "Wide promo tile"),
    
    // Box art
    ("BoxArt-1080x1080.png", 1080, 1080, "Box art square"),
    ("BoxArt-2160x1080.png", 2160, 1080, "Box art wide 2:1"),
    
    // Badge logo (for Store results)
    ("Badge-1920x1080.png", 1920, 1080, "Featured promotion badge"),
};

Directory.CreateDirectory(outputDir);
Directory.CreateDirectory(appAssetsDir);
Directory.CreateDirectory(storeAssetsDir);

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

Console.WriteLine("\n--- Package Assets ---");
foreach (var (name, width, height) in packageAssets)
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

Console.WriteLine("\n--- Store Listing Assets ---");
foreach (var (name, width, height, description) in storeAssets)
{
    var outputPath = System.IO.Path.Combine(storeAssetsDir, name);
    GenerateStoreAsset(svgContent, outputPath, width, height, name);
    Console.WriteLine($"Created: {name} ({width}x{height}) - {description}");
}

Console.WriteLine($"\nGenerated {packageAssets.Length + 2} package assets");
Console.WriteLine($"Generated {storeAssets.Length} store listing assets in StoreAssets/");

// Generate a Store asset with centered logo and branded background
static void GenerateStoreAsset(string svgContent, string outputPath, int width, int height, string name)
{
    // Dark gradient background with subtle brand color
    var bgColor = new Rgba32(20, 20, 30, 255); // Dark blue-ish gray
    
    using var canvas = new Image<Rgba32>(width, height, bgColor);
    
    // Add a subtle gradient overlay
    canvas.Mutate(ctx =>
    {
        // Create gradient effect manually with a semi-transparent overlay
        var gradientColor = new Rgba32(37, 99, 235, 40); // Blue with low alpha
        var gradientRect = new RectangularPolygon(0, 0, width, height);
        ctx.Fill(gradientColor, gradientRect);
    });
    
    // Calculate icon size based on asset dimensions
    // For square assets, icon takes up ~60% of the space
    // For wide/tall assets, icon is sized relative to the smaller dimension
    int iconSize;
    if (width == height)
    {
        iconSize = (int)(width * 0.5);
    }
    else if (width > height)
    {
        // Wide format - use height as reference
        iconSize = (int)(height * 0.5);
    }
    else
    {
        // Tall format - use width as reference
        iconSize = (int)(width * 0.5);
    }
    
    // Ensure minimum icon size for very small assets
    iconSize = Math.Max(iconSize, 80);
    
    using var icon = RenderSvgToImage(svgContent, iconSize, iconSize);
    
    // Center the icon
    var x = (width - iconSize) / 2;
    var y = (height - iconSize) / 2;
    
    // For tall formats, position icon in upper portion
    if (height > width * 1.3)
    {
        y = (int)(height * 0.3) - iconSize / 2;
    }
    
    canvas.Mutate(ctx => ctx.DrawImage(icon, new Point(x, y), 1f));
    
    // Add app name text for larger promotional assets
    if (width >= 500 && height >= 400)
    {
        // For now we just have the icon - text would require font rendering
        // which is more complex. The store allows adding text separately.
    }
    
    canvas.Save(outputPath);
}

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
