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

    /// <summary>
    /// Estadísticas de facturas para la UI
    /// </summary>
    public object Estadisticas => new
    {
        TotalFacturas = Facturas.Count,
        FacturasPagadas = Facturas.Count(f => f.Estado == "Pagada"),
        FacturasPendientes = Facturas.Count(f => f.Estado == "Pendiente"),
        TotalVentas = Facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total)
    };

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

    /// <summary>
    /// Refrescar facturas
    /// </summary>
    [RelayCommand]
    private async Task RefreshFacturasAsync()
    {
        await CargarFacturasAsync();
    }

    /// <summary>
    /// Generar reporte
    /// </summary>
    [RelayCommand]
    private async Task GenerarReporteAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La generación de reportes no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Enviar facturas
    /// </summary>
    [RelayCommand]
    private async Task EnviarFacturasAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "El envío masivo de facturas no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear nueva factura
    /// </summary>
    [RelayCommand]
    private async Task CrearFacturaAsync()
    {
        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La creación de facturas no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Ver factura
    /// </summary>
    [RelayCommand]
    private async Task VerFacturaAsync(FacturaDto? factura)
    {
        if (factura == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Detalles de Factura", 
                $"Número: {factura.NumeroFactura}\nCliente: {factura.ClienteNombre}\nTotal: {factura.Total:C}\nEstado: {factura.Estado}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error al ver factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Generar PDF
    /// </summary>
    [RelayCommand]
    private async Task GenerarPdfAsync(FacturaDto? factura)
    {
        if (factura == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "La generación de PDF no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Enviar factura
    /// </summary>
    [RelayCommand]
    private async Task EnviarFacturaAsync(FacturaDto? factura)
    {
        if (factura == null) return;

        try
        {
            await _dialogService.ShowAlertAsync("Función no disponible", 
                "El envío de facturas no está disponible en esta versión. Contacta al administrador.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Error: {ex.Message}");
        }
    }
} 