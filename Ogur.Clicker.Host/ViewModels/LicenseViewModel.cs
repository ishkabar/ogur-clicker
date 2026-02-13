using System.Windows.Input;
using Ogur.Clicker.Host.Commands;

namespace Ogur.Clicker.Host.ViewModels;

public class LicenseViewModel : ViewModelBase
{
    private string _expirationMessage = "Your license has expired.";

    public LicenseViewModel()
    {
        CloseCommand = new RelayCommand(OnClose);
    }

    public string ExpirationMessage
    {
        get => _expirationMessage;
        set => SetProperty(ref _expirationMessage, value);
    }

    public ICommand CloseCommand { get; }

    private void OnClose()
    {
        // This will be handled by the view to close application
    }
}