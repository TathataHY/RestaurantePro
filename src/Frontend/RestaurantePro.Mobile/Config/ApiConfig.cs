using System.Text;
using System.Text.Json;

namespace RestaurantePro.Mobile.Config;

/// <summary>
/// Configuración para la API del backend
/// </summary>
public static class ApiConfig
{
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private const string EnvironmentsAssetPath = "Config/environments.json";
    private const string ActiveEnvPreferenceKey = "env_selected";

    private static EnvironmentsConfig? _configCache;
    private static EnvironmentProfile? _activeProfileCache;

    public static string GetBaseUrl()
    {
        var profile = GetActiveProfile();
        return profile.BaseUrl.EndsWith("/") ? profile.BaseUrl : profile.BaseUrl + "/";
    }

    public static bool UsesBasicAuth()
    {
        return GetActiveProfile().UsesBasic;
    }

    public static string? GetBasicUser() => GetActiveProfile().BasicUser;
    public static string? GetBasicPass() => GetActiveProfile().BasicPass;

    public static string? GetTenantId() => GetActiveProfile().TenantId;

    public static string? GetEncodedBasicCredentials()
    {
        var p = GetActiveProfile();
        if (!p.UsesBasic || string.IsNullOrEmpty(p.BasicUser) || string.IsNullOrEmpty(p.BasicPass))
            return null;
        var raw = $"{p.BasicUser}:{p.BasicPass}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static void SetActiveEnvironment(string envId)
    {
        // Guardar preferencia; usamos Microsoft.Maui.Storage
        Microsoft.Maui.Storage.Preferences.Set(ActiveEnvPreferenceKey, envId);
        _activeProfileCache = null;
    }

    public static string GetActiveEnvironmentId()
    {
        var cfg = LoadConfig();
        var stored = Microsoft.Maui.Storage.Preferences.Get(ActiveEnvPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored) && cfg.Profiles.Any(p => p.Id == stored))
            return stored;
        if (cfg.Profiles == null || cfg.Profiles.Count == 0)
            return "local";
        return cfg.Default ?? cfg.Profiles.First().Id;
    }

    public static string GetActiveEnvironmentName()
    {
        return GetActiveProfile().Name;
    }

    public record EnvironmentOption(string Id, string Name);

    public static List<EnvironmentOption> GetEnvironmentOptions()
    {
        var cfg = LoadConfig();
        return cfg.Profiles.Select(p => new EnvironmentOption(p.Id, p.Name)).ToList();
    }

    private static EnvironmentProfile GetActiveProfile()
    {
        if (_activeProfileCache != null) return _activeProfileCache;
        var cfg = LoadConfig();
        var id = GetActiveEnvironmentId();
        _activeProfileCache = cfg.Profiles.FirstOrDefault(p => p.Id == id) ?? cfg.Profiles.First();
        return _activeProfileCache;
    }

    private static EnvironmentsConfig LoadConfig()
    {
        if (_configCache != null) return _configCache;
        using var stream = FileSystem.OpenAppPackageFileAsync(EnvironmentsAssetPath).GetAwaiter().GetResult();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _configCache = JsonSerializer.Deserialize<EnvironmentsConfig>(json, options) ?? new EnvironmentsConfig();
        if (_configCache.Profiles == null || _configCache.Profiles.Count == 0)
        {
            _configCache.Profiles = new List<EnvironmentProfile>
            {
                new EnvironmentProfile { Id = "local", Name = "Local", BaseUrl = "http://10.0.2.2:5000/", UsesBasic = false }
            };
            _configCache.Default ??= "local";
        }
        return _configCache;
    }

    private class EnvironmentsConfig
    {
        public List<EnvironmentProfile> Profiles { get; set; } = new();
        public string? Default { get; set; }
    }

    private class EnvironmentProfile
    {
        public string Id { get; set; } = "beta";
        public string Name { get; set; } = "Beta";
        public string BaseUrl { get; set; } = string.Empty;
        public bool UsesBasic { get; set; }
        public string? BasicUser { get; set; }
        public string? BasicPass { get; set; }
        public string? TenantId { get; set; }
    }
} 