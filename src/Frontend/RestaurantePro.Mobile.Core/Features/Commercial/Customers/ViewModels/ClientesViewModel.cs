using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Commercial.Customers.ViewModels;

/// <summary>
/// ViewModel para gestión de clientes
/// </summary>
public partial class ClientesViewModel : BaseViewModel
{
    private readonly IClientesService _clientesService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<ClienteSummaryDto> clientes = new();

    [ObservableProperty]
    private ClienteSummaryDto? clienteSeleccionado;

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private bool _soloActivos = true;

    [ObservableProperty]
    private int _totalClientes;

    [ObservableProperty]
    private int _clientesActivos;

    [ObservableProperty]
    private int _clientesNuevosHoy;

    #endregion

    /// <summary>
    /// Estadísticas de clientes para la UI
    /// </summary>
    public object Estadisticas => new
    {
        TotalClientes = Clientes.Count,
        ClientesActivos = Clientes.Count(c => c.Activo),
        ClientesNuevos = ClientesNuevosHoy,
        ClientesVIP = Clientes.Count(c => c.EsVIP)
    };

    public ClientesViewModel(
        IClientesService clientesService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clientesService = clientesService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    #region Comandos

    [RelayCommand]
    private async Task OnAppearingAsync()
    {
        await CargarClientesAsync();
        await CargarEstadisticasAsync();
    }

    [RelayCommand]
    private async Task CargarClientesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _clientesService.ObtenerClientesAsync(SoloActivos);
            
            if (response.Success && response.Data != null)
            {
                Clientes.Clear();
                foreach (var cliente in response.Data)
                {
                    Clientes.Add(cliente);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar clientes");
            }
        });
    }

    [RelayCommand]
    private async Task BuscarClientesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _clientesService.BuscarClientesAsync(Busqueda);
            
            if (response.Success && response.Data != null)
            {
                Clientes.Clear();
                foreach (var cliente in response.Data)
                {
                    Clientes.Add(cliente);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al buscar clientes");
            }
        });
    }

    [RelayCommand]
    private async Task CambiarFiltroActivosAsync()
    {
        await CargarClientesAsync();
    }

    [RelayCommand]
    private async Task SeleccionarClienteAsync(ClienteSummaryDto cliente)
    {
        if (cliente == null) return;

        ClienteSeleccionado = cliente;
        // Navegar a detalle de cliente
        await _navigationService.NavigateToAsync("clientedetalle", new Dictionary<string, object>
        {
            { "clienteId", cliente.Id }
        });
    }

    [RelayCommand]
    private async Task VerHistorialAsync(ClienteSummaryDto cliente)
    {
        if (cliente == null) return;

        await ExecuteAsync(async () =>
        {
            var response = await _clientesService.ObtenerHistorialComandasAsync(cliente.Id);
            
            if (response.Success && response.Data != null)
            {
                // Navegar a historial de comandas del cliente
                await _navigationService.NavigateToAsync("historialcliente", new Dictionary<string, object>
                {
                    { "clienteId", cliente.Id },
                    { "historial", response.Data }
                });
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar historial");
            }
        });
    }

    [RelayCommand]
    private async Task CrearClienteAsync()
    {
        await _navigationService.NavigateToAsync("crearcliente");
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        await CargarClientesAsync();
        await CargarEstadisticasAsync();
    }

    #endregion

    #region Métodos Privados

    private async Task CargarEstadisticasAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _clientesService.ObtenerEstadisticasAsync();
            
            if (response.Success && response.Data != null)
            {
                TotalClientes = response.Data.TotalClientes;
                ClientesActivos = response.Data.ClientesActivos;
                ClientesNuevosHoy = response.Data.ClientesNuevosHoy;
            }
        }, showLoading: false);
    }

    #endregion

    /// <summary>
    /// Refrescar clientes
    /// </summary>
    [RelayCommand]
    private async Task RefreshClientesAsync()
    {
        await CargarClientesAsync();
    }

    /// <summary>
    /// Cargar estadísticas
    /// </summary>
    [RelayCommand]
    private async Task LoadEstadisticasAsync()
    {
        await CargarEstadisticasAsync();
    }

    /// <summary>
    /// Filtrar VIP
    /// </summary>
    [RelayCommand]
    private async Task FiltrarVIPAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "El filtro VIP no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver cliente
    /// </summary>
    [RelayCommand]
    private async Task VerClienteAsync(ClienteSummaryDto? cliente)
    {
        if (cliente == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Detalles de Cliente", 
                $"Nombre: {cliente.NombreCompleto}\nEmail: {cliente.Email}\nTeléfono: {cliente.Telefono}\nActivo: {(cliente.Activo ? "Sí" : "No")}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al ver cliente: {ex.Message}");
        }
    }

    /// <summary>
    /// Editar cliente
    /// </summary>
    [RelayCommand]
    private async Task EditarClienteAsync(ClienteSummaryDto? cliente)
    {
        if (cliente == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La edición de clientes no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggle VIP
    /// </summary>
    [RelayCommand]
    private async Task ToggleVIPAsync(ClienteSummaryDto? cliente)
    {
        if (cliente == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "El cambio de estado VIP no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }
} 