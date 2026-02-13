namespace Ogur.Clicker.Core.Services;

public interface IMouseHookService
{
    event EventHandler<System.Drawing.Point>? LeftButtonClicked;
    event EventHandler<int>? MouseButtonClicked; // virtualKey dla dowolnego przycisku
    void StartListening();
    void StopListening();
}