using System.Windows;
using System.Windows.Input;
using Ogur.Clicker.Host.ViewModels;

namespace Ogur.Clicker.Host.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        
        // Emergency exit
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Q && 
                Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                Environment.Exit(0);
            }
        };

        // Subscribe to IsAlwaysOnTop changes
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsAlwaysOnTop))
        {
            this.Topmost = _viewModel.IsAlwaysOnTop;
        }
    }
}