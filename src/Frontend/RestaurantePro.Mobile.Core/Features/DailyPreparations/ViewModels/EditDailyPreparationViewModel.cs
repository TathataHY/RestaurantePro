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
        await CargarCategoriasAsync();
        await CargarPreparacionAsync();
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
        await BuscarProductosAsync(p.NombreProducto); // precargar lista

        ProductoSeleccionado = _productos.FirstOrDefault(x => x.Id == p.ProductoId);
        CantidadPreparada = p.CantidadPreparada;
        CantidadDisponible = p.CantidadDisponible;
        FechaVencimiento = p.FechaVencimiento;
        Observaciones = p.Observaciones ?? string.Empty;
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
            var size = 10;
            var term = string.IsNullOrWhiteSpace(termino ?? ProductoBusqueda) ? null : (termino ?? ProductoBusqueda);

            ApiResponse<List<ProductoDto>> result;
            if (CategoriaSeleccionada != null && string.IsNullOrWhiteSpace(term))
            {
                result = await _productosService.ObtenerProductosPorCategoriaAsync(CategoriaSeleccionada.Id, true);
            }
            else
            {
                result = await _productosService.ObtenerProductosPaginadosAsync(page, size, term, true);
            }

            Productos.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var p in result.Data)
                    Productos.Add(p);
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


