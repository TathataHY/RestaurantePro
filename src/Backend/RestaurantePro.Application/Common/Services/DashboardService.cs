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
            
            // Calcular ventas totales del período seleccionado (todos los turnos)
            var ventasTotalDia = await CalcularVentasTotalPeriodo(periodo);

            // Calcular métricas faltantes
            var (productosVendidosHoy, clientesAtendidosHoy, promedioTicket) = await CalcularMetricasAdicionales(facturas, turno, periodo);
            var tiempoPromedio = await CalcularTiempoPromedioComandasAsync(periodo, turno);
            var mesasDisponibles = mesas.Count() - mesasOcupadas;
            var comandasCompletadas = comandas.Count(c => c.Estado == EstadoComanda.Entregada);


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
            }
            else
            {
                // Para otros casos, usar los últimos N días desde hoy hacia atrás
                fechaFin = DateTime.Today.AddDays(1).AddTicks(-1); // Final del día de hoy
                fechaInicio = fechaFin.AddDays(-dias + 1).Date; // Inicio de los últimos N días
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


            for (int i = 0; i < diasAProcesar; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                var fechaInicioDia = fecha.Date.AddHours(horaInicio);
                var fechaFinDia = fecha.Date.AddHours(horaFin);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioDia, fechaFinDia);
                var facturasPagadas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
                var ventasDia = facturasPagadas.Sum(f => f.Total);


                ventasPorDia.Add(new DashboardVentaPorPeriodoDto
                {
                    Fecha = fecha,
                    Monto = ventasDia
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

    public async Task<List<MesaDetalleDto>> ObtenerMesasDetalleAsync()
    {
        _logger.LogInformation("🪑 Obteniendo detalles de las mesas para el mapa");

        try
        {
            var mesas = await _mesaRepository.ObtenerTodasAsync();

            return mesas.Select(m => new MesaDetalleDto
            {
                Id = m.Id,
                Numero = m.Numero,
                Capacidad = m.Capacidad,
                Estado = m.Estado.ToString(),
                Ubicacion = m.Ubicacion,
                Color = m.Estado switch
                {
                    EstadoMesa.Disponible => "#10b981", // Verde
                    EstadoMesa.Ocupada => "#ef4444",    // Rojo
                    EstadoMesa.Reservada => "#f59e0b",  // Amarillo
                    EstadoMesa.FueraDeServicio => "#6b7280", // Gris
                    _ => "#6b7280"
                }
            }).OrderBy(m => m.Numero).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener detalles de las mesas");
            return new List<MesaDetalleDto>();
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

            var ingresosPorHora = new List<DashboardIngresosPorHoraDto>();

            // Para períodos de "semana" y "mes", obtener todas las facturas y agrupar por hora
            if (periodo == "semana" || periodo == "mes")
            {
                // Obtener todas las facturas del período
                var todasLasFacturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
                var facturasPagadas = todasLasFacturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();

                _logger.LogInformation("💰 DEBUG: Período {Periodo} - Total facturas: {TotalFacturas}, Facturas pagadas: {FacturasPagadas}", 
                    periodo, todasLasFacturas.Count(), facturasPagadas.Count());

                // Inicializar todas las horas con 0
                for (int hora = horaInicio; hora < horaFin; hora++)
                {
                    ingresosPorHora.Add(new DashboardIngresosPorHoraDto
                    {
                        Hora = hora,
                        Monto = 0
                    });
                }

                // Agrupar facturas por hora y sumar ingresos
                var facturasPorHora = facturasPagadas.GroupBy(f => f.FechaEmision.Hour).ToList();
                
                foreach (var grupo in facturasPorHora)
                {
                    var hora = grupo.Key;
                    var ingresosHora = grupo.Sum(f => f.Total);
                    
                    // Buscar la entrada correspondiente a esta hora
                    var entrada = ingresosPorHora.FirstOrDefault(i => i.Hora == hora);
                    if (entrada != null)
                    {
                        entrada.Monto = ingresosHora;
                    }
                    
                    _logger.LogInformation("💰 DEBUG: Hora {Hora}: {Facturas} facturas, Total: {Ingresos}", 
                        hora, grupo.Count(), ingresosHora);
                }
            }
            else
            {
                // Para períodos de "hoy" y "ayer", usar la lógica original
            for (int hora = horaInicio; hora < horaFin; hora++)
            {
                var horaInicioActual = fechaInicio.Date.AddHours(hora);
                    var horaFinActual = fechaInicio.Date.AddHours(hora + 1).AddTicks(-1);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(horaInicioActual, horaFinActual);
                var ingresos = facturas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);

                ingresosPorHora.Add(new DashboardIngresosPorHoraDto
                {
                    Hora = hora,
                        Monto = ingresos
                });
                }
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
        var ayer = hoy.AddDays(-1);
        var finDelDiaAyer = ayer.AddDays(1).AddTicks(-1); // 23:59:59.9999999 de ayer

        return periodo switch
        {
            "ayer" => (ayer, finDelDiaAyer),
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
        
        // IMPORTANTE: Las métricas principales deben reflejar el período seleccionado
        // "VentasHoy" en realidad representa "Ventas del período seleccionado"
        
        var (fechaInicioPeriodo, fechaFinPeriodo) = CalcularRangoFechas(periodo);
        var facturasPeriodo = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioPeriodo, fechaFinPeriodo);
        var facturasPagadas = facturasPeriodo.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
        var ventasPeriodo = facturasPagadas.Sum(f => f.Total);
        
        

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
                var facturasHoyFiltradas = facturasPeriodo.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasPeriodo = facturasHoyFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "ayer")
            {
                var facturasAyerFiltradas = facturasPeriodo.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasPeriodo = facturasAyerFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "semana")
            {
                var facturasSemanaFiltradas = facturasPeriodo.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasPeriodo = facturasSemanaFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
            else if (periodo == "mes")
            {
                var facturasMesFiltradas = facturasPeriodo.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin);
                ventasPeriodo = facturasMesFiltradas.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            }
        }

        
        return (ventasPeriodo, ventasAyer, ventasSemana, ventasMes);
    }

    /// <summary>
    /// Calcula las ventas totales del día completo (todos los turnos)
    /// </summary>
    private async Task<decimal> CalcularVentasTotalPeriodo(string? periodo)
    {
        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            
            var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var facturasFiltradas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
            
            var total = facturasFiltradas.Sum(f => f.Total);
            
            
            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular ventas totales del período {Periodo}", periodo);
            return 0;
        }
    }

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
    private async Task<(int productosVendidosHoy, int clientesAtendidosHoy, decimal promedioTicket)> CalcularMetricasAdicionales(IEnumerable<Factura> facturas, string? turno, string? periodo)
    {
        try
        {
            
            // IMPORTANTE: Usar las facturas del período seleccionado, no las pasadas como parámetro
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var facturasPeriodo = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            
            // Aplicar filtro de turno si es necesario
            var facturasFiltradas = facturasPeriodo.ToList();
            if (turno != "todos")
            {
                var (horaInicio, horaFin) = CalcularRangoHoras(turno);
                facturasFiltradas = facturasPeriodo.Where(f => f.FechaEmision.Hour >= horaInicio && f.FechaEmision.Hour < horaFin).ToList();
            }


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


            return (productosVendidosHoy, clientesAtendidosHoy, promedioTicket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular métricas adicionales");
            return (0, 0, 0);
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
            
            // IMPORTANTE: Calcular tiempo para comandas FINALIZADAS (estado 5)
            // Esto refleja el tiempo total desde creación hasta finalización completa
            var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
            
            if (!comandasFinalizadas.Any())
            {
                _logger.LogInformation("⏱️ No hay comandas finalizadas para calcular tiempo promedio");
                return 0;
            }
            
            var tiempoTotalMinutos = comandasFinalizadas.Sum(c => 
                (int)((c.FechaActualizacion - c.FechaCreacion)?.TotalMinutes ?? 0));
            
            var tiempoPromedio = tiempoTotalMinutos / comandasFinalizadas.Count();
            
            _logger.LogInformation("⏱️ Tiempo promedio calculado - Total comandas: {TotalComandas}, Comandas finalizadas: {ComandasFinalizadas}, Tiempo total: {TiempoTotal} min, Tiempo promedio: {TiempoPromedio} min", 
                comandas.Count(), comandasFinalizadas.Count(), tiempoTotalMinutos, tiempoPromedio);
            
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
