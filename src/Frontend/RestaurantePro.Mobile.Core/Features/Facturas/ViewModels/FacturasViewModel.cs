using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Commercial;

namespace RestaurantePro.Mobile.Core.Features.Facturas.ViewModels;

/// <summary>
/// ViewModel para gestión de facturas
/// </summary>
public partial class FacturasViewModel : BaseViewModel
{
    private readonly IFacturasService _facturasService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<FacturaDto> facturas;

    [ObservableProperty]
    private FacturaDto? facturaSeleccionada;

    [ObservableProperty]
    private string filtroBusqueda = string.Empty;

    [ObservableProperty]
    private string filtroEstado = string.Empty;

    [ObservableProperty]
    private DateTime? fechaDesde;

    [ObservableProperty]
    private DateTime? fechaHasta;

    [ObservableProperty]
    private bool mostrarSoloPendientes;

    [ObservableProperty]
    private bool estaRefrescando;

    [ObservableProperty]
    private int totalFacturas;

    [ObservableProperty]
    private decimal totalVentas;

    [ObservableProperty]
    private int facturasPendientes;

    public FacturasViewModel(
        IFacturasService facturasService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _facturasService = facturasService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        Facturas = new ObservableCollection<FacturaDto>();
    }

    [RelayCommand]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await CargarFacturasAsync();
            await CargarEstadisticasAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar facturas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OnDisappearingAsync()
    {
        FacturaSeleccionada = null;
    }

    [RelayCommand]
    public async Task CargarFacturasAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var filtros = new FiltroFacturasDto
            {
                SearchTerm = string.IsNullOrWhiteSpace(FiltroBusqueda) ? null : FiltroBusqueda,
                Estado = string.IsNullOrWhiteSpace(FiltroEstado) ? null : FiltroEstado,
                FechaDesde = FechaDesde,
                FechaHasta = FechaHasta
            };

            var result = await _facturasService.ObtenerFacturasAsync(FechaDesde ?? DateTime.Today);

            if (result.Succeeded)
            {
                Facturas.Clear();
                foreach (var factura in result.Data)
                {
                    Facturas.Add(factura);
                }
                TotalFacturas = Facturas.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar facturas: {ex.Message}");
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
            var facturasPendientesResult = await _facturasService.ObtenerFacturasPendientesAsync();
            if (facturasPendientesResult.Succeeded)
            {
                FacturasPendientes = facturasPendientesResult.Data.Count;
            }

            // Calcular total de ventas de las facturas cargadas
            TotalVentas = Facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
        }
        catch (Exception ex)
        {
            // Log error silently for now
        }
    }

    [RelayCommand]
    public async Task RefrescarFacturasAsync()
    {
        if (EstaRefrescando) return;

        EstaRefrescando = true;
        try
        {
            await CargarFacturasAsync();
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
        await CargarFacturasAsync();
    }

    [RelayCommand]
    public async Task LimpiarFiltrosAsync()
    {
        FiltroBusqueda = string.Empty;
        FiltroEstado = string.Empty;
        FechaDesde = null;
        FechaHasta = null;
        MostrarSoloPendientes = false;
        await CargarFacturasAsync();
    }

    [RelayCommand]
    public async Task SeleccionarFacturaAsync(FacturaDto factura)
    {
        if (factura == null) return;

        FacturaSeleccionada = factura;
        await _navigationService.NavigateToAsync("FacturaDetallePage", new Dictionary<string, object>
        {
            { "FacturaId", factura.Id }
        });
    }

    [RelayCommand]
    public async Task CrearFacturaAsync()
    {
        await _navigationService.NavigateToAsync("CrearFacturaPage");
    }

    [RelayCommand]
    public async Task RegistrarPagoAsync(FacturaDto factura)
    {
        if (factura == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Desea registrar el pago de la factura {factura.NumeroFactura}?",
            "Registrar Pago");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var dto = new RegistrarPagoDto
            {
                FacturaId = factura.Id,
                MontoPagado = factura.Total,
                MetodoPago = "Efectivo",
                Observaciones = "Pago registrado desde móvil"
            };

            var result = await _facturasService.RegistrarPagoAsync(factura.Id, dto);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Pago registrado exitosamente");
                await RefrescarFacturasAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al registrar pago: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AnularFacturaAsync(FacturaDto factura)
    {
        if (factura == null) return;

        var confirmacion = await _dialogService.ShowConfirmAsync(
            $"¿Está seguro de que desea anular la factura {factura.NumeroFactura}?",
            "Anular Factura");

        if (!confirmacion) return;

        IsBusy = true;
        try
        {
            var dto = new AnularFacturaDto
            {
                FacturaId = factura.Id,
                MotivoAnulacion = "Anulada desde móvil",
                Observaciones = "Anulación solicitada por el usuario"
            };

            var result = await _facturasService.AnularFacturaAsync(factura.Id, dto);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Factura anulada exitosamente");
                await RefrescarFacturasAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al anular factura: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DescargarPdfAsync(FacturaDto factura)
    {
        if (factura == null) return;

        IsBusy = true;
        try
        {
            var result = await _facturasService.DescargarFacturaPdfAsync(factura.Id);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("PDF descargado exitosamente");
                // Aquí se podría implementar la lógica para guardar el PDF
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al descargar PDF: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EnviarPorEmailAsync(FacturaDto factura)
    {
        if (factura == null) return;

        var email = factura.ClienteEmail;
        if (string.IsNullOrWhiteSpace(email))
        {
            await _dialogService.ShowErrorAsync("No hay email disponible para esta factura");
            return;
        }

        if (string.IsNullOrWhiteSpace(email)) return;

        IsBusy = true;
        try
        {
            var result = await _facturasService.EnviarFacturaPorEmailAsync(factura.Id, email);

            if (result.Succeeded)
            {
                await _dialogService.ShowSuccessAsync("Factura enviada por email exitosamente");
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al enviar factura por email: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarFacturasPendientesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _facturasService.ObtenerFacturasPendientesAsync();

            if (result.Succeeded)
            {
                Facturas.Clear();
                foreach (var factura in result.Data)
                {
                    Facturas.Add(factura);
                }
                TotalFacturas = Facturas.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar facturas pendientes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CargarFacturasDelDiaAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var hoy = DateTime.Today;
            FechaDesde = hoy;
            FechaHasta = hoy.AddDays(1).AddSeconds(-1);

            await CargarFacturasAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al cargar facturas del día: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task BuscarFacturasAsync()
    {
        if (string.IsNullOrWhiteSpace(FiltroBusqueda)) return;

        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _facturasService.BuscarFacturasAsync(FiltroBusqueda, FechaDesde ?? DateTime.Today);

            if (result.Succeeded)
            {
                Facturas.Clear();
                foreach (var factura in result.Data)
                {
                    Facturas.Add(factura);
                }
                TotalFacturas = Facturas.Count;
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Error ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Error al buscar facturas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
} 