using RestaurantePro.Mobile.ViewModels;
using RestaurantePro.Mobile.Controls;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Views;

/// <summary>
/// Página de perfil moderna - V4
/// </summary>
public partial class ModernPerfilPage : ContentPage
{
    private readonly PerfilViewModel _viewModel;

    public ModernPerfilPage()
    {
        InitializeComponent();
        _viewModel = new PerfilViewModel();
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
        BottomTabBar.SelectedIndex = 4; // Seleccionar tab de Perfil
        BottomTabBar.TabSelected += (sender, tabIndex) => OnBottomTabSelected(sender, tabIndex);
    }

    private void SetupFilterTabs()
    {
        var filterTabs = new ObservableCollection<string>
        {
            "General",
            "Personal",
            "Laboral",
            "Estadísticas",
            "Seguridad"
        };

        FilterTabs.Tabs = filterTabs;
        FilterTabs.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
    }

    private void OnPageLoaded()
    {
        // Cargar datos del perfil
        LoadProfileData();
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
                Shell.Current.GoToAsync("//configuracion");
                break;
            case 4: // Perfil
                // Ya estamos en perfil
                break;
        }
    }

    private void OnFilterTabSelected(int selectedIndex)
    {
        // Implementar filtrado por categorías del perfil
        var filterTabs = new[] { "General", "Personal", "Laboral", "Estadísticas", "Seguridad" };
        var selectedTab = filterTabs[selectedIndex];
        System.Diagnostics.Debug.WriteLine($"Filtro seleccionado: {selectedTab}");
    }

    private void LoadProfileData()
    {
        try
        {
            // Los datos del perfil se cargan automáticamente en el ViewModel
            // Aquí podríamos agregar lógica adicional si es necesario
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile data: {ex.Message}");
        }
    }
} 