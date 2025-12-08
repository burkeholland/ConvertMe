# Dark Mode Toggle

**Branch:** `feature/dark-mode-toggle`
**Description:** Add a manual theme toggle allowing users to switch between Light, Dark, and System theme modes with preference persistence

## Goal
Enable users to manually control the application theme (Light/Dark/System) instead of only following the Windows system setting. The preference will persist across application restarts using a JSON settings file.

## Current State
- ✅ App already supports dark mode via WPF-UI library (`Theme="Unknown"` auto-detects system)
- ✅ All UI elements use `DynamicResource` bindings that respond to theme changes
- ❌ No manual toggle exists - users cannot override system theme
- ❌ No settings persistence mechanism exists

## Implementation Steps

### Step 1: Add Settings Model and Service
**Files:** `ConvertMe.App/Models/AppSettings.cs`, `ConvertMe.App/Services/SettingsService.cs`
**What:** Create a settings model to hold theme preference and a service to load/save settings to a JSON file in `%APPDATA%/ConvertMe/settings.json`. The model will include a `ThemeMode` enum (Light, Dark, System) defaulting to System.
**Testing:** 
- Verify settings file is created on first save
- Verify settings persist and reload correctly after app restart

### Step 2: Add Theme Toggle to ViewModel
**Files:** `ConvertMe.App/ViewModels/MainViewModel.cs`
**What:** Add `CurrentThemeMode` property with values (System, Light, Dark), a `ThemeModeOptions` collection for the UI, and wire up the SettingsService to load/save preferences. Add logic to apply the selected theme using WPF-UI's `ApplicationThemeManager`.
**Testing:**
- Change theme in memory and verify UI updates
- Verify theme preference loads correctly on ViewModel initialization

### Step 3: Add Theme Toggle UI
**Files:** `ConvertMe.App/MainWindow.xaml`
**What:** Add a ComboBox or segmented control in the existing Settings card (inside the Expander) allowing users to select System/Light/Dark theme. Bind to the ViewModel's theme properties.
**Testing:**
- Toggle between themes and verify immediate visual change
- Restart app and verify last selected theme is applied
- Select "System" and verify it follows Windows theme

### Step 4: Apply Theme on Startup
**Files:** `ConvertMe.App/App.xaml.cs`
**What:** Replace the current `Task.Run(() => ApplicationThemeManager.ApplySystemTheme())` with logic that loads saved settings and applies the user's theme preference before the main window shows.
**Testing:**
- Set theme to Dark, restart app, verify dark theme loads immediately
- Set theme to Light, restart app, verify light theme loads immediately
- Set theme to System, change Windows theme, restart app, verify it follows system
