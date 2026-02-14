// Ogur.Clicker.Host/ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using Ogur.Clicker.Host.Views;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Services;
using Ogur.Clicker.Host.Commands;
using Ogur.Clicker.Infrastructure.Services;

namespace Ogur.Clicker.Host.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IHotbarService _hotbarService;
    private readonly IMultiHotkeyService _multiHotkeyService;
    private readonly IKeyboardHookService _keyboardHookService;
    private readonly IMouseHookService _mouseHookService;

    private string _statusMessage = "Ready";
    private bool _isAlwaysOnTop = false;
    private bool _isCapturingHotkey = false;
    private HotbarSlotViewModel? _selectedSlot;
    private KeyboardInputMethod _selectedInputMethod = KeyboardInputMethod.SendInput;
    private readonly IGameFocusService _gameFocusService;
    private int? _targetProcessId;
    private string _targetProcessName = "Not set";
    private readonly DispatcherTimer _focusCheckTimer;
    private int _focusCheckIntervalMs = 100;



    private IntPtr _windowHandle;
    private bool _ignoreNextCapture = false;


    public MainViewModel(
        IHotbarService hotbarService,
        IMultiHotkeyService multiHotkeyService,
        IKeyboardHookService keyboardHookService,
        IMouseHookService mouseHookService,
        IGameFocusService gameFocusService)
    {
        _hotbarService = hotbarService;
        _multiHotkeyService = multiHotkeyService;
        _keyboardHookService = keyboardHookService;
        _mouseHookService = mouseHookService;
        _gameFocusService = gameFocusService;


        // Subscribe to events
        _hotbarService.SlotTriggered += OnSlotTriggered;
        _keyboardHookService.KeyPressed += OnKeyPressed;
        _mouseHookService.MouseButtonClicked += OnMouseButtonClicked;



        // Initialize slots
        LoadSlots();

        // Commands
        AddSlotCommand = new RelayCommand(AddSlot);
        SaveProfileCommand = new RelayCommand(SaveProfile);
        LoadProfileCommand = new RelayCommand(LoadProfile);
        CaptureHotkeyCommand = new RelayCommand(CaptureHotkey, () => _selectedSlot != null && !_isCapturingHotkey);
        RegisterAllHotkeysCommand = new RelayCommand(RegisterAllHotkeys);
        SwitchToPortableViewCommand = new RelayCommand(SwitchToPortableView);
        SetTargetProcessCommand = new RelayCommand(SetTargetProcess);

        _focusCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_focusCheckIntervalMs)
        };
        _focusCheckTimer.Tick += FocusCheckTimer_Tick;
        _focusCheckTimer.Start();
    }

    #region Properties

    public ObservableCollection<HotbarSlotViewModel> Slots { get; } = new();

    public HotbarSlotViewModel? SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            SetProperty(ref _selectedSlot, value);
            ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
        }
    }

    private void FocusCheckTimer_Tick(object? sender, EventArgs e)
    {
        bool hasFocus = _gameFocusService.IsGameFocused();

        foreach (var slot in Slots)
        {
            // Nie zmieniaj statusu jak slot jest w trakcie wykonywania
            if (slot.ExecutionStatus == SlotExecutionStatus.Executing)
                continue;

            var newStatus = hasFocus ? SlotExecutionStatus.Ready : SlotExecutionStatus.NoFocus;

            if (slot.ExecutionStatus != newStatus)
            {
                slot.ExecutionStatus = newStatus;
            }
        }
    }

    public int FocusCheckIntervalMs
    {
        get => _focusCheckIntervalMs;
        set
        {
            if (SetProperty(ref _focusCheckIntervalMs, value))
            {
                _focusCheckTimer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set => SetProperty(ref _isAlwaysOnTop, value);
    }

    public bool IsCapturingHotkey
    {
        get => _isCapturingHotkey;
        set
        {
            SetProperty(ref _isCapturingHotkey, value);
            ((RelayCommand)CaptureHotkeyCommand).RaiseCanExecuteChanged();
        }
    }

    public KeyboardInputMethod SelectedInputMethod
    {
        get => _selectedInputMethod;
        set
        {
            if (SetProperty(ref _selectedInputMethod, value))
            {
                _hotbarService.CurrentProfile.InputMethod = value;
            }
        }
    }

    public Array InputMethods => Enum.GetValues(typeof(KeyboardInputMethod));

    #endregion

    #region Commands

    public ICommand AddSlotCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand LoadProfileCommand { get; }
    public ICommand CaptureHotkeyCommand { get; }
    public ICommand RegisterAllHotkeysCommand { get; }
    public ICommand SwitchToPortableViewCommand { get; }
    public ICommand SetTargetProcessCommand { get; }



    #endregion

    #region Public Methods


    public int? TargetProcessId
    {
        get => _targetProcessId;
        set
        {
            if (SetProperty(ref _targetProcessId, value) && value.HasValue)
            {
                _gameFocusService.SetTargetProcess(value.Value);
                TargetProcessName = _gameFocusService.GetTargetProcessName() ?? "Unknown";
            }
        }
    }

    public string TargetProcessName
    {
        get => _targetProcessName;
        set => SetProperty(ref _targetProcessName, value);
    }


    public void SetWindowHandle(IntPtr handle)
    {
        _windowHandle = handle;
        RegisterAllHotkeys();
    }

    public void OnHotkeyPressed(int hotkeyId)
    {
        // This is called from MainWindow WndProc
        // MultiHotkeyService handles the mapping internally
        ((MultiHotkeyService)_multiHotkeyService).OnHotkeyPressed(hotkeyId);
    }

    #endregion

    #region Event Handlers

    private void OnSlotTriggered(object? sender, int slotNumber)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
            if (slot != null)
            {
                // Odśwież status
                slot.OnPropertyChanged(nameof(slot.ExecutionStatus));
                slot.OnPropertyChanged(nameof(slot.StatusTooltip));

                StatusMessage = $"Triggered slot {slotNumber}: {slot.KeyName} x{slot.PressCount} [{slot.ExecutionStatus}]";
            }
        });
    }


    private void OnKeyPressed(object? sender, (int virtualKey, bool isCtrl, bool isAlt, bool isShift) e)
    {
        if (!IsCapturingHotkey || SelectedSlot == null)
            return;

        // Ignoruj pierwszy event (kliknięcie capture button)
        if (_ignoreNextCapture)
        {
            _ignoreNextCapture = false;
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Block LMB/RMB
            if (e.virtualKey == 0x01 || e.virtualKey == 0x02)
            {
                StatusMessage = "LMB/RMB cannot be used as hotkey";
                StopCapture();
                return;
            }

            SelectedSlot.Slot.TriggerVirtualKey = e.virtualKey;
            SelectedSlot.Slot.TriggerCtrl = e.isCtrl;
            SelectedSlot.Slot.TriggerAlt = e.isAlt;
            SelectedSlot.Slot.TriggerShift = e.isShift;

            UpdateTriggerDisplay(SelectedSlot, e.virtualKey, e.isCtrl, e.isAlt, e.isShift);

            _hotbarService.UpdateSlot(SelectedSlot.SlotNumber, SelectedSlot.Slot);

            StopCapture();
            StatusMessage = $"Hotkey set for slot {SelectedSlot.SlotNumber}: {SelectedSlot.TriggerDisplay}";

            // Re-register all hotkeys
            RegisterAllHotkeys();
        });
    }

    private void OnMouseButtonClicked(object? sender, int virtualKey)
    {
        if (!IsCapturingHotkey || SelectedSlot == null)
            return;

        // Ignoruj pierwszy event
        if (_ignoreNextCapture)
        {
            _ignoreNextCapture = false;
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Block LMB/RMB
            if (virtualKey == 0x01 || virtualKey == 0x02)
            {
                StatusMessage = "LMB/RMB cannot be used as hotkey";
                StopCapture();
                return;
            }

            // Get current modifier keys
            bool ctrl = (System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Control) != 0;
            bool alt = (System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            bool shift = (System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Shift) != 0;

            SelectedSlot.Slot.TriggerVirtualKey = virtualKey;
            SelectedSlot.Slot.TriggerCtrl = ctrl;
            SelectedSlot.Slot.TriggerAlt = alt;
            SelectedSlot.Slot.TriggerShift = shift;

            UpdateTriggerDisplay(SelectedSlot, virtualKey, ctrl, alt, shift);

            _hotbarService.UpdateSlot(SelectedSlot.SlotNumber, SelectedSlot.Slot);

            StopCapture();
            StatusMessage = $"Hotkey set for slot {SelectedSlot.SlotNumber}: {SelectedSlot.TriggerDisplay}";

            // Re-register all hotkeys
            RegisterAllHotkeys();
        });
    }

    #endregion

    #region Command Implementations

    private void LoadSlots()
    {
        Slots.Clear();
        foreach (var slot in _hotbarService.CurrentProfile.Slots)
        {
            Slots.Add(new HotbarSlotViewModel(slot, EditSlot, RemoveSlot, MoveSlot));
        }
    }

    private void AddSlot()
    {
        var dialog = new AddSlotDialog();
        if (dialog.ShowDialog() == true)
        {
            var newSlot = new HotbarSlot
            {
                VirtualKey = dialog.VirtualKey,
                KeyName = dialog.KeyName,
                PressCount = 1,
                DelayMs = 50,
                IsEnabled = false
            };

            _hotbarService.AddSlot(newSlot);
            Slots.Add(new HotbarSlotViewModel(newSlot, EditSlot, RemoveSlot, MoveSlot));
            StatusMessage = $"Added slot: {newSlot.KeyName}";
        }
    }

    private void EditSlot(HotbarSlotViewModel slotVm)
    {
        SelectedSlot = slotVm;
        StatusMessage = $"Selected slot {slotVm.SlotNumber}: {slotVm.KeyName}";
    }

    private void RemoveSlot(HotbarSlotViewModel slotVm)
    {
        var result = MessageBox.Show(
            $"Remove slot {slotVm.SlotNumber} ({slotVm.KeyName})?",
            "Confirm Remove",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _hotbarService.RemoveSlot(slotVm.SlotNumber);
            Slots.Remove(slotVm);

            // Refresh slot numbers
            LoadSlots();

            StatusMessage = $"Removed slot: {slotVm.KeyName}";

            // Re-register hotkeys
            RegisterAllHotkeys();
        }
    }

    private void MoveSlot(HotbarSlotViewModel slotVm, bool moveUp)
    {
        int currentIndex = Slots.IndexOf(slotVm);
        int newIndex = moveUp ? currentIndex - 1 : currentIndex + 1;

        if (newIndex < 0 || newIndex >= Slots.Count)
            return;

        _hotbarService.MoveSlot(currentIndex, newIndex);
        LoadSlots();
        StatusMessage = $"Moved slot {slotVm.KeyName}";
    }

    private void CaptureHotkey()
    {
        if (SelectedSlot == null)
            return;

        IsCapturingHotkey = true;
        _ignoreNextCapture = true; // Ignoruj pierwszy event
        StatusMessage = "Press any key or mouse button combination...";

        _keyboardHookService.StartListening();
        _mouseHookService.StartListening();
    }

    private void StopCapture()
    {
        IsCapturingHotkey = false;
        _ignoreNextCapture = false; // Reset flag
        _keyboardHookService.StopListening();
        _mouseHookService.StopListening();
    }

    private void SaveProfile()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
            FileName = "hotbar_profile.json"
        };

        if (dialog.ShowDialog() == true)
        {
            // Update profile with current settings before saving
            _hotbarService.CurrentProfile.AlwaysOnTop = IsAlwaysOnTop;
            _hotbarService.CurrentProfile.TargetProcessName = TargetProcessName;
            _hotbarService.CurrentProfile.TargetProcessId = TargetProcessId;
            _hotbarService.CurrentProfile.FocusCheckIntervalMs = FocusCheckIntervalMs;

            _hotbarService.SaveProfile(dialog.FileName);
            StatusMessage = $"Profile saved: {dialog.FileName}";
        }
    }

    private void LoadProfile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var profile = _hotbarService.LoadProfileFromFile(dialog.FileName);
            _hotbarService.LoadProfile(profile);

            // Restore UI settings
            IsAlwaysOnTop = profile.AlwaysOnTop;
            FocusCheckIntervalMs = profile.FocusCheckIntervalMs;
            TargetProcessName = profile.TargetProcessName ?? "Not set";
            TargetProcessId = profile.TargetProcessId;

            LoadSlots();
            RegisterAllHotkeys();
            StatusMessage = $"Profile loaded: {dialog.FileName}";
        }
    }

    private void RegisterAllHotkeys()
    {
        if (_windowHandle == IntPtr.Zero)
            return;

        _hotbarService.UnregisterAllHotkeys(_windowHandle);
        _hotbarService.RegisterAllHotkeys(_windowHandle);
        StatusMessage = "Hotkeys registered";
    }

    public void Cleanup(IntPtr windowHandle)
    {
        // Auto-save current profile
        try
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OgurClicker"
            );
            Directory.CreateDirectory(appDataPath);

            var lastProfilePath = Path.Combine(appDataPath, "last_profile.json");

            _hotbarService.CurrentProfile.AlwaysOnTop = IsAlwaysOnTop;
            _hotbarService.CurrentProfile.TargetProcessName = TargetProcessName;
            _hotbarService.CurrentProfile.TargetProcessId = TargetProcessId;
            _hotbarService.CurrentProfile.FocusCheckIntervalMs = FocusCheckIntervalMs;

            _hotbarService.SaveProfile(lastProfilePath);
        }
        catch
        {
            // Ignore save errors
        }

        _focusCheckTimer?.Stop();
        _hotbarService.UnregisterAllHotkeys(windowHandle);
        _keyboardHookService.StopListening();
        _mouseHookService.StopListening();
    }

    private void SwitchToPortableView()
    {
        var portableView = new PortableView
        {
            DataContext = this
        };
        portableView.Show();

        // Ukryj główne okno
        Application.Current.MainWindow?.Hide();
    }

    private void SetTargetProcess()
    {
        var dialog = new ProcessSelectionDialog();
        if (dialog.ShowDialog() == true)
        {
            if (dialog.SelectedProcessId.HasValue)
            {
                TargetProcessId = dialog.SelectedProcessId.Value;
                StatusMessage = $"Target process set: {TargetProcessName} (PID: {TargetProcessId})";
            }
        }
    }


    #endregion

    #region Helpers

    private void UpdateTriggerDisplay(HotbarSlotViewModel slotVm, int virtualKey, bool ctrl, bool alt, bool shift)
    {
        var display = "";
        if (ctrl) display += "Ctrl+";
        if (alt) display += "Alt+";
        if (shift) display += "Shift+";

        // Check if it's a mouse button
        if (virtualKey >= 0x01 && virtualKey <= 0x06)
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

        slotVm.TriggerDisplay = display;
    }

    #endregion
}