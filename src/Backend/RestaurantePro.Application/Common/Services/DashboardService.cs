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

            // MIGRACIÓN PERMANENTE: Usar comandas finalizadas como fuente de ventas
            var comandasVentas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var comandasFinalizadasVentas = comandasVentas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();

            // Calcular ventas desde comandas finalizadas (pagadas)
            // SOLUCIÓN DEFINITIVA: Consulta SQL directa para obtener ventas reales
            var ventas = await _comandaRepository.ObtenerVentasTotalesAsync(fechaInicio, fechaFin);

            // Obtener comandas del período
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var comandasActivas = comandas.Count(c => c.Estado == EstadoComanda.EnProceso || c.Estado == EstadoComanda.Lista || c.Estado == EstadoComanda.Entregada);

            // Obtener mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);

            // Calcular crecimiento de ventas (comparar con período anterior) - BASADO EN COMANDAS
            var (fechaInicioAnterior, fechaFinAnterior) = CalcularRangoFechasAnterior(periodo);
            var comandasAnterior = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioAnterior, fechaFinAnterior);
            var ventasAnterior = comandasAnterior.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);
            var crecimientoVentas = ventasAnterior > 0
                ? ((ventas - ventasAnterior) / ventasAnterior) * 100
                : 0;

            // Calcular métricas específicas según el período
            var (ventasHoy, ventasAyer, ventasSemana, ventasMes) = await CalcularMetricasVentasPorPeriodo(periodo, turno);
            
            // Calcular ventas totales del período seleccionado (todos los turnos)
            var ventasTotalDia = await CalcularVentasTotalPeriodo(periodo);

            // Calcular métricas faltantes
            var (productosVendidosHoy, clientesAtendidosHoy, promedioTicket) = await CalcularMetricasAdicionales(comandasFinalizadasVentas, turno, periodo);
            var tiempoPromedio = await CalcularTiempoPromedioComandasAsync(periodo, turno);
            var mesasDisponibles = mesas.Count() - mesasOcupadas;
            var comandasCompletadas = comandas.Count(c => c.Estado == EstadoComanda.Finalizada);


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
        _logger.LogInformation("🍽️ Obteniendo productos más vendidos - Cantidad: {Cantidad}, Período: {Periodo}, Turno: {Turno} - BASADO EN COMANDAS", cantidad, periodo, turno);

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

            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, incluirItems: true);
            var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();

            var productosVendidos = comandasFinalizadas
                .SelectMany(c => c.Items ?? new List<ItemComanda>())
                .GroupBy(i => i.ProductoId)
                .Select(g => new DashboardProductoMasVendidoDto
                {
                    ProductoId = g.Key,
                    Nombre = $"Producto {g.Key.ToString()[..8]}...", // Temporal hasta obtener nombre real
                    CantidadVendida = (int)g.Sum(i => i.Cantidad),
                    Ingresos = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToList();

            // Obtener nombres reales de productos desde el repositorio
            foreach (var producto in productosVendidos)
            {
                try
                {
                    var productoEntity = await _productoRepository.ObtenerPorIdAsync(producto.ProductoId);
                    if (productoEntity != null)
                    {
                        producto.Nombre = productoEntity.Nombre;
                    }
                }
                catch
                {
                    // Si no se puede obtener el nombre, mantener el temporal
                }
            }

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
        _logger.LogInformation("📊 Obteniendo ventas por período - Días: {Dias}, Período: {Periodo}, Turno: {Turno} - BASADO EN COMANDAS", dias, periodo, turno);

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

                // PROTECCIÓN: Evitar fechas problemáticas de timezone
                try
                {
                    var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioDia, fechaFinDia);
                    var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
                    var ventasDia = comandasFinalizadas.Sum(c => c.Total?.Total ?? 0);

                    ventasPorDia.Add(new DashboardVentaPorPeriodoDto
                    {
                        Fecha = fecha,
                        Monto = ventasDia
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ Error con fecha {Fecha}: {Error} - Omitiendo día", fecha.ToString("yyyy-MM-dd"), ex.Message);
                    // Agregar día con ventas 0 para mantener continuidad
                    ventasPorDia.Add(new DashboardVentaPorPeriodoDto
                    {
                        Fecha = fecha,
                        Monto = 0
                    });
                }
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
                Completadas = comandas.Count(c => c.Estado == EstadoComanda.Finalizada),
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
        _logger.LogInformation("💰 Obteniendo ingresos por hora - Período: {Periodo}, Turno: {Turno} - BASADO EN COMANDAS", periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            var ingresosPorHora = new List<DashboardIngresosPorHoraDto>();

            // Para períodos de "semana" y "mes", obtener todas las facturas y agrupar por hora
            if (periodo == "semana" || periodo == "mes")
            {
                // Obtener todas las comandas del período
                var todasLasComandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
                var comandasFinalizadas = todasLasComandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();

                _logger.LogInformation("💰 DEBUG: Período {Periodo} - Total comandas: {TotalComandas}, Comandas finalizadas: {ComandasFinalizadas}", 
                    periodo, todasLasComandas.Count(), comandasFinalizadas.Count());

                // Inicializar todas las horas con 0
                for (int hora = horaInicio; hora < horaFin; hora++)
                {
                    ingresosPorHora.Add(new DashboardIngresosPorHoraDto
                    {
                        Hora = hora,
                        Monto = 0
                    });
                }

                // Agrupar comandas por hora y sumar ingresos
                var comandasPorHora = comandasFinalizadas.GroupBy(c => c.FechaCreacion.Hour).ToList();
                
                foreach (var grupo in comandasPorHora)
                {
                    var hora = grupo.Key;
                    var ingresosHora = grupo.Sum(c => c.Total?.Total ?? 0);
                    
                    // Buscar la entrada correspondiente a esta hora
                    var entrada = ingresosPorHora.FirstOrDefault(i => i.Hora == hora);
                    if (entrada != null)
                    {
                        entrada.Monto = ingresosHora;
                    }
                    
                    _logger.LogInformation("💰 DEBUG: Hora {Hora}: {Comandas} comandas, Total: {Ingresos}", 
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

                var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(horaInicioActual, horaFinActual);
                var ingresos = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);

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
            "mes" => (hoy.AddDays(-25), finDelDia), // Reducido para evitar problemas de timezone
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
            "mes" => (hoy.AddDays(-50), hoy.AddDays(-25)), // Reducido para evitar problemas de timezone
            _ => (hoy.AddDays(-1), hoy) // "hoy" por defecto
        };
    }

    private async Task<(decimal ventasHoy, decimal ventasAyer, decimal ventasSemana, decimal ventasMes)> CalcularMetricasVentasPorPeriodo(string periodo, string turno)
    {
        
        // IMPORTANTE: Las métricas principales deben reflejar el período seleccionado
        // "VentasHoy" en realidad representa "Ventas del período seleccionado"
        // CAMBIO: Ahora basamos las ventas en comandas finalizadas en lugar de facturas
        
        var (fechaInicioPeriodo, fechaFinPeriodo) = CalcularRangoFechas(periodo);
        var comandasPeriodo = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioPeriodo, fechaFinPeriodo);
        var comandasFinalizadas = comandasPeriodo.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
        var ventasPeriodo = comandasFinalizadas.Sum(c => c.Total?.Total ?? 0);
        
        

        // Calcular ventas de ayer (siempre del día anterior) - BASADO EN COMANDAS
        var (fechaInicioAyer, fechaFinAyer) = CalcularRangoFechas("ayer");
        var comandasAyer = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioAyer, fechaFinAyer);
        var ventasAyer = comandasAyer.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);

        // Calcular ventas de la semana (últimos 7 días) - BASADO EN COMANDAS
        var (fechaInicioSemana, fechaFinSemana) = CalcularRangoFechas("hoy");
        fechaInicioSemana = fechaInicioSemana.AddDays(-7); // Últimos 7 días
        var comandasSemana = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioSemana, fechaFinSemana);
        var ventasSemana = comandasSemana.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);

        // Calcular ventas del mes (últimos 30 días) - BASADO EN COMANDAS
        var (fechaInicioMes, fechaFinMes) = CalcularRangoFechas("hoy");
        fechaInicioMes = fechaInicioMes.AddDays(-29); // Últimos 30 días
        var comandasMes = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicioMes, fechaFinMes);
        var ventasMes = comandasMes.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);

        // Aplicar filtro de turno solo a la métrica específica del período solicitado - BASADO EN COMANDAS
        if (turno != "todos")
        {
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);
            
            // Solo aplicar filtro de turno a la métrica del período solicitado
            if (periodo == "hoy")
            {
                var comandasHoyFiltradas = comandasPeriodo.Where(c => c.FechaCreacion.Hour >= horaInicio && c.FechaCreacion.Hour < horaFin);
                ventasPeriodo = comandasHoyFiltradas.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);
            }
            else if (periodo == "ayer")
            {
                var comandasAyerFiltradas = comandasPeriodo.Where(c => c.FechaCreacion.Hour >= horaInicio && c.FechaCreacion.Hour < horaFin);
                ventasPeriodo = comandasAyerFiltradas.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);
            }
            else if (periodo == "semana")
            {
                var comandasSemanaFiltradas = comandasPeriodo.Where(c => c.FechaCreacion.Hour >= horaInicio && c.FechaCreacion.Hour < horaFin);
                ventasPeriodo = comandasSemanaFiltradas.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);
            }
            else if (periodo == "mes")
            {
                var comandasMesFiltradas = comandasPeriodo.Where(c => c.FechaCreacion.Hour >= horaInicio && c.FechaCreacion.Hour < horaFin);
                ventasPeriodo = comandasMesFiltradas.Where(c => c.Estado == EstadoComanda.Finalizada).Sum(c => c.Total?.Total ?? 0);
            }
        }

        
        return (ventasPeriodo, ventasAyer, ventasSemana, ventasMes);
    }

    /// <summary>
    /// Calcula las ventas totales del día completo (todos los turnos) - BASADO EN COMANDAS
    /// </summary>
    private async Task<decimal> CalcularVentasTotalPeriodo(string? periodo)
    {
        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
            
            var total = comandasFinalizadas.Sum(c => c.Total?.Total ?? 0);
            
            
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
            
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(hoy, mañana);
            var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
            
            return comandasFinalizadas.Sum(c => c.Total?.Total ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular ventas totales del día");
            return 0;
        }
    }

    /// <summary>
    /// Calcula métricas adicionales del dashboard - BASADO EN COMANDAS
    /// </summary>
    private async Task<(int productosVendidosHoy, int clientesAtendidosHoy, decimal promedioTicket)> CalcularMetricasAdicionales(IEnumerable<Comanda> comandasFinalizadas, string? turno, string? periodo)
    {
        try
        {
            
            // IMPORTANTE: Usar las comandas finalizadas que se pasan como parámetro
            var comandasFiltradas = comandasFinalizadas.ToList();


            // Calcular productos vendidos hoy (sumar cantidades de todos los detalles)
            var productosVendidosHoy = 0;
            var clientesUnicos = new HashSet<Guid>();
            var totalVentas = 0m;
            var cantidadComandas = 0;

            foreach (var comanda in comandasFiltradas)
            {
                if (comanda.Estado == EstadoComanda.Finalizada)
                {
                    // Contar productos vendidos
                    if (comanda.Items != null)
                    {
                        productosVendidosHoy += (int)comanda.Items.Sum(i => i.Cantidad);
                    }

                    // Contar clientes únicos (basado en la mesa asignada)
                    if (comanda.MesaId.HasValue)
                    {
                        clientesUnicos.Add(comanda.MesaId.Value); // Usamos mesa como proxy de cliente
                    }

                    // Acumular para promedio ticket
                    totalVentas += comanda.Total?.Total ?? 0;
                    cantidadComandas++;
                }
            }

            var clientesAtendidosHoy = clientesUnicos.Count;
            var promedioTicket = cantidadComandas > 0 ? totalVentas / cantidadComandas : 0;


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
        _logger.LogInformation("🏷️ Obteniendo ingresos por categoría - Período: {Periodo}, Turno: {Turno} - BASADO EN COMANDAS", periodo, turno);
        
        var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
        var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, incluirItems: true);
        
        // Filtrar por turno si no es "todos"
        if (turno != "todos")
        {
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);
            comandas = comandas.Where(c => 
                c.FechaCreacion.Hour >= horaInicio && 
                c.FechaCreacion.Hour < horaFin).ToList();
        }
        
        // Filtrar solo comandas finalizadas
        var comandasFinalizadas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada).ToList();
        
        // Agrupar por categoría de productos
        var ingresosPorCategoria = new Dictionary<string, decimal>();
        
        foreach (var comanda in comandasFinalizadas)
        {
            // Obtener items de la comanda con productos
            if (comanda.Items != null)
            {
                foreach (var item in comanda.Items)
                {
                    // Obtener categoría del producto desde el repositorio
                    var categoria = "Sin Categoría";
                    try
                    {
                        var productoEntity = await _productoRepository.ObtenerPorIdAsync(item.ProductoId);
                        if (productoEntity != null && !string.IsNullOrEmpty(productoEntity.CategoriaNombre))
                        {
                            categoria = productoEntity.CategoriaNombre;
                        }
                    }
                    catch
                    {
                        // Si no se puede obtener la categoría, usar "Sin Categoría"
                    }
                    
                    var monto = item.PrecioUnitario * item.Cantidad;
                    
                    if (ingresosPorCategoria.ContainsKey(categoria))
                        ingresosPorCategoria[categoria] += monto;
                    else
                        ingresosPorCategoria[categoria] = monto;
                }
            }
        }
        
                    // Convertir a DTO
                    var resultado = ingresosPorCategoria.Select(kvp => new DashboardIngresosPorCategoriaDto
                    {
                        Categoria = kvp.Key,
                        Monto = kvp.Value,
                        Porcentaje = comandasFinalizadas.Sum(c => c.Total?.Total ?? 0) > 0 ? 
                            Math.Round((kvp.Value / comandasFinalizadas.Sum(c => c.Total?.Total ?? 0)) * 100, 1) : 0
                    }).OrderByDescending(x => x.Monto).ToList();
        
        _logger.LogInformation("🏷️ Ingresos por categoría completados - Total categorías: {Count}", resultado.Count);
        
        return resultado;
    }

    #endregion
}
