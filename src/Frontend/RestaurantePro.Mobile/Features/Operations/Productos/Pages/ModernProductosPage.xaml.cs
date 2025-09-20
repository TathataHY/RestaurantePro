using RestaurantePro.Mobile.Controls;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Features.Operations.Productos.Pages;

/// <summary>
/// Página moderna de gestión de productos - V4 Modernización Visual
/// </summary>
public partial class ModernProductosPage : ContentPage
{
    private ProductosViewModel _viewModel;

    public ModernProductosPage(ProductosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // SetupBottomNavigation(); // Comentado porque ahora usamos tabs nativos
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
            new TabItem("Productos", "📦", "products"),
            new TabItem("Perfil", "👤", "profile")
        };

        // BottomTabBar.Tabs = tabs; // Comentado porque ahora usamos tabs nativos
        // BottomTabBar.SelectedIndex = 3; // Seleccionar tab de Productos  
        // BottomTabBar.TabSelected += OnBottomTabSelected;
    }

    private void SetupFilterTabs()
    {
        // TEMPORALMENTE COMENTADO PARA PRUEBAS
        /*
        FilterTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Todos",
            "Disponibles",
            "Agotados",
            "Populares"
        };

        FilterTabNavigation.TabSelectedCommand = new Command<int>(OnFilterTabSelected);
        */
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

        // Animar FAB si existe en la vista
        var fab = this.FindByName<FloatingActionButton>("MainFab");
        if (fab != null)
        {
            await fab.ScaleTo(1, 400, Easing.BounceOut);
        }
    }

    private async void OnBottomTabSelected(object sender, int tabIndex)
    {
        // Animación de transición
        await this.FadeTo(0, 150);
        
        // Aquí implementarías la navegación real (comentado porque ahora usamos tabs nativos)
        // var selectedTab = BottomTabBar.Tabs[tabIndex];
        await Application.Current.MainPage.DisplayAlert("Navegación", $"Navegando a tab {tabIndex}", "OK");
        
        await this.FadeTo(1, 150);
    }

    private async void OnVerProductoClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🔍 [ModernProductosPage] OnVerProductoClicked - botón presionado");
        
        if (sender is Button button && button.CommandParameter is ProductoDto producto)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ModernProductosPage] Producto obtenido: {producto.Nombre} (ID: {producto.Id})");
            
            // Llamar al comando del ViewModel
            if (_viewModel.VerProductoCommand.CanExecute(producto))
            {
                System.Diagnostics.Debug.WriteLine("🔍 [ModernProductosPage] Ejecutando comando...");
                await _viewModel.VerProductoCommand.ExecuteAsync(producto);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ [ModernProductosPage] El comando no se puede ejecutar");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("❌ [ModernProductosPage] No se pudo obtener el producto del CommandParameter");
        }
    }

    private async void OnFilterTabSelected(int tabIndex)
    {
        // TEMPORALMENTE COMENTADO PARA PRUEBAS
        /*
        // Filtrar productos según el tab seleccionado
        var tabName = FilterTabNavigation.Tabs[tabIndex];
        
        switch (tabIndex)
        {
            case 0: // Todos
                _viewModel.LoadProductosCommand.Execute(null);
                break;
            case 1: // Disponibles
                _viewModel.ToggleSoloDisponiblesCommand.Execute(null);
                break;
            case 2: // Agotados
                // Implementar filtro de agotados
                await Application.Current.MainPage.DisplayAlert("Filtro", "Mostrando productos agotados", "OK");
                break;
            case 3: // Populares
                _viewModel.LoadProductosPopularesCommand.Execute(null);
                break;
        }
        */
    }

    // Paginación incremental del CollectionView (igual que mesas)
    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_viewModel == null) return;
        // Scroll infinito: cargar siguiente página
        _viewModel.LoadMoreProductosCommand?.Execute(null);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refrescar datos cuando la página aparece
        if (_viewModel != null)
        {
            _viewModel.LoadProductosCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario (comentado porque ahora usamos tabs nativos)
        // BottomTabBar.TabSelected -= OnBottomTabSelected;
    }
} 