// Ogur.Clicker.Core/Services/IHotbarService.cs
using Ogur.Clicker.Core.Models;

namespace Ogur.Clicker.Core.Services;

public interface IHotbarService
{
    event EventHandler<int>? SlotTriggered;
    HotbarProfile CurrentProfile { get; }

    void LoadProfile(HotbarProfile profile);
    void SaveProfile(string path);
    HotbarProfile LoadProfileFromFile(string path);
    void AddSlot(HotbarSlot slot);
    void UpdateSlot(int slotNumber, HotbarSlot slot);
    HotbarSlot? GetSlot(int slotNumber);
    void RemoveSlot(int slotNumber);
    void MoveSlot(int fromIndex, int toIndex);
    void RegisterAllHotkeys(IntPtr windowHandle);
    void UnregisterAllHotkeys(IntPtr windowHandle);
    Task ExecuteSlotAsync(int slotNumber);
    void SetTargetProcess(int processId);
    void SetTargetProcessByName(string processName);
    void SetTargetProcessByWindowTitle(string windowTitle);
}