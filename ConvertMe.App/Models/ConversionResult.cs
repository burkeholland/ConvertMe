namespace ConvertMe.Models;

/// <summary>
/// Result of an image conversion operation.
/// </summary>
public class ConversionResult
{
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long OriginalSizeBytes { get; set; }
    public long NewSizeBytes { get; set; }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public int NewWidth { get; set; }
    public int NewHeight { get; set; }
    public TimeSpan ConversionTime { get; set; }

    public double CompressionRatio => OriginalSizeBytes > 0 
        ? (double)NewSizeBytes / OriginalSizeBytes * 100 
        : 100;

    public string FormattedOriginalSize => FormatFileSize(OriginalSizeBytes);
    public string FormattedNewSize => FormatFileSize(NewSizeBytes);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }

    public static ConversionResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
