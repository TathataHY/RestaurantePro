using RestaurantePro.Mobile.Core.Features.Commercial.Customers.ViewModels;
using RestaurantePro.Mobile.Controls;

namespace RestaurantePro.Mobile.Features.Commercial.Customers.Pages;

/// <summary>
/// Página moderna para gestión de clientes - V4
/// </summary>
public partial class ModernClientesPage : ContentPage
{
    private readonly ClientesViewModel _viewModel;

    public ModernClientesPage(ClientesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar tabs de filtro
        ConfigureFilterTabs();
        
        // Configurar bottom navigation
        ConfigureBottomNavigation();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarClientesCommand.ExecuteAsync(null);
    }

    private void ConfigureFilterTabs()
    {
        // Configurar tabs de filtro para clientes
        FilterTabNavigation.Tabs = new ObservableCollection<string> { "Todos", "Activos", "VIP", "Nuevos" };
        FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabChanged);
    }

    private void ConfigureBottomNavigation()
    {
        // Configurar navegación inferior
        var tabs = new ObservableCollection<TabItem>
        {
            new TabItem("Dashboard", "home.png"),
            new TabItem("Mesas", "table.png"),
            new TabItem("Comandas", "order.png"),
            new TabItem("Productos", "menu.png"),
            new TabItem("Cocina", "kitchen.png"),
            new TabItem("Reservas", "calendar.png"),
            new TabItem("Facturas", "invoice.png"),
            new TabItem("Clientes", "users.png")
        };
        BottomTabBar.Tabs = tabs;
        BottomTabBar.SelectedIndex = 7; // Clientes es el último tab
        BottomTabBar.TabSelected += OnBottomTabSelected;
    }

    private async void OnFilterTabChanged(int selectedIndex)
    {
        // Aplicar filtro según el tab seleccionado
        switch (selectedIndex)
        {
            case 0: // Todos
                await _viewModel.CargarClientesCommand.ExecuteAsync(null);
                break;
            case 1: // Activos
                await _viewModel.CargarClientesCommand.ExecuteAsync(null);
                break;
            case 2: // VIP
                await _viewModel.CargarClientesCommand.ExecuteAsync(null);
                break;
            case 3: // Nuevos
                await _viewModel.CargarClientesCommand.ExecuteAsync(null);
                break;
        }
    }

    private async void OnBottomTabSelected(object sender, int selectedIndex)
    {
        // Navegar a la página correspondiente
        switch (selectedIndex)
        {
            case 0: // Dashboard
                await Shell.Current.GoToAsync("//dashboard");
                break;
            case 1: // Mesas
                await Shell.Current.GoToAsync("//mesas");
                break;
            case 2: // Comandas
                await Shell.Current.GoToAsync("//comandas");
                break;
            case 3: // Productos
                await Shell.Current.GoToAsync("//productos");
                break;
            case 4: // Cocina
                await Shell.Current.GoToAsync("//preparaciones");
                break;
            case 5: // Reservas
                await Shell.Current.GoToAsync("//reservaciones");
                break;
            case 6: // Facturas
                await Shell.Current.GoToAsync("//facturas");
                break;
            case 7: // Clientes (actual)
                // Ya estamos aquí
                break;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar eventos
        if (BottomTabBar != null)
            BottomTabBar.TabSelected -= OnBottomTabSelected;
    }
} 