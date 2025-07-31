using RestaurantePro.Mobile.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de monitoreo de performance - V4
/// </summary>
public partial class PerformanceMonitorPage : ContentPage
{
    private readonly PerformanceMonitorViewModel _viewModel;

    public PerformanceMonitorPage(PerformanceMonitorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar eventos
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Evento para animaciones de entrada
        this.Loaded += (sender, e) =>
        {
            // Aquí se pueden agregar animaciones de entrada
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Configuración de la página
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Cleanup si es necesario
    }


} 