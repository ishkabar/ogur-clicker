using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Services;
using Ogur.Clicker.Host.Commands;
using MouseButton = Ogur.Clicker.Core.Models.MouseButton;

namespace Ogur.Clicker.Host.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IMouseService _mouseService;
    private readonly IPositionCaptureService _positionCaptureService;
    private readonly IGlobalMouseHotkeyService _mouseHotkeyService;
    private readonly IGlobalKeyboardHotkeyService _keyboardHotkeyService;

    private int _clickX;
    private int _clickY;
    private MouseButton _selectedButton = MouseButton.Left;
    private MouseInputMethod _selectedMethod = MouseInputMethod.SendInput;
    private string _statusMessage = "Ready";
    private string _hotkeyDisplay = "Not set";
    private bool _returnToOriginalPosition = false;
    private bool _isAlwaysOnTop = false;


    private int _hotkeyVirtualKey;
    private bool _hotkeyCtrl;
    private bool _hotkeyAlt;
    private bool _hotkeyShift;
    private bool _isHotkeyMouseButton;

    public MainViewModel(
        IMouseService mouseService,
        IPositionCaptureService positionCaptureService,
        IGlobalMouseHotkeyService mouseHotkeyService,
        IGlobalKeyboardHotkeyService keyboardHotkeyService)
    {
        _mouseService = mouseService;
        _positionCaptureService = positionCaptureService;
        _mouseHotkeyService = mouseHotkeyService;
        _keyboardHotkeyService = keyboardHotkeyService;

        // Subscribe to events
        _positionCaptureService.PositionCaptured += OnPositionCaptured;
        _mouseHotkeyService.HotkeyCaptured += OnMouseHotkeyCaptured;
        _mouseHotkeyService.HotkeyTriggered += OnHotkeyTriggered;
        _keyboardHotkeyService.HotkeyCaptured += OnKeyboardHotkeyCaptured;
        _keyboardHotkeyService.HotkeyTriggered += OnHotkeyTriggered;

        // Commands
        CapturePositionCommand = new RelayCommand(CapturePosition, () => !_positionCaptureService.IsCapturing);
        CaptureHotkeyCommand = new RelayCommand(CaptureHotkey, () => !IsCapturingHotkey);
        ExecuteClickCommand = new RelayCommand(ExecuteClick, CanExecuteClick);
        TestClickCommand = new RelayCommand(TestClick);
    }

    #region Properties

    public int ClickX
    {
        get => _clickX;
        set => SetProperty(ref _clickX, value);
    }

    public int ClickY
    {
        get => _clickY;
        set => SetProperty(ref _clickY, value);
    }

    public MouseButton SelectedButton
    {
        get => _selectedButton;
        set => SetProperty(ref _selectedButton, value);
    }
    
    public bool ReturnToOriginalPosition
    {
        get => _returnToOriginalPosition;
        set => SetProperty(ref _returnToOriginalPosition, value);
    }

    public MouseInputMethod SelectedMethod
    {
        get => _selectedMethod;
        set => SetProperty(ref _selectedMethod, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string HotkeyDisplay
    {
        get => _hotkeyDisplay;
        set => SetProperty(ref _hotkeyDisplay, value);
    }

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set => SetProperty(ref _isAlwaysOnTop, value);
    }
    
    public bool IsCapturingHotkey => _mouseHotkeyService.IsCapturing || _keyboardHotkeyService.IsCapturing;

    public Array MouseButtons => Enum.GetValues(typeof(MouseButton));
    public Array InputMethods => Enum.GetValues(typeof(MouseInputMethod));

    #endregion

    #region Commands

    public ICommand CapturePositionCommand { get; }
    public ICommand CaptureHotkeyCommand { get; }
    public ICommand ExecuteClickCommand { get; }
    public ICommand TestClickCommand { get; }

    #endregion
#region Event Handlers

    private void OnPositionCaptured(object? sender, System.Drawing.Point point)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ClickX = point.X;
            ClickY = point.Y;
            StatusMessage = $"Position captured: ({ClickX}, {ClickY})";
            
            ((RelayCommand)CapturePositionCommand).RaiseCanExecuteChanged();
            
        });
    }

    private void OnMouseHotkeyCaptured(object? sender, (int virtualKey, bool ctrl, bool alt, bool shift) hotkey)
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        // Block LMB and RMB
        if (hotkey.virtualKey == 0x01 || hotkey.virtualKey == 0x02)
        {
            StatusMessage = "LMB/RMB cannot be used as hotkey. Use MB3/MB4/MB5 or keyboard keys.";
            
            // Stop capture mode
            _mouseHotkeyService.StopCapture();
            _keyboardHotkeyService.StopCapture();
            ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
            return;
        }
        
        // Unregister keyboard hotkey if was registered
        if (_keyboardHotkeyService.IsRegistered)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                var handle = new WindowInteropHelper(mainWindow).Handle;
                _keyboardHotkeyService.UnregisterHotkey(handle);
            }
        }
        
        _hotkeyVirtualKey = hotkey.virtualKey;
        _hotkeyCtrl = hotkey.ctrl;
        _hotkeyAlt = hotkey.alt;
        _hotkeyShift = hotkey.shift;
        _isHotkeyMouseButton = true;

        UpdateHotkeyDisplay(hotkey.virtualKey, hotkey.ctrl, hotkey.alt, hotkey.shift, true);
        
        // Register only mouse hotkey
        _mouseHotkeyService.RegisterHotkey(hotkey.virtualKey, hotkey.ctrl, hotkey.alt, hotkey.shift);
        
        // Stop keyboard capture - not needed anymore
        _keyboardHotkeyService.StopCapture();
        
        StatusMessage = $"Hotkey registered: {HotkeyDisplay}";
        ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
        
    });
}

    private void OnKeyboardHotkeyCaptured(object? sender, (int virtualKey, bool ctrl, bool alt, bool shift) hotkey)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Unregister mouse hotkey if was registered
            if (_mouseHotkeyService.IsRegistered)
            {
                _mouseHotkeyService.UnregisterHotkey();
            }
        
            _hotkeyVirtualKey = hotkey.virtualKey;
            _hotkeyCtrl = hotkey.ctrl;
            _hotkeyAlt = hotkey.alt;
            _hotkeyShift = hotkey.shift;
            _isHotkeyMouseButton = false;

            UpdateHotkeyDisplay(hotkey.virtualKey, hotkey.ctrl, hotkey.alt, hotkey.shift, false);

            var mainWindow = Application.Current.MainWindow;
            
            if (mainWindow != null)
            {
                var handle = new WindowInteropHelper(mainWindow).Handle;
            
                bool success = _keyboardHotkeyService.RegisterHotkey(handle, hotkey.virtualKey, hotkey.ctrl, hotkey.alt, hotkey.shift);
                
            
                // Stop mouse capture - not needed anymore
                _mouseHotkeyService.StopCapture();
            
                StatusMessage = success 
                    ? $"Hotkey registered: {HotkeyDisplay}" 
                    : "Failed to register hotkey (maybe already in use)";
            }
        
            ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
        });
    }
    private void OnHotkeyTriggered(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ExecuteClick();
        });
    }

    #endregion

    #region Command Implementations

    private void CapturePosition()
    {
        StatusMessage = "Click anywhere to capture position...";
        _positionCaptureService.StartCapture();
        ((RelayCommand)CapturePositionCommand).RaiseCanExecuteChanged();
    }

    private void CaptureHotkey()
    {
        StatusMessage = "Press any key or mouse button combination...";
        _mouseHotkeyService.StartCapture();
        _keyboardHotkeyService.StartCapture();
        ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
    }

    private bool CanExecuteClick()
    {
        return ClickX > 0 || ClickY > 0;
    }

    private void ExecuteClick()
    {
        // Save original position if needed
        System.Drawing.Point? originalPosition = null;
        if (ReturnToOriginalPosition)
        {
            originalPosition = _mouseService.GetCurrentPosition();
        }

        var action = new ClickAction(ClickX, ClickY, SelectedButton);
        bool success = _mouseService.TryClick(action, SelectedMethod);

        // Return to original position
        if (ReturnToOriginalPosition && originalPosition.HasValue)
        {
            Thread.Sleep(15);
            
            _mouseService.SetCursorPosition(originalPosition.Value.X, originalPosition.Value.Y);
        }
        
        StatusMessage = success
            ? $"Click executed at ({ClickX}, {ClickY})"
            : $"Click failed at ({ClickX}, {ClickY})";
    }

    private void TestClick()
    {
        var currentPos = _mouseService.GetCurrentPosition();
        var action = new ClickAction(currentPos.X, currentPos.Y, SelectedButton);
        bool success = _mouseService.TryClick(action, SelectedMethod);

        

        StatusMessage = success
            ? $"Test click successful at ({currentPos.X}, {currentPos.Y})"
            : "Test click failed";
    }

    #endregion

    #region Helpers

    private void UpdateHotkeyDisplay(int virtualKey, bool ctrl, bool alt, bool shift, bool isMouseButton)
    {
        var display = "";
        if (ctrl) display += "Ctrl+";
        if (alt) display += "Alt+";
        if (shift) display += "Shift+";

        if (isMouseButton)
        {
            display += virtualKey switch
            {
                0x01 => "LMB",
                0x02 => "RMB",
                0x04 => "MMB",
                0x05 => "MB4",
                0x06 => "MB5",
                _ => $"MB{virtualKey}"
            };
        }
        else
        {
            var key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(virtualKey);
            display += key.ToString();
        }

        HotkeyDisplay = display;
    }

    #endregion
}