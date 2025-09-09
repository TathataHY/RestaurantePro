using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Models.ViewModels;

namespace RestaurantePro.Mobile.Core.Features.Analytics.ViewModels;

/// <summary>
/// ViewModel para analytics y métricas operativas
/// </summary>
public partial class AnalyticsViewModel : BaseViewModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    #region Propiedades Observables

    [ObservableProperty]
    private MetricasDiaDto? metricasDia;

    [ObservableProperty]
    private ObservableCollection<TopProductoDto> topProductos = new();

    [ObservableProperty]
    private OcupacionMesasDto? ocupacionMesas;

    [ObservableProperty]
    private TiempoPreparacionDto? tiempoPreparacion;

    [ObservableProperty]
    private ObservableCollection<VentasHoraDto> ventasPorHora = new();

    [ObservableProperty]
    private DateTime fechaSeleccionada = DateTime.Today;

    [ObservableProperty]
    private DateTime fechaDesde = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime fechaHasta = DateTime.Today;

    [ObservableProperty]
    private bool estaRefrescando;

    [ObservableProperty]
    private string filtroPeriodo = "Hoy";

    #endregion

    #region Constructor

    public AnalyticsViewModel(
        IAnalyticsService analyticsService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _analyticsService = analyticsService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        Title = "Analytics y Métricas";
    }

    #endregion

    #region Comandos

    /// <summary>
    /// Cargar analytics al aparecer la página
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OnAppearingAsync()
    {
        if (IsBusy) return;
        
        await ExecuteAsync(async () =>
        {
            await CargarMetricasDiaAsync();
            await CargarTopProductosAsync();
            await CargarOcupacionMesasAsync();
            await CargarTiempoPreparacionAsync();
            await CargarVentasPorHoraAsync();
        });
    }

    /// <summary>
    /// Cargar métricas del día
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarMetricasDiaAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            var response = await _analyticsService.ObtenerMetricasDiaAsync();
            
            if (response.Success && response.Data != null)
            {
                MetricasDia = response.Data;
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar métricas del día");
            }
        });
    }

    /// <summary>
    /// Cargar top productos
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarTopProductosAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            var response = await _analyticsService.ObtenerTopProductosAsync(10, FechaDesde, FechaHasta);
            
            if (response.Success && response.Data != null)
            {
                TopProductos.Clear();
                foreach (var producto in response.Data)
                {
                    TopProductos.Add(producto);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar top productos");
            }
        });
    }

    /// <summary>
    /// Cargar ocupación de mesas
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarOcupacionMesasAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            var response = await _analyticsService.ObtenerOcupacionMesasAsync(FechaSeleccionada);
            
            if (response.Success && response.Data != null)
            {
                OcupacionMesas = response.Data;
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar ocupación de mesas");
            }
        });
    }

    /// <summary>
    /// Cargar tiempo de preparación
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarTiempoPreparacionAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            var response = await _analyticsService.ObtenerTiempoPreparacionAsync(FechaDesde, FechaHasta);
            
            if (response.Success && response.Data != null)
            {
                TiempoPreparacion = response.Data;
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar tiempo de preparación");
            }
        });
    }

    /// <summary>
    /// Cargar ventas por hora
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CargarVentasPorHoraAsync()
    {
        if (IsBusy) return;

        await ExecuteAsync(async () =>
        {
            var response = await _analyticsService.ObtenerVentasPorHoraAsync(FechaSeleccionada);
            
            if (response.Success && response.Data != null)
            {
                VentasPorHora.Clear();
                foreach (var venta in response.Data)
                {
                    VentasPorHora.Add(venta);
                }
            }
            else
            {
                await _dialogService.ShowErrorAsync(response.Message ?? "Error al cargar ventas por hora");
            }
        });
    }

    /// <summary>
    /// Refrescar todas las métricas
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task RefrescarAsync()
    {
        EstaRefrescando = true;
        
        await CargarMetricasDiaAsync();
        await CargarTopProductosAsync();
        await CargarOcupacionMesasAsync();
        await CargarTiempoPreparacionAsync();
        await CargarVentasPorHoraAsync();
        
        EstaRefrescando = false;
    }

    /// <summary>
    /// Cambiar período de análisis
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CambiarPeriodoAsync()
    {
        switch (FiltroPeriodo)
        {
            case "Hoy":
                FechaDesde = DateTime.Today;
                FechaHasta = DateTime.Today;
                break;
            case "Esta Semana":
                FechaDesde = DateTime.Today.AddDays(-7);
                FechaHasta = DateTime.Today;
                break;
            case "Este Mes":
                FechaDesde = DateTime.Today.AddDays(-30);
                FechaHasta = DateTime.Today;
                break;
            case "Personalizado":
                // Aquí se podría abrir un diálogo para seleccionar fechas
                break;
        }
        
        await RefrescarAsync();
    }

    /// <summary>
    /// Ver detalle de producto
    /// </summary>
    [RelayCommand]
    public async Task VerDetalleProductoAsync(TopProductoDto producto)
    {
        if (producto == null) return;

        await _navigationService.NavigateToAsync("productodetalle", new Dictionary<string, object>
        {
            { "productoId", producto.ProductoId }
        });
    }

    #endregion
} 