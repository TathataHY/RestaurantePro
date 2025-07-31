using RestaurantePro.Mobile.ViewModels;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de configuración - V4
/// </summary>
public partial class ConfiguracionPage : ContentPage
{
    private readonly ConfiguracionViewModel _viewModel;

    public ConfiguracionPage()
    {
        InitializeComponent();
        _viewModel = new ConfiguracionViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Cargar configuraciones guardadas
        LoadSavedSettings();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos
        _viewModel?.Dispose();
    }

    private void LoadSavedSettings()
    {
        try
        {
            // Las configuraciones se cargan automáticamente en el ViewModel
            // Aquí podríamos agregar lógica adicional si es necesario
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }
} 