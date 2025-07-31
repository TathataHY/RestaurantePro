using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;
using System.Collections.ObjectModel;

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
        
        SetupBottomNavigation();
        
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
            new TabItem("Productos", "📦", "products"),
            new TabItem("Perfil", "👤", "profile")
        };

        BottomTabBar.Tabs = tabs;
        BottomTabBar.SelectedIndex = 1; // Seleccionar tab de Mesas
        BottomTabBar.TabSelected += OnBottomTabSelected;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            _viewModel.RefreshMesasCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario
        BottomTabBar.TabSelected -= OnBottomTabSelected;
    }
} 