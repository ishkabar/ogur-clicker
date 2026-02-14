// Ogur.Clicker.Host/Views/PortableView.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ogur.Clicker.Host.ViewModels;

namespace Ogur.Clicker.Host.Views;

public partial class PortableView : Window
{
    public PortableView()
    {
        InitializeComponent();
    }

    private void Draggable_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Blokuj drag na buttonach
        if (e.OriginalSource is Button || e.Source is Button)
            return;

        try
        {
            DragMove();
        }
        catch
        {
            // Ignore errors
        }
    }

    private void ReturnToNormal_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.Activate();
        }
        Close();
    }

    private void ToggleAlwaysOnTop_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.IsAlwaysOnTop = !vm.IsAlwaysOnTop;
        }
    }
}