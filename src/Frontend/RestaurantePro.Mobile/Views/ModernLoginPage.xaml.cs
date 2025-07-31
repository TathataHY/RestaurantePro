using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de Login moderna - V4 Modernización Visual
/// </summary>
public partial class ModernLoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public ModernLoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar eventos
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Evento para manejar el botón de "Olvidaste contraseña"
        this.Loaded += (sender, e) =>
        {
            // Aquí se pueden agregar animaciones de entrada
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Verificar estado de autenticación al aparecer
        if (_viewModel.CheckAuthStatusCommand.CanExecute(null))
        {
            await _viewModel.CheckAuthStatusCommand.ExecuteAsync(null);
        }
        
        // Aplicar animaciones de entrada
        await ApplyEntryAnimations();
    }

    private async Task ApplyEntryAnimations()
    {
        // Animación de entrada para el logo
        if (this.FindByName<Border>("LogoBorder") is Border logoBorder)
        {
            await logoBorder.ScaleTo(1.1, 500, Easing.BounceOut);
            await logoBorder.ScaleTo(1.0, 300, Easing.SpringOut);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario
    }
} 