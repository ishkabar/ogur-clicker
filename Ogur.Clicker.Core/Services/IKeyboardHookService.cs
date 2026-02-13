namespace Ogur.Clicker.Core.Services;

public interface IKeyboardHookService
{
    event EventHandler<(int virtualKey, bool isCtrl, bool isAlt, bool isShift)>? KeyPressed;
    void StartListening();
    void StopListening();
}