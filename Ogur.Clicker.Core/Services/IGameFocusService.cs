// Ogur.Clicker.Core/Services/IGameFocusService.cs
namespace Ogur.Clicker.Core.Services;

public interface IGameFocusService
{
    bool IsGameFocused();
    void SetTargetProcess(int processId);
    void SetTargetProcessByName(string processName);
    void SetTargetProcessByWindowTitle(string windowTitle);
    int? GetTargetProcessId();
    string? GetTargetProcessName();
}