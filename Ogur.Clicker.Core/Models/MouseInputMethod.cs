namespace Ogur.Clicker.Core.Models;

public enum MouseInputMethod
{
    /// <summary>
    /// Standard SendInput API (user-mode)
    /// </summary>
    SendInput,
    
    /// <summary>
    /// SendInput with INPUT_HARDWARE flag (simulates hardware)
    /// </summary>
    SendInputHardware,
    
    /// <summary>
    /// Direct message posting to window handle
    /// </summary>
    PostMessage,
    
    /// <summary>
    /// Legacy DirectInput API (fallback)
    /// </summary>
    DirectInput
}