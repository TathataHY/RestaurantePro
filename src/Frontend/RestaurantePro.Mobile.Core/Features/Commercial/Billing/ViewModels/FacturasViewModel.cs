using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Commercial.Billing.ViewModels;

/// <summary>
/// ViewModel para gestión de facturas
/// </summary>
public partial class FacturasViewModel : BaseViewModel
{
    private readonly IFacturasService _facturasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private ObservableCollection<FacturaDto> _facturas = new();

    [ObservableProperty]
    private FacturaDto? _facturaSeleccionada;

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private DateTime _fechaSeleccionada = DateTime.Today;

    [ObservableProperty]
    private int _totalFacturasHoy;

    [ObservableProperty]
    private decimal _totalVentasHoy;

    [ObservableProperty]
    private decimal _promedioFacturaHoy;

    #endregion

    public FacturasViewModel(
        IFacturasService facturasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _facturasService = facturasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    #region Comandos

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        await CargarFacturasAsync();
        await CargarEstadisticasAsync();
    }

    [RelayCommand]
    private async Task CargarFacturasAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _facturasService.ObtenerFacturasAsync(FechaSeleccionada);
            
            if (response.Success && response.Data != null)
            {
                Facturas.Clear();
                foreach (var factura in response.Data)
                {
                    Facturas.Add(factura);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar facturas");
            }
        });
    }

    [RelayCommand]
    private async Task BuscarFacturasAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _facturasService.BuscarFacturasAsync(Busqueda, FechaSeleccionada);
            
            if (response.Success && response.Data != null)
            {
                Facturas.Clear();
                foreach (var factura in response.Data)
                {
                    Facturas.Add(factura);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al buscar facturas");
            }
        });
    }

    [RelayCommand]
    private async Task CambiarFechaAsync()
    {
        await CargarFacturasAsync();
        await CargarEstadisticasAsync();
    }

    [RelayCommand]
    private async Task SeleccionarFacturaAsync(FacturaDto factura)
    {
        if (factura == null) return;

        FacturaSeleccionada = factura;
        // Navegar a detalle de factura
        await _navigationService.NavigateToAsync("facturadetalle", new Dictionary<string, object>
        {
            { "facturaId", factura.Id }
        });
    }

    [RelayCommand]
    private async Task ImprimirFacturaAsync(FacturaDto factura)
    {
        if (factura == null) return;

        await ExecuteAsync(async () =>
        {
            var response = await _facturasService.ImprimirFacturaAsync(factura.Id);
            
            if (response.Success)
            {
                await _dialogService.ShowSuccessAsync("Factura enviada a impresión");
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al imprimir factura");
            }
        });
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        await CargarFacturasAsync();
        await CargarEstadisticasAsync();
    }

    #endregion

    #region Métodos Privados

    private async Task CargarEstadisticasAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _facturasService.ObtenerEstadisticasAsync(DateTime.Today);
            
            if (response.Success && response.Data != null)
            {
                TotalFacturasHoy = response.Data.TotalFacturas;
                TotalVentasHoy = response.Data.TotalVentas;
                PromedioFacturaHoy = response.Data.PromedioFactura;
            }
        }, showLoading: false);
    }

    #endregion
} 