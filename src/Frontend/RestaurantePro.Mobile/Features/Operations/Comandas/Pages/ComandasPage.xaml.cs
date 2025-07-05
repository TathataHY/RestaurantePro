using RestaurantePro.Mobile.Features.Operations.Comandas.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

/// <summary>
/// Página para gestión de comandas del restaurante
/// </summary>
public partial class ComandasPage : ContentPage
{
    /// <summary>
    /// ViewModel asociado a esta página
    /// </summary>
    public ComandasViewModel ViewModel => (ComandasViewModel)BindingContext;

    /// <summary>
    /// Constructor de la página
    /// </summary>
    public ComandasPage(ComandasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
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
            "Filtrar por Fecha",
            "Filtrar por Mesa",
            "Limpiar Filtros");

        switch (action)
        {
            case "Filtrar por Estado":
                await MostrarFiltroEstado();
                break;
            case "Filtrar por Fecha":
                await MostrarFiltroFecha();
                break;
            case "Filtrar por Mesa":
                await MostrarFiltroMesa();
                break;
            case "Limpiar Filtros":
                await ViewModel.ClearFiltersCommand.ExecuteAsync(null);
                break;
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por estado
    /// </summary>
    private async Task MostrarFiltroEstado()
    {
        var estados = ViewModel.EstadosDisponibles.ToArray();
        
        var selectedEstado = await DisplayActionSheet(
            "Filtrar por Estado",
            "Cancelar",
            null,
            estados);

        if (selectedEstado != null && selectedEstado != "Cancelar")
        {
            ViewModel.FiltroEstado = selectedEstado == "Todos" ? string.Empty : selectedEstado;
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por fecha
    /// </summary>
    private async Task MostrarFiltroFecha()
    {
        var fechas = new[] { "Todas", "Hoy", "Ayer", "Última semana", "Último mes" };
        
        var selectedFecha = await DisplayActionSheet(
            "Filtrar por Fecha",
            "Cancelar",
            null,
            fechas);

        if (selectedFecha != null && selectedFecha != "Cancelar")
        {
            ViewModel.FiltroFecha = selectedFecha switch
            {
                "Hoy" => DateTime.Today,
                "Ayer" => DateTime.Today.AddDays(-1),
                "Última semana" => DateTime.Today.AddDays(-7),
                "Último mes" => DateTime.Today.AddDays(-30),
                _ => null
            };
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Mostrar selector de filtro por mesa
    /// </summary>
    private async Task MostrarFiltroMesa()
    {
        var mesaIdString = await DisplayPromptAsync(
            "Filtrar por Mesa",
            "Ingrese el ID de la mesa (deje vacío para mostrar todas):",
            "OK",
            "Cancelar",
            "ID de Mesa",
            -1,
            null);

        if (mesaIdString != null)
        {
            if (string.IsNullOrWhiteSpace(mesaIdString))
            {
                ViewModel.FiltroMesaId = null;
            }
            else if (Guid.TryParse(mesaIdString, out var mesaId))
            {
                ViewModel.FiltroMesaId = mesaId;
            }
            else
            {
                await DisplayAlert("Error", "ID de mesa inválido", "OK");
                return;
            }
            
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Evento cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Cargar datos si es necesario
        if (ViewModel.Comandas.Count == 0)
        {
            await ViewModel.LoadComandasCommand.ExecuteAsync(null);
        }
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