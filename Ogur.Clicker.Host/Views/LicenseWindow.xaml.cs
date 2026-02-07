using System.Windows;
using System.Windows;
using Ogur.Clicker.Host.ViewModels;

namespace Ogur.Clicker.Host.Views;

public partial class LicenseWindow : Window
{
    public LicenseWindow(LicenseViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}