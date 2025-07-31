using RestaurantePro.Mobile.ViewModels;
using RestaurantePro.Mobile.Controls;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de configuración moderna - V4
/// </summary>
public partial class ModernConfiguracionPage : ContentPage
{
    private readonly ConfiguracionViewModel _viewModel;

    public ModernConfiguracionPage()
    {
        InitializeComponent();
        _viewModel = new ConfiguracionViewModel();
        BindingContext = _viewModel;
        
        SetupBottomNavigation();
        SetupFilterTabs();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        OnPageLoaded();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.Dispose();
    }

    private void SetupBottomNavigation()
    {
        var tabs = new ObservableCollection<TabItem>
        {
            new TabItem("Dashboard", "🏠", "dashboard"),
            new TabItem("Mesas", "🪑", "tables"),
            new TabItem("Comandas", "🍽️", "orders"),
            new TabItem("Configuración", "⚙️", "settings"),
            new TabItem("Perfil", "👤", "profile")
        };

        BottomTabBar.Tabs = tabs;
        BottomTabBar.SelectedIndex = 3; // Seleccionar tab de Configuración
        BottomTabBar.TabSelected += (sender, tabIndex) => OnBottomTabSelected(sender, tabIndex);
    }

    private void SetupFilterTabs()
    {
        var filterTabs = new ObservableCollection<string>
        {
            "General",
            "Tema",
            "Notificaciones",
            "Performance",
            "Accesibilidad"
        };

        FilterTabs.Tabs = filterTabs;
        FilterTabs.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
    }

    private void OnPageLoaded()
    {
        // Cargar configuraciones guardadas
        LoadSavedSettings();
    }

    private void OnBottomTabSelected(object sender, int tabIndex)
    {
        switch (tabIndex)
        {
            case 0: // Dashboard
                Shell.Current.GoToAsync("//dashboard");
                break;
            case 1: // Mesas
                Shell.Current.GoToAsync("//mesas");
                break;
            case 2: // Comandas
                Shell.Current.GoToAsync("//comandas");
                break;
            case 3: // Configuración
                // Ya estamos en configuración
                break;
            case 4: // Perfil
                Shell.Current.GoToAsync("//perfil");
                break;
        }
    }

    private void OnFilterTabSelected(int selectedIndex)
    {
        // Implementar filtrado por categorías de configuración
        var filterTabs = new[] { "General", "Tema", "Notificaciones", "Performance", "Accesibilidad" };
        var selectedTab = filterTabs[selectedIndex];
        System.Diagnostics.Debug.WriteLine($"Filtro seleccionado: {selectedTab}");
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