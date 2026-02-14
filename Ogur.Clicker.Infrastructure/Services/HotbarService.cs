// Ogur.Clicker.Infrastructure/Services/HotbarService.cs
using System.IO;
using System.Text.Json;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class HotbarService : IHotbarService
{
    private readonly IKeyboardService _keyboardService;
    private readonly IMultiHotkeyService _multiHotkeyService;
    private readonly IGameFocusService _gameFocusService;
    private readonly Dictionary<int, HotbarSlot> _slotsByNumber = new();
    private readonly Dictionary<int, int> _hotkeyIdToSlotNumber = new();

    public event EventHandler<int>? SlotTriggered;
    public HotbarProfile CurrentProfile { get; private set; } = new();

    public HotbarService(
        IKeyboardService keyboardService,
        IMultiHotkeyService multiHotkeyService,
        IGameFocusService gameFocusService)
    {
        _keyboardService = keyboardService;
        _multiHotkeyService = multiHotkeyService;
        _gameFocusService = gameFocusService;

        _multiHotkeyService.HotkeyTriggered += OnHotkeyTriggered;

        InitializeDefaultSlots();
    }

    private void OnHotkeyTriggered(object? sender, int hotkeyId)
    {
        if (!CheckGameFocus())
        {
            return;
        }

        if (_hotkeyIdToSlotNumber.TryGetValue(hotkeyId, out var slotNumber))
        {
            _ = ExecuteSlotAsync(slotNumber);
        }
    }

    private void InitializeDefaultSlots()
    {
        for (int i = 1; i <= 8; i++)
        {
            var slot = new HotbarSlot
            {
                SlotNumber = i,
                VirtualKey = 0x30 + i,
                KeyName = i.ToString(),
                PressCount = 1,
                DelayMs = 50,
                IsEnabled = false
            };
            CurrentProfile.Slots.Add(slot);
            _slotsByNumber[i] = slot;
        }

        for (int i = 1; i <= 4; i++)
        {
            var slot = new HotbarSlot
            {
                SlotNumber = 8 + i,
                VirtualKey = 0x6F + i,
                KeyName = $"F{i}",
                PressCount = 1,
                DelayMs = 50,
                IsEnabled = false
            };
            CurrentProfile.Slots.Add(slot);
            _slotsByNumber[8 + i] = slot;
        }
    }

    public void SaveProfile(string path)
    {
        CurrentProfile.TargetProcessName = _gameFocusService.GetTargetProcessName();
        CurrentProfile.TargetProcessId = _gameFocusService.GetTargetProcessId();

        var json = JsonSerializer.Serialize(CurrentProfile, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }

    public void LoadProfile(HotbarProfile profile)
    {
        CurrentProfile = profile;
        _slotsByNumber.Clear();
        foreach (var slot in profile.Slots)
        {
            _slotsByNumber[slot.SlotNumber] = slot;
        }

        // Restore target process
        if (profile.TargetProcessId.HasValue)
        {
            _gameFocusService.SetTargetProcess(profile.TargetProcessId.Value);
        }
        else if (!string.IsNullOrEmpty(profile.TargetProcessName))
        {
            _gameFocusService.SetTargetProcessByName(profile.TargetProcessName);
        }
    }

    public HotbarProfile LoadProfileFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HotbarProfile>(json) ?? new HotbarProfile();
    }

    public void AddSlot(HotbarSlot slot)
    {
        slot.SlotNumber = CurrentProfile.Slots.Count + 1;
        CurrentProfile.Slots.Add(slot);
        _slotsByNumber[slot.SlotNumber] = slot;
    }

    public void UpdateSlot(int slotNumber, HotbarSlot slot)
    {
        var existing = CurrentProfile.Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (existing != null)
        {
            var index = CurrentProfile.Slots.IndexOf(existing);
            slot.SlotNumber = slotNumber;
            CurrentProfile.Slots[index] = slot;
            _slotsByNumber[slotNumber] = slot;
        }
    }

    public HotbarSlot? GetSlot(int slotNumber)
    {
        return _slotsByNumber.TryGetValue(slotNumber, out var slot) ? slot : null;
    }

    public void RemoveSlot(int slotNumber)
    {
        CurrentProfile.Slots.RemoveAll(s => s.SlotNumber == slotNumber);
        _slotsByNumber.Remove(slotNumber);

        for (int i = 0; i < CurrentProfile.Slots.Count; i++)
        {
            CurrentProfile.Slots[i].SlotNumber = i + 1;
        }

        _slotsByNumber.Clear();
        foreach (var slot in CurrentProfile.Slots)
        {
            _slotsByNumber[slot.SlotNumber] = slot;
        }
    }

    public void MoveSlot(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= CurrentProfile.Slots.Count ||
            toIndex < 0 || toIndex >= CurrentProfile.Slots.Count)
            return;

        var slot = CurrentProfile.Slots[fromIndex];
        CurrentProfile.Slots.RemoveAt(fromIndex);
        CurrentProfile.Slots.Insert(toIndex, slot);

        for (int i = 0; i < CurrentProfile.Slots.Count; i++)
        {
            CurrentProfile.Slots[i].SlotNumber = i + 1;
        }

        _slotsByNumber.Clear();
        foreach (var s in CurrentProfile.Slots)
        {
            _slotsByNumber[s.SlotNumber] = s;
        }
    }

    public void RegisterAllHotkeys(IntPtr windowHandle)
    {
        _hotkeyIdToSlotNumber.Clear();

        foreach (var slot in CurrentProfile.Slots.Where(s => s.IsEnabled && s.TriggerVirtualKey != 0))
        {
            int hotkeyId = _multiHotkeyService.RegisterHotkey(
                windowHandle,
                slot.TriggerVirtualKey,
                slot.TriggerCtrl,
                slot.TriggerAlt,
                slot.TriggerShift
            );

            if (hotkeyId != -1)
            {
                _hotkeyIdToSlotNumber[hotkeyId] = slot.SlotNumber;
            }
        }
    }

    public void UnregisterAllHotkeys(IntPtr windowHandle)
    {
        _multiHotkeyService.UnregisterAll(windowHandle);
        _hotkeyIdToSlotNumber.Clear();
    }

    public void SetTargetProcess(int processId)
    {
        _gameFocusService.SetTargetProcess(processId);
    }

    public void SetTargetProcessByName(string processName)
    {
        _gameFocusService.SetTargetProcessByName(processName);
    }

    public void SetTargetProcessByWindowTitle(string windowTitle)
    {
        _gameFocusService.SetTargetProcessByWindowTitle(windowTitle);
    }

    public async Task ExecuteSlotAsync(int slotNumber)
    {
        if (!_slotsByNumber.TryGetValue(slotNumber, out var slot) || !slot.IsEnabled)
            return;

        // Double-check focus (na wypadek race condition)
        if (!CheckGameFocus())
        {
            slot.ExecutionStatus = SlotExecutionStatus.NoFocus;
            SlotTriggered?.Invoke(this, slotNumber);
            return;
        }

        slot.ExecutionStatus = SlotExecutionStatus.Executing;
        SlotTriggered?.Invoke(this, slotNumber);

        try
        {
            await _keyboardService.PressKeyAsync(
                slot.VirtualKey,
                slot.PressCount,
                slot.DelayMs,
                CurrentProfile.InputMethod
            );
        }
        finally
        {
            slot.ExecutionStatus = CheckGameFocus() ? SlotExecutionStatus.Ready : SlotExecutionStatus.NoFocus;
            SlotTriggered?.Invoke(this, slotNumber);
        }
    }

    private bool CheckGameFocus()
    {
        return _gameFocusService.IsGameFocused();
    }
}