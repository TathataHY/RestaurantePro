using Microsoft.Maui.Animations;

namespace RestaurantePro.Mobile.Animations;

/// <summary>
/// Sistema de animaciones de transición para V4
/// </summary>
public static class TransitionAnimations
{
    /// <summary>
    /// Animación de fade in para elementos
    /// </summary>
    public static async Task FadeInAsync(this VisualElement element, uint duration = 300)
    {
        element.Opacity = 0;
        await element.FadeTo(1, duration, Easing.CubicOut);
    }

    /// <summary>
    /// Animación de fade out para elementos
    /// </summary>
    public static async Task FadeOutAsync(this VisualElement element, uint duration = 300)
    {
        await element.FadeTo(0, duration, Easing.CubicIn);
    }

    /// <summary>
    /// Animación de slide in desde la derecha
    /// </summary>
    public static async Task SlideInFromRightAsync(this VisualElement element, uint duration = 400)
    {
        var originalTranslationX = element.TranslationX;
        element.TranslationX = 200;
        element.Opacity = 0;
        
        await Task.WhenAll(
            element.TranslateTo(originalTranslationX, element.TranslationY, duration, Easing.CubicOut),
            element.FadeTo(1, duration, Easing.CubicOut)
        );
    }

    /// <summary>
    /// Animación de slide in desde abajo
    /// </summary>
    public static async Task SlideInFromBottomAsync(this VisualElement element, uint duration = 400)
    {
        var originalTranslationY = element.TranslationY;
        element.TranslationY = 100;
        element.Opacity = 0;
        
        await Task.WhenAll(
            element.TranslateTo(element.TranslationX, originalTranslationY, duration, Easing.CubicOut),
            element.FadeTo(1, duration, Easing.CubicOut)
        );
    }

    /// <summary>
    /// Animación de escala con bounce
    /// </summary>
    public static async Task ScaleInWithBounceAsync(this VisualElement element, uint duration = 500)
    {
        element.Scale = 0;
        element.Opacity = 0;
        
        await Task.WhenAll(
            element.ScaleTo(1.1, (uint)(duration * 0.6), Easing.CubicOut),
            element.FadeTo(1, (uint)(duration * 0.6), Easing.CubicOut)
        );
        
        await element.ScaleTo(1, (uint)(duration * 0.4), Easing.BounceOut);
    }

    /// <summary>
    /// Animación de pulse para elementos importantes
    /// </summary>
    public static async Task PulseAsync(this VisualElement element, uint duration = 1000)
    {
        var originalScale = element.Scale;
        
        await element.ScaleTo(originalScale * 1.05, (uint)(duration / 2), Easing.CubicOut);
        await element.ScaleTo(originalScale, (uint)(duration / 2), Easing.CubicIn);
    }

    /// <summary>
    /// Animación de shake para errores
    /// </summary>
    public static async Task ShakeAsync(this VisualElement element, uint duration = 500)
    {
        var originalTranslationX = element.TranslationX;
        var shakeDistance = 10;
        
        for (int i = 0; i < 3; i++)
        {
            await element.TranslateTo(originalTranslationX + shakeDistance, element.TranslationY, (uint)(duration / 6), Easing.Linear);
            await element.TranslateTo(originalTranslationX - shakeDistance, element.TranslationY, (uint)(duration / 6), Easing.Linear);
        }
        
        await element.TranslateTo(originalTranslationX, element.TranslationY, (uint)(duration / 6), Easing.Linear);
    }

    /// <summary>
    /// Animación de success con checkmark
    /// </summary>
    public static async Task SuccessAnimationAsync(this VisualElement element, uint duration = 800)
    {
        var originalScale = element.Scale;
        var originalBackgroundColor = element.BackgroundColor;
        
        // Cambiar color a verde
        element.BackgroundColor = Colors.Green;
        await element.ScaleTo(originalScale * 1.2, (uint)(duration / 2), Easing.CubicOut);
        await element.ScaleTo(originalScale, (uint)(duration / 2), Easing.CubicIn);
        
        // Restaurar color original
        element.BackgroundColor = originalBackgroundColor;
    }

    /// <summary>
    /// Animación de loading con rotación
    /// </summary>
    public static async Task RotateLoadingAsync(this VisualElement element, uint duration = 1000)
    {
        await element.RotateTo(360, duration, Easing.Linear);
    }

    /// <summary>
    /// Animación de entrada para cards en lista
    /// </summary>
    public static async Task StaggeredCardAnimationAsync(this IEnumerable<VisualElement> elements, uint delay = 100, uint duration = 400)
    {
        var tasks = new List<Task>();
        var index = 0;
        
        foreach (var element in elements)
        {
            var task = Task.Delay((int)(delay * index))
                .ContinueWith(async _ => await element.ScaleInWithBounceAsync(duration));
            tasks.Add(task);
            index++;
        }
        
        await Task.WhenAll(tasks);
    }
} 