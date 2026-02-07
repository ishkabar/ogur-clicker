namespace Ogur.Clicker.Core.Services;

public interface IGlobalKeyboardHotkeyService
{
    event EventHandler? HotkeyTriggered;
    event EventHandler<(int virtualKey, bool ctrl, bool alt, bool shift)>? HotkeyCaptured;
    
    void StartCapture();
    void StopCapture();
    bool RegisterHotkey(IntPtr windowHandle, int virtualKey, bool ctrl, bool alt, bool shift);
    void UnregisterHotkey(IntPtr windowHandle);
    
    bool IsCapturing { get; }
    bool IsRegistered { get; }
}