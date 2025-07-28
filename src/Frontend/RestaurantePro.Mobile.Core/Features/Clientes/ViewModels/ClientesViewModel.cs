using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Commercial;

namespace RestaurantePro.Mobile.Core.Features.Clientes.ViewModels;

/// <summary>
/// ViewModel para gestión de clientes
/// </summary>
public partial class ClientesViewModel : BaseViewModel
{
    private readonly IClientesService _clientesService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ClienteSummaryDto> clientes;

    [ObservableProperty]
    private ClienteSummaryDto? clienteSeleccionado;

    [ObservableProperty]
    private string filtroBusqueda = string.Empty;

    [ObservableProperty]
    private string filtroSegmento = string.Empty;

    [ObservableProperty]
    private bool mostrarSoloActivos = true;

    [ObservableProperty]
    private bool mostrarSoloConTarjetaFidelizacion = false;

    [ObservableProperty]
    private bool mostrarSoloClientesFrecuentes = false;

    [ObservableProperty]
    private DateTime? fechaRegistroDesde;

    [ObservableProperty]
    private DateTime? fechaRegistroHasta;

    [ObservableProperty]
    private bool estaRefrescando;

    [ObservableProperty]
    private int totalClientes;

    [ObservableProperty]
    private int clientesActivos;

    [ObservableProperty]
    private int clientesConTarjetaFidelizacion;

    public ClientesViewModel(
        IClientesService clientesService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clientesService = clientesService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        Clientes = new ObservableCollection<ClienteSummaryDto>();
    }

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await CargarClientesAsync();
            await CargarEstadisticasAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OnDisappearingAsync()
    {
        ClienteSeleccionado = null;
    }

    [RelayCommand]
    public async Task CargarClientesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var filtros = new FiltroClientesDto
            {
                SearchTerm = string.IsNullOrWhiteSpace(FiltroBusqueda) ? null : FiltroBusqueda,
                SoloActivos = MostrarSoloActivos,
                Segmento = string.IsNullOrWhiteSpace(FiltroSegmento) ? null : FiltroSegmento,
                SoloConTarjetaFidelizacion = MostrarSoloConTarjetaFidelizacion ? true : null,
                SoloClientesFrecuentes = MostrarSoloClientesFrecuentes,
                FechaRegistroDesde = FechaRegistroDesde,
                FechaRegistroHasta = FechaRegistroHasta
            };

            var result = await _clientesService.ObtenerClientesAsync(filtros);

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = result.Data.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarEstadisticasAsync()
    {
        try
        {
            // Cargar estadísticas básicas
            var clientesActivosResult = await _clientesService.ObtenerClientesAsync(new FiltroClientesDto { SoloActivos = true });
            if (clientesActivosResult.Succeeded)
            {
                ClientesActivos = clientesActivosResult.Data.Count;
            }

            var clientesConTarjetaResult = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();
            if (clientesConTarjetaResult.Succeeded)
            {
                ClientesConTarjetaFidelizacion = clientesConTarjetaResult.Data.Count;
            }
        }
        catch (Exception ex)
        {
            // Log error silently for now
        }
    }

    [RelayCommand]
    public async Task RefrescarClientesAsync()
    {
        if (EstaRefrescando) return;

        EstaRefrescando = true;
        try
        {
            await CargarClientesAsync();
            await CargarEstadisticasAsync();
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand]
    public async Task AplicarFiltrosAsync()
    {
        await CargarClientesAsync();
    }

    [RelayCommand]
    public async Task LimpiarFiltrosAsync()
    {
        FiltroBusqueda = string.Empty;
        FiltroSegmento = string.Empty;
        MostrarSoloActivos = true;
        MostrarSoloConTarjetaFidelizacion = false;
        MostrarSoloClientesFrecuentes = false;
        FechaRegistroDesde = null;
        FechaRegistroHasta = null;
        await CargarClientesAsync();
    }

    [RelayCommand]
    public async Task SeleccionarClienteAsync(ClienteSummaryDto cliente)
    {
        if (cliente == null) return;

        ClienteSeleccionado = cliente;
        await _navigationService.NavigateToAsync("ClienteDetallePage", new Dictionary<string, object>
        {
            { "ClienteId", cliente.Id }
        });
    }

    [RelayCommand]
    public async Task CrearClienteAsync()
    {
        await _navigationService.NavigateToAsync("CrearClientePage");
    }

    [RelayCommand]
    public async Task BuscarClientesAsync()
    {
        if (string.IsNullOrWhiteSpace(FiltroBusqueda)) return;

        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _clientesService.BuscarClientesAsync(FiltroBusqueda);

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = Clientes.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al buscar clientes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarClientesPorSegmentoAsync(string segmento)
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _clientesService.ObtenerClientesPorSegmentoAsync(segmento);

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = Clientes.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes por segmento: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarClientesConTarjetaFidelizacionAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = Clientes.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes con tarjeta de fidelización: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarClientesFrecuentesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _clientesService.ObtenerClientesFrecuentesAsync();

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = Clientes.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes frecuentes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarClientesDelMesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var result = await _clientesService.ObtenerClientesPorFechaRegistroAsync(inicioMes, finMes);

            if (result.Succeeded)
            {
                Clientes.Clear();
                foreach (var cliente in result.Data)
                {
                    Clientes.Add(cliente);
                }
                TotalClientes = Clientes.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar clientes del mes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DesactivarClienteAsync(ClienteSummaryDto cliente)
    {
        if (cliente == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Está seguro de que desea desactivar al cliente {cliente.NombreCompleto}?",
            "Desactivar Cliente");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var result = await _clientesService.DesactivarClienteAsync(cliente.Id);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Cliente desactivado exitosamente");
                await RefrescarClientesAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al desactivar cliente: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
} 