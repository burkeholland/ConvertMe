using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace ImageConverter.Services;

/// <summary>
/// Service for managing Windows shell context menu registration.
/// Creates a cascading submenu with format options.
/// </summary>
public class ShellIntegrationService
{
    // Constants for registry configuration
    private const string MenuName = "Convert Image";
    private const string RegistryKeyName = "ImageConverter";
    private const int SeparatorFlag = 0x20;  // CommandFlags value for separator before menu item
    
    private static readonly string[] ImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".ico"
    ];

    private static readonly (string Name, string Arg)[] FormatOptions =
    [
        ("JPEG", "jpeg"),
        ("PNG", "png"),
        ("WebP", "webp"),
        ("GIF", "gif"),
        ("BMP", "bmp"),
        ("TIFF", "tiff"),
        ("ICO", "ico"),
    ];

    /// <summary>
    /// Check if the application is registered in the shell context menu.
    /// </summary>
    public static bool IsRegistered()
    {
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey($"SystemFileAssociations\\.jpg\\shell\\{RegistryKeyName}");
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if the current process has administrator privileges.
    /// </summary>
    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Register the application in the Windows shell context menu with cascading submenu.
    /// Requires administrator privileges.
    /// </summary>
    public static void Register(string executablePath)
    {
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException("Administrator privileges required to register shell extension.");
        }

        foreach (var ext in ImageExtensions)
        {
            RegisterCascadingMenu($"SystemFileAssociations\\{ext}\\shell\\{RegistryKeyName}", executablePath);
        }

        RegisterCascadingMenu($"SystemFileAssociations\\image\\shell\\{RegistryKeyName}", executablePath);
    }

    /// <summary>
    /// Unregister the application from the Windows shell context menu.
    /// Requires administrator privileges.
    /// </summary>
    public static void Unregister()
    {
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException("Administrator privileges required to unregister shell extension.");
        }

        foreach (var ext in ImageExtensions)
        {
            DeleteRegistryKey($"SystemFileAssociations\\{ext}\\shell\\{RegistryKeyName}");
        }

        DeleteRegistryKey($"SystemFileAssociations\\image\\shell\\{RegistryKeyName}");
    }

    /// <summary>
    /// Register cascading context menu at the specified registry path.
    /// </summary>
    private static void RegisterCascadingMenu(string keyPath, string executablePath)
    {
        try
        {
            // Clean slate - remove existing key first
            Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);

            using var shellKey = Registry.ClassesRoot.CreateSubKey(keyPath);
            if (shellKey == null) return;

            // Configure cascading menu properties
            shellKey.SetValue("MUIVerb", MenuName);
            shellKey.SetValue("Icon", $"\"{executablePath}\",0");
            shellKey.SetValue("SubCommands", "");  // Enables submenu

            using var subShellKey = shellKey.CreateSubKey("shell");
            if (subShellKey == null) return;

            // Add format options to submenu
            int order = 0;
            foreach (var (name, arg) in FormatOptions)
            {
                using var formatKey = subShellKey.CreateSubKey(arg);
                if (formatKey == null) continue;

                formatKey.SetValue("MUIVerb", $"Convert to {name}");
                formatKey.SetValue("CommandFlags", order, RegistryValueKind.DWord);

                using var commandKey = formatKey.CreateSubKey("command");
                commandKey?.SetValue("", $"\"{executablePath}\" --convert \"{arg}\" \"%1\"");
                
                order++;
            }

            // Add Custom option with separator
            using var customKey = subShellKey.CreateSubKey("custom");
            if (customKey != null)
            {
                customKey.SetValue("MUIVerb", "Custom...");
                customKey.SetValue("CommandFlags", SeparatorFlag | order, RegistryValueKind.DWord);

                using var commandKey = customKey.CreateSubKey("command");
                commandKey?.SetValue("", $"\"{executablePath}\" --custom \"%1\"");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to register menu at {keyPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a registry key tree, ignoring errors if it doesn't exist.
    /// </summary>
    private static void DeleteRegistryKey(string keyPath)
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete registry key {keyPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Restart as administrator to perform registration.
    /// </summary>
    public static bool RestartAsAdministrator(string arguments = "")
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return false;

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
