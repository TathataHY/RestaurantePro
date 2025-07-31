using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Splash Page moderna con animaciones fluidas - V4 Modernización Visual
/// </summary>
public partial class SplashPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public SplashPage(IAuthService authService, INavigationService navigationService)
    {
        InitializeComponent();
        _authService = authService;
        _navigationService = navigationService;
        
        // Configurar animaciones iniciales
        SetupInitialState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Iniciar secuencia de animaciones
        await StartSplashAnimationAsync();
    }

    /// <summary>
    /// Configura el estado inicial de los elementos (ocultos)
    /// </summary>
    private void SetupInitialState()
    {
        // Ocultar elementos inicialmente
        LogoContainer.Opacity = 0;
        LogoContainer.Scale = 0.5f;
        
        AppTitle.Opacity = 0;
        AppTitle.TranslationY = 20;
        
        AppSubtitle.Opacity = 0;
        AppSubtitle.TranslationY = 20;
        
        VersionLabel.Opacity = 0;
        VersionLabel.TranslationY = 20;
        
        LoadingIndicator.Opacity = 0;
        LoadingText.Opacity = 0;
        ProgressBar.Opacity = 0;
        ProgressBar.Progress = 0;
    }

    /// <summary>
    /// Secuencia principal de animaciones del splash
    /// </summary>
    private async Task StartSplashAnimationAsync()
    {
        try
        {
            // 1. Animación del logo (fade in + scale)
            await LogoContainer.FadeTo(1, 800, Easing.CubicOut);
            await LogoContainer.ScaleTo(1, 600, Easing.BounceOut);
            
            // 2. Animación del título (slide up + fade)
            await Task.WhenAll(
                AppTitle.FadeTo(1, 600, Easing.CubicOut),
                AppTitle.TranslateTo(0, 0, 600, Easing.CubicOut)
            );
            
            // 3. Animación del subtítulo
            await Task.Delay(200);
            await Task.WhenAll(
                AppSubtitle.FadeTo(1, 600, Easing.CubicOut),
                AppSubtitle.TranslateTo(0, 0, 600, Easing.CubicOut)
            );
            
            // 4. Animación de la versión
            await Task.Delay(200);
            await Task.WhenAll(
                VersionLabel.FadeTo(1, 600, Easing.CubicOut),
                VersionLabel.TranslateTo(0, 0, 600, Easing.CubicOut)
            );
            
            // 5. Mostrar loading elements
            await Task.Delay(300);
            await Task.WhenAll(
                LoadingIndicator.FadeTo(1, 400, Easing.CubicOut),
                LoadingText.FadeTo(1, 400, Easing.CubicOut),
                ProgressBar.FadeTo(1, 400, Easing.CubicOut)
            );
            
            // 6. Simular progreso de carga
            await SimulateLoadingProgressAsync();
            
            // 7. Verificar autenticación y navegar
            await CheckAuthenticationAndNavigateAsync();
        }
        catch (Exception ex)
        {
            // En caso de error, navegar directamente al login
            await NavigateToLoginAsync();
        }
    }

    /// <summary>
    /// Simula el progreso de carga con animación fluida
    /// </summary>
    private async Task SimulateLoadingProgressAsync()
    {
        var progressSteps = new[] { 0.2, 0.4, 0.6, 0.8, 1.0 };
        var loadingTexts = new[] 
        { 
            "Cargando configuración...",
            "Inicializando servicios...", 
            "Conectando con el servidor...",
            "Preparando interfaz...",
            "¡Listo!"
        };

        for (int i = 0; i < progressSteps.Length; i++)
        {
            // Actualizar texto de carga
            LoadingText.Text = loadingTexts[i];
            
            // Animar progreso
            await ProgressBar.ProgressTo(progressSteps[i], 800, Easing.CubicOut);
            
            // Pausa entre pasos
            await Task.Delay(400);
        }
    }

    /// <summary>
    /// Verifica el estado de autenticación y navega apropiadamente
    /// </summary>
    private async Task CheckAuthenticationAndNavigateAsync()
    {
        try
        {
            // Verificar si el usuario ya está autenticado
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                // Usuario autenticado - ir al dashboard
                await NavigateToDashboardAsync();
            }
            else
            {
                // Usuario no autenticado - ir al login
                await NavigateToLoginAsync();
            }
        }
        catch (Exception ex)
        {
            // En caso de error, ir al login
            await NavigateToLoginAsync();
        }
    }

    /// <summary>
    /// Navega al dashboard principal
    /// </summary>
    private async Task NavigateToDashboardAsync()
    {
        // Animación de salida
        await Task.WhenAll(
            this.FadeTo(0, 500, Easing.CubicIn),
            this.ScaleTo(0.95f, 500, Easing.CubicIn)
        );
        
        // Navegar al dashboard
        await _navigationService.NavigateToAsync("//main/dashboard");
    }

    /// <summary>
    /// Navega a la página de login
    /// </summary>
    private async Task NavigateToLoginAsync()
    {
        // Animación de salida
        await Task.WhenAll(
            this.FadeTo(0, 500, Easing.CubicIn),
            this.ScaleTo(0.95f, 500, Easing.CubicIn)
        );
        
        // Navegar al login
        await _navigationService.NavigateToAsync("//login");
    }
} 