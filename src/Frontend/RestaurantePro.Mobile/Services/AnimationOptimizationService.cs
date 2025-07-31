using System.Diagnostics;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio para optimización de animaciones - V4
/// </summary>
public class AnimationOptimizationService
{
    private static AnimationOptimizationService _instance;
    private static readonly object _lock = new object();

    public static AnimationOptimizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AnimationOptimizationService();
                    }
                }
            }
            return _instance;
        }
    }

    private AnimationOptimizationService()
    {
        // Configurar preferencias de animación basadas en el dispositivo
        ConfigureAnimationPreferences();
    }

    /// <summary>
    /// Configura las preferencias de animación según el dispositivo
    /// </summary>
    private void ConfigureAnimationPreferences()
    {
        try
        {
            // Verificar si las animaciones están habilitadas en el sistema
            var animationsEnabled = Preferences.Get("AnimationsEnabled", true);
            
            if (!animationsEnabled)
            {
                Debug.WriteLine("Animations disabled by user preference");
                return;
            }

            // Configurar calidad de animación según el dispositivo
            var animationQuality = GetOptimalAnimationQuality();
            Preferences.Set("AnimationQuality", animationQuality.ToString());
            
            Debug.WriteLine($"Animation quality set to: {animationQuality}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error configuring animation preferences: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la calidad óptima de animación para el dispositivo
    /// </summary>
    private AnimationQuality GetOptimalAnimationQuality()
    {
        try
        {
            // Detectar capacidades del dispositivo
            var deviceInfo = DeviceInfo.Current;
            
            // Dispositivos de gama alta pueden manejar animaciones más complejas
            if (deviceInfo.Platform == DevicePlatform.iOS)
            {
                // iOS generalmente tiene buen rendimiento de animaciones
                return AnimationQuality.High;
            }
            else if (deviceInfo.Platform == DevicePlatform.Android)
            {
                // Android puede variar según el dispositivo
                return AnimationQuality.Medium;
            }
            else
            {
                // Otros dispositivos
                return AnimationQuality.Low;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error detecting device capabilities: {ex.Message}");
            return AnimationQuality.Medium;
        }
    }

    /// <summary>
    /// Ejecuta una animación optimizada
    /// </summary>
    public async Task RunOptimizedAnimationAsync(VisualElement element, AnimationType animationType, uint duration = 250)
    {
        try
        {
            if (!IsAnimationEnabled())
            {
                return;
            }

            var quality = GetCurrentAnimationQuality();
            var optimizedDuration = GetOptimizedDuration(duration, quality);

            switch (animationType)
            {
                case AnimationType.FadeIn:
                    await element.FadeTo(1, optimizedDuration);
                    break;
                case AnimationType.FadeOut:
                    await element.FadeTo(0, optimizedDuration);
                    break;
                case AnimationType.ScaleIn:
                    await element.ScaleTo(1, optimizedDuration, Easing.SpringOut);
                    break;
                case AnimationType.ScaleOut:
                    await element.ScaleTo(0, optimizedDuration, Easing.SpringIn);
                    break;
                case AnimationType.SlideInFromRight:
                    await element.TranslateTo(0, 0, optimizedDuration, Easing.CubicOut);
                    break;
                case AnimationType.SlideOutToRight:
                    await element.TranslateTo(element.Width, 0, optimizedDuration, Easing.CubicIn);
                    break;
                case AnimationType.Bounce:
                    await element.ScaleTo(1.1, optimizedDuration / 2, Easing.BounceOut);
                    await element.ScaleTo(1, optimizedDuration / 2, Easing.BounceOut);
                    break;
                default:
                    Debug.WriteLine($"Unknown animation type: {animationType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running optimized animation: {ex.Message}");
        }
    }

    /// <summary>
    /// Ejecuta una animación de entrada optimizada
    /// </summary>
    public async Task RunEntranceAnimationAsync(VisualElement element, uint delay = 0)
    {
        try
        {
            if (!IsAnimationEnabled())
            {
                element.Opacity = 1;
                element.Scale = 1;
                return;
            }

            // Configurar estado inicial
            element.Opacity = 0;
            element.Scale = 0.8;

            // Esperar delay si se especifica
            if (delay > 0)
            {
                await Task.Delay((int)delay);
            }

            // Ejecutar animación de entrada
            var quality = GetCurrentAnimationQuality();
            var duration = GetOptimizedDuration(300, quality);

            await Task.WhenAll(
                element.FadeTo(1, duration, Easing.CubicOut),
                element.ScaleTo(1, duration, Easing.SpringOut)
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running entrance animation: {ex.Message}");
        }
    }

    /// <summary>
    /// Ejecuta una animación de salida optimizada
    /// </summary>
    public async Task RunExitAnimationAsync(VisualElement element)
    {
        try
        {
            if (!IsAnimationEnabled())
            {
                element.Opacity = 0;
                element.Scale = 0.8;
                return;
            }

            var quality = GetCurrentAnimationQuality();
            var duration = GetOptimizedDuration(200, quality);

            await Task.WhenAll(
                element.FadeTo(0, duration, Easing.CubicIn),
                element.ScaleTo(0.8, duration, Easing.CubicIn)
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running exit animation: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si las animaciones están habilitadas
    /// </summary>
    private bool IsAnimationEnabled()
    {
        return Preferences.Get("AnimationsEnabled", true);
    }

    /// <summary>
    /// Obtiene la calidad actual de animación
    /// </summary>
    private AnimationQuality GetCurrentAnimationQuality()
    {
        var qualityString = Preferences.Get("AnimationQuality", AnimationQuality.Medium.ToString());
        if (Enum.TryParse<AnimationQuality>(qualityString, out var quality))
        {
            return quality;
        }
        return AnimationQuality.Medium;
    }

    /// <summary>
    /// Obtiene la duración optimizada según la calidad
    /// </summary>
    private uint GetOptimizedDuration(uint baseDuration, AnimationQuality quality)
    {
        return quality switch
        {
            AnimationQuality.High => baseDuration,
            AnimationQuality.Medium => (uint)(baseDuration * 0.8),
            AnimationQuality.Low => (uint)(baseDuration * 0.6),
            _ => baseDuration
        };
    }

    /// <summary>
    /// Habilita o deshabilita las animaciones
    /// </summary>
    public void SetAnimationsEnabled(bool enabled)
    {
        Preferences.Set("AnimationsEnabled", enabled);
        Debug.WriteLine($"Animations {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Establece la calidad de animación
    /// </summary>
    public void SetAnimationQuality(AnimationQuality quality)
    {
        Preferences.Set("AnimationQuality", quality.ToString());
        Debug.WriteLine($"Animation quality set to: {quality}");
    }
}

/// <summary>
/// Tipos de animación disponibles
/// </summary>
public enum AnimationType
{
    FadeIn,
    FadeOut,
    ScaleIn,
    ScaleOut,
    SlideInFromRight,
    SlideOutToRight,
    Bounce
}

/// <summary>
/// Calidad de animación
/// </summary>
public enum AnimationQuality
{
    Low,    // Animaciones simples y rápidas
    Medium, // Animaciones balanceadas
    High    // Animaciones complejas y fluidas
} 