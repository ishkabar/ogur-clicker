// Ogur.Clicker.Host/Views/LoginWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Ogur.Clicker.Host.ViewModels;

namespace Ogur.Clicker.Host.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly MainWindow _mainWindow;


    public LoginWindow(LoginViewModel viewModel, MainWindow mainWindow)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.LoginSucceeded += OnLoginSucceeded;

        // Bind password
        PasswordBox.PasswordChanged += (s, e) =>
        {
            _viewModel.Password = PasswordBox.Password;
        };

        if (!string.IsNullOrEmpty(_viewModel.Password))
        {
            PasswordBox.Password = _viewModel.Password;
        }
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        // Show MainWindow
        var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        if (mainWindow != null)
        {
            mainWindow.Show();
            Application.Current.MainWindow = _mainWindow; // ✅ Set as main window
            Close();
        }
    }
}