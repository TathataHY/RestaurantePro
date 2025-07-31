using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.Inventory.Reservaciones.ViewModels;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Features.Inventory.Reservaciones.Pages;

/// <summary>
/// Página moderna de gestión de reservaciones - V4 Modernización Visual
/// </summary>
public partial class ModernReservacionesPage : ContentPage
{
    private ReservacionesViewModel _viewModel;

    public ModernReservacionesPage(ReservacionesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        SetupBottomNavigation();
        SetupFilterTabs();
        
        // Configurar animaciones de entrada
        Loaded += OnPageLoaded;
    }

    private void SetupBottomNavigation()
    {
        var tabs = new ObservableCollection<TabItem>
        {
            new TabItem("Dashboard", "🏠", "dashboard"),
            new TabItem("Mesas", "🪑", "tables"),
            new TabItem("Comandas", "🍽️", "orders"),
            new TabItem("Reservaciones", "📅", "reservations"),
            new TabItem("Perfil", "👤", "profile")
        };

        BottomTabBar.Tabs = tabs;
        BottomTabBar.SelectedIndex = 3; // Seleccionar tab de Reservaciones
        BottomTabBar.TabSelected += OnBottomTabSelected;
    }

    private void SetupFilterTabs()
    {
        FilterTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Todas",
            "Confirmadas",
            "Pendientes",
            "Canceladas"
        };

        FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Animaciones de entrada
        await Task.Delay(100);
        
        // Animar cards de estadísticas
        var statCards = this.FindByName<ScrollView>("StatCardsScrollView")?.Content as HorizontalStackLayout;
        if (statCards != null)
        {
            for (int i = 0; i < statCards.Children.Count; i++)
            {
                if (statCards.Children[i] is View view)
                {
                    await view.FadeTo(1, 300);
                    await Task.Delay(100);
                }
            }
        }

        // Animar FAB
        if (MainFab != null)
        {
            await MainFab.ScaleTo(1, 400, Easing.BounceOut);
        }
    }

    private async void OnBottomTabSelected(object sender, int tabIndex)
    {
        // Animación de transición
        await this.FadeTo(0, 150);
        
        // Aquí implementarías la navegación real
        var selectedTab = BottomTabBar.Tabs[tabIndex];
        await Application.Current.MainPage.DisplayAlert("Navegación", $"Navegando a: {selectedTab.Title}", "OK");
        
        await this.FadeTo(1, 150);
    }

    private async void OnFilterTabSelected(int tabIndex)
    {
        // Filtrar reservaciones según el tab seleccionado
        var tabName = FilterTabNavigation.Tabs[tabIndex];
        
        switch (tabIndex)
        {
            case 0: // Todas
                _viewModel.CargarReservacionesCommand.Execute(null);
                break;
            case 1: // Confirmadas
                // Implementar filtro de confirmadas
                await Application.Current.MainPage.DisplayAlert("Filtro", "Mostrando reservaciones confirmadas", "OK");
                break;
            case 2: // Pendientes
                // Implementar filtro de pendientes
                await Application.Current.MainPage.DisplayAlert("Filtro", "Mostrando reservaciones pendientes", "OK");
                break;
            case 3: // Canceladas
                // Implementar filtro de canceladas
                await Application.Current.MainPage.DisplayAlert("Filtro", "Mostrando reservaciones canceladas", "OK");
                break;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            _viewModel.CargarReservacionesCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario
        BottomTabBar.TabSelected -= OnBottomTabSelected;
    }
} 