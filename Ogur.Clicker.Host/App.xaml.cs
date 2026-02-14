// Ogur.Clicker.Host/App.xaml.cs
using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ogur.Core.DependencyInjection;
using Ogur.Core.Hub;
using Ogur.Clicker.Core.Services;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Constants;
using Ogur.Clicker.Infrastructure.Services;
using Ogur.Clicker.Host.ViewModels;
using Ogur.Clicker.Host.Views;

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
            _watchdogTimer.Stop();
            _watchdogTimer.Start();
        };
        _watchdogTimer.Start();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddOgurCore(context.Configuration);
                services.AddOgurHub(context.Configuration);

                services.PostConfigure<HubOptions>(options =>
                {
                    options.HubUrl = "https://api.hub.ogur.dev";
                    options.ApiKey = HubConstants.ApiKey;
                    options.ApplicationName = HubConstants.ApplicationName;
                    options.ApplicationVersion = HubConstants.ApplicationVersion;
                });

                services.AddSingleton(AppSettings.Load());

                // Mouse Services
                services.AddSingleton<IMouseService, MouseService>();
                services.AddSingleton<IPositionCaptureService, PositionCaptureService>();
                services.AddSingleton<IGlobalMouseHotkeyService, GlobalMouseHotkeyService>();

                // Keyboard Services
                services.AddSingleton<IKeyboardService, KeyboardService>();

                // Hook Services
                services.AddSingleton<IMouseHookService, MouseHookService>();
                services.AddSingleton<IKeyboardHookService, KeyboardHookService>();

                // Hotkey Services
                services.AddSingleton<IGlobalKeyboardHotkeyService, GlobalKeyboardHotkeyService>();
                services.AddSingleton<IMultiHotkeyService, MultiHotkeyService>();

                // Hotbar Service
                services.AddSingleton<IHotbarService, HotbarService>();
                services.AddSingleton<IGameFocusService, GameFocusService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<LicenseViewModel>();

                // Windows
                services.AddTransient<LoginWindow>();
                services.AddSingleton<MainWindow>();
                services.AddTransient<LicenseWindow>();
            })
            .Build();

        var expirationDate = new DateTime(2026, 10, 10);
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

            TryAutoLoadProfile();
        }
    }

    private void TryAutoLoadProfile()
    {
        try
        {
            var lastProfilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OgurClicker",
                "last_profile.json"
            );

            if (File.Exists(lastProfilePath))
            {
                var hotbarService = Host?.Services.GetRequiredService<IHotbarService>();
                if (hotbarService != null)
                {
                    var profile = hotbarService.LoadProfileFromFile(lastProfilePath);
                    hotbarService.LoadProfile(profile);

                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (mainWindow?.DataContext is MainViewModel viewModel)
                    {
                        viewModel.IsAlwaysOnTop = profile.AlwaysOnTop;
                        viewModel.FocusCheckIntervalMs = profile.FocusCheckIntervalMs;
                        viewModel.TargetProcessName = profile.TargetProcessName ?? "Not set";
                        viewModel.TargetProcessId = profile.TargetProcessId;
                    }
                }
            }
        }
        catch { }
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
        try
        {
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
        Environment.Exit(0);
    }
}