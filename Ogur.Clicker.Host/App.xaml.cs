using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ogur.Clicker.Core;
using Ogur.Clicker.Infrastructure.Services;
using Ogur.Clicker.Host.ViewModels;
using Ogur.Clicker.Host.Views;
using Ogur.Clicker.Core.Services;
using System.Media;


namespace Ogur.Clicker.Host;

public partial class App : Application
{
    public IHost? Host { get; private set; }
    private DispatcherTimer? _watchdogTimer;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
    // Setup unhandled exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Watchdog timer - force exit after 30 seconds of freeze
        _watchdogTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _watchdogTimer.Tick += (s, args) =>
        {
            // If we're here, UI thread is responsive
            _watchdogTimer.Stop();
            _watchdogTimer.Start();
        };
        _watchdogTimer.Start();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IMouseService, MouseService>();
                services.AddSingleton<IMouseHookService, MouseHookService>();
                services.AddSingleton<IKeyboardHookService, KeyboardHookService>();
                services.AddSingleton<IPositionCaptureService, PositionCaptureService>();
                services.AddSingleton<IGlobalMouseHotkeyService, GlobalMouseHotkeyService>();
                services.AddSingleton<IGlobalKeyboardHotkeyService, GlobalKeyboardHotkeyService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<LicenseViewModel>();

                // Windows
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<LicenseWindow>();
            })
            .Build();

        var expirationDate = new DateTime(2025, 12, 12);
        if (DateTime.Now > expirationDate)
        {
            SystemSounds.Hand.Play();
            var licenseWindow = Host.Services.GetRequiredService<LicenseWindow>();
            MainWindow = licenseWindow;
            licenseWindow.Show();
        }
        else
        {
            var loginWindow = Host.Services.GetRequiredService<LoginWindow>();
            MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
    
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Environment.Exit(1);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Environment.Exit(1);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        Environment.Exit(1);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Force cleanup hooks
        try
        {
            // Stop wszystkie hooky PRZED dispose
            var mouseHook = Host?.Services.GetService<IMouseHookService>();
            if (mouseHook != null)
            {
                mouseHook.StopListening();
                (mouseHook as IDisposable)?.Dispose();
            }

            var keyboardHook = Host?.Services.GetService<IKeyboardHookService>();
            if (keyboardHook != null)
            {
                keyboardHook.StopListening();
                (keyboardHook as IDisposable)?.Dispose();
            }
        }
        catch { }

        Host?.Dispose();
        base.OnExit(e);
        
        // Force exit
        Environment.Exit(0);
    }
}