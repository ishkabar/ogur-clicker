// Ogur.Clicker.Host/ViewModels/HotbarSlotViewModel.cs
using System.Windows.Input;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Host.Commands;

namespace Ogur.Clicker.Host.ViewModels;

public class HotbarSlotViewModel : ViewModelBase
{
    private HotbarSlot _slot;
    private bool _isExecuting;
    private readonly Action<HotbarSlotViewModel> _editAction;
    private readonly Action<HotbarSlotViewModel> _removeAction;
    private readonly Action<HotbarSlotViewModel, bool> _moveAction;


    public HotbarSlotViewModel(
        HotbarSlot slot,
        Action<HotbarSlotViewModel> editAction,
        Action<HotbarSlotViewModel> removeAction,
        Action<HotbarSlotViewModel, bool> moveAction)
    {
        _slot = slot;
        _editAction = editAction;
        _removeAction = removeAction;
        _moveAction = moveAction;

        EditCommand = new RelayCommand(() => _editAction(this));
        RemoveCommand = new RelayCommand(() => _removeAction(this));
        MoveUpCommand = new RelayCommand(() => _moveAction(this, true));
        MoveDownCommand = new RelayCommand(() => _moveAction(this, false));
    }

    public new void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
    }

    public HotbarSlot Slot
    {
        get => _slot;
        set => SetProperty(ref _slot, value);
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        set => SetProperty(ref _isExecuting, value);
    }


    public int SlotNumber
    {
        get => _slot.SlotNumber;
        set
        {
            if (_slot.SlotNumber != value)
            {
                _slot.SlotNumber = value;
                OnPropertyChanged();
            }
        }
    }

    public string KeyName
    {
        get => _slot.KeyName;
        set
        {
            if (_slot.KeyName != value)
            {
                _slot.KeyName = value;
                OnPropertyChanged();
            }
        }
    }

    public string TriggerDisplay
    {
        get => _slot.TriggerDisplay;
        set
        {
            if (_slot.TriggerDisplay != value)
            {
                _slot.TriggerDisplay = value;
                OnPropertyChanged();
            }
        }
    }

    public int PressCount
    {
        get => _slot.PressCount;
        set
        {
            if (_slot.PressCount != value)
            {
                _slot.PressCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int DelayMs
    {
        get => _slot.DelayMs;
        set
        {
            if (_slot.DelayMs != value)
            {
                _slot.DelayMs = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEnabled
    {
        get => _slot.IsEnabled;
        set
        {
            if (_slot.IsEnabled != value)
            {
                _slot.IsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public SlotExecutionStatus ExecutionStatus
    {
        get => Slot.ExecutionStatus;
        set
        {
            if (Slot.ExecutionStatus != value)
            {
                Slot.ExecutionStatus = value;
                OnPropertyChanged(nameof(ExecutionStatus));
                OnPropertyChanged(nameof(StatusTooltip));
            }
        }
    }

    public string StatusTooltip => ExecutionStatus switch
    {
        SlotExecutionStatus.Ready => "Ready",
        SlotExecutionStatus.Executing => "Executing...",
        SlotExecutionStatus.NoFocus => "Game has no focus",
        _ => "Unknown"
    };

    public string StatusText => IsExecuting ? "Executing..." : "Ready";

    public ICommand EditCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
}