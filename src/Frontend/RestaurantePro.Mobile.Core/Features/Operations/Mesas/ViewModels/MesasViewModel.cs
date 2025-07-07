using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Operations.Mesas.ViewModels;

/// <summary>
/// ViewModel para gestión de mesas - Funcionalidad operativa principal
/// </summary>
public partial class MesasViewModel : BaseViewModel
{
    private readonly IMesasService _mesasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<MesaDto> mesas = new();

    [ObservableProperty]
    private MesaDto? selectedMesa;

    [ObservableProperty]
    private EstadoMesasDto? estadoMesas;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private string filtroUbicacion = string.Empty;

    [ObservableProperty]
    private int? filtroCapacidadMinima;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    #endregion

    #region Constructor

    public MesasViewModel(
        IMesasService mesasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _mesasService = mesasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Gestión de Mesas";
        
        // Cargar datos iniciales
        _ = LoadMesasAsync();
    }

    #endregion

    #region Comandos Principales

    /// <summary>
    /// Cargar todas las mesas con filtros aplicados
    /// </summary>
    [RelayCommand]
    private async Task LoadMesasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _mesasService.ObtenerMesasAsync(
                string.IsNullOrWhiteSpace(FiltroEstado) ? null : FiltroEstado,
                string.IsNullOrWhiteSpace(FiltroUbicacion) ? null : FiltroUbicacion,
                FiltroCapacidadMinima);

            if (response.Success)
            {
                Mesas.Clear();
                foreach (var mesa in response.Data ?? new List<MesaDto>())
                {
                    Mesas.Add(mesa);
                }
            }
            else
            {
                await ShowErrorAsync(response.Message ?? "Error al cargar las mesas");
                await _dialogService.ShowAlertAsync("Error", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Error inesperado: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", ErrorMessage);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refrescar la lista de mesas
    /// </summary>
    [RelayCommand]
    private async Task RefreshMesasAsync()
    {
        IsRefreshing = true;
        await LoadMesasAsync();
    }

    /// <summary>
    /// Cargar estadísticas de ocupación
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var response = await _mesasService.ObtenerEstadoOcupacionAsync();

            if (response.Success)
            {
                EstadoMesas = response.Data;
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
    /// Asignar una mesa a un cliente
    /// </summary>
    [RelayCommand]
    private async Task AsignarMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var clienteId = await _dialogService.ShowPromptAsync(
            "Asignar Mesa",
            $"Ingrese el ID del cliente para la mesa {mesa.Numero}:");

        if (string.IsNullOrWhiteSpace(clienteId))
            return;

        if (!Guid.TryParse(clienteId, out var clienteGuid))
        {
            await _dialogService.ShowAlertAsync("Error", "ID de cliente inválido");
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _mesasService.AsignarMesaAsync(mesa.Id, clienteGuid);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Mesa {mesa.Numero} asignada correctamente");
                await LoadMesasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al asignar la mesa");
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
    /// Liberar una mesa
    /// </summary>
    [RelayCommand]
    private async Task LiberarMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            "Confirmar",
            $"¿Está seguro que desea liberar la mesa {mesa.Numero}?");

        if (!confirmacion) return;

        var motivo = await _dialogService.ShowPromptAsync(
            "Motivo de Liberación",
            "Ingrese el motivo para liberar la mesa:");

        if (string.IsNullOrWhiteSpace(motivo))
            return;

        IsBusy = true;

        try
        {
            var response = await _mesasService.LiberarMesaAsync(mesa.Id, motivo);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Mesa {mesa.Numero} liberada correctamente");
                await LoadMesasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Error al liberar la mesa");
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
    /// Cambiar estado de una mesa
    /// </summary>
    [RelayCommand]
    private async Task CambiarEstadoMesaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        var nuevoEstado = await _dialogService.ShowActionSheetAsync(
            "Cambiar Estado",
            "Seleccione el nuevo estado:",
            "Cancelar",
            "Disponible", "Ocupada", "Reservada", "Mantenimiento");

        if (string.IsNullOrWhiteSpace(nuevoEstado) || nuevoEstado == "Cancelar")
            return;

        IsBusy = true;

        try
        {
            var response = await _mesasService.CambiarEstadoMesaAsync(mesa.Id, nuevoEstado);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Éxito", $"Estado de mesa {mesa.Numero} cambiado a {nuevoEstado}");
                await LoadMesasAsync();
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
    /// Buscar la mejor mesa disponible
    /// </summary>
    [RelayCommand]
    private async Task BuscarMejorMesaAsync()
    {
        var capacidadStr = await _dialogService.ShowPromptAsync(
            "Buscar Mesa",
            "Ingrese la capacidad requerida:");

        if (string.IsNullOrWhiteSpace(capacidadStr))
            return;

        if (!int.TryParse(capacidadStr, out var capacidad) || capacidad <= 0)
        {
            await _dialogService.ShowAlertAsync("Error", "Capacidad inválida");
            return;
        }

        IsBusy = true;

        try
        {
            var response = await _mesasService.BuscarMejorMesaAsync(capacidad);

            if (response.Success && response.Data != null)
            {
                var mesa = response.Data;
                var mensaje = $"Mesa sugerida: {mesa.Numero}\nCapacidad: {mesa.Capacidad}\nUbicación: {mesa.Zona}";
                
                var asignar = await _dialogService.ShowConfirmAsync(
                    "Mesa Encontrada",
                    mensaje + "\n\n¿Desea asignar esta mesa?");

                if (asignar)
                {
                    await AsignarMesaAsync(mesa);
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Información", "No se encontró una mesa disponible con esa capacidad");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al buscar mesa: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Comandos de Filtros

    /// <summary>
    /// Aplicar filtros de búsqueda
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadMesasAsync();
    }

    /// <summary>
    /// Limpiar todos los filtros
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        FiltroEstado = string.Empty;
        FiltroUbicacion = string.Empty;
        FiltroCapacidadMinima = null;
        SearchText = string.Empty;
        
        await LoadMesasAsync();
    }

    #endregion

    #region Comandos de Navegación

    /// <summary>
    /// Navegar a la página de detalle de mesa
    /// </summary>
    [RelayCommand]
    private async Task NavigateToMesaDetailAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        await _navigationService.NavigateToAsync("mesa-detalle", new Dictionary<string, object>
        {
            ["mesaId"] = mesa.Id.ToString()
        });
    }

    /// <summary>
    /// Navegar a crear nueva comanda para la mesa
    /// </summary>
    [RelayCommand]
    private async Task NavigateToNewComandaAsync(MesaDto mesa)
    {
        if (mesa == null) return;

        await _navigationService.NavigateToAsync("comandas", new Dictionary<string, object>
        {
            ["mesaId"] = mesa.Id.ToString()
        });
    }

    #endregion

    #region Métodos de Utilidad

    /// <summary>
    /// Obtener color basado en el estado de la mesa
    /// </summary>
    public string GetEstadoColor(string estado)
    {
        return estado?.ToLower() switch
        {
            "disponible" => "Green",
            "ocupada" => "Red",
            "reservada" => "Orange",
            "mantenimiento" => "Gray",
            _ => "Black"
        };
    }

    /// <summary>
    /// Obtener icono basado en el estado de la mesa
    /// </summary>
    public string GetEstadoIcon(string estado)
    {
        return estado?.ToLower() switch
        {
            "disponible" => "✓",
            "ocupada" => "●",
            "reservada" => "⏰",
            "mantenimiento" => "🔧",
            _ => "?"
        };
    }

    #endregion
} 