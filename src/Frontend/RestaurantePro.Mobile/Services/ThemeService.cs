using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio para gestión de temas (Dark/Light) - V4
/// </summary>
public class ThemeService : INotifyPropertyChanged
{
    private static ThemeService _instance;
    private static readonly object _lock = new object();

    public static ThemeService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ThemeService();
                    }
                }
            }
            return _instance;
        }
    }

    public enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    private ThemeMode _currentTheme;
    private bool _isDarkMode;

    public ThemeMode CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                ApplyThemeInternal();
                OnPropertyChanged();
            }
        }
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        private set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                OnPropertyChanged();
            }
        }
    }

    public event EventHandler<ThemeMode> ThemeChanged;

    private ThemeService()
    {
        LoadThemePreference();
    }

    /// <summary>
    /// Carga la preferencia de tema guardada
    /// </summary>
    private void LoadThemePreference()
    {
        try
        {
            var savedTheme = Preferences.Get("ThemeMode", ThemeMode.System.ToString());
            if (Enum.TryParse<ThemeMode>(savedTheme, out var theme))
            {
                CurrentTheme = theme;
            }
            else
            {
                CurrentTheme = ThemeMode.System;
            }
        }
        catch
        {
            CurrentTheme = ThemeMode.System;
        }
    }

    /// <summary>
    /// Guarda la preferencia de tema
    /// </summary>
    private void SaveThemePreference()
    {
        try
        {
            Preferences.Set("ThemeMode", CurrentTheme.ToString());
        }
        catch
        {
            // Ignorar errores de persistencia
        }
    }

    /// <summary>
    /// Aplica el tema actual (método privado)
    /// </summary>
    private void ApplyThemeInternal()
    {
        var effectiveTheme = GetEffectiveTheme();
        IsDarkMode = effectiveTheme == ThemeMode.Dark;

        try
        {
            // Aplicar tema a la aplicación de forma segura
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplyThemeToApplication(effectiveTheme);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
        
        // Guardar preferencia
        SaveThemePreference();
        
        // Notificar cambio
        ThemeChanged?.Invoke(this, CurrentTheme);
    }

    /// <summary>
    /// Aplica el tema a la aplicación
    /// </summary>
    private void ApplyThemeToApplication(ThemeMode theme)
    {
        if (Application.Current?.Resources == null) return;

        try
        {
            // Limpiar diccionarios de tema existentes
            var existingThemes = Application.Current.Resources.MergedDictionaries
                .Where(d => d.Source?.OriginalString?.Contains("Themes") == true)
                .ToList();
            
            foreach (var existingTheme in existingThemes)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
            }
            
            // Agregar el nuevo tema
            var themeDictionary = GetThemeResourceDictionary(theme);
            Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ApplyThemeToApplication: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el tema efectivo (considerando configuración del sistema)
    /// </summary>
    private ThemeMode GetEffectiveTheme()
    {
        if (CurrentTheme == ThemeMode.System)
        {
            // Detectar tema del sistema
            return Application.Current.RequestedTheme == AppTheme.Dark ? ThemeMode.Dark : ThemeMode.Light;
        }
        
        return CurrentTheme;
    }

    /// <summary>
    /// Obtiene el diccionario de recursos del tema
    /// </summary>
    private ResourceDictionary GetThemeResourceDictionary(ThemeMode theme)
    {
        return theme switch
        {
            ThemeMode.Dark => new ResourceDictionary
            {
                Source = new Uri("Resources/Styles/Themes/DarkTheme.xaml", UriKind.Relative)
            },
            ThemeMode.Light => new ResourceDictionary
            {
                Source = new Uri("Resources/Styles/Themes/LightTheme.xaml", UriKind.Relative)
            },
            _ => new ResourceDictionary
            {
                Source = new Uri("Resources/Styles/Themes/LightTheme.xaml", UriKind.Relative)
            }
        };
    }

    /// <summary>
    /// Cambia al tema claro
    /// </summary>
    public void SetLightTheme()
    {
        CurrentTheme = ThemeMode.Light;
    }

    /// <summary>
    /// Cambia al tema oscuro
    /// </summary>
    public void SetDarkTheme()
    {
        CurrentTheme = ThemeMode.Dark;
    }

    /// <summary>
    /// Cambia al tema del sistema
    /// </summary>
    public void SetSystemTheme()
    {
        CurrentTheme = ThemeMode.System;
    }

    /// <summary>
    /// Cambia el tema de forma asíncrona
    /// </summary>
    public async Task SetThemeAsync(ThemeMode theme)
    {
        CurrentTheme = theme;
        await Task.Delay(100); // Pequeña pausa para la animación
    }

    /// <summary>
    /// Alterna entre tema claro y oscuro
    /// </summary>
    public void ToggleTheme()
    {
        if (CurrentTheme == ThemeMode.Light)
        {
            SetDarkTheme();
        }
        else
        {
            SetLightTheme();
        }
    }

    /// <summary>
    /// Aplica el tema actual (método público para inicialización)
    /// </summary>
    public void ApplyTheme()
    {
        var effectiveTheme = GetEffectiveTheme();
        IsDarkMode = effectiveTheme == ThemeMode.Dark;

        try
        {
            // Aplicar tema a la aplicación de forma segura
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplyThemeToApplication(effectiveTheme);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
        
        // Guardar preferencia
        SaveThemePreference();
        
        // Notificar cambio
        ThemeChanged?.Invoke(this, effectiveTheme);
    }

    /// <summary>
    /// Obtiene el color de superficie primario actual
    /// </summary>
    public Color GetSurfacePrimaryColor()
    {
        return IsDarkMode ? Color.FromArgb("#121212") : Color.FromArgb("#FFFFFF");
    }

    /// <summary>
    /// Obtiene el color de texto primario actual
    /// </summary>
    public Color GetTextPrimaryColor()
    {
        return IsDarkMode ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#212529");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 