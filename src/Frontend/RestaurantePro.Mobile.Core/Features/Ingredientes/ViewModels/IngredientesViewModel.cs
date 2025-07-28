using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Dialog;
using System.Collections.ObjectModel;

namespace RestaurantePro.Mobile.Core.Features.Ingredientes.ViewModels;

/// <summary>
/// ViewModel para la gestión de ingredientes
/// </summary>
public partial class IngredientesViewModel : BaseViewModel
{
    private readonly IIngredientesService _ingredientesService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<IngredientesViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<IngredienteSummaryDto> _ingredientes = new();

    [ObservableProperty]
    private IngredienteSummaryDto? _ingredienteSeleccionado;

    [ObservableProperty]
    private FiltroIngredientesDto _filtro = new();

    [ObservableProperty]
    private bool _mostrarSoloBajoStock;

    [ObservableProperty]
    private bool _mostrarSoloActivos = true;

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private ReporteValoracionDto? _reporteValoracion;

    [ObservableProperty]
    private List<IngredienteSummaryDto> _ingredientesBajoStock = new();

    [ObservableProperty]
    private bool _mostrarReporte = false;

    [ObservableProperty]
    private bool _estaCargando = false;

    [ObservableProperty]
    private bool _estaRefrescando = false;

    [ObservableProperty]
    private int _paginaActual = 1;

    [ObservableProperty]
    private bool _hayMasPaginas = true;

    public IngredientesViewModel(
        IIngredientesService ingredientesService,
        IDialogService dialogService,
        INavigationService navigationService,
        ILogger<IngredientesViewModel> logger)
    {
        _ingredientesService = ingredientesService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task CargarIngredientesAsync()
    {
        if (EstaCargando) return;

        try
        {
            EstaCargando = true;
            
            // Aplicar filtros
            Filtro.Busqueda = Busqueda;
            Filtro.SoloBajoStock = MostrarSoloBajoStock;
            Filtro.SoloActivos = MostrarSoloActivos;
            Filtro.PageNumber = 1;
            Filtro.PageSize = 20;

            var resultado = await _ingredientesService.ObtenerIngredientesAsync(MostrarSoloActivos);
            
            if (resultado.Succeeded)
            {
                foreach (var ingrediente in resultado.Data)
                {
                    Ingredientes.Add(ingrediente);
                }
                
                HayMasPaginas = false; // Por ahora, asumimos que no hay paginación
                PaginaActual++;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar ingredientes");
            await _dialogService.ShowAlertAsync("Error", "Error de conexión al cargar ingredientes");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasIngredientesAsync()
    {
        if (EstaCargando || !HayMasPaginas) return;

        try
        {
            EstaCargando = true;
            
            Filtro.PageNumber = PaginaActual + 1;
            var resultado = await _ingredientesService.ObtenerIngredientesAsync(MostrarSoloActivos);
            
            if (resultado.Succeeded)
            {
                foreach (var ingrediente in resultado.Data)
                {
                    Ingredientes.Add(ingrediente);
                }
                
                HayMasPaginas = false; // Por ahora, asumimos que no hay paginación
                PaginaActual++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar más ingredientes");
            await _dialogService.ShowAlertAsync("Error", "Error de conexión al cargar más ingredientes");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        if (EstaRefrescando) return;

        try
        {
            EstaRefrescando = true;
            await CargarIngredientesAsync();
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        await CargarIngredientesAsync();
    }

    [RelayCommand]
    private async Task AplicarFiltrosAsync()
    {
        await CargarIngredientesAsync();
    }

    [RelayCommand]
    private async Task CargarIngredientesBajoStockAsync()
    {
        try
        {
            var resultado = await _ingredientesService.ObtenerIngredientesBajoStockAsync();
            
            if (resultado.Succeeded)
            {
                IngredientesBajoStock = resultado.Data;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar ingredientes con bajo stock");
            await _dialogService.ShowAlertAsync("Error", "Error de conexión al cargar ingredientes con bajo stock");
        }
    }

    [RelayCommand]
    private async Task GenerarReporteValoracionAsync()
    {
        try
        {
            var resultado = await _ingredientesService.GenerarReporteValoracionAsync(Filtro);
            
            if (resultado.Succeeded)
            {
                ReporteValoracion = resultado.Data.FirstOrDefault();
                MostrarReporte = true;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de valoración");
            await _dialogService.ShowAlertAsync("Error", "Error de conexión al generar reporte");
        }
    }

    [RelayCommand]
    private async Task VerDetalleIngredienteAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAVer = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAVer == null) return;

        try
        {
            await _navigationService.NavigateToAsync("DetalleIngredientePage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAVer.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar al detalle del ingrediente");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir el detalle del ingrediente");
        }
    }

    [RelayCommand]
    private async Task CrearIngredienteAsync()
    {
        try
        {
            await _navigationService.NavigateToAsync("CrearIngredientePage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a crear ingrediente");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de crear ingrediente");
        }
    }

    [RelayCommand]
    private async Task EditarIngredienteAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAEditar = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAEditar == null) return;

        try
        {
            await _navigationService.NavigateToAsync("EditarIngredientePage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAEditar.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a editar ingrediente");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de editar ingrediente");
        }
    }

    [RelayCommand]
    private async Task EliminarIngredienteAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAEliminar = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAEliminar == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar eliminación",
            $"¿Está seguro que desea eliminar el ingrediente '{ingredienteAEliminar.Nombre}'? Esta acción no se puede deshacer.");

        if (!confirmacion) return;

        try
        {
            var resultado = await _ingredientesService.EliminarIngredienteAsync(ingredienteAEliminar.Id);
            
            if (resultado.Succeeded)
            {
                Ingredientes.Remove(ingredienteAEliminar);
                await _dialogService.ShowAlertAsync("Éxito", "Ingrediente eliminado correctamente");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ingrediente: {Id}", ingredienteAEliminar.Id);
            await _dialogService.ShowAlertAsync("Error", "Error de conexión al eliminar ingrediente");
        }
    }

    [RelayCommand]
    private async Task RegistrarMovimientoAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAMover = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAMover == null) return;

        try
        {
            await _navigationService.NavigateToAsync("RegistrarMovimientoPage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAMover.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a registrar movimiento");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de registrar movimiento");
        }
    }

    [RelayCommand]
    private async Task ConsumirStockAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAConsumir = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAConsumir == null) return;

        try
        {
            await _navigationService.NavigateToAsync("ConsumirStockPage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAConsumir.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a consumir stock");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de consumir stock");
        }
    }

    [RelayCommand]
    private async Task RegistrarLoteAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteALote = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteALote == null) return;

        try
        {
            await _navigationService.NavigateToAsync("RegistrarLotePage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteALote.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a registrar lote");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de registrar lote");
        }
    }

    [RelayCommand]
    private async Task VerMovimientosAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAMovimientos = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAMovimientos == null) return;

        try
        {
            await _navigationService.NavigateToAsync("MovimientosIngredientePage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAMovimientos.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a movimientos del ingrediente");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de movimientos");
        }
    }

    [RelayCommand]
    private async Task AsociarProveedorAsync(IngredienteSummaryDto? ingrediente = null)
    {
        var ingredienteAAsociar = ingrediente ?? IngredienteSeleccionado;
        if (ingredienteAAsociar == null) return;

        try
        {
            await _navigationService.NavigateToAsync("AsociarProveedorPage", new Dictionary<string, object>
            {
                { "IngredienteId", ingredienteAAsociar.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar a asociar proveedor");
            await _dialogService.ShowAlertAsync("Error", "Error al abrir la página de asociar proveedor");
        }
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        MostrarSoloBajoStock = false;
        MostrarSoloActivos = true;
        Filtro = new FiltroIngredientesDto();
    }

    [RelayCommand]
    private void OcultarReporte()
    {
        MostrarReporte = false;
        ReporteValoracion = null;
    }

    partial void OnBusquedaChanged(string value)
    {
        // Implementar búsqueda en tiempo real si es necesario
    }

    partial void OnMostrarSoloBajoStockChanged(bool value)
    {
        _ = AplicarFiltrosAsync();
    }

    partial void OnMostrarSoloActivosChanged(bool value)
    {
        _ = AplicarFiltrosAsync();
    }
} 