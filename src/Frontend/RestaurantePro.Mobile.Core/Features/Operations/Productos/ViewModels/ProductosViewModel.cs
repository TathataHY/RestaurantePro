using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

/// <summary>
/// ViewModel para gestión de productos del menú
/// </summary>
public partial class ProductosViewModel : BaseViewModel
{
    private readonly IProductosService _productosService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ProductoDto> productos = new();

    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> categorias = new();

    [ObservableProperty]
    private ProductoDto? selectedProducto;

    [ObservableProperty]
    private CategoriaProductoDto? selectedCategoria;

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloDisponibles = true;

    [ObservableProperty]
    private bool mostrarFiltros;

    [ObservableProperty]
    private int totalProductos;

    [ObservableProperty]
    private int productosDisponibles;

    [ObservableProperty]
    private int productosAgotados;

    // ========================================
    // CONSTRUCTOR
    // ========================================

    public ProductosViewModel(
        IProductosService productosService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _productosService = productosService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        Title = "Productos";
    }

    // ========================================
    // PROPIEDADES CALCULADAS
    // ========================================

    /// <summary>
    /// Indica si hay productos cargados
    /// </summary>
    public bool TieneProductos => Productos.Any();

    /// <summary>
    /// Indica si hay categorías cargadas
    /// </summary>
    public bool TieneCategorias => Categorias.Any();

    /// <summary>
    /// Mensaje para mostrar cuando no hay productos
    /// </summary>
    public string MensajeSinProductos => !string.IsNullOrWhiteSpace(TextoBusqueda)
        ? $"No se encontraron productos para '{TextoBusqueda}'"
        : SelectedCategoria != null
            ? $"No hay productos en la categoría '{SelectedCategoria.Nombre}'"
            : "No hay productos disponibles";

    /// <summary>
    /// Porcentaje de productos disponibles
    /// </summary>
    public double PorcentajeDisponibilidad => TotalProductos > 0 
        ? (double)ProductosDisponibles / TotalProductos * 100 
        : 0;

    // ========================================
    // COMANDOS PRINCIPALES
    // ========================================

    /// <summary>
    /// Cargar productos con filtros aplicados
    /// </summary>
    [RelayCommand]
    private async Task LoadProductosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Cargar categorías si no están cargadas
            if (!TieneCategorias)
            {
                await LoadCategoriasAsync();
            }

            RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<ProductoDto>> result;

            // Determinar qué tipo de consulta hacer
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                // Búsqueda por texto
                result = await _productosService.BuscarProductosAsync(TextoBusqueda, MostrarSoloDisponibles);
            }
            else if (SelectedCategoria != null)
            {
                // Filtrar por categoría
                result = await _productosService.ObtenerProductosPorCategoriaAsync(SelectedCategoria.Id, MostrarSoloDisponibles);
            }
            else
            {
                // Obtener todos los productos
                result = await _productosService.ObtenerProductosPaginadosAsync(
                    pageNumber: 1, 
                    pageSize: 100, 
                    filtro: null, 
                    soloActivos: MostrarSoloDisponibles);
            }

            if (result.Success && result.Data != null)
            {
                Productos.Clear();
                foreach (var producto in result.Data)
                {
                    Productos.Add(producto);
                }

                ActualizarEstadisticas();
            }
            else
            {
                SetError("Error al cargar productos", result.Message);
                await _dialogService.ShowErrorAsync(result.Message ?? "Error al cargar productos");
            }
        }
        catch (Exception _)
        {
            SetError("Error inesperado", "Error inesperado al cargar productos");
            await _dialogService.ShowErrorAsync("Error inesperado al cargar productos");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cargar categorías disponibles
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriasAsync()
    {
        try
        {
            var result = await _productosService.ObtenerCategoriasAsync();

            if (result.Success && result.Data != null)
            {
                Categorias.Clear();
                foreach (var categoria in result.Data)
                {
                    Categorias.Add(categoria);
                }
            }
        }
        catch (Exception _)
        {
            await _dialogService.ShowErrorAsync("Error al cargar categorías");
        }
    }

    /// <summary>
    /// Buscar productos por texto
    /// </summary>
    [RelayCommand]
    private async Task BuscarProductosAsync()
    {
        await LoadProductosAsync();
    }

    /// <summary>
    /// Limpiar búsqueda y filtros
    /// </summary>
    [RelayCommand]
    private async Task LimpiarBusquedaAsync()
    {
        TextoBusqueda = string.Empty;
        SelectedCategoria = null;
        await LoadProductosAsync();
    }

    /// <summary>
    /// Ver detalle de un producto
    /// </summary>
    [RelayCommand]
    private async Task VerDetalleProductoAsync(ProductoDto? producto)
    {
        if (producto == null) return;

        try
        {
            // Navegar a la página de detalle del producto
            await _navigationService.NavigateToAsync("producto-detalle", new Dictionary<string, object>
            {
                ["productoId"] = producto.Id.ToString()
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al navegar al detalle: {ex.Message}");
        }
    }

    /// <summary>
    /// Agregar producto a una comanda
    /// </summary>
    [RelayCommand]
    private async Task AgregarAComandaAsync(ProductoDto? producto)
    {
        if (producto == null) return;

        try
        {
            if (!producto.PuedeAgregarAComanda)
            {
                await _dialogService.ShowAlertAsync("Producto no disponible", 
                    "Este producto no está disponible en este momento");
                return;
            }

            // Navegar a comandas con el producto seleccionado
            await _navigationService.NavigateToAsync("comandas", new Dictionary<string, object>
            {
                ["productoId"] = producto.Id.ToString(),
                ["accion"] = "agregar"
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al agregar producto: {ex.Message}");
        }
    }

    /// <summary>
    /// Filtrar productos por categoría
    /// </summary>
    [RelayCommand]
    private async Task FiltrarPorCategoriaAsync(CategoriaProductoDto? categoria)
    {
        SelectedCategoria = categoria;
        TextoBusqueda = string.Empty; // Limpiar búsqueda al filtrar por categoría
        await LoadProductosAsync();
    }

    /// <summary>
    /// Mostrar/ocultar panel de filtros
    /// </summary>
    [RelayCommand]
    private void MostrarFiltrosCommand()
    {
        MostrarFiltros = !MostrarFiltros;
    }

    /// <summary>
    /// Toggle para mostrar solo productos disponibles
    /// </summary>
    [RelayCommand]
    private async Task ToggleSoloDisponiblesAsync()
    {
        MostrarSoloDisponibles = !MostrarSoloDisponibles;
        await LoadProductosAsync();
    }

    /// <summary>
    /// Cargar productos populares
    /// </summary>
    [RelayCommand]
    private async Task LoadProductosPopularesAsync()
    {
        try
        {
            IsBusy = true;

            var result = await _productosService.ObtenerProductosPopularesAsync(limite: 10);

            if (result.Success && result.Data != null)
            {
                Productos.Clear();
                foreach (var producto in result.Data)
                {
                    Productos.Add(producto);
                }

                ActualizarEstadisticas();
                
                // Marcar que estamos mostrando populares
                SelectedCategoria = null;
                TextoBusqueda = string.Empty;
            }
            else
            {
                await _dialogService.ShowErrorAsync("Error al cargar productos populares");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ========================================
    // MÉTODOS PRIVADOS
    // ========================================

    /// <summary>
    /// Actualizar estadísticas de productos
    /// </summary>
    private void ActualizarEstadisticas()
    {
        TotalProductos = Productos.Count;
        ProductosDisponibles = Productos.Count(p => p.PuedeAgregarAComanda);
        ProductosAgotados = TotalProductos - ProductosDisponibles;

        // Notificar cambios en propiedades calculadas
        OnPropertyChanged(nameof(TieneProductos));
        OnPropertyChanged(nameof(PorcentajeDisponibilidad));
        OnPropertyChanged(nameof(MensajeSinProductos));
    }

    /// <summary>
    /// Establecer estado de error
    /// </summary>
    private void SetError(string title, string message)
    {
        HasError = true;
        ErrorMessage = message;
        Title = title;
    }

    // ========================================
    // MÉTODOS PÚBLICOS
    // ========================================

    /// <summary>
    /// Inicializar el ViewModel
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadCategoriasAsync();
        await LoadProductosAsync();
    }

    /// <summary>
    /// Limpiar recursos del ViewModel
    /// </summary>
    public void Cleanup()
    {
        Productos.Clear();
        Categorias.Clear();
        SelectedProducto = null;
        SelectedCategoria = null;
        TextoBusqueda = string.Empty;
    }
} 