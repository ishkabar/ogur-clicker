using System.Drawing;
using Ogur.Clicker.Core.Models;

namespace Ogur.Clicker.Core.Services;

public interface IMouseService
{
    /// <summary>
    /// Attempts to click at specified position with specified button
    /// </summary>
    /// <param name="action">Click action containing position and button</param>
    /// <returns>True if click was successful, false otherwise</returns>
    bool TryClick(ClickAction action);
    
    /// <summary>
    /// Attempts to click using specific input method
    /// </summary>
    /// <param name="action">Click action containing position and button</param>
    /// <param name="method">Input method to use</param>
    /// <returns>True if click was successful, false otherwise</returns>
    bool TryClick(ClickAction action, MouseInputMethod method);
    
    /// <summary>
    /// Gets current mouse cursor position
    /// </summary>
    Point GetCurrentPosition();
    
    /// <summary>
    /// Currently active input method
    /// </summary>
    MouseInputMethod CurrentMethod { get; }
    
    void SetCursorPosition(int x, int y);
}