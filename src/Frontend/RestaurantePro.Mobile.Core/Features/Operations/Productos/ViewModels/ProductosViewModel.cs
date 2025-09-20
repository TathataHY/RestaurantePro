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
    private CancellationTokenSource? _searchCts;
    
    // Paginación del servidor (igual que mesas)
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _hasMorePages = true;
    private bool _isLoadingMore = false;
    private const int DefaultPageSize = 10; // Optimizado para carga ultra-rápida

    [ObservableProperty]
    private ObservableCollection<ProductoDto> productos = new();

    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> categorias = new();

    [ObservableProperty]
    private ProductoDto? selectedProducto;

    [ObservableProperty]
    private CategoriaProductoDto? selectedCategoria;

    [ObservableProperty]
    private CategoriaProductoDto? categoriaSeleccionada;

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloDisponibles = true;

    [ObservableProperty]
    private bool mostrarFiltros;

    [ObservableProperty]
    private bool mostrarEstadisticas = true;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private int totalProductos;

    [ObservableProperty]
    private int productosDisponibles;

    [ObservableProperty]
    private int productosAgotados;

    [ObservableProperty]
    private int totalCategorias;

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

    /// <summary>
    /// Retardo para debounce de búsqueda (ms). Ajustable para pruebas.
    /// </summary>
    public int DebounceDelayMs { get; set; } = 300;

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

    /// <summary>
    /// Estadísticas de productos para la UI
    /// </summary>
    public object Estadisticas => new
    {
        TotalProductos,
        ProductosDisponibles,
        ProductosAgotados,
        TotalCategorias
    };

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

        // Resetear paginación para nueva búsqueda
        _currentPage = 1;
        _hasMorePages = true;

        // 🚀 OPTIMIZACIÓN: Cargar categorías en paralelo (no bloquear productos)
        var categoriasTask = !TieneCategorias ? LoadCategoriasAsync() : Task.CompletedTask;
        
        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($"⚡ [ProductosViewModel] Iniciando carga paralela - Productos: SÍ, Categorías: {(!TieneCategorias ? "SÍ" : "NO (ya cargadas)")}");

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
                // Obtener productos paginados (reducido de 100 a 20)
                result = await _productosService.ObtenerProductosPaginadosAsync(
                    pageNumber: _currentPage, 
                    pageSize: DefaultPageSize, 
                    filtro: null, 
                    soloActivos: MostrarSoloDisponibles);
            }

            if (result.Success && result.Data != null)
            {
                // Si es la primera página, limpiar colección
                if (_currentPage == 1)
                {
                    Productos.Clear();
                }

                // Agregar nuevos productos
                foreach (var producto in result.Data)
                {
                    Productos.Add(producto);
                }

                // Actualizar información de paginación
                _hasMorePages = result.Data.Count == DefaultPageSize;
                
                System.Diagnostics.Debug.WriteLine($"✅ [ProductosViewModel] Productos cargados: {result.Data.Count}, Total en colección: {Productos.Count}, Más páginas: {_hasMorePages}");

                ActualizarEstadisticas();
                
                // ⚡ Esperar que terminen las categorías (sin bloquear productos)
                await categoriasTask;
            }
            else
            {
                SetError("Error al cargar productos", result.Message);
                await _dialogService.ShowErrorAsync(result.Message ?? "Error al cargar productos");
                
                // ⚡ Esperar que terminen las categorías incluso si hay error
                await categoriasTask;
            }
        }
        catch (Exception _)
        {
            SetError("Error inesperado", "Error inesperado al cargar productos");
            await _dialogService.ShowErrorAsync("Error inesperado al cargar productos");
            
            // ⚡ Esperar que terminen las categorías incluso si hay excepción
            try { await categoriasTask; } catch { /* Ignorar errores de categorías */ }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cargar siguiente página de productos (scroll infinito - igual que mesas)
    /// </summary>
    [RelayCommand]
    private async Task LoadMoreProductosAsync()
    {
        if (IsBusy || !_hasMorePages || _isLoadingMore || IsRefreshing) 
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ProductosViewModel] LoadMoreProductosAsync - Saltando: IsBusy={IsBusy}, HasMorePages={_hasMorePages}, IsLoadingMore={_isLoadingMore}, IsRefreshing={IsRefreshing}");
            return;
        }
        
        _isLoadingMore = true;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ProductosViewModel] LoadMoreProductosAsync - Cargando página {_currentPage + 1}");
            
            // Verificar nuevamente que no se inició un refresh mientras esperábamos
            if (IsRefreshing)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [ProductosViewModel] LoadMoreProductosAsync - Refresh detectado, cancelando LoadMore");
                return;
            }
            
            // Incrementar página y cargar más productos
            _currentPage++;
            await LoadMoreProductosInternalAsync();
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    /// <summary>
    /// Método interno para cargar más productos (solo para paginación)
    /// </summary>
    private async Task LoadMoreProductosInternalAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ProductosViewModel] LoadMoreProductosInternalAsync - Página {_currentPage}");

            // Obtener productos de la página actual
            var result = await _productosService.ObtenerProductosPaginadosAsync(
                pageNumber: _currentPage, 
                pageSize: DefaultPageSize, 
                filtro: TextoBusqueda, 
                soloActivos: MostrarSoloDisponibles);

            if (result.Success && result.Data != null)
            {
                // Agregar nuevos productos SIN limpiar la colección
                foreach (var producto in result.Data)
                {
                    Productos.Add(producto);
                }

                // Actualizar información de paginación (si devuelve 10 = página completa, hay más)
                _hasMorePages = result.Data.Count == DefaultPageSize;
                
                System.Diagnostics.Debug.WriteLine($"✅ [ProductosViewModel] Más productos cargados: {result.Data.Count}/10, Total: {Productos.Count}, Más páginas: {_hasMorePages}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [ProductosViewModel] Error cargando más productos: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ProductosViewModel] Excepción cargando más productos: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"⚡ [ProductosViewModel] Cargando categorías...");
            var result = await _productosService.ObtenerCategoriasAsync();

            if (result.Success && result.Data != null)
            {
                Categorias.Clear();
                foreach (var categoria in result.Data)
                {
                    Categorias.Add(categoria);
                }
                TotalCategorias = Categorias.Count;
                System.Diagnostics.Debug.WriteLine($"✅ [ProductosViewModel] Categorías cargadas: {TotalCategorias}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [ProductosViewModel] Error cargando categorías: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ProductosViewModel] Excepción cargando categorías: {ex.Message}");
            // No mostrar error de categorías al usuario (no es crítico)
        }
    }

    /// <summary>
    /// Buscar productos por texto
    /// </summary>
    [RelayCommand]
    private async Task BuscarProductosAsync()
    {
        // Debounce: cancelar búsqueda anterior si existe
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(DebounceDelayMs, token);
            if (token.IsCancellationRequested) return;
            await LoadProductosAsync();
        }
        catch (TaskCanceledException)
        {
            // cancelado a propósito, ignorar
        }
    }

    /// <summary>
    /// Limpiar búsqueda y filtros
    /// </summary>
    [RelayCommand]
    private async Task LimpiarBusquedaAsync()
    {
        TextoBusqueda = string.Empty;
        SelectedCategoria = null;
        CategoriaSeleccionada = null;
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
        CategoriaSeleccionada = categoria;
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

    /// <summary>
    /// Refrescar productos
    /// </summary>
    [RelayCommand]
    private async Task RefreshProductosAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadProductosAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Cargar productos disponibles
    /// </summary>
    [RelayCommand]
    private async Task LoadProductosDisponiblesAsync()
    {
        MostrarSoloDisponibles = true;
        await LoadProductosAsync();
    }

    /// <summary>
    /// Cargar estadísticas
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        await LoadProductosAsync();
    }

    /// <summary>
    /// Crear nuevo producto
    /// </summary>
    [RelayCommand]
    private async Task CrearProductoAsync()
    {
        try
        {
            await _navigationService.NavigateToAsync("producto-crear");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al navegar: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver producto
    /// </summary>
    [RelayCommand]
    private async Task VerProductoAsync(ProductoDto? producto)
    {
        if (producto == null) 
        {
            System.Diagnostics.Debug.WriteLine("🔍 [ProductosViewModel] VerProductoAsync - producto es null");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [ProductosViewModel] Navegando a detalle del producto: {producto.Nombre} (ID: {producto.Id})");
            
            await _navigationService.NavigateToAsync("producto-detalle", new Dictionary<string, object>
            {
                ["productoId"] = producto.Id.ToString()
            });
            
            System.Diagnostics.Debug.WriteLine("🔍 [ProductosViewModel] Navegación completada exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ProductosViewModel] Error en navegación: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Error al ver producto: {ex.Message}");
        }
    }

    /// <summary>
    /// Editar producto
    /// </summary>
    [RelayCommand]
    private async Task EditarProductoAsync(ProductoDto? producto)
    {
        if (producto == null) return;

        try
        {
            await _navigationService.NavigateToAsync("producto-editar", new Dictionary<string, object>
            {
                ["productoId"] = producto.Id.ToString()
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al editar producto: {ex.Message}");
        }
    }

    /// <summary>
    /// Eliminar producto (temporalmente deshabilitado - requiere implementación en backend)
    /// </summary>
    [RelayCommand]
    private async Task EliminarProductoAsync(ProductoDto? producto)
    {
        if (producto == null) return;

        try
        {
            var confirmar = await _dialogService.ShowConfirmAsync(
                title: "Eliminar producto",
                message: $"¿Seguro que deseas eliminar '{producto.Nombre}'?",
                accept: "Eliminar",
                cancel: "Cancelar");

            if (!confirmar) return;

            var result = await _productosService.EliminarProductoAsync(producto.Id);
            if (result.Success)
            {
                await _dialogService.ShowSuccessAsync("Producto eliminado");
                await LoadProductosAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message ?? "No se pudo eliminar el producto");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error: {ex.Message}");
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
        TotalCategorias = Categorias.Count;



        // Notificar cambios en propiedades calculadas
        OnPropertyChanged(nameof(TieneProductos));
        OnPropertyChanged(nameof(PorcentajeDisponibilidad));
        OnPropertyChanged(nameof(MensajeSinProductos));
        OnPropertyChanged(nameof(Estadisticas));
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
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = null;
    }
} 