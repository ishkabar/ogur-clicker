// Ogur.Clicker.Core/Models/HotbarSlot.cs
namespace Ogur.Clicker.Core.Models;


public class HotbarSlot
{
    public int SlotNumber { get; set; }
    public int VirtualKey { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public int PressCount { get; set; } = 1;
    public int DelayMs { get; set; } = 50;

    // Trigger hotkey
    public int TriggerVirtualKey { get; set; }
    public bool TriggerCtrl { get; set; }
    public bool TriggerAlt { get; set; }
    public bool TriggerShift { get; set; }
    public string TriggerDisplay { get; set; } = "Not set";

    public SlotExecutionStatus ExecutionStatus { get; set; } = SlotExecutionStatus.NoFocus;
    public bool IsEnabled { get; set; } = true;
}