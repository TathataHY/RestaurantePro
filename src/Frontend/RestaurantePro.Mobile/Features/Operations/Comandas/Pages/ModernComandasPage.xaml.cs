using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Controls;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

/// <summary>
/// Página moderna para gestión de comandas del restaurante - V4
/// </summary>
public partial class ModernComandasPage : ContentPage
{
    private readonly ComandasViewModel _viewModel;

    /// <summary>
    /// ViewModel asociado a esta página
    /// </summary>
    public ComandasViewModel ViewModel => _viewModel;

    /// <summary>
    /// Constructor de la página
    /// </summary>
    public ModernComandasPage(ComandasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar componentes V4
        SetupV4Components();
    }

    /// <summary>
    /// Configurar componentes específicos de V4
    /// </summary>
    private void SetupV4Components()
    {
        // Configurar Tab Navigation con filtros
        SetupFilterTabs();
        
        // Configurar Bottom Tab Bar
        SetupBottomTabBar();
        
        // Configurar Floating Action Button
        SetupFloatingActionButton();
    }


    /// <summary>
    /// Configurar tabs de filtro
    /// </summary>
    private void SetupFilterTabs()
    {
        var filterTabs = this.FindByName<TabNavigation>("FilterTabNavigation");
        if (filterTabs != null)
        {
            // Configurar tabs para filtros de estado
            filterTabs.Tabs = new ObservableCollection<string> { "Todas", "Pendientes", "En Progreso", "Completadas" };
            filterTabs.TabSelectedCommand = new Command<int>(OnFilterTabChanged);
        }
    }

    /// <summary>
    /// Configurar bottom tab bar
    /// </summary>
    private void SetupBottomTabBar()
    {
        // COMENTADO PARA USAR TABS NATIVOS
        /*
        if (BottomTabBar != null)
        {
            // Configurar tabs de navegación inferior
            var tabs = new ObservableCollection<TabItem>
            {
                new TabItem("Comandas", "🍽️"),
                new TabItem("Productos", "📦"),
                new TabItem("Mesas", "🪑"),
                new TabItem("Config", "⚙️")
            };
            BottomTabBar.Tabs = tabs;
            BottomTabBar.TabSelected += OnBottomTabChanged;
        }
        */
    }

    /// <summary>
    /// Configurar floating action button
    /// </summary>
    private void SetupFloatingActionButton()
    {
        var mainFab = this.FindByName<FloatingActionButton>("MainFab");
        if (mainFab != null)
        {
            // El FAB ya está configurado en XAML con el comando CrearComandaCommand
            // Aquí podríamos agregar configuración adicional si es necesario
        }
    }

    /// <summary>
    /// Evento cuando cambia el tab de filtro
    /// </summary>
    private async void OnFilterTabChanged(int selectedIndex)
    {
        // Aplicar filtro según el tab seleccionado
        switch (selectedIndex)
        {
            case 0: // Todas
                _viewModel.FiltroEstado = string.Empty;
                break;
            case 1: // Pendientes
                _viewModel.FiltroEstado = "Pendiente";
                break;
            case 2: // En Progreso
                _viewModel.FiltroEstado = "En Progreso";
                break;
            case 3: // Completadas
                _viewModel.FiltroEstado = "Completada";
                break;
        }

        if (_viewModel.ApplyFiltersCommand.CanExecute(null))
        {
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Evento cuando cambia el tab inferior
    /// </summary>
    private async void OnBottomTabChanged(object sender, int selectedIndex)
    {
        // Navegar a la página correspondiente
        switch (selectedIndex)
        {
            case 0: // Comandas
                // Ya estamos en esta página
                break;
            case 1: // Productos
                await Shell.Current.GoToAsync("//productos");
                break;
            case 2: // Mesas
                await Shell.Current.GoToAsync("//mesas");
                break;
            case 3: // Config
                await Shell.Current.GoToAsync("//configuracion");
                break;
        }
    }

    /// <summary>
    /// Evento cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Cargar datos al aparecer la página
        if (_viewModel.LoadComandasCommand.CanExecute(null))
        {
            await _viewModel.LoadComandasCommand.ExecuteAsync(null);
        }
        
        // Cargar estadísticas
        if (_viewModel.LoadEstadisticasCommand.CanExecute(null))
        {
            await _viewModel.LoadEstadisticasCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Mostrar selector rápido de rango de fecha
    /// </summary>
    private async void OnFiltroFechaClicked(object sender, EventArgs e)
    {
        var opciones = new[] { "Todas", "Hoy", "Ayer", "Última semana", "Último mes" };
        var seleccion = await DisplayActionSheet("Filtrar por Fecha", "Cancelar", null, opciones);
        if (string.IsNullOrWhiteSpace(seleccion) || seleccion == "Cancelar") return;

        _viewModel.FiltroFecha = seleccion switch
        {
            "Hoy" => DateTime.Today,
            "Ayer" => DateTime.Today.AddDays(-1),
            "Última semana" => DateTime.Today.AddDays(-7),
            "Último mes" => DateTime.Today.AddDays(-30),
            _ => null
        };

        if (_viewModel.ApplyFiltersCommand.CanExecute(null))
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Mostrar selector de mesa (por ahora por ID simple)
    /// </summary>
    private async void OnFiltroMesaClicked(object sender, EventArgs e)
    {
        var texto = await DisplayPromptAsync("Filtrar por Mesa", "Ingrese el ID de la mesa (vacío = todas)", "OK", "Cancelar", "ID de Mesa");
        if (texto == null) return;

        if (string.IsNullOrWhiteSpace(texto))
        {
            _viewModel.FiltroMesaId = null;
        }
        else if (Guid.TryParse(texto, out var mesaId))
        {
            _viewModel.FiltroMesaId = mesaId;
        }
        else
        {
            await DisplayAlert("Error", "ID de mesa inválido", "OK");
            return;
        }

        if (_viewModel.ApplyFiltersCommand.CanExecute(null))
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Evento cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar eventos para evitar memory leaks
        var filterTabs = this.FindByName<TabNavigation>("FilterTabNavigation");
        if (filterTabs != null)
        {
            filterTabs.TabSelectedCommand = null;
        }
        
        // COMENTADO PARA USAR TABS NATIVOS
        /*
        if (BottomTabBar != null)
        {
            BottomTabBar.TabSelected -= OnBottomTabChanged;
        }
        */
    }

    private async void OnMasOpcionesClicked(object sender, EventArgs e)
    {
        var accion = await DisplayActionSheet(
            "Acciones",
            "Cerrar",
            null,
            "Limpiar filtros",
            "Recargar",
            "Estadísticas");

        switch (accion)
        {
            case "Limpiar filtros":
                if (_viewModel.ClearFiltersCommand.CanExecute(null))
                    await _viewModel.ClearFiltersCommand.ExecuteAsync(null);
                break;
            case "Recargar":
                if (_viewModel.RefreshComandasCommand.CanExecute(null))
                    await _viewModel.RefreshComandasCommand.ExecuteAsync(null);
                break;
            case "Estadísticas":
                if (_viewModel.LoadEstadisticasCommand.CanExecute(null))
                    await _viewModel.LoadEstadisticasCommand.ExecuteAsync(null);
                break;
            default:
                break;
        }
    }
} 