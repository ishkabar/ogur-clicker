// Ogur.Clicker.Core/Models/HotbarProfile.cs
namespace Ogur.Clicker.Core.Models;

public class HotbarProfile
{
    public string Name { get; set; } = "Default";
    public List<HotbarSlot> Slots { get; set; } = new();
    public KeyboardInputMethod InputMethod { get; set; } = KeyboardInputMethod.SendInput;

    // Window settings
    public bool AlwaysOnTop { get; set; } = false;

    // Target process settings
    public string? TargetProcessName { get; set; }
    public int? TargetProcessId { get; set; }

    // Focus check settings
    public int FocusCheckIntervalMs { get; set; } = 100;
}