using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Application.Common.Models.Dashboard;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Common.Services;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard administrativo
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IFacturaRepository _facturaRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IComandaRepository comandaRepository,
        IFacturaRepository facturaRepository,
        IMesaRepository mesaRepository,
        IProductoRepository productoRepository,
        ILogger<DashboardService> logger)
    {
        _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
        _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
        _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DashboardResumenDto> ObtenerResumenAsync(string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("📊 Obteniendo resumen completo del dashboard - Período: {Periodo}, Turno: {Turno}", periodo, turno);

        try
        {
            var metricas = await ObtenerMetricasAsync(periodo, turno);
            var productosMasVendidos = await ObtenerProductosMasVendidosAsync(5, periodo, turno);
            var ventasPorPeriodo = await ObtenerVentasPorPeriodoAsync(7, periodo, turno);
            var estadoMesas = await ObtenerEstadoMesasAsync();
            var comandasPorEstado = await ObtenerComandasPorEstadoAsync(periodo, turno);
            var ingresosPorHora = await ObtenerIngresosPorHoraAsync(periodo, turno);

            return new DashboardResumenDto
            {
                Metricas = metricas,
                ProductosMasVendidos = productosMasVendidos,
                VentasPorPeriodo = ventasPorPeriodo,
                EstadoMesas = estadoMesas,
                ComandasPorEstado = comandasPorEstado,
                IngresosPorHora = ingresosPorHora,
                UltimaActualizacion = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener resumen del dashboard");
            throw;
        }
    }

    public async Task<DashboardMetricasDto> ObtenerMetricasAsync(string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("📈 Obteniendo métricas del dashboard - Período: {Periodo}, Turno: {Turno}", periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            // Obtener facturas del período
            var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var facturasFiltradas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();

            // Calcular ventas
            var ventas = facturasFiltradas.Sum(f => f.Total);

            // Obtener comandas del período
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var comandasActivas = comandas.Count(c => c.Estado == EstadoComanda.EnProceso);

            // Obtener mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);

            // Calcular crecimiento de ventas (comparar con período anterior)
            var (fechaInicioAnterior, fechaFinAnterior) = CalcularRangoFechasAnterior(periodo);
            var facturasAnterior = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioAnterior, fechaFinAnterior);
            var ventasAnterior = facturasAnterior.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            var crecimientoVentas = ventasAnterior > 0
                ? ((ventas - ventasAnterior) / ventasAnterior) * 100
                : 0;

            // Calcular métricas específicas según el período
            var (ventasHoy, ventasAyer, ventasSemana, ventasMes) = await CalcularMetricasVentasPorPeriodo(periodo, turno);

            return new DashboardMetricasDto
            {
                VentasHoy = ventasHoy,
                VentasAyer = ventasAyer,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                ComandasActivas = comandasActivas,
                MesasOcupadas = mesasOcupadas,
                TotalMesas = mesas.Count(),
                CrecimientoVentas = crecimientoVentas,
                UltimaActualizacion = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener métricas del dashboard");
            throw;
        }
    }

    public async Task<List<DashboardProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(int cantidad = 5, string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("🍽️ Obteniendo productos más vendidos - Cantidad: {Cantidad}, Período: {Periodo}, Turno: {Turno}", cantidad, periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var facturasFiltradas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();

            var productosVendidos = facturasFiltradas
                .SelectMany(f => f.Detalles)
                .GroupBy(d => new { d.ProductoId, d.Descripcion })
                .Select(g => new DashboardProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Descripcion,
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToList();

            return productosVendidos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos más vendidos");
            throw;
        }
    }

    public async Task<List<DashboardVentaPorPeriodoDto>> ObtenerVentasPorPeriodoAsync(int dias = 7, string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("📊 Obteniendo ventas por período - Días: {Dias}, Período: {Periodo}, Turno: {Turno}", dias, periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            var ventasPorDia = new List<DashboardVentaPorPeriodoDto>();

            for (int i = 0; i < dias; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                var fechaInicioDia = fecha.Date.AddHours(horaInicio);
                var fechaFinDia = fecha.Date.AddHours(horaFin);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioDia, fechaFinDia);
                var ventasDia = facturas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

                ventasPorDia.Add(new DashboardVentaPorPeriodoDto
                {
                    Fecha = fecha,
                    Ventas = ventasDia
                });
            }

            return ventasPorDia;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ventas por período");
            throw;
        }
    }

    public async Task<DashboardEstadoMesasDto> ObtenerEstadoMesasAsync()
    {
        _logger.LogInformation("🪑 Obteniendo estado de las mesas");

        try
        {
            var mesas = await _mesaRepository.ObtenerTodasAsync();

            return new DashboardEstadoMesasDto
            {
                Disponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible),
                Ocupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada),
                Reservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada),
                EnLimpieza = mesas.Count(m => m.Estado == EstadoMesa.EnLimpieza),
                Total = mesas.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estado de las mesas");
            throw;
        }
    }

    public async Task<DashboardComandasPorEstadoDto> ObtenerComandasPorEstadoAsync(string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("📋 Obteniendo comandas por estado - Período: {Periodo}, Turno: {Turno}", periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            return new DashboardComandasPorEstadoDto
            {
                Pendientes = comandas.Count(c => c.Estado == EstadoComanda.Creada),
                EnProceso = comandas.Count(c => c.Estado == EstadoComanda.EnProceso),
                Listas = comandas.Count(c => c.Estado == EstadoComanda.Lista),
                Entregadas = comandas.Count(c => c.Estado == EstadoComanda.Entregada),
                Canceladas = comandas.Count(c => c.Estado == EstadoComanda.Cancelada),
                Total = comandas.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener comandas por estado");
            throw;
        }
    }

    public async Task<List<DashboardIngresosPorHoraDto>> ObtenerIngresosPorHoraAsync(string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("💰 Obteniendo ingresos por hora - Período: {Periodo}, Turno: {Turno}", periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            var ingresosPorHora = new List<DashboardIngresosPorHoraDto>();

            for (int hora = horaInicio; hora < horaFin; hora++)
            {
                var horaInicioActual = fechaInicio.Date.AddHours(hora);
                var horaFinActual = fechaInicio.Date.AddHours(hora + 1);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(horaInicioActual, horaFinActual);
                var ingresos = facturas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

                ingresosPorHora.Add(new DashboardIngresosPorHoraDto
                {
                    Hora = hora,
                    Ingresos = ingresos
                });
            }

            return ingresosPorHora;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingresos por hora");
            throw;
        }
    }

    #region Métodos Privados

    private (DateTime fechaInicio, DateTime fechaFin) CalcularRangoFechas(string? periodo)
    {
        var hoy = DateTime.Today;
        var ahora = DateTime.Now;

        return periodo switch
        {
            "ayer" => (hoy.AddDays(-1), hoy),
            "semana" => (hoy.AddDays(-7), hoy),
            "mes" => (hoy.AddDays(-30), hoy),
            _ => (hoy, ahora) // "hoy" por defecto
        };
    }

    private (int horaInicio, int horaFin) CalcularRangoHoras(string? turno)
    {
        return turno switch
        {
            "mañana" => (6, 12),
            "tarde" => (12, 18),
            "noche" => (18, 24),
            "madrugada" => (0, 6),
            _ => (0, 24) // "todos" por defecto
        };
    }

    private (DateTime fechaInicio, DateTime fechaFin) CalcularRangoFechasAnterior(string? periodo)
    {
        var hoy = DateTime.Today;

        return periodo switch
        {
            "ayer" => (hoy.AddDays(-2), hoy.AddDays(-1)),
            "semana" => (hoy.AddDays(-14), hoy.AddDays(-7)),
            "mes" => (hoy.AddDays(-60), hoy.AddDays(-30)),
            _ => (hoy.AddDays(-1), hoy) // "hoy" por defecto
        };
    }

    private async Task<(decimal ventasHoy, decimal ventasAyer, decimal ventasSemana, decimal ventasMes)> CalcularMetricasVentasPorPeriodo(string periodo, string turno)
    {
        // Calcular ventas de hoy (siempre del día actual)
        var (fechaInicioHoy, fechaFinHoy) = CalcularRangoFechas("hoy");
        var facturasHoy = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioHoy, fechaFinHoy);
        var ventasHoy = facturasHoy.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

        // Calcular ventas de ayer (siempre del día anterior)
        var (fechaInicioAyer, fechaFinAyer) = CalcularRangoFechas("ayer");
        var facturasAyer = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioAyer, fechaFinAyer);
        var ventasAyer = facturasAyer.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

        // Calcular ventas de la semana (últimos 7 días)
        var (fechaInicioSemana, fechaFinSemana) = CalcularRangoFechas("hoy");
        fechaInicioSemana = fechaInicioSemana.AddDays(-7); // Últimos 7 días
        var facturasSemana = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioSemana, fechaFinSemana);
        var ventasSemana = facturasSemana.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

        // Calcular ventas del mes (últimos 30 días)
        var (fechaInicioMes, fechaFinMes) = CalcularRangoFechas("hoy");
        fechaInicioMes = fechaInicioMes.AddDays(-29); // Últimos 30 días
        var facturasMes = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioMes, fechaFinMes);
        var ventasMes = facturasMes.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

        // Aplicar filtro de turno solo a la métrica específica del período solicitado
        if (turno != "todos")
        {
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);
            
            // Solo aplicar filtro de turno a la métrica del período solicitado
            if (periodo == "hoy")
            {
                var facturasHoyFiltradas = facturasHoy.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasHoy = facturasHoyFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "ayer")
            {
                var facturasAyerFiltradas = facturasAyer.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasAyer = facturasAyerFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "semana")
            {
                var facturasSemanaFiltradas = facturasSemana.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasSemana = facturasSemanaFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "mes")
            {
                var facturasMesFiltradas = facturasMes.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasMes = facturasMesFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
        }

        return (ventasHoy, ventasAyer, ventasSemana, ventasMes);
    }

    #endregion
}
