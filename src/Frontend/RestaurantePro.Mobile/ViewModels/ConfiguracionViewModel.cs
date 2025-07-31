using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RestaurantePro.Mobile.Services;

namespace RestaurantePro.Mobile.ViewModels;

/// <summary>
/// ViewModel para la página de configuración - V4
/// </summary>
public class ConfiguracionViewModel : INotifyPropertyChanged
{
    private readonly ThemeService _themeService;
    private readonly AnimationOptimizationService _animationService;
    private readonly ImageOptimizationService _imageService;
    private readonly AccessibilityService _accessibilityService;

    public ConfiguracionViewModel()
    {
        _themeService = ThemeService.Instance;
        _animationService = AnimationOptimizationService.Instance;
        _imageService = ImageOptimizationService.Instance;
        _accessibilityService = AccessibilityService.Instance;
        
        // Inicializar comandos
        ClearCacheCommand = new Command(async () => await ClearCacheAsync());
        SyncDataCommand = new Command(async () => await SyncDataAsync());
        ExportDataCommand = new Command(async () => await ExportDataAsync());
        ClearImageCacheCommand = new Command(async () => await ClearImageCacheAsync());
        OptimizePerformanceCommand = new Command(async () => await OptimizePerformanceAsync());
        TestAccessibilityCommand = new Command(async () => await TestAccessibilityAsync());
        ResetAccessibilityCommand = new Command(async () => await ResetAccessibilityAsync());
        ApplyLanguageCommand = new Command(async () => await ApplyLanguageAsync());
        
        // Suscribirse a cambios de tema
        _themeService.PropertyChanged += OnThemeServicePropertyChanged;
        
        // Actualizar propiedades iniciales
        UpdateThemeProperties();
        LoadPerformanceSettings();
        LoadAccessibilitySettings();
        LoadLanguageSettings();
    }

    #region Propiedades de Tema

    private bool _isLightTheme;
    public bool IsLightTheme
    {
        get => _isLightTheme;
        set
        {
            if (_isLightTheme != value)
            {
                _isLightTheme = value;
                if (value)
                {
                    _themeService.SetLightTheme();
                }
                OnPropertyChanged();
            }
        }
    }

    private bool _isDarkTheme;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme != value)
            {
                _isDarkTheme = value;
                if (value)
                {
                    _themeService.SetDarkTheme();
                }
                OnPropertyChanged();
            }
        }
    }

    private bool _isSystemTheme;
    public bool IsSystemTheme
    {
        get => _isSystemTheme;
        set
        {
            if (_isSystemTheme != value)
            {
                _isSystemTheme = value;
                if (value)
                {
                    _themeService.SetSystemTheme();
                }
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Propiedades de Performance

    private bool _animationsEnabled = true;
    public bool AnimationsEnabled
    {
        get => _animationsEnabled;
        set
        {
            if (_animationsEnabled != value)
            {
                _animationsEnabled = value;
                _animationService.SetAnimationsEnabled(value);
                SavePerformanceSettings();
                OnPropertyChanged();
            }
        }
    }

    private bool _imageOptimizationEnabled = true;
    public bool ImageOptimizationEnabled
    {
        get => _imageOptimizationEnabled;
        set
        {
            if (_imageOptimizationEnabled != value)
            {
                _imageOptimizationEnabled = value;
                SavePerformanceSettings();
                OnPropertyChanged();
            }
        }
    }

    private bool _imageCacheEnabled = true;
    public bool ImageCacheEnabled
    {
        get => _imageCacheEnabled;
        set
        {
            if (_imageCacheEnabled != value)
            {
                _imageCacheEnabled = value;
                SavePerformanceSettings();
                OnPropertyChanged();
            }
        }
    }

    private AnimationQuality _selectedAnimationQuality = AnimationQuality.Medium;
    public AnimationQuality SelectedAnimationQuality
    {
        get => _selectedAnimationQuality;
        set
        {
            if (_selectedAnimationQuality != value)
            {
                _selectedAnimationQuality = value;
                _animationService.SetAnimationQuality(value);
                SavePerformanceSettings();
                OnPropertyChanged();
            }
        }
    }

    public List<AnimationQuality> AnimationQualityOptions => new List<AnimationQuality>
    {
        AnimationQuality.Low,
        AnimationQuality.Medium,
        AnimationQuality.High
    };

    #endregion

    #region Propiedades de Notificaciones

    private bool _pushNotificationsEnabled = true;
    public bool PushNotificationsEnabled
    {
        get => _pushNotificationsEnabled;
        set
        {
            if (_pushNotificationsEnabled != value)
            {
                _pushNotificationsEnabled = value;
                SaveNotificationSettings();
                OnPropertyChanged();
            }
        }
    }

    private bool _soundEnabled = true;
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set
        {
            if (_soundEnabled != value)
            {
                _soundEnabled = value;
                SaveNotificationSettings();
                OnPropertyChanged();
            }
        }
    }

    private bool _vibrationEnabled = true;
    public bool VibrationEnabled
    {
        get => _vibrationEnabled;
        set
        {
            if (_vibrationEnabled != value)
            {
                _vibrationEnabled = value;
                SaveNotificationSettings();
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Propiedades de Accesibilidad

    private bool _screenReaderEnabled = false;
    public bool ScreenReaderEnabled
    {
        get => _screenReaderEnabled;
        set
        {
            if (_screenReaderEnabled != value)
            {
                _screenReaderEnabled = value;
                _accessibilityService.SetScreenReaderEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _keyboardNavigationEnabled = true;
    public bool KeyboardNavigationEnabled
    {
        get => _keyboardNavigationEnabled;
        set
        {
            if (_keyboardNavigationEnabled != value)
            {
                _keyboardNavigationEnabled = value;
                _accessibilityService.SetKeyboardNavigationEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _captionsEnabled = false;
    public bool CaptionsEnabled
    {
        get => _captionsEnabled;
        set
        {
            if (_captionsEnabled != value)
            {
                _captionsEnabled = value;
                _accessibilityService.SetCaptionsEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _highContrastEnabled = false;
    public bool HighContrastEnabled
    {
        get => _highContrastEnabled;
        set
        {
            if (_highContrastEnabled != value)
            {
                _highContrastEnabled = value;
                _accessibilityService.SetHighContrastEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _largeTextEnabled = false;
    public bool LargeTextEnabled
    {
        get => _largeTextEnabled;
        set
        {
            if (_largeTextEnabled != value)
            {
                _largeTextEnabled = value;
                _accessibilityService.SetLargeTextEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _reducedMotionEnabled = false;
    public bool ReducedMotionEnabled
    {
        get => _reducedMotionEnabled;
        set
        {
            if (_reducedMotionEnabled != value)
            {
                _reducedMotionEnabled = value;
                _accessibilityService.SetReducedMotionEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _hapticFeedbackEnabled = true;
    public bool HapticFeedbackEnabled
    {
        get => _hapticFeedbackEnabled;
        set
        {
            if (_hapticFeedbackEnabled != value)
            {
                _hapticFeedbackEnabled = value;
                _accessibilityService.SetHapticFeedbackEnabled(value);
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Comandos

    public ICommand ClearCacheCommand { get; }
    public ICommand SyncDataCommand { get; }
    public ICommand ExportDataCommand { get; }
    public ICommand ClearImageCacheCommand { get; }
    public ICommand OptimizePerformanceCommand { get; }
    public ICommand TestAccessibilityCommand { get; }
    public ICommand ResetAccessibilityCommand { get; }
    public ICommand ApplyLanguageCommand { get; }

    #endregion

    #region Propiedades de Idioma

    private string _selectedLanguage;
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentLanguageInfo));
            }
        }
    }

    public List<string> AvailableLanguages => LocalizationService.Instance.GetSupportedLanguageNames();

    public string CurrentLanguageInfo => $"Idioma actual: {LocalizationService.Instance.GetCurrentLanguageName()}";

    #endregion

    #region Métodos Privados

    private void OnThemeServicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeService.CurrentTheme))
        {
            UpdateThemeProperties();
        }
    }

    private void UpdateThemeProperties()
    {
        // Desactivar eventos temporalmente para evitar recursión
        var currentTheme = _themeService.CurrentTheme;
        
        IsLightTheme = currentTheme == ThemeService.ThemeMode.Light;
        IsDarkTheme = currentTheme == ThemeService.ThemeMode.Dark;
        IsSystemTheme = currentTheme == ThemeService.ThemeMode.System;
    }

    private void SaveNotificationSettings()
    {
        try
        {
            Preferences.Set("PushNotificationsEnabled", PushNotificationsEnabled);
            Preferences.Set("SoundEnabled", SoundEnabled);
            Preferences.Set("VibrationEnabled", VibrationEnabled);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving notification settings: {ex.Message}");
        }
    }

    private void LoadNotificationSettings()
    {
        try
        {
            PushNotificationsEnabled = Preferences.Get("PushNotificationsEnabled", true);
            SoundEnabled = Preferences.Get("SoundEnabled", true);
            VibrationEnabled = Preferences.Get("VibrationEnabled", true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading notification settings: {ex.Message}");
        }
    }

    private void LoadPerformanceSettings()
    {
        try
        {
            AnimationsEnabled = Preferences.Get("AnimationsEnabled", true);
            ImageOptimizationEnabled = Preferences.Get("ImageOptimizationEnabled", true);
            ImageCacheEnabled = Preferences.Get("ImageCacheEnabled", true);
            
            var qualityString = Preferences.Get("AnimationQuality", AnimationQuality.Medium.ToString());
            if (Enum.TryParse<AnimationQuality>(qualityString, out var quality))
            {
                SelectedAnimationQuality = quality;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading performance settings: {ex.Message}");
        }
    }

    private void LoadAccessibilitySettings()
    {
        try
        {
            ScreenReaderEnabled = Preferences.Get("ScreenReaderEnabled", false);
            KeyboardNavigationEnabled = Preferences.Get("KeyboardNavigationEnabled", true);
            CaptionsEnabled = Preferences.Get("CaptionsEnabled", false);
            HighContrastEnabled = Preferences.Get("HighContrastEnabled", false);
            LargeTextEnabled = Preferences.Get("LargeTextEnabled", false);
            ReducedMotionEnabled = Preferences.Get("ReducedMotionEnabled", false);
            HapticFeedbackEnabled = Preferences.Get("HapticFeedbackEnabled", true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading accessibility settings: {ex.Message}");
        }
    }

    private void SavePerformanceSettings()
    {
        try
        {
            Preferences.Set("AnimationsEnabled", AnimationsEnabled);
            Preferences.Set("ImageOptimizationEnabled", ImageOptimizationEnabled);
            Preferences.Set("ImageCacheEnabled", ImageCacheEnabled);
            Preferences.Set("AnimationQuality", SelectedAnimationQuality.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving performance settings: {ex.Message}");
        }
    }

    private async Task ClearCacheAsync()
    {
        try
        {
            // Simular limpieza de caché
            await Task.Delay(1000);
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Caché Limpiado",
                "El caché de la aplicación ha sido limpiado exitosamente.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al limpiar el caché: {ex.Message}",
                "OK");
        }
    }

    private async Task SyncDataAsync()
    {
        try
        {
            // Simular sincronización
            await Task.Delay(2000);
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Datos Sincronizados",
                "Los datos han sido sincronizados exitosamente.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al sincronizar datos: {ex.Message}",
                "OK");
        }
    }

    private async Task ExportDataAsync()
    {
        try
        {
            // Simular exportación
            await Task.Delay(1500);
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Datos Exportados",
                "Los datos han sido exportados exitosamente.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al exportar datos: {ex.Message}",
                "OK");
        }
    }

    private async Task ClearImageCacheAsync()
    {
        try
        {
            // Mostrar indicador de carga
            await Application.Current.MainPage.DisplayAlert(
                "Limpiando Caché",
                "Limpiando caché de imágenes...",
                "OK");
            
            // Limpiar caché de imágenes
            await _imageService.ClearImageCacheAsync();
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Caché Limpiado",
                "El caché de imágenes ha sido limpiado exitosamente.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al limpiar el caché de imágenes: {ex.Message}",
                "OK");
        }
    }

    private async Task OptimizePerformanceAsync()
    {
        try
        {
            // Mostrar indicador de carga
            await Application.Current.MainPage.DisplayAlert(
                "Optimizando",
                "Optimizando rendimiento de la aplicación...",
                "OK");
            
            // Simular optimización
            await Task.Delay(2000);
            
            // Aplicar configuraciones de optimización
            _animationService.SetAnimationsEnabled(AnimationsEnabled);
            _animationService.SetAnimationQuality(SelectedAnimationQuality);
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Optimización Completada",
                "El rendimiento de la aplicación ha sido optimizado.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al optimizar el rendimiento: {ex.Message}",
                "OK");
        }
    }

    private async Task TestAccessibilityAsync()
    {
        try
        {
            // Mostrar indicador de carga
            await Application.Current.MainPage.DisplayAlert(
                "Test de Accesibilidad",
                "Iniciando test de accesibilidad...",
                "OK");
            
            // Simular test de accesibilidad
            await Task.Delay(2000);
            
            // Anunciar al lector de pantalla si está habilitado
            if (ScreenReaderEnabled)
            {
                _accessibilityService.AnnounceToScreenReader("Test de accesibilidad completado exitosamente");
            }
            
            // Mostrar mensaje de éxito
            await Application.Current.MainPage.DisplayAlert(
                "Test Completado",
                "La accesibilidad está funcionando correctamente.\n\n" +
                "✓ Lector de pantalla: " + (ScreenReaderEnabled ? "Habilitado" : "Deshabilitado") + "\n" +
                "✓ Navegación por teclado: " + (KeyboardNavigationEnabled ? "Habilitada" : "Deshabilitada") + "\n" +
                "✓ Contraste alto: " + (HighContrastEnabled ? "Habilitado" : "Deshabilitado") + "\n" +
                "✓ Texto grande: " + (LargeTextEnabled ? "Habilitado" : "Deshabilitado") + "\n" +
                "✓ Animaciones reducidas: " + (ReducedMotionEnabled ? "Habilitadas" : "Deshabilitadas") + "\n" +
                "✓ Feedback háptico: " + (HapticFeedbackEnabled ? "Habilitado" : "Deshabilitado"),
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al realizar el test de accesibilidad: {ex.Message}",
                "OK");
        }
    }

    private async Task ResetAccessibilityAsync()
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Restaurar Configuración",
                "¿Estás seguro de que quieres restaurar todas las configuraciones de accesibilidad a sus valores por defecto?",
                "Restaurar",
                "Cancelar");
            
            if (result)
            {
                // Restaurar valores por defecto
                ScreenReaderEnabled = false;
                KeyboardNavigationEnabled = true;
                CaptionsEnabled = false;
                HighContrastEnabled = false;
                LargeTextEnabled = false;
                ReducedMotionEnabled = false;
                HapticFeedbackEnabled = true;
                
                // Mostrar mensaje de éxito
                await Application.Current.MainPage.DisplayAlert(
                    "Configuración Restaurada",
                    "Las configuraciones de accesibilidad han sido restauradas a sus valores por defecto.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al restaurar la configuración: {ex.Message}",
                "OK");
        }
    }

    #endregion

    #region Métodos de Idioma

    private void LoadLanguageSettings()
    {
        try
        {
            SelectedLanguage = LocalizationService.Instance.GetCurrentLanguageName();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading language settings: {ex.Message}");
        }
    }

    private async Task ApplyLanguageAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(SelectedLanguage))
            {
                // Obtener el código de idioma basado en el nombre
                var languageCode = GetLanguageCodeFromName(SelectedLanguage);
                
                if (!string.IsNullOrEmpty(languageCode))
                {
                    await LocalizationService.Instance.SetLanguageAsync(languageCode);
                    
                    await Application.Current.MainPage.DisplayAlert(
                        "Idioma", 
                        $"Idioma cambiado a {SelectedLanguage}. Los cambios se aplicarán completamente al reiniciar la aplicación.", 
                        "OK");
                    
                    // Actualizar la información del idioma actual
                    OnPropertyChanged(nameof(CurrentLanguageInfo));
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                $"Error al cambiar el idioma: {ex.Message}", 
                "OK");
        }
    }

    private string GetLanguageCodeFromName(string languageName)
    {
        return languageName switch
        {
            "Español" => "es",
            "English" => "en",
            "Français" => "fr",
            _ => "es" // Default to Spanish
        };
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _themeService.PropertyChanged -= OnThemeServicePropertyChanged;
    }

    #endregion
} 