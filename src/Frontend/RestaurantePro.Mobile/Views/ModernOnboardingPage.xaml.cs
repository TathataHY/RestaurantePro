using RestaurantePro.Mobile.Core.Features.Onboarding.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de Onboarding moderna - V4 Modernización Visual
/// </summary>
public partial class ModernOnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;

    public ModernOnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar eventos
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Evento para manejar cambios en el carrusel
        OnboardingCarousel.PositionChanged += OnCarouselPositionChanged;
        
        // Evento para animaciones de entrada
        this.Loaded += (sender, e) =>
        {
            // Animaciones iniciales
        };
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        // Aplicar animaciones cuando cambia la posición del carrusel
        ApplyCarouselAnimations(e.CurrentPosition);
    }

    private async void ApplyCarouselAnimations(int position)
    {
        // Animación para el icono actual
        if (OnboardingCarousel.CurrentItem is OnboardingItem currentItem)
        {
            // Aquí se pueden agregar animaciones específicas por posición
            await Task.Delay(100); // Pequeña pausa para la animación
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Aplicar animaciones de entrada
        await ApplyEntryAnimations();
    }

    private async Task ApplyEntryAnimations()
    {
        // Animación de entrada para el carrusel
        if (OnboardingCarousel != null)
        {
            await OnboardingCarousel.FadeTo(1, 500, Easing.CubicOut);
        }
        
        // Animación para los indicadores
        if (OnboardingIndicator != null)
        {
            await OnboardingIndicator.ScaleTo(1.1, 300, Easing.BounceOut);
            await OnboardingIndicator.ScaleTo(1.0, 200, Easing.SpringOut);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar eventos
        if (OnboardingCarousel != null)
        {
            OnboardingCarousel.PositionChanged -= OnCarouselPositionChanged;
        }
    }
} 