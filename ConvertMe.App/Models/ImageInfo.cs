namespace ConvertMe.Models;

/// <summary>
/// Represents information about a source image file.
/// </summary>
public class ImageInfo
{
    public required string FilePath { get; set; }
    public required string FileName { get; set; }
    public required string Extension { get; set; }
    public required string Directory { get; set; }
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? DetectedFormat { get; set; }
    public bool HasTransparency { get; set; }

    public string FormattedFileSize
    {
        get
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            int order = 0;
            double size = FileSizeBytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }

    public string Dimensions => $"{Width} × {Height}";
    public double AspectRatio => Height > 0 ? (double)Width / Height : 1;
}
