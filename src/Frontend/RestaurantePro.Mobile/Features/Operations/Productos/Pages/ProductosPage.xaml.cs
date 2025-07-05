using RestaurantePro.Mobile.Features.Operations.Productos.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Productos.Pages;

/// <summary>
/// Página para gestión de productos del menú
/// </summary>
public partial class ProductosPage : ContentPage
{
    private readonly ProductosViewModel _viewModel;

    public ProductosPage(ProductosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Se ejecuta cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Se ejecuta cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        _viewModel.Cleanup();
        base.OnDisappearing();
    }

    /// <summary>
    /// Manejar navegación con parámetros
    /// </summary>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // Aquí se pueden manejar parámetros de navegación si es necesario
        // Por ejemplo, si se navega desde otra página con un filtro específico
    }
} 