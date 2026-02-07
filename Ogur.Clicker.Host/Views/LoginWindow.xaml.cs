using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Ogur.Clicker.Host.ViewModels;
using Ogur.Clicker.Host.ViewModels;

namespace Ogur.Clicker.Host.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var mainWindow = app.Host?.Services.GetRequiredService<Views.MainWindow>();
    
        if (mainWindow != null)
        {
            Application.Current.MainWindow = mainWindow; // DODAJ
            mainWindow.Show();
            this.Close();
        }
    }
    
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}