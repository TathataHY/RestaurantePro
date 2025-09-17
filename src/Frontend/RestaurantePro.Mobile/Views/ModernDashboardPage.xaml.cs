using RestaurantePro.Mobile.Animations;
using RestaurantePro.Mobile.ViewModels;
using RestaurantePro.Mobile.Controls;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Views;

public partial class ModernDashboardPage : ContentPage
{
    private ModernDashboardViewModel _viewModel;

    public ModernDashboardPage(ModernDashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        SetupBottomNavigation();
        SetupOrdersTabNavigation();
        
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

        // BottomTabBar.Tabs = tabs;
        // BottomTabBar.TabSelected += OnBottomTabSelected;
    }

    private void SetupOrdersTabNavigation()
    {
        OrdersTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Todas",
            "Pendientes",
            "En Progreso",
            "Listas"
        };

        OrdersTabNavigation.TabSelectedCommand = new Command<int>(OnOrderTabSelected);
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
                    await view.CardEntranceAsync(i * 100);
                }
            }
        }

        // Animar botones de acciones rápidas
        var actionButtons = this.FindByName<Grid>("ActionButtonsGrid");
        if (actionButtons != null)
        {
            for (int i = 0; i < actionButtons.Children.Count; i++)
            {
                if (actionButtons.Children[i] is View view)
                {
                    await view.FadeInAsync(300);
                    await Task.Delay(50);
                }
            }
        }
    }

    private async void OnBottomTabSelected(object sender, int tabIndex)
    {
        // Animación de transición
        await this.FadeTo(0, 150);
        
        // Aquí implementarías la navegación real
        // var selectedTab = BottomTabBar.Tabs[tabIndex];
        await Application.Current.MainPage.DisplayAlert("Navegación", $"Navegando a tab: {tabIndex}", "OK");
        
        await this.FadeTo(1, 150);
    }

    private async void OnOrderTabSelected(int tabIndex)
    {
        // Filtrar comandas según el tab seleccionado
        var tabName = OrdersTabNavigation.Tabs[tabIndex];
        
        System.Diagnostics.Debug.WriteLine($"🔍 [ModernDashboardPage] Tab seleccionado: {tabName}");
        
        // Implementar filtrado real de comandas
        if (_viewModel != null)
        {
            await _viewModel.FilterOrdersByStatusAsync(tabName);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            System.Diagnostics.Debug.WriteLine("🔄 [ModernDashboardPage] OnAppearing - Refrescando datos");
            _viewModel.RefreshData();
        }
    }


} 