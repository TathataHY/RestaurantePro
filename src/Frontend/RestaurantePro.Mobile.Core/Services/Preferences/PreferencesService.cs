using Microsoft.Maui.Storage;

namespace RestaurantePro.Mobile.Core.Services.Preferences;

/// <summary>
/// Implementación del servicio de preferencias usando .NET MAUI Preferences
/// </summary>
public class PreferencesService : IPreferencesService
{
    /// <summary>
    /// Obtiene un valor de preferencia
    /// </summary>
    public T Get<T>(string key, T defaultValue = default)
    {
        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (string)(object)defaultValue);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (int)(object)defaultValue);
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (bool)(object)defaultValue);
            }
            else if (typeof(T) == typeof(double))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (double)(object)defaultValue);
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (float)(object)defaultValue);
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (long)(object)defaultValue);
            }
            else
            {
                // Para tipos complejos, usar JSON
                var json = Microsoft.Maui.Storage.Preferences.Get(key, string.Empty);
                if (string.IsNullOrEmpty(json))
                {
                    return defaultValue;
                }
                
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Establece un valor de preferencia
    /// </summary>
    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            if (typeof(T) == typeof(string))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (string)(object)value);
            }
            else if (typeof(T) == typeof(int))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (int)(object)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (bool)(object)value);
            }
            else if (typeof(T) == typeof(double))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (double)(object)value);
            }
            else if (typeof(T) == typeof(float))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (float)(object)value);
            }
            else if (typeof(T) == typeof(long))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (long)(object)value);
            }
            else
            {
                // Para tipos complejos, usar JSON
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                Microsoft.Maui.Storage.Preferences.Set(key, json);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Log error si es necesario
        }
    }

    /// <summary>
    /// Elimina una preferencia
    /// </summary>
    public void Remove(string key)
    {
        try
        {
            Microsoft.Maui.Storage.Preferences.Remove(key);
        }
        catch (Exception)
        {
            // Log error si es necesario
        }
    }

    /// <summary>
    /// Verifica si existe una preferencia
    /// </summary>
    public bool ContainsKey(string key)
    {
        try
        {
            return Microsoft.Maui.Storage.Preferences.ContainsKey(key);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Limpia todas las preferencias
    /// </summary>
    public void Clear()
    {
        try
        {
            Microsoft.Maui.Storage.Preferences.Clear();
        }
        catch (Exception)
        {
            // Log error si es necesario
        }
    }
} 