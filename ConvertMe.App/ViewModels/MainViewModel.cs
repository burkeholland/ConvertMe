using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConvertMe.Models;
using ConvertMe.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Media;
using System.Windows;
using ImageFormat = ConvertMe.Models.ImageFormat;
using ResizeMode = ConvertMe.Models.ResizeMode;

namespace ConvertMe.ViewModels;

/// <summary>
/// Main view model for the image converter application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ImageConversionService _conversionService;

    public MainViewModel()
    {
        _conversionService = new ImageConversionService();
        // Only include formats that can be conversion targets (excludes SVG)
        AvailableFormats = new ObservableCollection<FormatOption>(
            Enum.GetValues<ImageFormat>()
                .Where(f => f.CanBeConversionTarget())
                .Select(f => new FormatOption(f))
        );
        SelectedFormat = AvailableFormats.First(f => f.Format == ImageFormat.Png);
    }

    // Image source properties
    [ObservableProperty]
    private string? _sourceFilePath;

    [ObservableProperty]
    private ImageInfo? _imageInfo;

    [ObservableProperty]
    private bool _isImageLoaded;

    // Format selection
    [ObservableProperty]
    private ObservableCollection<FormatOption> _availableFormats;

    [ObservableProperty]
    private FormatOption _selectedFormat;

    // Quality settings
    [ObservableProperty]
    private int _quality = 85;

    [ObservableProperty]
    private bool _showQualitySlider;

    // Target size settings
    [ObservableProperty]
    private bool _useTargetSize;

    [ObservableProperty]
    private int _targetSizeKb = 500;

    // Resize settings
    [ObservableProperty]
    private bool _enableResize;

    [ObservableProperty]
    private ResizeMode _resizeMode = ResizeMode.None;

    [ObservableProperty]
    private int _targetWidth;

    [ObservableProperty]
    private int _targetHeight;

    [ObservableProperty]
    private bool _maintainAspectRatio = true;

    [ObservableProperty]
    private int _resizePercentage = 100;

    // Output settings
    [ObservableProperty]
    private string? _outputPath;

    [ObservableProperty]
    private bool _overwriteExisting;

    // Status
    [ObservableProperty]
    private bool _isConverting;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private ConversionResult? _lastResult;

    // Shell integration
    [ObservableProperty]
    private bool _isShellIntegrated;

    partial void OnSelectedFormatChanged(FormatOption value)
    {
        ShowQualitySlider = value.Format.SupportsQuality();
        UpdateOutputPath();
    }

    partial void OnSourceFilePathChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadImageInfoAsync(value);
        }
    }

    partial void OnEnableResizeChanged(bool value)
    {
        ResizeMode = value ? ResizeMode.ExactSize : ResizeMode.None;
    }

    private async Task LoadImageInfoAsync(string filePath)
    {
        try
        {
            StatusMessage = "Loading image...";
            HasError = false;
            ImageInfo = await _conversionService.GetImageInfoAsync(filePath);
            IsImageLoaded = true;
            TargetWidth = ImageInfo.Width;
            TargetHeight = ImageInfo.Height;
            UpdateOutputPath();
            StatusMessage = $"Loaded: {ImageInfo.FileName}";
        }
        catch (Exception ex)
        {
            HasError = true;
            StatusMessage = $"Error loading image: {ex.Message}";
            IsImageLoaded = false;
        }
    }

    private void UpdateOutputPath()
    {
        if (string.IsNullOrEmpty(SourceFilePath)) return;

        var directory = Path.GetDirectoryName(SourceFilePath) ?? "";
        var nameWithoutExt = Path.GetFileNameWithoutExtension(SourceFilePath);
        var newExtension = SelectedFormat.Format.GetFileExtension();

        OutputPath = Path.Combine(directory, $"{nameWithoutExt}{newExtension}");
    }

    [RelayCommand]
    private Task BrowseSourceAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Image to Convert",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp;*.tiff;*.tif;*.svg|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            SourceFilePath = dialog.FileName;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save Converted Image As",
            Filter = GetSaveFilter(),
            FileName = Path.GetFileName(OutputPath ?? "converted")
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    private string GetSaveFilter()
    {
        var ext = SelectedFormat.Format.GetFileExtension().TrimStart('.');
        var name = SelectedFormat.Format.GetDisplayName();
        return $"{name}|*.{ext}|All Files|*.*";
    }

    [RelayCommand]
    private async Task ConvertAsync()
    {
        if (string.IsNullOrEmpty(SourceFilePath) || !IsImageLoaded)
        {
            StatusMessage = "Please select an image first.";
            HasError = true;
            return;
        }

        IsConverting = true;
        HasError = false;
        StatusMessage = "Converting...";

        try
        {
            var options = BuildConversionOptions();
            var result = await _conversionService.ConvertAsync(SourceFilePath, options);

            LastResult = result;

            if (result.Success)
            {
                // Play Windows notification sound on successful conversion
                SystemSounds.Asterisk.Play();
                StatusMessage = $"✓ Converted successfully! Size: {result.FormattedNewSize} ({result.CompressionRatio:F1}% of original)";
                HasError = false;
            }
            else
            {
                StatusMessage = $"✗ {result.ErrorMessage}";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Conversion failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsConverting = false;
        }
    }

    private ConversionOptions BuildConversionOptions()
    {
        return new ConversionOptions
        {
            TargetFormat = SelectedFormat.Format,
            Quality = Quality,
            TargetSizeKb = UseTargetSize ? TargetSizeKb : 0,
            OutputPath = OutputPath,
            OverwriteExisting = OverwriteExisting,
            MaintainAspectRatio = MaintainAspectRatio,
            ResizeMode = EnableResize ? ResizeMode : Models.ResizeMode.None,
            TargetWidth = EnableResize ? (ResizeMode == Models.ResizeMode.Percentage ? ResizePercentage : TargetWidth) : 0,
            TargetHeight = EnableResize ? TargetHeight : 0
        };
    }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        if (LastResult?.Success == true && !string.IsNullOrEmpty(LastResult.OutputPath))
        {
            var directory = Path.GetDirectoryName(LastResult.OutputPath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{LastResult.OutputPath}\"");
            }
        }
    }

    [RelayCommand]
    private void CheckShellIntegration()
    {
        IsShellIntegrated = ShellIntegrationService.IsRegistered();
    }

    [RelayCommand]
    private void ToggleShellIntegration()
    {
        try
        {
            if (IsShellIntegrated)
            {
                if (ShellIntegrationService.IsAdministrator())
                {
                    ShellIntegrationService.Unregister();
                    IsShellIntegrated = false;
                    StatusMessage = "Shell integration removed.";
                }
                else
                {
                    ShellIntegrationService.RestartAsAdministrator("--unregister");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                if (ShellIntegrationService.IsAdministrator())
                {
                    var exePath = Environment.ProcessPath ?? "";
                    ShellIntegrationService.Register(exePath);
                    IsShellIntegrated = true;
                    StatusMessage = "Shell integration enabled!";
                }
                else
                {
                    ShellIntegrationService.RestartAsAdministrator("--register");
                    Application.Current.Shutdown();
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            HasError = true;
        }
    }

    public void LoadFromCommandLine(string filePath)
    {
        if (File.Exists(filePath) && ImageConversionService.IsSupportedFormat(filePath))
        {
            SourceFilePath = filePath;
        }
    }
}

/// <summary>
/// Represents a format option in the UI.
/// </summary>
public class FormatOption
{
    public ImageFormat Format { get; }
    public string DisplayName { get; }
    public bool SupportsQuality { get; }
    public bool SupportsTransparency { get; }

    public FormatOption(ImageFormat format)
    {
        Format = format;
        DisplayName = format.GetDisplayName();
        SupportsQuality = format.SupportsQuality();
        SupportsTransparency = format.SupportsTransparency();
    }

    public override string ToString() => DisplayName;
}
