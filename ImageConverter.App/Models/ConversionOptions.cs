namespace ImageConverter.Models;

/// <summary>
/// Options for image conversion including format, quality, and resize settings.
/// </summary>
public record ConversionOptions
{
    public ImageFormat TargetFormat { get; init; } = ImageFormat.Png;
    
    /// <summary>
    /// Quality setting for lossy formats (JPEG, WebP). Range: 1-100.
    /// </summary>
    public int Quality { get; init; } = 85;
    
    /// <summary>
    /// Target file size in KB. 0 means no size limit.
    /// </summary>
    public long TargetSizeKb { get; init; } = 0;
    
    /// <summary>
    /// Resize mode for the image.
    /// </summary>
    public ResizeMode ResizeMode { get; init; } = ResizeMode.None;
    
    /// <summary>
    /// Target width for resize operations.
    /// </summary>
    public int TargetWidth { get; init; } = 0;
    
    /// <summary>
    /// Target height for resize operations.
    /// </summary>
    public int TargetHeight { get; init; } = 0;
    
    /// <summary>
    /// Maintain aspect ratio when resizing.
    /// </summary>
    public bool MaintainAspectRatio { get; init; } = true;
    
    /// <summary>
    /// Output path for the converted image. If null, uses same directory with new extension.
    /// </summary>
    public string? OutputPath { get; init; }
    
    /// <summary>
    /// Overwrite existing file if it exists.
    /// </summary>
    public bool OverwriteExisting { get; init; } = false;
}

/// <summary>
/// Resize modes for image conversion.
/// </summary>
public enum ResizeMode
{
    None,
    ExactSize,
    MaxWidth,
    MaxHeight,
    Percentage
}
