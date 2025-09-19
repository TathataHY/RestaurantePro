using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Mesas.Pages;

/// <summary>
/// Página para gestión de mesas del restaurante
/// </summary>
public partial class MesasPage : ContentPage
{
    /// <summary>
    /// ViewModel asociado a esta página
    /// </summary>
    private readonly MesasViewModel _viewModel;

    /// <summary>
    /// Constructor de la página
    /// </summary>
    public MesasPage(MesasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Evento cuando se hace clic en el botón de filtros
    /// </summary>
    private async void OnFiltrosClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet(
            "Seleccionar Filtro",
            "Cancelar",
            null,
            "Filtrar por Estado",
            "Filtrar por Ubicación",
            "Filtrar por Capacidad",
            "Limpiar Filtros");

        switch (action)
        {
            case "Filtrar por Estado":
                await MostrarFiltroEstado();
                break;
            case "Filtrar por Ubicación":
                await MostrarFiltroUbicacion();
                break;
            case "Filtrar por Capacidad":
                await MostrarFiltroCapacidad();
                break;
            case "Limpiar Filtros":
                await _viewModel.ClearFiltersCommand.ExecuteAsync(null);
                break;
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por estado
    /// </summary>
    private async Task MostrarFiltroEstado()
    {
        var estados = new[] { "Todos", "Disponible", "Ocupada", "Reservada", "Mantenimiento", "Fuera de servicio" };
        
        var selectedEstado = await DisplayActionSheet(
            "Filtrar por Estado",
            "Cancelar",
            null,
            estados);

        if (selectedEstado != null && selectedEstado != "Cancelar")
        {
            _viewModel.FiltroEstado = selectedEstado == "Todos" ? string.Empty : selectedEstado;
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por ubicación
    /// </summary>
    private async Task MostrarFiltroUbicacion()
    {
        var ubicaciones = new[] { "Todas", "Terraza", "Salón Principal", "Área VIP", "Patio", "Área Infantil" };
        
        var selectedUbicacion = await DisplayActionSheet(
            "Filtrar por Ubicación",
            "Cancelar",
            null,
            ubicaciones);

        if (selectedUbicacion != null && selectedUbicacion != "Cancelar")
        {
            _viewModel.FiltroUbicacion = selectedUbicacion == "Todas" ? string.Empty : selectedUbicacion;
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por capacidad
    /// </summary>
    private async Task MostrarFiltroCapacidad()
    {
        var capacidades = new[] { "Todas", "2 personas", "4 personas", "6 personas", "8+ personas" };
        
        var selectedCapacidad = await DisplayActionSheet(
            "Filtrar por Capacidad",
            "Cancelar",
            null,
            capacidades);

        if (selectedCapacidad != null && selectedCapacidad != "Cancelar")
        {
            _viewModel.FiltroCapacidadMinima = selectedCapacidad switch
            {
                "2 personas" => 2,
                "4 personas" => 4,
                "6 personas" => 6,
                "8+ personas" => 8,
                _ => null
            };
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Evento cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        System.Diagnostics.Debug.WriteLine("🔄 MesasPage.OnAppearing() - Verificando estado de datos");
        
        // Solo cargar datos si no hay mesas cargadas
        if (_viewModel.Mesas.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("🔄 No hay mesas cargadas, ejecutando LoadMesasCommand");
            
            if (_viewModel.LoadMesasCommand.CanExecute(null))
            {
                await _viewModel.LoadMesasCommand.ExecuteAsync(null);
            }
            
            if (_viewModel.LoadEstadisticasCommand.CanExecute(null))
            {
                System.Diagnostics.Debug.WriteLine("🔄 Ejecutando LoadEstadisticasCommand");
                await _viewModel.LoadEstadisticasCommand.ExecuteAsync(null);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"✅ Mesas ya cargadas ({_viewModel.Mesas.Count} mesas), no es necesario recargar");
        }
        
        System.Diagnostics.Debug.WriteLine("✅ MesasPage.OnAppearing() - Proceso completado");
    }

    /// <summary>
    /// Evento cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario
        SearchBar.Text = string.Empty;
    }
} 