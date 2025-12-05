using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageConverter.Views;

/// <summary>
/// A control that displays an image preview with size information.
/// </summary>
public class ImagePreviewControl : Border
{
    public static readonly DependencyProperty SourcePathProperty =
        DependencyProperty.Register(
            nameof(SourcePath),
            typeof(string),
            typeof(ImagePreviewControl),
            new PropertyMetadata(null, OnSourcePathChanged));

    public string? SourcePath
    {
        get => (string?)GetValue(SourcePathProperty);
        set => SetValue(SourcePathProperty, value);
    }

    private Image _imageControl = null!;
    private TextBlock _sizeText = null!;

    public ImagePreviewControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        CornerRadius = new CornerRadius(8);
        Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));
        Padding = new Thickness(8);
        MinHeight = 150;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _imageControl = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(_imageControl, 0);

        _sizeText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
            FontSize = 11,
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(_sizeText, 1);

        grid.Children.Add(_imageControl);
        grid.Children.Add(_sizeText);

        Child = grid;
    }

    private static void OnSourcePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImagePreviewControl control && e.NewValue is string path)
        {
            control.LoadImage(path);
        }
    }

    private void LoadImage(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                _imageControl.Source = null;
                _sizeText.Text = "";
                return;
            }

            // Load image with lower resolution for preview
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 400; // Limit preview size
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            bitmap.Freeze();

            _imageControl.Source = bitmap;
            _sizeText.Text = $"Preview: {bitmap.PixelWidth} × {bitmap.PixelHeight}";
        }
        catch
        {
            _imageControl.Source = null;
            _sizeText.Text = "Unable to load preview";
        }
    }
}
