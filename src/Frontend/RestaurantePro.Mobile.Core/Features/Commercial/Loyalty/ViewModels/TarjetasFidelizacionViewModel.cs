using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Commercial.Loyalty.ViewModels;

/// <summary>
/// ViewModel para gestión de tarjetas de fidelización
/// </summary>
public partial class TarjetasFidelizacionViewModel : BaseViewModel
{
    private readonly ITarjetasFidelizacionService _tarjetasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private TarjetaFidelizacionDto? _tarjetaActual;

    [ObservableProperty]
    private ObservableCollection<TransaccionPuntosDto> _historial = new();

    [ObservableProperty]
    private string _codigoTarjeta = string.Empty;

    #endregion

    public TarjetasFidelizacionViewModel(
        ITarjetasFidelizacionService tarjetasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _tarjetasService = tarjetasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    #region Comandos

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        // Limpiar estado al aparecer
        TarjetaActual = null;
        Historial.Clear();
        CodigoTarjeta = string.Empty;
    }

    [RelayCommand]
    private async Task BuscarTarjetaAsync()
    {
        if (string.IsNullOrWhiteSpace(CodigoTarjeta))
        {
            await _dialogService.ShowErrorAsync("Ingrese un código de tarjeta válido");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var response = await _tarjetasService.ObtenerTarjetaPorCodigoAsync(CodigoTarjeta);
            
            if (response.Success && response.Data != null)
            {
                TarjetaActual = response.Data;
                await CargarHistorialAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Tarjeta no encontrada");
            }
        });
    }

    [RelayCommand]
    private async Task ActivarTarjetaAsync()
    {
        if (string.IsNullOrWhiteSpace(CodigoTarjeta))
        {
            await _dialogService.ShowErrorAsync("Ingrese un código de tarjeta válido");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var response = await _tarjetasService.ActivarTarjetaAsync(CodigoTarjeta, "Cliente");
            
            if (response.Success && response.Data != null)
            {
                TarjetaActual = response.Data;
                await _dialogService.ShowSuccessAsync("Tarjeta activada correctamente");
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al activar tarjeta");
            }
        });
    }

    [RelayCommand]
    private async Task AplicarDescuentoAsync()
    {
        if (TarjetaActual == null)
        {
            await _dialogService.ShowErrorAsync("Debe seleccionar una tarjeta primero");
            return;
        }

        // Navegar a pantalla de aplicación de descuento
        await _navigationService.NavigateToAsync("aplicardescuento", new Dictionary<string, object>
        {
            { "tarjetaId", TarjetaActual.Id }
        });
    }

    [RelayCommand]
    private async Task CargarHistorialAsync()
    {
        if (TarjetaActual == null) return;

        await ExecuteAsync(async () =>
        {
            var response = await _tarjetasService.ObtenerHistorialAsync(TarjetaActual.Id);
            
            if (response.Success && response.Data != null)
            {
                Historial.Clear();
                foreach (var transaccion in response.Data)
                {
                    Historial.Add(transaccion);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar historial");
            }
        }, showLoading: false);
    }

    [RelayCommand]
    private async Task RecargarPuntosAsync()
    {
        if (TarjetaActual == null)
        {
            await _dialogService.ShowErrorAsync("Debe seleccionar una tarjeta primero");
            return;
        }

        // Navegar a pantalla de recarga de puntos
        await _navigationService.NavigateToAsync("recargarpuntos", new Dictionary<string, object>
        {
            { "tarjetaId", TarjetaActual.Id }
        });
    }

    [RelayCommand]
    private async Task BloquearTarjetaAsync()
    {
        if (TarjetaActual == null)
        {
            await _dialogService.ShowErrorAsync("Debe seleccionar una tarjeta primero");
            return;
        }

        var confirmacion = await _dialogService.ShowConfirmationAsync(
            "Bloquear Tarjeta",
            "¿Está seguro que desea bloquear esta tarjeta? Esta acción no se puede deshacer.");

        if (confirmacion)
        {
            await ExecuteAsync(async () =>
            {
                var response = await _tarjetasService.BloquearTarjetaAsync(TarjetaActual.Id);
                
                if (response.Success)
                {
                    // Recargar la tarjeta actual para obtener el estado actualizado
                    var tarjetaResponse = await _tarjetasService.ObtenerTarjetaAsync(TarjetaActual.Id);
                    if (tarjetaResponse.Success && tarjetaResponse.Data != null)
                    {
                        TarjetaActual = tarjetaResponse.Data;
                    }
                    await _dialogService.ShowSuccessAsync("Tarjeta bloqueada correctamente");
                }
                else
                {
                    await _dialogService.ShowErrorAsync(response.Message ?? "Error al bloquear tarjeta");
                }
            });
        }
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        if (TarjetaActual != null)
        {
            await CargarHistorialAsync();
        }
    }

    #endregion
} 