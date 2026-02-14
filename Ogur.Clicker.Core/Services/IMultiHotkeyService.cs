// Ogur.Clicker.Core/Services/IMultiHotkeyService.cs
namespace Ogur.Clicker.Core.Services;

public interface IMultiHotkeyService
{
    /// <summary>
    /// Registers a global hotkey and returns its ID
    /// </summary>
    int RegisterHotkey(IntPtr hwnd, int virtualKey, bool ctrl, bool alt, bool shift);

    /// <summary>
    /// Unregisters specific hotkey by ID
    /// </summary>
    void UnregisterHotkey(IntPtr hwnd, int hotkeyId);

    /// <summary>
    /// Unregisters all hotkeys
    /// </summary>
    void UnregisterAll(IntPtr hwnd);

    /// <summary>
    /// Triggered when any registered hotkey is pressed
    /// </summary>
    event EventHandler<int>? HotkeyTriggered;

    /// <summary>
    /// Check if specific hotkey ID is registered
    /// </summary>
    bool IsRegistered(int hotkeyId);
}