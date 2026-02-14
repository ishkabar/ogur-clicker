// Ogur.Clicker.Core/Services/IKeyboardService.cs
using Ogur.Clicker.Core.Models;

namespace Ogur.Clicker.Core.Services;

public interface IKeyboardService
{
    /// <summary>
    /// Press key X times with delay
    /// </summary>
    Task PressKeyAsync(int virtualKey, int pressCount, int delayMs, KeyboardInputMethod method);

    /// <summary>
    /// Single key press with optional hold time
    /// </summary>
    void PressKey(int virtualKey, KeyboardInputMethod method, int holdTimeMs = 10);

    /// <summary>
    /// Key down
    /// </summary>
    void KeyDown(int virtualKey, KeyboardInputMethod method);

    /// <summary>
    /// Key up
    /// </summary>
    void KeyUp(int virtualKey, KeyboardInputMethod method);
}