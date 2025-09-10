using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Productos.Pages;

/// <summary>
/// Página para mostrar productos de una categoría específica
/// </summary>
public partial class ProductosPorCategoriaPage : ContentPage, IQueryAttributable
{
    private ProductosPorCategoriaViewModel _viewModel;

    public ProductosPorCategoriaPage(ProductosPorCategoriaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Configurar animaciones de entrada
        Loaded += OnPageLoaded;
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
    }

    private void OnMostrarSoloDisponiblesChanged(object sender, CheckedChangedEventArgs e)
    {
        // El ViewModel maneja la lógica del filtro
        // Este método solo existe para satisfacer el binding del XAML
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Cargar productos cuando la página aparece
        if (_viewModel != null)
        {
            _viewModel.CargarProductosCommand.Execute(null);
        }
    }

    /// <summary>
    /// Implementación de IQueryAttributable para recibir parámetros de navegación
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("categoriaId", out var categoriaIdObj) && 
            Guid.TryParse(categoriaIdObj?.ToString(), out var categoriaIdParsed))
        {
            _viewModel.CategoriaId = categoriaIdParsed;
        }

        if (query.TryGetValue("categoriaNombre", out var categoriaNombreObj))
        {
            _viewModel.CategoriaNombre = categoriaNombreObj?.ToString() ?? "Productos";
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // Los parámetros se manejan en ApplyQueryAttributes
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos si es necesario
    }
}
