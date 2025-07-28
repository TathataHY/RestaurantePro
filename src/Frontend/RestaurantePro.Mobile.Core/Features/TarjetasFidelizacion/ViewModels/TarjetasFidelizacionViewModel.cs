using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Commercial;

namespace RestaurantePro.Mobile.Core.Features.TarjetasFidelizacion.ViewModels;

/// <summary>
/// ViewModel para la gestión de tarjetas de fidelización
/// </summary>
public partial class TarjetasFidelizacionViewModel : BaseViewModel
{
    private readonly ITarjetasFidelizacionService _tarjetasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<TarjetaFidelizacionDto> _tarjetas = new();

    [ObservableProperty]
    private TarjetaFidelizacionDto? _tarjetaSeleccionada;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _filtroEstado = string.Empty;

    [ObservableProperty]
    private string _filtroNivel = string.Empty;

    [ObservableProperty]
    private Guid? _filtroClienteId;

    [ObservableProperty]
    private bool _mostrarFiltros = false;

    [ObservableProperty]
    private EstadisticasTarjetaDto? _estadisticas;

    [ObservableProperty]
    private List<HistorialPuntosDto> _historialPuntos = new();

    [ObservableProperty]
    private bool _mostrarHistorial = false;

    public TarjetasFidelizacionViewModel(
        ITarjetasFidelizacionService tarjetasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _tarjetasService = tarjetasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await CargarTarjetasAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar tarjetas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OnDisappearingAsync()
    {
        TarjetaSeleccionada = null;
        MostrarHistorial = false;
        HistorialPuntos.Clear();
    }

    [RelayCommand]
    public async Task CargarTarjetasAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var filtro = new FiltroTarjetasFidelizacionDto
            {
                Estado = string.IsNullOrEmpty(FiltroEstado) ? null : FiltroEstado,
                Nivel = string.IsNullOrEmpty(FiltroNivel) ? null : FiltroNivel,
                ClienteId = FiltroClienteId,
                PageNumber = 1,
                PageSize = 50
            };

            var result = await _tarjetasService.ObtenerTarjetasAsync(filtro);

            if (result.Succeeded)
            {
                Tarjetas.Clear();
                foreach (var tarjeta in result.Data)
                {
                    Tarjetas.Add(tarjeta);
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar tarjetas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task CrearTarjetaAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // TODO: Implementar navegación a página de creación
            await _dialogService.ShowAlertAsync("Info", "Funcionalidad de creación en desarrollo");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al crear tarjeta: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EditarTarjetaAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            // TODO: Implementar navegación a página de edición
            await _dialogService.ShowAlertAsync("Info", $"Editar tarjeta {TarjetaSeleccionada.NumeroTarjeta}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al editar tarjeta: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ActivarTarjetaAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            var confirmacion = await _dialogService.ShowConfirmAsync(
                "Confirmar Activación",
                $"¿Está seguro de que desea activar la tarjeta {TarjetaSeleccionada.NumeroTarjeta}?");

            if (!confirmacion) return;

            var result = await _tarjetasService.ActivarTarjetaAsync(TarjetaSeleccionada.NumeroTarjeta, TarjetaSeleccionada.ClienteNombre);

            if (result.Succeeded)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Tarjeta activada exitosamente");
                await CargarTarjetasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al activar tarjeta: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DesactivarTarjetaAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            var confirmacion = await _dialogService.ShowConfirmAsync(
                "Confirmar Desactivación",
                $"¿Está seguro de que desea desactivar la tarjeta {TarjetaSeleccionada.NumeroTarjeta}?");

            if (!confirmacion) return;

            var result = await _tarjetasService.DesactivarTarjetaAsync(TarjetaSeleccionada.Id);

            if (result.Succeeded)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Tarjeta desactivada exitosamente");
                await CargarTarjetasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al desactivar tarjeta: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AgregarPuntosAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            // TODO: Implementar diálogo para ingresar puntos
            await _dialogService.ShowAlertAsync("Info", "Funcionalidad de agregar puntos en desarrollo");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al agregar puntos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CanjearPuntosAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            // TODO: Implementar diálogo para canjear puntos
            await _dialogService.ShowAlertAsync("Info", "Funcionalidad de canjear puntos en desarrollo");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al canjear puntos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task VerHistorialAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            var result = await _tarjetasService.ObtenerHistorialPuntosAsync(TarjetaSeleccionada.Id);

            if (result.Succeeded)
            {
                HistorialPuntos.Clear();
                if (result.Data != null)
                {
                    // Si el servicio devuelve un solo objeto, lo agregamos a la lista
                    HistorialPuntos.Add(result.Data);
                }
                MostrarHistorial = true;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al cargar historial: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarEstadisticasAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            var result = await _tarjetasService.ObtenerEstadisticasAsync(TarjetaSeleccionada.Id);

            if (result.Succeeded)
            {
                Estadisticas = result.Data;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
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

    [RelayCommand]
    public async Task EliminarTarjetaAsync()
    {
        if (IsBusy || TarjetaSeleccionada == null) return;

        try
        {
            IsBusy = true;

            var confirmacion = await _dialogService.ShowConfirmAsync(
                "Confirmar Eliminación",
                $"¿Está seguro de que desea eliminar la tarjeta {TarjetaSeleccionada.NumeroTarjeta}? Esta acción no se puede deshacer.");

            if (!confirmacion) return;

            var result = await _tarjetasService.EliminarTarjetaAsync(TarjetaSeleccionada.Id);

            if (result.Succeeded)
            {
                await _dialogService.ShowAlertAsync("Éxito", "Tarjeta eliminada exitosamente");
                TarjetaSeleccionada = null;
                await CargarTarjetasAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al eliminar tarjeta: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AplicarFiltrosAsync()
    {
        await CargarTarjetasAsync();
        MostrarFiltros = false;
    }

    [RelayCommand]
    public async Task LimpiarFiltrosAsync()
    {
        FiltroEstado = string.Empty;
        FiltroNivel = string.Empty;
        FiltroClienteId = null;
        await CargarTarjetasAsync();
    }

    [RelayCommand]
    public async Task RefrescarAsync()
    {
        IsRefreshing = true;
        await CargarTarjetasAsync();
    }

    partial void OnTarjetaSeleccionadaChanged(TarjetaFidelizacionDto? value)
    {
        if (value != null)
        {
            _ = CargarEstadisticasAsync();
        }
        else
        {
            Estadisticas = null;
            MostrarHistorial = false;
            HistorialPuntos.Clear();
        }
    }
} 