# Ogur.Clicker

[![wakatime](https://wakatime.com/badge/github/ishkabar/ogur-clicker.svg?style=flat-square)](https://wakatime.com/badge/github/ishkabar/ogur-clicker)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp)
![WPF](https://img.shields.io/badge/WPF-Windows-7B68EE?style=flat-square)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=flat-square&logo=windows)


A high-precision Windows desktop application for automated mouse clicking with customizable hotkeys and multiple input methods.

## Features

- **Position Capture**: Click anywhere on screen to capture coordinates
- **Global Hotkeys**: Register keyboard or mouse button combinations (MB3/MB4/MB5)
- **Multiple Input Methods**: 
  - SendInput (standard)
  - SendInput Hardware
  - PostMessage
  - DirectInput (mouse_event)
- **Return to Origin**: Optional cursor position restoration after click
- **Always on Top**: Pin window above other applications
- **License System**: Built-in expiration date validation

## Architecture
```
Ogur.Clicker/
├── Core/              # Domain models and service interfaces
├── Infrastructure/    # Service implementations (hooks, mouse control)
├── Host/              # WPF UI layer (MVVM)
```

### Core Layer
- `ClickAction`, `MouseButton`, `MouseInputMethod` - Domain models
- Service contracts for mouse control, keyboard/mouse hooks, hotkey management

### Infrastructure Layer
- **MouseService**: Win32 API wrappers for clicking (SendInput/PostMessage/mouse_event)
- **MouseHookService**: Low-level mouse hook (WH_MOUSE_LL)
- **KeyboardHookService**: Low-level keyboard hook (WH_KEYBOARD_LL)
- **GlobalMouseHotkeyService**: Mouse button hotkey registration with modifier keys
- **GlobalKeyboardHotkeyService**: RegisterHotKey API integration
- **PositionCaptureService**: One-click coordinate capture

### Host Layer
- **LoginWindow**: Startup authentication screen
- **MainWindow**: Primary UI with position/hotkey capture, click execution
- **LicenseWindow**: Expiration enforcement (Dec 12, 2025)
- **MainViewModel**: Orchestrates services, manages state, command routing

## Technical Details

### Hook Management
- LMB/RMB blocked as hotkeys to prevent interference
- `_ignoreNextTrigger` flag prevents double-firing on registration
- Automatic hook cleanup on app exit with forced Environment.Exit(0)

### Emergency Exit
- `Ctrl+Alt+Q` force quits from MainWindow
- Watchdog timer (30s) monitors UI responsiveness

### Click Execution
- Configurable delay before click (10ms default)
- Optional cursor return with 15ms delay
- Window handle detection for PostMessage method

## Requirements

- Windows 10/11
- .NET 8.0
- Administrator privileges (for global hooks)

## Usage

1. Launch application
2. Click "Capture Position" → Click target location
3. Click "Capture Hotkey" → Press key/mouse button combo
4. Configure mouse button and input method
5. Trigger hotkey to execute click

## Configuration

- **Mouse Button**: Left/Right/Middle
- **Input Method**: SendInput, SendInput Hardware, PostMessage, DirectInput
- **Return to Original**: Toggle cursor restoration
- **Always on Top**: Pin window

## License

Expires: December 12, 2025
