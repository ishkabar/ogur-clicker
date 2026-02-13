using System.Windows.Input;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class GlobalMouseHotkeyService : IGlobalMouseHotkeyService
{
    private readonly IMouseHookService _mouseHookService;
    private bool _isCapturing;
    private bool _isRegistered;
    private bool _ignoreNextTrigger; 
    
    private int _registeredVirtualKey;
    private bool _registeredCtrl;
    private bool _registeredAlt;
    private bool _registeredShift;

    public event EventHandler? HotkeyTriggered;
    public event EventHandler<(int virtualKey, bool ctrl, bool alt, bool shift)>? HotkeyCaptured;
    
    public bool IsCapturing => _isCapturing;
    public bool IsRegistered => _isRegistered;

    public GlobalMouseHotkeyService(IMouseHookService mouseHookService)
    {
        _mouseHookService = mouseHookService;
        _mouseHookService.MouseButtonClicked += OnMouseButtonClicked;
    }

    public void StartCapture()
    {
        if (_isCapturing)
            return;

        _isCapturing = true;
        
        if (!_isRegistered)
            _mouseHookService.StartListening();
    }

    public void StopCapture()
    {
        if (!_isCapturing)
            return;

        _isCapturing = false;
        
        if (!_isRegistered)
            _mouseHookService.StopListening();
    }

    public void RegisterHotkey(int virtualKey, bool ctrl, bool alt, bool shift)
    {
        // Block LMB and RMB - they fuck things up
        if (virtualKey == 0x01 || virtualKey == 0x02)
        {
            return;
        }
    
        _registeredVirtualKey = virtualKey;
        _registeredCtrl = ctrl;
        _registeredAlt = alt;
        _registeredShift = shift;
        _isRegistered = true;
        _ignoreNextTrigger = true; 
    
        _mouseHookService.StartListening();
    }

    public void UnregisterHotkey()
    {
        _isRegistered = false;
        
        if (!_isCapturing)
            _mouseHookService.StopListening();
    }

private void OnMouseButtonClicked(object? sender, int virtualKey)
{
    // Early exit - optymalizacja
    if (!_isCapturing && !_isRegistered)
    {
        return;
    }

    var ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
    var altPressed = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
    var shiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
    
    // Capture mode
    if (_isCapturing)
    {
        _isCapturing = false;
        
        if (!_isRegistered)
            _mouseHookService.StopListening();
        
        HotkeyCaptured?.Invoke(this, (virtualKey, ctrlPressed, altPressed, shiftPressed));
        return;
    }

    // Trigger mode
    
    if (_isRegistered && 
        virtualKey == _registeredVirtualKey &&
        ctrlPressed == _registeredCtrl && 
        altPressed == _registeredAlt && 
        shiftPressed == _registeredShift)
    {
        // Skip first trigger after registration
        if (_ignoreNextTrigger)
        {
            _ignoreNextTrigger = false;
            return;
        }
        HotkeyTriggered?.Invoke(this, EventArgs.Empty);
    }
    else
    {
    }
}
}