// Ogur.Clicker.Infrastructure/Services/MultiHotkeyService.cs
using System.Runtime.InteropServices;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class MultiHotkeyService : IMultiHotkeyService
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    private readonly Dictionary<int, bool> _registeredHotkeys = new();
    private int _nextId = 1;

    public event EventHandler<int>? HotkeyTriggered;

    public int RegisterHotkey(IntPtr hwnd, int virtualKey, bool ctrl, bool alt, bool shift)
    {
        uint modifiers = MOD_NOREPEAT;
        if (ctrl) modifiers |= MOD_CONTROL;
        if (alt) modifiers |= MOD_ALT;
        if (shift) modifiers |= MOD_SHIFT;

        int hotkeyId = _nextId++;

        bool success = RegisterHotKey(hwnd, hotkeyId, modifiers, (uint)virtualKey);

        if (success)
        {
            _registeredHotkeys[hotkeyId] = true;
            return hotkeyId;
        }

        return -1; // Registration failed
    }

    public void UnregisterHotkey(IntPtr hwnd, int hotkeyId)
    {
        if (_registeredHotkeys.ContainsKey(hotkeyId))
        {
            UnregisterHotKey(hwnd, hotkeyId);
            _registeredHotkeys.Remove(hotkeyId);
        }
    }

    public void UnregisterAll(IntPtr hwnd)
    {
        foreach (var hotkeyId in _registeredHotkeys.Keys.ToList())
        {
            UnregisterHotKey(hwnd, hotkeyId);
        }
        _registeredHotkeys.Clear();
    }

    public bool IsRegistered(int hotkeyId)
    {
        return _registeredHotkeys.ContainsKey(hotkeyId);
    }

    public void OnHotkeyPressed(int hotkeyId)
    {
        if (_registeredHotkeys.ContainsKey(hotkeyId))
        {
            HotkeyTriggered?.Invoke(this, hotkeyId);
        }
    }
}