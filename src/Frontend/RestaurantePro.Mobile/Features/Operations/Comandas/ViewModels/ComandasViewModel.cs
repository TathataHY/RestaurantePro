using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.ViewModels;

/// <summary>
/// ViewModel para gestión de comandas - Funcionalidad operativa principal
/// </summary>
public partial class ComandasViewModel : BaseViewModel
{
    private readonly IComandasService _comandasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<ComandaDto> comandas = new();

    [ObservableProperty]
    private ComandaDto? selectedComanda;

    [ObservableProperty]
    private EstadisticasComandasDto? estadisticas;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private DateTime? filtroFecha;

    [ObservableProperty]
    private Guid? filtroMesaId;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool soloActivas = true;

    #endregion

    #region Estados Disponibles

    public List<string> EstadosDisponibles => new()
    {
        "Todos",
        "Pendiente",
        "En Preparación", 
        "Lista",
        "Entregada",
        "Cancelada",
        "Finalizada"
    };

    #endregion

    #region Constructor

    public ComandasViewModel(
        IComandasService comandasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _comandasService = comandasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Gestión de Comandas";
        
        // Cargar datos iniciales
        _ = LoadComandasAsync();
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar todas las comandas con filtros aplicados
    /// </summary>
    [RelayCommand]
    private async Task LoadComandasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _comandasService.BuscarComandasAsync(
                estado: string.IsNullOrWhiteSpace(FiltroEstado) || FiltroEstado == "Todos" ? null : FiltroEstado,
                mesaId: FiltroMesaId,
                fechaDesde: FiltroFecha,
                fechaHasta: FiltroFecha?.AddDays(1));

            if (response.Success)
            {
                // Si solo queremos activas, filtrar localmente
                var comandasFiltradas = response.Data ?? new List<ComandaDto>();
                if (SoloActivas)
                {
                    comandasFiltradas = comandasFiltradas.Where(c => c.EstaActiva).ToList();
                }

                Comandas.Clear();
                foreach (var comanda in comandasFiltradas)
                {
                    Comandas.Add(comanda);
                }
            }
            else
            {
                ErrorMessage = response.Message ?? "Error al cargar las comandas";
                await _dialogService.ShowAlertAsync("Error", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error inesperado: {ex.Message}";
            await _dialogService.ShowAlertAsync("Error", ErrorMessage);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refrescar la lista de comandas
    /// </summary>
    [RelayCommand]
    private async Task RefreshComandasAsync()
    {
        IsRefreshing = true;
        await LoadComandasAsync();
    }

    /// <summary>
    /// Cargar estadísticas de comandas
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.ObtenerEstadisticasAsync();

            if (response.Success)
            {
                Estadisticas = response.Data;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "No se pudieron cargar las estadísticas");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar estadísticas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Comandos de Operaciones

    /// <summary>
    /// Crear una nueva comanda
    /// </summary>
    [RelayCommand]
    private async Task CrearComandaAsync()
    {
        var mesaId = await _dialogService.ShowPromptAsync(
            "Nueva Comanda",
            "Ingrese el ID de la mesa:");

        if (string.IsNullOrWhiteSpace(mesaId))
            return;

        if (!Guid.TryParse(mesaId, out var mesaGuid))
        {
            await _dialogService.ShowAlertAsync("Error", "ID de mesa inválido");
            return;
        }

        IsBusy = true;

        try
        {
            var request = new CrearComandaRequest
            {
                MesaId = mesaGuid,
                Observaciones = "Comanda creada desde móvil"
            };

            var response = await _comandasService.CrearComandaAsync(request);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Comanda creada correctamente");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al crear la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cambiar el estado de una comanda
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var estadosPermitidos = ObtenerEstadosPermitidos(comanda.Estado);
        
        if (!estadosPermitidos.Any())
        {
            await _dialogService.ShowAlertAsync("Información", "No hay cambios de estado disponibles para esta comanda");
            return;
        }

        var nuevoEstado = await _dialogService.ShowActionSheetAsync(
            "Cambiar Estado",
            "Seleccione el nuevo estado:",
            "Cancelar",
            estadosPermitidos.ToArray());

        if (string.IsNullOrWhiteSpace(nuevoEstado))
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(comanda.Id, nuevoEstado);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado cambiado a: {nuevoEstado}");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cambiar el estado");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Finalizar una comanda
    /// </summary>
    [RelayCommand]
    private async Task FinalizarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Está seguro de finalizar la comanda {comanda.Numero}?");

        if (!confirmar) return;

        // Preguntar método de pago
        var metodoPago = await _dialogService.ShowActionSheetAsync(
            "Método de Pago",
            "Seleccione el método de pago:",
            "Cancelar",
            "Efectivo", "Tarjeta", "Transferencia");

        if (string.IsNullOrWhiteSpace(metodoPago) || metodoPago == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.FinalizarComandaAsync(comanda.Id, metodoPago);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Comanda finalizada correctamente");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al finalizar la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cancelar una comanda
    /// </summary>
    [RelayCommand]
    private async Task CancelarComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Cancelar Comanda",
            $"Ingrese el motivo de cancelación para la comanda {comanda.Numero}:");

        if (string.IsNullOrWhiteSpace(motivo))
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CancelarComandaAsync(comanda.Id, motivo);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Comanda cancelada correctamente");
                await LoadComandasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al cancelar la comanda");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Comandos de Filtros

    /// <summary>
    /// Aplicar filtros a la lista de comandas
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadComandasAsync();
    }

    /// <summary>
    /// Limpiar todos los filtros
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        FiltroEstado = string.Empty;
        FiltroFecha = null;
        FiltroMesaId = null;
        SearchText = string.Empty;
        SoloActivas = true;
        
        await LoadComandasAsync();
    }

    /// <summary>
    /// Alternar entre comandas activas y todas
    /// </summary>
    [RelayCommand]
    private async Task ToggleActivasAsync()
    {
        SoloActivas = !SoloActivas;
        await LoadComandasAsync();
    }

    #endregion

    #region Comandos de Navegación

    /// <summary>
    /// Navegar al detalle de una comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToComandaDetailAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync($"comandadetail?id={comanda.Id}");
    }

    /// <summary>
    /// Navegar a agregar productos a una comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToAgregarProductosAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync($"agregarproductos?comandaId={comanda.Id}");
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Obtener los estados permitidos para una comanda según su estado actual
    /// </summary>
    private List<string> ObtenerEstadosPermitidos(string estadoActual)
    {
        return estadoActual.ToLowerInvariant() switch
        {
            "pendiente" => new List<string> { "En Preparación", "Cancelada" },
            "en_preparacion" => new List<string> { "Lista", "Cancelada" },
            "lista" => new List<string> { "Entregada" },
            "entregada" => new List<string> { "Finalizada" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Obtener el color del estado para la UI
    /// </summary>
    public string GetEstadoColor(string estado)
    {
        return estado.ToLowerInvariant() switch
        {
            "pendiente" => "#FFC107",      // Amarillo
            "en_preparacion" => "#FF9800", // Naranja
            "lista" => "#4CAF50",          // Verde
            "entregada" => "#2196F3",      // Azul
            "cancelada" => "#F44336",      // Rojo
            "finalizada" => "#9E9E9E",     // Gris
            _ => "#607D8B"                 // Gris azulado por defecto
        };
    }

    /// <summary>
    /// Obtener el icono del estado para la UI
    /// </summary>
    public string GetEstadoIcon(string estado)
    {
        return estado.ToLowerInvariant() switch
        {
            "pendiente" => "clock",
            "en_preparacion" => "chef_hat",
            "lista" => "check_circle",
            "entregada" => "delivery",
            "cancelada" => "cancel",
            "finalizada" => "done_all",
            _ => "help"
        };
    }

    #endregion
} 