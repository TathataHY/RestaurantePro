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
            var ingresosPorCategoria = await ObtenerIngresosPorCategoriaAsync(periodo, turno);

            return new DashboardResumenDto
            {
                Metricas = metricas,
                ProductosMasVendidos = productosMasVendidos,
                VentasPorPeriodo = ventasPorPeriodo,
                EstadoMesas = estadoMesas,
                ComandasPorEstado = comandasPorEstado,
                IngresosPorHora = ingresosPorHora,
                IngresosPorCategoria = ingresosPorCategoria,
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
            var comandasActivas = comandas.Count(c => c.Estado == EstadoComanda.EnProceso || c.Estado == EstadoComanda.Lista);

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
            
            // Calcular ventas totales del día completo (todos los turnos)
            var ventasTotalDia = await CalcularVentasTotalDia();

            // Calcular métricas faltantes
            var (productosVendidosHoy, clientesAtendidosHoy, promedioTicket, tiempoPromedio) = await CalcularMetricasAdicionales(facturas, turno);
            var mesasDisponibles = mesas.Count() - mesasOcupadas;
            var comandasCompletadas = comandas.Count(c => c.Estado == EstadoComanda.Entregada);

            _logger.LogInformation("🔍 DEBUG: Métricas adicionales - ProductosVendidosHoy: {ProductosVendidosHoy}, ClientesAtendidosHoy: {ClientesAtendidosHoy}, PromedioTicket: {PromedioTicket}, MesasDisponibles: {MesasDisponibles}, ComandasCompletadas: {ComandasCompletadas}", 
                productosVendidosHoy, clientesAtendidosHoy, promedioTicket, mesasDisponibles, comandasCompletadas);

            return new DashboardMetricasDto
            {
                VentasHoy = ventasHoy,
                VentasAyer = ventasAyer,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                VentasTotalDia = ventasTotalDia,
                ComandasActivas = comandasActivas,
                MesasOcupadas = mesasOcupadas,
                MesasDisponibles = mesasDisponibles,
                TotalMesas = mesas.Count(),
                ComandasCompletadas = comandasCompletadas,
                ProductosVendidosHoy = productosVendidosHoy,
                ClientesAtendidosHoy = clientesAtendidosHoy,
                PromedioTicket = promedioTicket,
                TiempoPromedio = tiempoPromedio,
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
            // Calcular fechas según el período solicitado
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            
            // Si el período es específico (hoy, ayer, etc.), usar solo ese día
            // Si no, usar los últimos N días desde hoy hacia atrás
            if (periodo == "hoy" || periodo == "ayer" || periodo == "semana" || periodo == "mes")
            {
                // Para períodos específicos, usar solo ese período
                _logger.LogInformation("🔍 DEBUG: Ventas por período específico - Fecha inicio: {FechaInicio}, Fecha fin: {FechaFin}", fechaInicio, fechaFin);
            }
            else
            {
                // Para otros casos, usar los últimos N días desde hoy hacia atrás
                fechaFin = DateTime.Today.AddDays(1).AddTicks(-1); // Final del día de hoy
                fechaInicio = fechaFin.AddDays(-dias + 1).Date; // Inicio de los últimos N días
                _logger.LogInformation("🔍 DEBUG: Ventas por período - Últimos {Dias} días - Fecha inicio: {FechaInicio}, Fecha fin: {FechaFin}", dias, fechaInicio, fechaFin);
            }
            
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            // Aplicar filtro de turno a las fechas
            if (turno != "todos")
            {
                fechaInicio = fechaInicio.Date.AddHours(horaInicio);
                fechaFin = fechaFin.Date.AddHours(horaFin);
            }

            var ventasPorDia = new List<DashboardVentaPorPeriodoDto>();

            // Determinar cuántos días procesar según el período
            int diasAProcesar;
            if (periodo == "hoy")
            {
                diasAProcesar = 1; // Solo el día de hoy
            }
            else if (periodo == "ayer")
            {
                diasAProcesar = 1; // Solo el día de ayer
            }
            else if (periodo == "semana")
            {
                diasAProcesar = 7; // Últimos 7 días
            }
            else if (periodo == "mes")
            {
                diasAProcesar = 30; // Últimos 30 días
            }
            else
            {
                diasAProcesar = dias; // Usar el parámetro dias
            }

            _logger.LogInformation("🔍 DEBUG: Procesando {DiasAProcesar} días para período '{Periodo}'", diasAProcesar, periodo);

            for (int i = 0; i < diasAProcesar; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                var fechaInicioDia = fecha.Date.AddHours(horaInicio);
                var fechaFinDia = fecha.Date.AddHours(horaFin);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioDia, fechaFinDia);
                var facturasPagadas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
                var ventasDia = facturasPagadas.Sum(f => f.Total);

                _logger.LogInformation("🔍 DEBUG: Día {Fecha} - Facturas encontradas: {FacturasCount}, Facturas pagadas: {PagadasCount}, Ventas: {Ventas}", 
                    fecha.ToString("yyyy-MM-dd"), facturas.Count(), facturasPagadas.Count, ventasDia);

                ventasPorDia.Add(new DashboardVentaPorPeriodoDto
                {
                    Fecha = fecha,
                    Monto = ventasDia
                });
            }

            _logger.LogInformation("🔍 DEBUG: Ventas por período completadas - Total días: {Dias}", ventasPorDia.Count);
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
                EnLimpieza = mesas.Count(m => m.Estado == EstadoMesa.FueraDeServicio), // Mapear FueraDeServicio a EnLimpieza para el frontend
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
                var horaFinActual = fechaInicio.Date.AddHours(hora + 1).AddTicks(-1); // Incluir solo hasta 59:59.9999999

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(horaInicioActual, horaFinActual);
                var ingresos = facturas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

                ingresosPorHora.Add(new DashboardIngresosPorHoraDto
                {
                    Hora = hora,
                    Monto = ingresos
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
        var finDelDia = hoy.AddDays(1).AddTicks(-1); // 23:59:59.9999999

        return periodo switch
        {
            "ayer" => (hoy.AddDays(-1), hoy),
            "semana" => (hoy.AddDays(-7), finDelDia),
            "mes" => (hoy.AddDays(-30), finDelDia),
            _ => (hoy, finDelDia) // "hoy" por defecto - hasta el final del día
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
        _logger.LogInformation("🔍 DEBUG: CalcularMetricasVentasPorPeriodo - Periodo: {Periodo}, Turno: {Turno}", periodo, turno);
        
        // Calcular ventas de hoy (siempre del día actual)
        var (fechaInicioHoy, fechaFinHoy) = CalcularRangoFechas("hoy");
        var facturasHoy = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioHoy, fechaFinHoy);
        var facturasPagadas = facturasHoy.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
        var ventasHoy = facturasPagadas.Sum(f => f.Total);
        
        _logger.LogInformation("🔍 DEBUG: Rango fechas hoy: {FechaInicio} a {FechaFin}", fechaInicioHoy, fechaFinHoy);
        _logger.LogInformation("🔍 DEBUG: Facturas hoy encontradas: {Count}, Facturas pagadas: {Pagadas}, Ventas hoy iniciales: {VentasHoy}", 
            facturasHoy.Count(), facturasPagadas.Count, ventasHoy);
        
        // Log detallado de cada factura
        foreach (var factura in facturasHoy)
        {
            _logger.LogInformation("🔍 DEBUG: Factura {Id} - Estado: {Estado}, Fecha: {Fecha}, Total: {Total}", 
                factura.Id, factura.Estado, factura.FechaEmision, factura.Total);
        }

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
            _logger.LogInformation("🔍 DEBUG: Aplicando filtro de turno - Hora inicio: {HoraInicio}, Hora fin: {HoraFin}", horaInicio, horaFin);
            
            // Solo aplicar filtro de turno a la métrica del período solicitado
            if (periodo == "hoy")
            {
                var facturasHoyFiltradas = facturasHoy.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasHoy = facturasHoyFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
                _logger.LogInformation("🔍 DEBUG: Facturas hoy filtradas: {Count}, Ventas hoy filtradas: {VentasHoy}", facturasHoyFiltradas.Count(), ventasHoy);
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

        _logger.LogInformation("🔍 DEBUG: Valores finales - VentasHoy: {VentasHoy}, VentasAyer: {VentasAyer}, VentasSemana: {VentasSemana}, VentasMes: {VentasMes}", 
            ventasHoy, ventasAyer, ventasSemana, ventasMes);
        
        return (ventasHoy, ventasAyer, ventasSemana, ventasMes);
    }

    /// <summary>
    /// Calcula las ventas totales del día completo (todos los turnos)
    /// </summary>
    private async Task<decimal> CalcularVentasTotalDia()
    {
        try
        {
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);
            
            var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(hoy, mañana);
            var facturasFiltradas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
            
            return facturasFiltradas.Sum(f => f.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular ventas totales del día");
            return 0;
        }
    }

    /// <summary>
    /// Calcula métricas adicionales del dashboard
    /// </summary>
    private async Task<(int productosVendidosHoy, int clientesAtendidosHoy, decimal promedioTicket)> CalcularMetricasAdicionales(IEnumerable<Factura> facturas, string? turno)
    {
        try
        {
            _logger.LogInformation("🔍 DEBUG: CalcularMetricasAdicionales - Iniciando cálculo");
            
            // Aplicar filtro de turno si es necesario
            var facturasFiltradas = facturas.ToList();
            if (turno != "todos")
            {
                var (horaInicio, horaFin) = CalcularRangoHoras(turno);
                facturasFiltradas = facturas.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin).ToList();
            }

            _logger.LogInformation("🔍 DEBUG: Facturas filtradas para métricas adicionales: {Count}", facturasFiltradas.Count);

            // Calcular productos vendidos hoy (sumar cantidades de todos los detalles)
            var productosVendidosHoy = 0;
            var clientesUnicos = new HashSet<Guid>();
            var totalVentas = 0m;
            var cantidadFacturas = 0;

            foreach (var factura in facturasFiltradas)
            {
                if (factura.Estado == EstadoFactura.Pagada)
                {
                    // Contar productos vendidos
                    if (factura.Detalles != null)
                    {
                        productosVendidosHoy += (int)factura.Detalles.Sum(d => d.Cantidad);
                        _logger.LogInformation("🔍 DEBUG: Factura {Id} - Detalles: {DetallesCount}, Cantidad total: {CantidadTotal}", 
                            factura.Id, factura.Detalles.Count, factura.Detalles.Sum(d => d.Cantidad));
                    }

                    // Contar clientes únicos
                    if (factura.ClienteId.HasValue)
                    {
                        clientesUnicos.Add(factura.ClienteId.Value);
                    }

                    // Acumular para promedio ticket
                    totalVentas += factura.Total;
                    cantidadFacturas++;
                }
            }

            var clientesAtendidosHoy = clientesUnicos.Count;
            var promedioTicket = cantidadFacturas > 0 ? totalVentas / cantidadFacturas : 0;

            // Calcular tiempo promedio de comandas (en minutos)
            var tiempoPromedio = await CalcularTiempoPromedioComandasAsync(periodo, turno);

            _logger.LogInformation("🔍 DEBUG: Resultados - ProductosVendidosHoy: {ProductosVendidosHoy}, ClientesAtendidosHoy: {ClientesAtendidosHoy}, PromedioTicket: {PromedioTicket}, TiempoPromedio: {TiempoPromedio}", 
                productosVendidosHoy, clientesAtendidosHoy, promedioTicket, tiempoPromedio);

            return (productosVendidosHoy, clientesAtendidosHoy, promedioTicket, tiempoPromedio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular métricas adicionales");
            return (0, 0, 0, 0);
        }
    }

    private async Task<int> CalcularTiempoPromedioComandasAsync(string periodo, string turno)
    {
        try
        {
            _logger.LogInformation("⏱️ Calculando tiempo promedio de comandas - Período: {Periodo}, Turno: {Turno}", periodo, turno);
            
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            
            // Filtrar por turno si no es "todos"
            if (turno != "todos")
            {
                var (horaInicio, horaFin) = CalcularRangoHoras(turno);
                comandas = comandas.Where(c => 
                    c.FechaCreacion.Hour >= horaInicio && 
                    c.FechaCreacion.Hour < horaFin).ToList();
            }
            
            if (!comandas.Any())
            {
                _logger.LogInformation("⏱️ No hay comandas para calcular tiempo promedio");
                return 0;
            }
            
            var tiempoTotalMinutos = comandas.Sum(c => 
                (int)(c.FechaActualizacion - c.FechaCreacion).TotalMinutes);
            
            var tiempoPromedio = tiempoTotalMinutos / comandas.Count();
            
            _logger.LogInformation("⏱️ Tiempo promedio calculado - Total comandas: {TotalComandas}, Tiempo total: {TiempoTotal} min, Tiempo promedio: {TiempoPromedio} min", 
                comandas.Count(), tiempoTotalMinutos, tiempoPromedio);
            
            return tiempoPromedio;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular tiempo promedio de comandas");
            return 0;
        }
    }

    private async Task<List<DashboardIngresosPorCategoriaDto>> ObtenerIngresosPorCategoriaAsync(string periodo, string turno)
    {
        _logger.LogInformation("🏷️ Obteniendo ingresos por categoría - Período: {Periodo}, Turno: {Turno}", periodo, turno);
        
        var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
        var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
        
        // Filtrar por turno si no es "todos"
        if (turno != "todos")
        {
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);
            facturas = facturas.Where(f => 
                f.FechaEmision.Hour >= horaInicio && 
                f.FechaEmision.Hour < horaFin).ToList();
        }
        
        // Filtrar solo facturas pagadas
        var facturasPagadas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
        
        // Agrupar por categoría de productos
        var ingresosPorCategoria = new Dictionary<string, decimal>();
        
        foreach (var factura in facturasPagadas)
        {
            // Obtener detalles de la factura con productos
            var detalles = await _facturaRepository.ObtenerDetallesConProductosAsync(factura.Id);
            
            foreach (var detalle in detalles)
            {
                var categoria = detalle.Producto?.CategoriaNombre ?? "Sin Categoría";
                var monto = detalle.PrecioUnitario * detalle.Cantidad;
                
                if (ingresosPorCategoria.ContainsKey(categoria))
                    ingresosPorCategoria[categoria] += monto;
                else
                    ingresosPorCategoria[categoria] = monto;
            }
        }
        
                    // Convertir a DTO
                    var resultado = ingresosPorCategoria.Select(kvp => new DashboardIngresosPorCategoriaDto
                    {
                        Categoria = kvp.Key,
                        Monto = kvp.Value,
                        Porcentaje = facturasPagadas.Sum(f => f.Total) > 0 ? 
                            Math.Round((kvp.Value / facturasPagadas.Sum(f => f.Total)) * 100, 1) : 0
                    }).OrderByDescending(x => x.Monto).ToList();
        
        _logger.LogInformation("🏷️ Ingresos por categoría completados - Total categorías: {Count}", resultado.Count);
        
        return resultado;
    }

    #endregion
}
