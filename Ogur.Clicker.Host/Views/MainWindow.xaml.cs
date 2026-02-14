// Ogur.Clicker.Host/Views/MainWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Ogur.Clicker.Host.ViewModels;
using Ogur.Clicker.Infrastructure.Services;

namespace Ogur.Clicker.Host.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private const int WM_HOTKEY = 0x0312;

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

        // Hook WndProc after window is loaded
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook(WndProc);

        // Set window handle in ViewModel
        _viewModel.SetWindowHandle(helper.Handle);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup
        var helper = new WindowInteropHelper(this);
        _viewModel.Cleanup(helper.Handle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int hotkeyId = wParam.ToInt32();
            _viewModel.OnHotkeyPressed(hotkeyId);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsAlwaysOnTop))
        {
            this.Topmost = _viewModel.IsAlwaysOnTop;
        }
    }
}