// Ogur.Clicker.Core/Models/AppSettings.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ogur.Clicker.Core.Models;

public class AppSettings
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OgurClicker",
        "config.json"
    );

    public string Username { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public bool RememberMe { get; set; } = false;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public void SetPassword(string password)
    {
        HashedPassword = HashPassword(password);
    }

    public string? GetPassword()
    {
        return string.IsNullOrEmpty(HashedPassword) ? null : UnhashPassword(HashedPassword);
    }

    private static string HashPassword(string password)
    {
        var entropy = Encoding.UTF8.GetBytes("OgurClicker2025");
        var data = Encoding.UTF8.GetBytes(password);
        var encrypted = ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    private static string UnhashPassword(string hash)
    {
        try
        {
            var entropy = Encoding.UTF8.GetBytes("OgurClicker2025");
            var encrypted = Convert.FromBase64String(hash);
            var decrypted = ProtectedData.Unprotect(encrypted, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return "";
        }
    }
}