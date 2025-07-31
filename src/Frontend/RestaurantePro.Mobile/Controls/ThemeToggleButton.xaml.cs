using RestaurantePro.Mobile.Services;
using RestaurantePro.Mobile.Animations;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Botón de cambio de tema con animaciones - V4
/// </summary>
public partial class ThemeToggleButton : ContentView
{
    public static readonly BindableProperty IsDarkThemeProperty =
        BindableProperty.Create(nameof(IsDarkTheme), typeof(bool), typeof(ThemeToggleButton), false, propertyChanged: OnIsDarkThemeChanged);

    public bool IsDarkTheme
    {
        get => (bool)GetValue(IsDarkThemeProperty);
        set => SetValue(IsDarkThemeProperty, value);
    }

    public event EventHandler<bool> ThemeChanged;

    private bool _isAnimating = false;

    public ThemeToggleButton()
    {
        InitializeComponent();
        
        // Configurar tap gesture
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnThemeToggleTapped;
        GestureRecognizers.Add(tapGesture);
        
        // Inicializar estado
        UpdateThemeVisuals();
    }

    private static void OnIsDarkThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemeToggleButton toggleButton)
        {
            toggleButton.UpdateThemeVisuals();
        }
    }

    private async void OnThemeToggleTapped(object sender, EventArgs e)
    {
        if (_isAnimating) return;

        _isAnimating = true;

        try
        {
            // Feedback háptico
            HapticFeedback.Selection();

            // Cambiar tema
            IsDarkTheme = !IsDarkTheme;
            ThemeChanged?.Invoke(this, IsDarkTheme);

            // Animar cambio
            await AnimateThemeChange();

            // Aplicar tema
            await ApplyTheme();
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task AnimateThemeChange()
    {
        // Animar indicador
        var targetColumn = IsDarkTheme ? 1 : 0;
        var targetX = targetColumn * 30; // 30px por columna

        await ThemeIndicator.TranslateTo(targetX, 0, 300, Easing.CubicOut);

        // Animar iconos
        if (IsDarkTheme)
        {
            // Cambiar a tema oscuro
            await Task.WhenAll(
                SunIcon.FadeTo(0.5, 200, Easing.CubicOut),
                MoonIcon.FadeTo(1.0, 200, Easing.CubicOut)
            );
        }
        else
        {
            // Cambiar a tema claro
            await Task.WhenAll(
                SunIcon.FadeTo(1.0, 200, Easing.CubicOut),
                MoonIcon.FadeTo(0.5, 200, Easing.CubicOut)
            );
        }
    }

    private void UpdateThemeVisuals()
    {
        if (IsDarkTheme)
        {
            // Tema oscuro
            ThemeIndicator.HorizontalOptions = LayoutOptions.End;
            SunIcon.Opacity = 0.5;
            MoonIcon.Opacity = 1.0;
        }
        else
        {
            // Tema claro
            ThemeIndicator.HorizontalOptions = LayoutOptions.Start;
            SunIcon.Opacity = 1.0;
            MoonIcon.Opacity = 0.5;
        }
    }

    private async Task ApplyTheme()
    {
        try
        {
            var themeService = ThemeService.Instance;
            var newTheme = IsDarkTheme ? ThemeService.ThemeMode.Dark : ThemeService.ThemeMode.Light;
            await themeService.SetThemeAsync(newTheme);
        }
        catch (Exception ex)
        {
            // Log error pero no fallar
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
    }
} 