using System.IO;
using System.Windows;
using ConvertMe.Services;
using ConvertMe.ViewModels;
using Wpf.Ui.Controls;

namespace ConvertMe;

/// <summary>
/// Main application window with drag-drop support.
/// </summary>
public partial class MainWindow : FluentWindow
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Check shell integration status on load
        ViewModel.CheckShellIntegrationCommand.Execute(null);
    }

    /// <summary>
    /// Load a file from command line argument.
    /// </summary>
    public void LoadFile(string filePath)
    {
        if (File.Exists(filePath) && ImageConversionService.IsSupportedFormat(filePath))
        {
            ViewModel.LoadFromCommandLine(filePath);
        }
    }

    /// <summary>
    /// Handle drag over for file drop.
    /// </summary>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0 && ImageConversionService.IsSupportedFormat(files[0]))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        e.Handled = true;
    }

    /// <summary>
    /// Handle file drop.
    /// </summary>
    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0 && ImageConversionService.IsSupportedFormat(files[0]))
            {
                ViewModel.SourceFilePath = files[0];
            }
        }
    }
}
