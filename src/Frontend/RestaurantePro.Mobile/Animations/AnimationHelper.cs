namespace RestaurantePro.Mobile.Animations;

public static class AnimationHelper
{
    /// <summary>
    /// Animación de fade in para elementos
    /// </summary>
    public static async Task FadeInAsync(this View view, uint duration = 300)
    {
        view.Opacity = 0;
        await view.FadeTo(1, duration, Easing.CubicOut);
    }

    /// <summary>
    /// Animación de fade out para elementos
    /// </summary>
    public static async Task FadeOutAsync(this View view, uint duration = 300)
    {
        await view.FadeTo(0, duration, Easing.CubicIn);
    }

    /// <summary>
    /// Animación de slide in desde la izquierda
    /// </summary>
    public static async Task SlideInFromLeftAsync(this View view, uint duration = 300)
    {
        view.TranslationX = -view.Width;
        await view.TranslateTo(0, 0, duration, Easing.CubicOut);
    }

    /// <summary>
    /// Animación de slide in desde la derecha
    /// </summary>
    public static async Task SlideInFromRightAsync(this View view, uint duration = 300)
    {
        view.TranslationX = view.Width;
        await view.TranslateTo(0, 0, duration, Easing.CubicOut);
    }

    /// <summary>
    /// Animación de escala con bounce
    /// </summary>
    public static async Task ScaleWithBounceAsync(this View view, double scale = 1.1, uint duration = 200)
    {
        await view.ScaleTo(scale, duration, Easing.BounceOut);
        await view.ScaleTo(1, duration, Easing.BounceOut);
    }

    /// <summary>
    /// Animación de pulso para elementos
    /// </summary>
    public static async Task PulseAsync(this View view, uint duration = 1000)
    {
        var animation = new Microsoft.Maui.Controls.Animation(v => view.Opacity = 0.3 + (0.7 * v), 0, 1);
        animation.Commit(view, "PulseAnimation", 16, duration, Easing.Linear, null, () => true);
    }

    /// <summary>
    /// Animación de shake para errores
    /// </summary>
    public static async Task ShakeAsync(this View view, uint duration = 500)
    {
        var originalX = view.TranslationX;
        var shakeDistance = 10;

        await view.TranslateTo(originalX + shakeDistance, 0, 50, Easing.Linear);
        await view.TranslateTo(originalX - shakeDistance, 0, 100, Easing.Linear);
        await view.TranslateTo(originalX + shakeDistance, 0, 100, Easing.Linear);
        await view.TranslateTo(originalX - shakeDistance, 0, 100, Easing.Linear);
        await view.TranslateTo(originalX, 0, 50, Easing.Linear);
    }

    /// <summary>
    /// Animación de éxito con checkmark
    /// </summary>
    public static async Task SuccessAnimationAsync(this View view, uint duration = 800)
    {
        // Escalar y cambiar color a verde
        await view.ScaleTo(1.2, duration / 2, Easing.CubicOut);
        view.BackgroundColor = Color.FromArgb("#27AE60");
        await view.ScaleTo(1, duration / 2, Easing.CubicIn);
    }

    /// <summary>
    /// Animación de carga con rotación
    /// </summary>
    public static async Task RotateLoadingAsync(this View view, uint duration = 1000)
    {
        var animation = new Microsoft.Maui.Controls.Animation(v => view.Rotation = v * 360, 0, 1);
        animation.Commit(view, "RotateAnimation", 16, duration, Easing.Linear, null, () => true);
    }

    /// <summary>
    /// Animación de entrada para cards
    /// </summary>
    public static async Task CardEntranceAsync(this View view, int delay = 0, uint duration = 400)
    {
        view.Opacity = 0;
        view.Scale = 0.8;
        view.TranslationY = 50;

        await Task.Delay(delay);
        
        await Task.WhenAll(
            view.FadeTo(1, duration, Easing.CubicOut),
            view.ScaleTo(1, duration, Easing.CubicOut),
            view.TranslateTo(0, 0, duration, Easing.CubicOut)
        );
    }

    /// <summary>
    /// Animación de botón presionado
    /// </summary>
    public static async Task ButtonPressAsync(this View view, uint duration = 150)
    {
        await view.ScaleTo(0.95, duration, Easing.CubicOut);
        await view.ScaleTo(1, duration, Easing.CubicOut);
    }
} 