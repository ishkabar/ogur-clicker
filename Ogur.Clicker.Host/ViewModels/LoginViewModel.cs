// Ogur.Clicker.Host/ViewModels/LoginViewModel.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ogur.Abstractions.Hub;
using Ogur.Clicker.Core.Models;

namespace Ogur.Clicker.Host.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ILicenseValidator _licenseValidator;
    private readonly AppSettings _settings;
    private bool _isClearing;

    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _rememberMe;
    [ObservableProperty] private string? _errorMessage;

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(
        IAuthService authService,
        ILicenseValidator licenseValidator,
        AppSettings settings)
    {
        _authService = authService;
        _licenseValidator = licenseValidator;
        _settings = settings;

        // Load saved credentials
        if (_settings.RememberMe)
        {
            Username = _settings.Username;
            Password = _settings.GetPassword();
            RememberMe = true;
        }
    }

    partial void OnUsernameChanged(string? value)
    {
        if (!_isClearing)
            ErrorMessage = null;
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string? value)
    {
        if (!_isClearing)
            ErrorMessage = null;
        LoginCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(Username, Password, ct);

            if (result.Success)
            {
                var licenseResult = await _licenseValidator.ValidateAsync(ct);

                if (!licenseResult.IsValid)
                {
                    ErrorMessage = $"License validation failed: {licenseResult.ErrorMessage}";
                    _authService.Logout();
                    return;
                }

                // Save credentials if RememberMe is checked
                if (RememberMe)
                {
                    _settings.Username = Username;
                    _settings.SetPassword(Password);
                    _settings.RememberMe = true;
                    _settings.Save();
                }
                else
                {
                    _settings.Username = "";
                    _settings.HashedPassword = "";
                    _settings.RememberMe = false;
                    _settings.Save();
                }

                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var errorMsg = result.ErrorMessage ?? "Login failed";

                // Parse JSON error if needed
                if (errorMsg.StartsWith("{"))
                {
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(errorMsg);
                        if (json.RootElement.TryGetProperty("error", out var errorProp))
                        {
                            errorMsg = errorProp.GetString() ?? errorMsg;
                        }
                    }
                    catch { }
                }

                ErrorMessage = errorMsg;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _isClearing = true;
            if (!RememberMe)
                Password = string.Empty;
            _isClearing = false;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !IsLoading;
}