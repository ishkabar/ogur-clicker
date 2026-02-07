namespace Ogur.Clicker.Core.Services;

public interface IGlobalMouseHotkeyService
{
    event EventHandler? HotkeyTriggered;
    event EventHandler<(int virtualKey, bool ctrl, bool alt, bool shift)>? HotkeyCaptured;
    
    void StartCapture();
    void StopCapture();
    void RegisterHotkey(int virtualKey, bool ctrl, bool alt, bool shift);
    void UnregisterHotkey();
    
    bool IsCapturing { get; }
    bool IsRegistered { get; }
}