# Getting into Windows 11's Main Context Menu

Windows 11 changed how context menus work. Here are your options:

## Option 1: MSIX Packaging (Recommended for Distribution)

Package the app as MSIX with a proper manifest. This is the Microsoft-recommended approach.

**Pros:** Works properly, distributable via Store or sideloading
**Cons:** Requires Windows SDK, signing certificate for production

### Steps:
1. Install Visual Studio with MSIX packaging tools
2. Create a Windows Application Packaging Project
3. Add the `desktop4:FileExplorerContextMenus` extension to the manifest
4. Build and install the MSIX package

## Option 2: Sparse Package (Development/Testing)

Use `Add-AppxPackage -Register` with an external manifest.

**Pros:** No packaging required, uses existing EXE
**Cons:** Requires Developer Mode enabled on the PC

### Steps:
1. Enable Developer Mode (Settings > Privacy & Security > For developers)
2. Run `Packaging\RegisterWin11Menu.ps1` as Administrator

## Option 3: User Registry Workaround (Force Classic Menu)

Force Windows 11 to always show the classic context menu for all apps.

**Pros:** Simple, works immediately
**Cons:** Affects all context menus system-wide, not just this app

### To enable classic context menu globally:
```powershell
# Run as Administrator
reg add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
# Restart Explorer
Stop-Process -Name explorer -Force
```

### To revert to Windows 11 modern menu:
```powershell
reg delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f
Stop-Process -Name explorer -Force
```

---

## Current Status

The app currently uses the classic registry approach which works in:
- ✅ Windows 10 (main menu)
- ✅ Windows 11 (under "Show more options")

To appear in Windows 11's main menu, you need to use Option 1 or 2 above.
