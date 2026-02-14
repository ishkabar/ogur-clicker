// Ogur.Clicker.Infrastructure/Services/GameFocusService.cs
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class GameFocusService : IGameFocusService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private int? _targetProcessId;
    private bool _lastFocusState;
    private DateTime _lastCheck = DateTime.MinValue;
    private const int CACHE_MS = 50;

    public bool IsGameFocused()
    {
        if (_targetProcessId == null)
            return false;

        // Cache - żeby nie sprawdzać za często jak timer woła co 100ms
        if ((DateTime.Now - _lastCheck).TotalMilliseconds < CACHE_MS)
            return _lastFocusState;

        try
        {
            var foregroundWindow = GetForegroundWindow();
            GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);

            _lastFocusState = foregroundProcessId == _targetProcessId;
            _lastCheck = DateTime.Now;
            return _lastFocusState;
        }
        catch
        {
            _lastFocusState = false;
            return false;
        }
    }

    public void SetTargetProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            _targetProcessId = processId;
        }
        catch
        {
            _targetProcessId = null;
        }
    }

    public void SetTargetProcessByName(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                _targetProcessId = processes[0].Id;
            }
        }
        catch
        {
            _targetProcessId = null;
        }
    }

    public void SetTargetProcessByWindowTitle(string windowTitle)
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero &&
                           p.MainWindowTitle.Contains(windowTitle, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (processes.Count > 0)
            {
                _targetProcessId = processes[0].Id;
            }
        }
        catch
        {
            _targetProcessId = null;
        }
    }

    public int? GetTargetProcessId() => _targetProcessId;

    public string? GetTargetProcessName()
    {
        if (_targetProcessId == null)
            return null;

        try
        {
            var process = Process.GetProcessById(_targetProcessId.Value);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }
}