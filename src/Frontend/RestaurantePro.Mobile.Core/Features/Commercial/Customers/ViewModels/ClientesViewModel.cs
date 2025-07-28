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
} 