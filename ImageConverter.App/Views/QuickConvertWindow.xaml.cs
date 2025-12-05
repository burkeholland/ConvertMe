using System.Windows;
using ImageConverter.Models;
using ImageConverter.Services;
using Wpf.Ui.Controls;
using ImageFormat = ImageConverter.Models.ImageFormat;

namespace ImageConverter.Views;

/// <summary>
/// Quick convert window for one-click format conversion.
/// </summary>
public partial class QuickConvertWindow : FluentWindow
{
    private readonly string _sourceFilePath;
    private readonly ImageConversionService _conversionService;

    public string? ResultPath { get; private set; }
    public bool OpenMainWindow { get; private set; }

    public QuickConvertWindow(string sourceFilePath)
    {
        InitializeComponent();
        _sourceFilePath = sourceFilePath;
        _conversionService = new ImageConversionService();
    }

    private async void OnFormatClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Wpf.Ui.Controls.Button button || button.Tag is not string formatName)
            return;

        if (!Enum.TryParse<ImageFormat>(formatName, true, out var format))
            return;

        IsEnabled = false;
        TxtStatus.Text = $"Converting to {format}...";

        try
        {
            var options = new ConversionOptions
            {
                TargetFormat = format,
                Quality = format == ImageFormat.Jpeg || format == ImageFormat.WebP ? 85 : 100
            };

            var result = await _conversionService.ConvertAsync(_sourceFilePath, options);

            if (result.Success)
            {
                ResultPath = result.OutputPath;
                TxtStatus.Text = $"✓ Saved: {System.IO.Path.GetFileName(result.OutputPath)}";
                
                // Open the output folder with file selected
                if (!string.IsNullOrEmpty(result.OutputPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{result.OutputPath}\"");
                }

                await Task.Delay(1000);
                DialogResult = true;
                Close();
            }
            else
            {
                TxtStatus.Text = $"✗ {result.ErrorMessage}";
                IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"✗ Error: {ex.Message}";
            IsEnabled = true;
        }
    }

    private void OnMoreOptionsClick(object sender, RoutedEventArgs e)
    {
        OpenMainWindow = true;
        DialogResult = false;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
