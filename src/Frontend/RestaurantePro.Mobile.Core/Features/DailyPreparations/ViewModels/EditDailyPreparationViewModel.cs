using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Services.Dialog;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Categorias;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

public partial class EditDailyPreparationViewModel : AuthorizedBaseViewModel
{
    private readonly IDailyPreparationsService _dailyPreparationsService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IProductosService _productosService;
    private readonly ICategoriasService _categoriasService;

    private Guid _preparacionId;

    [ObservableProperty]
    private string _titulo = "Editar Preparación";

    // Categorías y productos
    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> _categorias = new();

    [ObservableProperty]
    private CategoriaProductoDto? _categoriaSeleccionada;

    [ObservableProperty]
    private string _productoBusqueda = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductoDto> _productos = new();

    [ObservableProperty]
    private ProductoDto? _productoSeleccionado;

    [ObservableProperty]
    private int _cantidadPreparada;

    [ObservableProperty]
    private int _cantidadDisponible;

    [ObservableProperty]
    private DateTime _fechaVencimiento = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public EditDailyPreparationViewModel(
        IDailyPreparationsService dailyPreparationsService,
        IAuthService authService,
        IDialogService dialogService,
        INavigationService navigationService,
        IProductosService productosService,
        ICategoriasService categoriasService,
        IAuthorizationService authorizationService,
        IAuthorizationValidator authorizationValidator,
        AuthorizationUIHelper authorizationUIHelper,
        ILogger<EditDailyPreparationViewModel> logger) 
        : base(authorizationService, authorizationValidator, authorizationUIHelper, dialogService, logger)
    {
        _dailyPreparationsService = dailyPreparationsService;
        _authService = authService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _productosService = productosService;
        _categoriasService = categoriasService;
    }

    public async Task InitializeAsync(Guid preparacionId)
    {
        _preparacionId = preparacionId;
        
        // 🚀 OPTIMIZACIÓN: Cargar categorías y preparación en paralelo
        var categoriasTask = CargarCategoriasAsync();
        var preparacionTask = CargarPreparacionAsync();
        
        // Esperar ambas tareas en paralelo
        await Task.WhenAll(categoriasTask, preparacionTask);
        
        System.Diagnostics.Debug.WriteLine($"⚡ [EditDailyPreparationViewModel] Carga paralela completada para preparación: {preparacionId}");
    }

    private async Task CargarPreparacionAsync()
    {
        var result = await _dailyPreparationsService.GetPreparacionDiariaAsync(_preparacionId);
        if (!result.Succeeded || result.Data == null)
        {
            await _dialogService.ShowErrorAsync(result.Error ?? "No se pudo cargar la preparación");
            return;
        }

        var p = result.Data;
        
        // 🎯 OPTIMIZACIÓN: Buscar productos por nombre del producto actual
        await BuscarProductosAsync(p.NombreProducto);

        // 🔍 Verificar si el producto está en la lista cargada
        ProductoSeleccionado = Productos.FirstOrDefault(x => x.Id == p.ProductoId);
        
        // 🚀 Si no está en la lista, cargarlo específicamente
        if (ProductoSeleccionado == null)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [EditDailyPreparation] Producto {p.ProductoId} no encontrado en lista, cargando específicamente...");
            await CargarProductoEspecificoAsync(p.ProductoId, p.NombreProducto);
        }
        CantidadPreparada = p.CantidadPreparada;
        CantidadDisponible = p.CantidadDisponible;
        FechaVencimiento = p.FechaVencimiento;
        Observaciones = p.Observaciones ?? string.Empty;
    }

    /// <summary>
    /// Cargar un producto específico desde el servidor y agregarlo a la lista
    /// </summary>
    private async Task CargarProductoEspecificoAsync(Guid productoId, string nombreProducto)
    {
        try
        {
            // 🔍 Intentar obtener el producto completo desde el servidor
            var result = await _productosService.ObtenerProductoPorIdAsync(productoId);
            
            if (result.Success && result.Data != null)
            {
                // 🚀 Agregar el producto real a la lista
                if (!Productos.Any(x => x.Id == productoId))
                {
                    Productos.Insert(0, result.Data); // Insertar al inicio para que sea visible
                    System.Diagnostics.Debug.WriteLine($"✅ [EditDailyPreparation] Producto completo agregado: {result.Data.Nombre}");
                }
                
                // 🎯 Seleccionar el producto
                ProductoSeleccionado = Productos.FirstOrDefault(x => x.Id == productoId);
            }
            else
            {
                // 🔧 Fallback: Crear producto temporal si no se puede obtener del servidor
                var productoTemp = new ProductoDto
                {
                    Id = productoId,
                    Nombre = nombreProducto,
                    Activo = true,
                    Precio = 0,
                    CantidadDisponible = 0,
                    CategoriaNombre = "Sin categoría"
                };
                
                Productos.Insert(0, productoTemp);
                ProductoSeleccionado = productoTemp;
                System.Diagnostics.Debug.WriteLine($"⚠️ [EditDailyPreparation] Producto temporal creado: {nombreProducto}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [EditDailyPreparation] Error cargando producto específico: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CargarCategoriasAsync()
    {
        try
        {
            var result = await _categoriasService.ObtenerCategoriasAsync();
            Categorias.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var c in result.Data)
                    Categorias.Add(c);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar categorías: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BuscarProductosAsync(string? termino = null)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var page = 1;
            var size = 10; // ⚡ Solo 10 productos para carga ultra-rápida
            var term = string.IsNullOrWhiteSpace(termino ?? ProductoBusqueda) ? null : (termino ?? ProductoBusqueda);

            // 🚀 OPTIMIZACIÓN: Usar SOLO paginación para evitar carga masiva
            var result = await _productosService.ObtenerProductosPaginadosAsync(page, size, term, true);

            Productos.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var p in result.Data)
                    Productos.Add(p);
                
                System.Diagnostics.Debug.WriteLine($"⚡ [EditDailyPreparation] Productos cargados: {result.Data.Count}/10 - Término: '{term}'");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al buscar productos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    [RequirePermission(AppPermission.ActualizarEstadoPreparaciones)]
    private async Task GuardarCambiosAsync()
    {
        if (IsBusy) return;

        if (ProductoSeleccionado == null)
        {
            await _dialogService.ShowErrorAsync("Seleccione un producto.");
            return;
        }
        if (CantidadPreparada <= 0)
        {
            await _dialogService.ShowErrorAsync("La cantidad preparada debe ser mayor que 0.");
            return;
        }
        if (CantidadDisponible < 0 || CantidadDisponible > CantidadPreparada)
        {
            await _dialogService.ShowErrorAsync("La disponible debe estar entre 0 y la preparada.");
            return;
        }
        if (FechaVencimiento.Date <= DateTime.Today)
        {
            await _dialogService.ShowErrorAsync("La fecha de vencimiento debe ser futura (desde mañana).");
            return;
        }

        IsBusy = true;
        try
        {
            var chefIdStr = await _authService.GetUserIdAsync();
            var chefId = Guid.TryParse(chefIdStr, out var g) ? g : Guid.Empty;

            var cmd = new ActualizarPreparacionDiariaCommand
            {
                ProductoId = ProductoSeleccionado.Id,
                CantidadPreparada = CantidadPreparada,
                CantidadDisponible = CantidadDisponible,
                ChefId = chefId,
                FechaVencimiento = FechaVencimiento,
                Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones
            };

            var result = await _dailyPreparationsService.ActualizarPreparacionDiariaAsync(_preparacionId, cmd);
            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Preparación actualizada correctamente");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "No se pudo actualizar");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al actualizar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await _navigationService.GoBackAsync();
    }
}


