using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;

/// <summary>
/// ViewModel para mostrar productos de una categoría específica
/// </summary>
public partial class ProductosPorCategoriaViewModel : BaseViewModel
{
    private readonly IProductosService _productosService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<ProductoDto> productos = new();

    [ObservableProperty]
    private CategoriaProductoDto? categoriaSeleccionada;

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloDisponibles = true;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string tituloPagina = "Productos";

    [ObservableProperty]
    private EstadisticasProductosDto estadisticas = new();

    [ObservableProperty]
    private Guid categoriaId;

    [ObservableProperty]
    private string categoriaNombre = "Productos";

    #endregion

    #region Constructor

    public ProductosPorCategoriaViewModel(
        IProductosService productosService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _productosService = productosService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Constructor con parámetros para navegación
    /// </summary>
    public ProductosPorCategoriaViewModel(
        IProductosService productosService,
        IDialogService dialogService,
        INavigationService navigationService,
        Guid categoriaId,
        string categoriaNombre) : this(productosService, dialogService, navigationService)
    {
        // Configurar la categoría desde los parámetros
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = categoriaNombre,
            Descripcion = "Productos de esta categoría",
            Activa = true
        };
        
        ConfigurarCategoria(categoria);
    }

    #endregion

    #region Propiedades Calculadas

    /// <summary>
    /// Indica si hay productos cargados
    /// </summary>
    public bool TieneProductos => Productos.Any();

    /// <summary>
    /// Mensaje para mostrar cuando no hay productos
    /// </summary>
    public string MensajeSinProductos => !string.IsNullOrWhiteSpace(TextoBusqueda)
        ? $"No se encontraron productos para '{TextoBusqueda}'"
        : !string.IsNullOrWhiteSpace(CategoriaNombre)
            ? $"No hay productos en la categoría '{CategoriaNombre}'"
            : "No hay productos disponibles";

    #endregion

    #region Comandos

    /// <summary>
    /// Cargar productos de la categoría
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarProductosAsync()
    {
        if (IsBusy || CategoriaId == Guid.Empty) return;

        await ExecuteAsync(async () =>
        {
            IsRefreshing = true;
            
            var response = await _productosService.ObtenerProductosPorCategoriaAsync(
                CategoriaId, MostrarSoloDisponibles, CancellationToken.None);
            
            if (response.Success && response.Data != null)
            {
                Productos.Clear();
                foreach (var producto in response.Data)
                {
                    Productos.Add(producto);
                }
                
                ActualizarEstadisticas();
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar productos");
            }
        });
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Refrescar productos
    /// </summary>
    [RelayCommand]
    public async Task RefreshProductosAsync()
    {
        await CargarProductosAsync();
    }

    /// <summary>
    /// Buscar productos
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task BuscarProductosAsync()
    {
        if (IsBusy || CategoriaSeleccionada == null) return;

        await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                // Si no hay búsqueda, cargar todos los productos de la categoría
                await CargarProductosAsync();
                return;
            }

            var response = await _productosService.BuscarProductosAsync(TextoBusqueda);
            
            if (response.Success && response.Data != null)
            {
                // Filtrar solo los productos de la categoría actual
                var productosFiltrados = response.Data
                    .Where(p => p.CategoriaId == CategoriaSeleccionada.Id)
                    .ToList();

                Productos.Clear();
                foreach (var producto in productosFiltrados)
                {
                    Productos.Add(producto);
                }
                
                ActualizarEstadisticas();
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al buscar productos");
            }
        });
    }

    /// <summary>
    /// Aplicar filtros
    /// </summary>
    [RelayCommand]
    public async Task AplicarFiltrosAsync()
    {
        await CargarProductosAsync();
    }

    /// <summary>
    /// Ver detalle de producto
    /// </summary>
    [RelayCommand]
    public async Task VerProductoAsync(ProductoDto producto)
    {
        if (producto == null) return;

        try
        {
            await _navigationService.NavigateToAsync("productodetalle", new Dictionary<string, object>
            {
                { "productoId", producto.Id }
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al navegar al detalle: {ex.Message}");
        }
    }

    /// <summary>
    /// Agregar producto a comanda
    /// </summary>
    [RelayCommand]
    public async Task AgregarAComandaAsync(ProductoDto producto)
    {
        if (producto == null) return;

        try
        {
            // Navegar a crear comanda con el producto pre-seleccionado
            await _navigationService.NavigateToAsync("crearcomanda", new Dictionary<string, object>
            {
                { "productoId", producto.Id },
                { "categoriaId", CategoriaSeleccionada?.Id }
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al agregar a comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Volver a la página de categorías
    /// </summary>
    [RelayCommand]
    public async Task VolverACategoriasAsync()
    {
        try
        {
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al volver: {ex.Message}");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Configurar la categoría seleccionada
    /// </summary>
    public void ConfigurarCategoria(CategoriaProductoDto categoria)
    {
        CategoriaSeleccionada = categoria;
        TituloPagina = $"Productos - {categoria.Nombre}";
    }

    /// <summary>
    /// Configurar la categoría desde parámetros de navegación
    /// </summary>
    public void ConfigurarDesdeParametros(IDictionary<string, object> parametros)
    {
        if (parametros.TryGetValue("categoriaId", out var categoriaIdObj) && 
            parametros.TryGetValue("categoriaNombre", out var categoriaNombreObj))
        {
            var categoria = new CategoriaProductoDto
            {
                Id = (Guid)categoriaIdObj,
                Nombre = categoriaNombreObj.ToString() ?? "Categoría",
                Descripcion = "Productos de esta categoría",
                Activa = true
            };
            
            ConfigurarCategoria(categoria);
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Actualizar estadísticas de productos
    /// </summary>
    private void ActualizarEstadisticas()
    {
        Estadisticas = new EstadisticasProductosDto
        {
            TotalProductos = Productos.Count,
            ProductosDisponibles = Productos.Count(p => p.EstadoDisponibilidad == "Disponible"),
            ProductosAgotados = Productos.Count(p => p.EstadoDisponibilidad == "Agotado"),
            TotalCategorias = 1 // Solo una categoría en esta vista
        };
    }

    #endregion
}

/// <summary>
/// DTO para estadísticas de productos
/// </summary>
public class EstadisticasProductosDto
{
    public int TotalProductos { get; set; }
    public int ProductosDisponibles { get; set; }
    public int ProductosAgotados { get; set; }
    public int TotalCategorias { get; set; }
}
