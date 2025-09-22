using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Services.Dialog;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using RestaurantePro.Mobile.Core.Services.Categorias;

namespace RestaurantePro.Mobile.Core.Features.DailyPreparations.ViewModels;

public partial class CreateDailyPreparationViewModel : AuthorizedBaseViewModel
{
    private readonly IDailyPreparationsService _dailyPreparationsService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IProductosService _productosService;

    // Búsqueda y selección de producto
    [ObservableProperty]
    private string _productoBusqueda = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductoDto> _productos = new();

    [ObservableProperty]
    private ProductoDto? _productoSeleccionado;

    partial void OnProductoSeleccionadoChanged(ProductoDto? value)
    {
        if (value != null)
        {
            // Mantener compatibilidad con lógica existente
            ProductoIdText = value.Id.ToString();
        }
    }

    [ObservableProperty]
    private string _productoIdText = string.Empty;

    // Categorías
    [ObservableProperty]
    private ObservableCollection<CategoriaProductoDto> _categorias = new();

    [ObservableProperty]
    private CategoriaProductoDto? _categoriaSeleccionada;

    partial void OnCategoriaSeleccionadaChanged(CategoriaProductoDto? value)
    {
        // Refrescar productos al cambiar de categoría
        _ = BuscarProductosAsync();
    }

    [ObservableProperty]
    private int _cantidad = 1;

    [ObservableProperty]
    private DateTime _fechaVencimiento = DateTime.Today.AddDays(1).AddHours(20); // Mañana a las 8:00 PM por defecto

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public CreateDailyPreparationViewModel(
        IDailyPreparationsService dailyPreparationsService,
        IAuthService authService,
        IDialogService dialogService,
        INavigationService navigationService,
        IProductosService productosService,
        IAuthorizationService authorizationService,
        IAuthorizationValidator authorizationValidator,
        AuthorizationUIHelper authorizationUIHelper,
        ILogger<CreateDailyPreparationViewModel> logger) 
        : base(authorizationService, authorizationValidator, authorizationUIHelper, dialogService, logger)
    {
        _dailyPreparationsService = dailyPreparationsService;
        _authService = authService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _productosService = productosService;
    }

    [RelayCommand]
    [RequirePermission(AppPermission.ActualizarEstadoPreparaciones)]
    private async Task CrearPreparacionAsync()
    {
        if (IsBusy) return;

        // Priorizar el seleccionado en UI; si no, usar texto GUID
        var tieneSeleccion = ProductoSeleccionado != null;
        Guid productoId;
        if (tieneSeleccion)
        {
            productoId = ProductoSeleccionado!.Id;
        }
        else if (!Guid.TryParse(ProductoIdText, out productoId))
        {
            await _dialogService.ShowErrorAsync("ProductoId inválido. Usa un GUID válido.");
            return;
        }
        if (Cantidad <= 0)
        {
            await _dialogService.ShowErrorAsync("La cantidad debe ser mayor que 0.");
            return;
        }
        if (FechaVencimiento.Date <= DateTime.Today)
        {
            await _dialogService.ShowErrorAsync($"La fecha de vencimiento debe ser futura. Mínimo: {DateTime.Today.AddDays(1):dd/MM/yyyy}");
            return;
        }

        IsBusy = true;
        try
        {
            var userIdStr = await _authService.GetUserIdAsync();
            var chefId = Guid.TryParse(userIdStr, out var chefGuid) ? chefGuid : Guid.Empty;

            var cmd = new CrearPreparacionDiariaCommand
            {
                ProductoId = productoId,
                Cantidad = Cantidad,
                ChefId = chefId,
                FechaVencimiento = FechaVencimiento,
                Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones
            };

            var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(cmd);
            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Menú creado correctamente.");
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "No se pudo crear la preparación.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al crear preparación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BuscarProductosAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var page = 1;
            var size = 10; // ⚡ Solo 10 productos para carga ultra-rápida
            var term = string.IsNullOrWhiteSpace(ProductoBusqueda) ? null : ProductoBusqueda;
            
            RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<ProductoDto>> result;
            
            // 🎯 FILTRO POR CATEGORÍA: Priorizar filtro por categoría seleccionada
            if (CategoriaSeleccionada != null)
            {
                // Si hay categoría seleccionada, filtrar por categoría
                result = await _productosService.ObtenerProductosPorCategoriaAsync(CategoriaSeleccionada.Id, true);
                System.Diagnostics.Debug.WriteLine($"🗂️ [CreateDailyPreparation] Filtrando por categoría: '{CategoriaSeleccionada.Nombre}' (ID: {CategoriaSeleccionada.Id})");
            }
            else if (!string.IsNullOrWhiteSpace(term))
            {
                // Si no hay categoría pero hay término de búsqueda, buscar por texto
                result = await _productosService.BuscarProductosAsync(term, true);
                System.Diagnostics.Debug.WriteLine($"🔍 [CreateDailyPreparation] Buscando por texto: '{term}'");
            }
            else
            {
                // Si no hay filtros, usar paginación básica
                result = await _productosService.ObtenerProductosPaginadosAsync(page, size, term, true);
                System.Diagnostics.Debug.WriteLine($"⚡ [CreateDailyPreparation] Carga paginada básica - Página: {page}, Tamaño: {size}");
            }
            
            Productos.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var p in result.Data)
                    Productos.Add(p);
                
                System.Diagnostics.Debug.WriteLine($"✅ [CreateDailyPreparation] Productos cargados: {result.Data.Count} - Categoría: '{CategoriaSeleccionada?.Nombre ?? "Todas"}', Término: '{term ?? "N/A"}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [CreateDailyPreparation] Error al cargar productos: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [CreateDailyPreparation] Excepción al buscar productos: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Error al buscar productos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarCategoriasAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🗂️ [CreateDailyPreparation] Iniciando carga de categorías...");
            
            var resp = await _productosService.ObtenerCategoriasAsync();
            Categorias.Clear();
            
            if (resp.Success && resp.Data != null)
            {
                foreach (var c in resp.Data)
                    Categorias.Add(c);
                
                System.Diagnostics.Debug.WriteLine($"✅ [CreateDailyPreparation] Categorías cargadas exitosamente: {Categorias.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [CreateDailyPreparation] Error al cargar categorías: {resp.Message}");
                // No mostrar error al usuario, solo log para debug
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [CreateDailyPreparation] Excepción al cargar categorías: {ex.Message}");
            // Solo mostrar error crítico al usuario
            await _dialogService.ShowErrorAsync($"Error al cargar categorías: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await _navigationService.GoBackAsync();
    }
}


