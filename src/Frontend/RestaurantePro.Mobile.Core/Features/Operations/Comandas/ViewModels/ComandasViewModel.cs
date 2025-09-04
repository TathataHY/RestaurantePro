using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Comandas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using ComandaModels = RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;

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
        _ = LoadEstadisticasAsync();
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
                fechaHasta: FiltroFecha?.AddDays(1),
                clienteNombre: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

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
    public async Task LoadEstadisticasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.ObtenerEstadisticasAsync();

            if (response.Success && response.Data != null)
            {
                Estadisticas = response.Data;
            }
            else
            {
                // Fallback seguro a cero para que la UI muestre siempre la banda
                Estadisticas = new EstadisticasComandasDto();
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

        var observaciones = await _dialogService.ShowPromptAsync(
            "Observaciones",
            "Ingrese observaciones para la comanda (opcional):");

        IsBusy = true;

        try
        {
            var request = new ComandaModels.CrearComandaRequest
            {
                MeseroId = "11111111-1111-1111-1111-111111111111", // Usuario administrador por defecto
                MesaId = mesaGuid.ToString(),
                ClienteId = null,
                Observaciones = observaciones,
                ProductosIniciales = new List<ComandaModels.ProductoComandaRequest>(), // Lista vacía por ahora
                Items = new List<ComandaModels.ProductoComandaRequest>() // Lista vacía por ahora
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
    /// Ver detalles de una comanda
    /// </summary>
    [RelayCommand]
    private async Task VerDetalleAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await NavigateToComandaDetailAsync(comanda);
    }

    /// <summary>
    /// Editar una comanda existente
    /// </summary>
    [RelayCommand]
    private async Task EditarAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await NavigateToAgregarProductosAsync(comanda);
    }

    /// <summary>
    /// Cambiar estado de una comanda
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoComandaAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        var estadosPermitidos = ObtenerEstadosPermitidos(comanda.Estado);
        
        if (!estadosPermitidos.Any())
        {
            await _dialogService.ShowAlertAsync("Información", 
                "No hay cambios de estado disponibles para esta comanda");
            return;
        }

        var nuevoEstado = await _dialogService.ShowActionSheetAsync(
            "Cambiar Estado",
            "Seleccione el nuevo estado:",
            "Cancelar",
            estadosPermitidos.ToArray());

        if (string.IsNullOrWhiteSpace(nuevoEstado) || nuevoEstado == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CambiarEstadoComandaAsync(comanda.Id, nuevoEstado);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado cambiado a {nuevoEstado}");
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

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Está seguro que desea finalizar la comanda #{comanda.Numero}?");

        if (!confirmacion) return;

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
            var response = await _comandasService.FinalizarComandaAsync(
                comanda.Id, 
                metodoPago, 
                $"Finalizada - Total: {comanda.Total:C}");

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} finalizada correctamente");
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

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar Cancelación",
            $"¿Está seguro que desea cancelar la comanda #{comanda.Numero}?");

        if (!confirmacion) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Motivo de Cancelación",
            "Ingrese el motivo de la cancelación:");

        if (string.IsNullOrWhiteSpace(motivo))
            return;

        IsBusy = true;

        try
        {
            var response = await _comandasService.CancelarComandaAsync(comanda.Id, motivo);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", 
                    $"Comanda #{comanda.Numero} cancelada correctamente");
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

    /// <summary>
    /// Mostrar/ocultar filtros
    /// </summary>
    [RelayCommand]
    private void MostrarFiltrosCommand()
    {
        // Esta funcionalidad se maneja desde la UI
        // Aquí solo notificamos el cambio si es necesario
    }

    #endregion

    #region Comandos de Filtros

    /// <summary>
    /// Aplicar filtros de búsqueda
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadComandasAsync();
    }

    /// <summary>
    /// Buscar comanda específica
    /// </summary>
    [RelayCommand]
    private async Task BuscarComandaAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await _dialogService.ShowAlertAsync("Información", "Ingrese un término de búsqueda");
            return;
        }

        // Primero intentar buscar por cliente
        await LoadComandasAsync();
        
        // Si no se encontraron resultados y el texto parece ser un número, buscar por número de comanda
        if (!Comandas.Any() && int.TryParse(SearchText, out var numeroComanda))
        {
            // Buscar en todas las comandas por número
            var response = await _comandasService.BuscarComandasAsync();
            if (response.Success && response.Data != null)
            {
                var comandaEncontrada = response.Data.FirstOrDefault(c => c.Numero.ToString() == numeroComanda.ToString());
                if (comandaEncontrada != null)
                {
                    Comandas.Clear();
                    Comandas.Add(comandaEncontrada);
                }
            }
        }
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
        
        await LoadComandasAsync();
    }

    /// <summary>
    /// Toggle entre solo activas y todas
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
    /// Navegar a la página de detalle de comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToComandaDetailAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync("comanda-detalle", new Dictionary<string, object>
        {
            ["comandaId"] = comanda.Id.ToString()
        });
    }

    /// <summary>
    /// Navegar a agregar productos a la comanda
    /// </summary>
    [RelayCommand]
    private async Task NavigateToAgregarProductosAsync(ComandaDto comanda)
    {
        if (comanda == null) return;

        await _navigationService.NavigateToAsync("productos", new Dictionary<string, object>
        {
            ["comandaId"] = comanda.Id.ToString()
        });
    }

    #endregion

    #region Métodos de Utilidad

    /// <summary>
    /// Obtener estados permitidos según el estado actual
    /// </summary>
    private List<string> ObtenerEstadosPermitidos(string estadoActual)
    {
        return estadoActual?.ToLower() switch
        {
            "pendiente" => new List<string> { "En Preparación", "Cancelada" },
            "en preparación" => new List<string> { "Lista", "Cancelada" },
            "lista" => new List<string> { "Entregada" },
            "entregada" => new List<string> { "Finalizada" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Obtener color basado en el estado de la comanda
    /// </summary>
    public string GetEstadoColor(string estado)
    {
        return estado?.ToLower() switch
        {
            "pendiente" => "Orange",
            "en preparación" => "Blue", 
            "lista" => "Green",
            "entregada" => "Purple",
            "finalizada" => "Gray",
            "cancelada" => "Red",
            _ => "Black"
        };
    }

    /// <summary>
    /// Obtener icono basado en el estado de la comanda
    /// </summary>
    public string GetEstadoIcon(string estado)
    {
        return estado?.ToLower() switch
        {
            "pendiente" => "⏳",
            "en preparación" => "👨‍🍳",
            "lista" => "✅",
            "entregada" => "🍽️",
            "finalizada" => "✔️",
            "cancelada" => "❌",
            _ => "?"
        };
    }

    #endregion
} 