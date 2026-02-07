using System.Windows.Input;
using Ogur.Clicker.Host.Commands;

namespace Ogur.Clicker.Host.ViewModels;

public class LoginViewModel : ViewModelBase
{
    public LoginViewModel()
    {
        ContinueCommand = new RelayCommand(OnContinue);
    }

    public ICommand ContinueCommand { get; }

    private void OnContinue()
    {
        // This will be handled by the view to open MainWindow
    }
}