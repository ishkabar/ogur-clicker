// Ogur.Clicker.Host/Views/AddSlotDialog.xaml.cs
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Ogur.Clicker.Host.Views;

public partial class AddSlotDialog : Window
{
    public int VirtualKey { get; private set; }
    public string KeyName { get; private set; } = string.Empty;

    public AddSlotDialog()
    {
        InitializeComponent();
        Debug.WriteLine("=== CONSTRUCTOR ===");
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        Debug.WriteLine($"=== OnKeyDown FIRED: Key={e.Key} ===");

        // Ignore modifier-only keys
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.System)
        {
            Debug.WriteLine("IGNORED: Modifier key");
            e.Handled = true;
            return;
        }

        // Ignore Escape
        if (e.Key == Key.Escape)
        {
            Debug.WriteLine("IGNORED: Escape");
            e.Handled = true;
            return;
        }

        // Capture the key
        VirtualKey = KeyInterop.VirtualKeyFromKey(e.Key);
        KeyName = e.Key.ToString();

        Debug.WriteLine($"CAPTURED: VK=0x{VirtualKey:X2}, Name={KeyName}");

        KeyPreview.Text = KeyName;
        VirtualKeyPreview.Text = $"VK: 0x{VirtualKey:X2}";

        Debug.WriteLine("CLOSING DIALOG with DialogResult=true");

        // Auto-close
        DialogResult = true;
        Close();

        e.Handled = true;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("=== AddButton_Click ===");
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("=== CancelButton_Click ===");
        DialogResult = false;
        Close();
    }
}