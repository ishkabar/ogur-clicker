# Ogur.Clicker

[![wakatime](https://wakatime.com/badge/github/ishkabar/ogur-clicker.svg?style=flat-square)](https://wakatime.com/badge/github/ishkabar/ogur-clicker)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp)
![WPF](https://img.shields.io/badge/WPF-Windows-7B68EE?style=flat-square)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=flat-square&logo=windows)

A high-precision Windows automation tool with customizable hotkeys, multi-slot execution, and process-aware focus detection.

## Features

- **Hotbar System**: Multi-slot keyboard execution with customizable keys and delays
- **Focus Detection**: Process-aware execution - hotkeys only work when target window is active
- **Multiple Input Methods**:
  - SendInput (standard)
  - keybd_event (legacy)
- **Profile Management**: Save/load configurations with auto-restore on startup
- **Portable View**: Compact overlay window with real-time status indicators
- **Ogur.Hub Integration**: Authentication and license validation
- **Always on Top**: Pin windows above other applications

## Architecture
```
Ogur.Clicker/
├── Core/              # Domain models, services, constants
├── Infrastructure/    # Service implementations (hooks, keyboard, focus)
├── Host/              # WPF UI layer (MVVM)
```

### Core Layer
- `HotbarProfile`, `HotbarSlot` - Domain models with persistence
- `AppSettings` - Encrypted credential storage (ProtectedData)
- Service contracts for keyboard control, hooks, hotbar management, focus detection

### Infrastructure Layer
- **HotbarService**: Slot management, profile save/load, hotkey orchestration
- **KeyboardService**: Win32 API wrappers (SendInput/keybd_event)
- **KeyboardHookService**: Low-level keyboard hook (WH_KEYBOARD_LL)
- **GameFocusService**: Process focus detection with Windows API (GetForegroundWindow)
- **GlobalKeyboardHotkeyService**: RegisterHotKey API integration
- **MultiHotkeyService**: Multi-key combination support

### Host Layer
- **LoginWindow**: Ogur.Hub authentication with remember me
- **MainWindow**: Primary UI with slot configuration, focus settings
- **PortableView**: Compact overlay with status indicators (NoFocus/Ready/Executing)
- **LicenseWindow**: Expiration enforcement
- **MainViewModel**: Service orchestration, profile management, command routing

## Technical Details

### Focus Detection
- 100ms timer checks if target process has focus
- Status: `NoFocus` (gray) → `Ready` (green) → `Executing` (red)
- Hotkeys blocked when target process not focused

### Profile Persistence
- Auto-save on exit: `%AppData%/OgurClicker/last_profile.json`
- Auto-load on startup
- Stores: slots, input method, always-on-top, target process, focus interval
- Backward compatible with old profiles (property initializers)

### Hook Management
- Automatic cleanup on app exit with forced Environment.Exit(0)
- `_ignoreNextTrigger` flag prevents double-firing on registration

### Emergency Exit
- `Ctrl+Alt+Q` force quits from MainWindow
- Watchdog timer (30s) monitors UI responsiveness

## Requirements

- Windows 10/11
- .NET 8.0
- Administrator privileges (for global hooks)
- Ogur.Hub account (authentication required)

## Usage

1. Launch application → Login with Ogur.Hub credentials
2. Select target process (Process Selection Dialog)
3. Configure slots: Hotkey → Keys to execute → Delay → Press count
4. Choose input method (SendInput/keybd_event)
5. Open Portable View for compact overlay
6. Trigger hotkeys when target process is focused

## Configuration

- **Input Method**: SendInput, keybd_event
- **Focus Check Interval**: 100ms default
- **Target Process**: Set via Process Selection Dialog
- **Always on Top**: Pin windows
- **Profile Auto-Save**: Enabled by default

## Code Signing

Self-signed certificate included for distribution:
- `install-cert.bat` - Installs Ogur Development certificate
- `sign.bat` - Sign single file (drag & drop)
- `sign-multiple.bat` - Sign multiple files at once

## License

Ogur.Hub license validation - Expires: October 10, 2026