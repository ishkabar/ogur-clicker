using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class GlobalKeyboardHotkeyService : IGlobalKeyboardHotkeyService
{
    private readonly IKeyboardHookService _keyboardHookService;
    private bool _isCapturing;
    private bool _isRegistered;
    private bool _ignoreNextTrigger;
    private IntPtr _registeredWindowHandle;
    private HwndSource? _hwndSource;

    private const int HOTKEY_ID = 9001;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;

    public event EventHandler? HotkeyTriggered;
    public event EventHandler<(int virtualKey, bool ctrl, bool alt, bool shift)>? HotkeyCaptured;
    
    public bool IsCapturing => _isCapturing;
    public bool IsRegistered => _isRegistered;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public GlobalKeyboardHotkeyService(IKeyboardHookService keyboardHookService)
    {
        _keyboardHookService = keyboardHookService;
        _keyboardHookService.KeyPressed += OnKeyPressed;
    }

    public void StartCapture()
    {
        if (_isCapturing)
            return;

        _isCapturing = true;
        _keyboardHookService.StartListening();
    }

    public void StopCapture()
    {
        if (!_isCapturing)
            return;

        _isCapturing = false;
        _keyboardHookService.StopListening();
    }

    
    public bool RegisterHotkey(IntPtr windowHandle, int virtualKey, bool ctrl, bool alt, bool shift)
    {
        UnregisterHotkey(windowHandle);

        _registeredWindowHandle = windowHandle;

        if (_hwndSource == null)
        {
            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(HwndHook);
        }

        uint modifiers = 0;
        if (ctrl) modifiers |= MOD_CONTROL;
        if (alt) modifiers |= MOD_ALT;
        if (shift) modifiers |= MOD_SHIFT;
        
        bool result = RegisterHotKey(windowHandle, HOTKEY_ID, modifiers, (uint)virtualKey);
        
        if (result)
        {
            _isRegistered = true;
            _ignoreNextTrigger = true;
        }

        return result;
    }

    public void UnregisterHotkey(IntPtr windowHandle)
    {
        
        if (_isRegistered && _registeredWindowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_registeredWindowHandle, HOTKEY_ID);
            _isRegistered = false;
        }
    }

    private void OnKeyPressed(object? sender, (int virtualKey, bool isCtrl, bool isAlt, bool isShift) keyInfo)
    {
        if (!_isCapturing)
            return;
        
        _isCapturing = false;
        _keyboardHookService.StopListening();
        
        HotkeyCaptured?.Invoke(this, (keyInfo.virtualKey, keyInfo.isCtrl, keyInfo.isAlt, keyInfo.isShift));
    }

    
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            if (_ignoreNextTrigger)
            {
                _ignoreNextTrigger = false;
                handled = true;
                return IntPtr.Zero;
            }
            
            HotkeyTriggered?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }
}