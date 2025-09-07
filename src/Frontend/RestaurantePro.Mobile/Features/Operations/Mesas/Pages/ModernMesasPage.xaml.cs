using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.Pages;

/// <summary>
/// Página moderna de gestión de mesas - V4 Modernización Visual
/// </summary>
public partial class ModernMesasPage : ContentPage
{
    private MesasViewModel _viewModel;

    public ModernMesasPage(MesasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar animaciones de entrada
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Animaciones de entrada simples
        await Task.Delay(100);
        
        // Animar la página completa
        await this.FadeTo(1, 500);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        System.Diagnostics.Debug.WriteLine("🔍 ModernMesasPage.OnAppearing - Iniciando");
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            System.Diagnostics.Debug.WriteLine("🔍 Ejecutando LoadMesasCommand");
            _viewModel.LoadMesasCommand?.Execute(null);
            
            // Solo cargar estadísticas en background, sin mostrar diálogo
            System.Diagnostics.Debug.WriteLine("🔍 Cargando estadísticas en background");
            _ = _viewModel.LoadEstadisticasAsync();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("❌ _viewModel es null");
        }
    }

    // Paginación incremental del CollectionView
    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_viewModel == null) return;
        // Scroll infinito: anexar siguiente página del buffer local
        _viewModel.LoadMoreMesasCommand?.Execute(null);
    }

    private void OnEstadoFilterChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && _viewModel != null)
        {
            _viewModel.FiltroEstado = picker.SelectedItem?.ToString() ?? string.Empty;
            _viewModel.LoadMesasCommand?.Execute(null);
        }
    }

    private void OnCapacidadFilterChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && _viewModel != null)
        {
            var capacidadStr = picker.SelectedItem?.ToString() ?? string.Empty;
            _viewModel.FiltroCapacidad = capacidadStr;
            _viewModel.FiltroCapacidadMinima = capacidadStr switch
            {
                "2 personas" => 2,
                "4 personas" => 4,
                "6 personas" => 6,
                "8+ personas" => 8,
                _ => null
            };
            _viewModel.LoadMesasCommand?.Execute(null);
        }
    }
} 
