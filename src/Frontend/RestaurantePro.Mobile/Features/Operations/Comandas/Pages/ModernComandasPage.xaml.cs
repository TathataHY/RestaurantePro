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
        if (FilterTabNavigation != null)
        {
            // Configurar tabs para filtros de estado
            FilterTabNavigation.Tabs = new ObservableCollection<string> { "Todas", "Pendientes", "En Progreso", "Completadas" };
            FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabChanged);
        }
    }

    /// <summary>
    /// Configurar bottom tab bar
    /// </summary>
    private void SetupBottomTabBar()
    {
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
    }

    /// <summary>
    /// Configurar floating action button
    /// </summary>
    private void SetupFloatingActionButton()
    {
        if (MainFab != null)
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
    /// Evento cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar eventos para evitar memory leaks
        if (FilterTabNavigation != null)
        {
            FilterTabNavigation.TabSelectedCommand = null;
        }
        
        if (BottomTabBar != null)
        {
            BottomTabBar.TabSelected -= OnBottomTabChanged;
        }
    }
} 