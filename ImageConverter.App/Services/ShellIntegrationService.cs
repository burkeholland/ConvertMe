using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace ImageConverter.Services;

/// <summary>
/// Service for managing Windows shell context menu registration.
/// </summary>
public class ShellIntegrationService
{
    private const string MenuName = "Convert Image...";
    private const string RegistryKeyName = "ImageConverter";
    
    private static readonly string[] ImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".ico"
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
    /// Register the application in the Windows shell context menu.
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
            RegisterForExtension(ext, executablePath);
        }

        // Also register for all image types via SystemFileAssociations
        RegisterForSystemFileAssociation("image", executablePath);
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
            UnregisterForExtension(ext);
        }

        UnregisterForSystemFileAssociation("image");
    }

    /// <summary>
    /// Register context menu for a specific file extension.
    /// </summary>
    private static void RegisterForExtension(string extension, string executablePath)
    {
        var keyPath = $"SystemFileAssociations\\{extension}\\shell\\{RegistryKeyName}";

        try
        {
            using var shellKey = Registry.ClassesRoot.CreateSubKey(keyPath);
            if (shellKey == null) return;

            shellKey.SetValue("", MenuName);
            shellKey.SetValue("Icon", $"\"{executablePath}\",0");
            
            // Position at the top of the context menu
            shellKey.SetValue("Position", "Top");

            using var commandKey = shellKey.CreateSubKey("command");
            commandKey?.SetValue("", $"\"{executablePath}\" \"%1\"");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to register for {extension}: {ex.Message}");
        }
    }

    /// <summary>
    /// Register for system file association (applies to all files of a type).
    /// </summary>
    private static void RegisterForSystemFileAssociation(string type, string executablePath)
    {
        var keyPath = $"SystemFileAssociations\\{type}\\shell\\{RegistryKeyName}";

        try
        {
            using var shellKey = Registry.ClassesRoot.CreateSubKey(keyPath);
            if (shellKey == null) return;

            shellKey.SetValue("", MenuName);
            shellKey.SetValue("Icon", $"\"{executablePath}\",0");
            shellKey.SetValue("Position", "Top");

            using var commandKey = shellKey.CreateSubKey("command");
            commandKey?.SetValue("", $"\"{executablePath}\" \"%1\"");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to register for {type}: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregister context menu for a specific file extension.
    /// </summary>
    private static void UnregisterForExtension(string extension)
    {
        var keyPath = $"SystemFileAssociations\\{extension}\\shell\\{RegistryKeyName}";

        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to unregister for {extension}: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregister from system file association.
    /// </summary>
    private static void UnregisterForSystemFileAssociation(string type)
    {
        var keyPath = $"SystemFileAssociations\\{type}\\shell\\{RegistryKeyName}";

        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to unregister for {type}: {ex.Message}");
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
