// Ogur.Clicker.Host/Views/ProcessSelectionDialog.xaml.cs
using System.Diagnostics;
using System.Windows;

namespace Ogur.Clicker.Host.Views;

public partial class ProcessSelectionDialog : Window
{
    public int? SelectedProcessId { get; private set; }
    public string? SelectedProcessName { get; private set; }

    public ProcessSelectionDialog()
    {
        InitializeComponent();
        LoadProcesses();
    }

    private void LoadProcesses()
    {
        var processes = Process.GetProcesses()
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => new
            {
                p.Id,
                p.ProcessName,
                p.MainWindowTitle,
                Handle = p.MainWindowHandle.ToString()
            })
            .OrderBy(p => p.ProcessName)
            .ToList();

        ProcessesListView.ItemsSource = processes;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessesListView.SelectedItem != null)
        {
            dynamic selected = ProcessesListView.SelectedItem;
            SelectedProcessId = selected.Id;
            SelectedProcessName = selected.ProcessName;
            DialogResult = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadProcesses();
    }
}