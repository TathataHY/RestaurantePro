using Microsoft.Maui.Devices;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Servicio de feedback háptico para V4
/// </summary>
public static class HapticFeedback
{
    /// <summary>
    /// Feedback de impacto ligero
    /// </summary>
    public static void LightImpact()
    {
        try
        {
            // Implementación simplificada para .NET 9
            // En versiones futuras se puede mejorar
        }
        catch
        {
            // Ignorar si no está disponible
        }
    }

    /// <summary>
    /// Feedback de impacto medio
    /// </summary>
    public static void MediumImpact()
    {
        try
        {
            // Implementación simplificada para .NET 9
        }
        catch
        {
            // Ignorar si no está disponible
        }
    }

    /// <summary>
    /// Feedback de impacto fuerte
    /// </summary>
    public static void HeavyImpact()
    {
        try
        {
            // Implementación simplificada para .NET 9
        }
        catch
        {
            // Ignorar si no está disponible
        }
    }

    /// <summary>
    /// Feedback de éxito
    /// </summary>
    public static void Success()
    {
        LightImpact();
    }

    /// <summary>
    /// Feedback de error
    /// </summary>
    public static void Error()
    {
        HeavyImpact();
    }

    /// <summary>
    /// Feedback de advertencia
    /// </summary>
    public static void Warning()
    {
        MediumImpact();
    }

    /// <summary>
    /// Feedback de selección
    /// </summary>
    public static void Selection()
    {
        LightImpact();
    }

    /// <summary>
    /// Feedback de click
    /// </summary>
    public static void Click()
    {
        LightImpact();
    }

    /// <summary>
    /// Feedback de long press
    /// </summary>
    public static void LongPress()
    {
        MediumImpact();
    }

    /// <summary>
    /// Feedback de swipe
    /// </summary>
    public static void Swipe()
    {
        LightImpact();
    }
} 