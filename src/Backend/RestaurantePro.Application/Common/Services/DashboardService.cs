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
                IngresosPorHora = ingresosPorHora
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

            // Obtener mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
            var mesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible);

            // Obtener comandas del período
            var comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var comandasActivas = comandas.Count(c => c.Estado != EstadoComanda.Finalizada && 
                                                      c.Estado != EstadoComanda.Cancelada);
            var comandasCompletadas = comandas.Count(c => c.Estado == EstadoComanda.Finalizada);

            // Calcular productos vendidos
            var productosVendidos = comandas
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .SelectMany(c => c.Items)
                .Sum(i => i.Cantidad);

            // Calcular clientes atendidos
            var clientesAtendidos = comandas
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .Select(c => c.ClienteId)
                .Distinct()
                .Count();

            // Calcular promedio de ticket
            var promedioTicket = comandasCompletadas > 0 
                ? ventas / comandasCompletadas 
                : 0;

            // Calcular crecimiento de ventas (comparar con período anterior)
            var (fechaInicioAnterior, fechaFinAnterior) = CalcularRangoFechasAnterior(periodo);
            var facturasAnterior = await _facturaRepository.ObtenerPorRangoFechasAsync(fechaInicioAnterior, fechaFinAnterior);
            var ventasAnterior = facturasAnterior.Where(f => f.Estado == EstadoFactura.Pagada).Sum(f => f.Total);
            var crecimientoVentas = ventasAnterior > 0 
                ? ((ventas - ventasAnterior) / ventasAnterior) * 100 
                : 0;

            return new DashboardMetricasDto
            {
                VentasHoy = ventas,
                VentasAyer = ventasAnterior,
                VentasSemana = ventas, // Se ajustará según el período
                VentasMes = ventas,   // Se ajustará según el período
                MesasOcupadas = mesasOcupadas,
                MesasDisponibles = mesasDisponibles,
                TotalMesas = mesas.Count(),
                ComandasActivas = comandasActivas,
                ComandasCompletadas = comandasCompletadas,
                ProductosVendidosHoy = productosVendidos,
                ClientesAtendidosHoy = clientesAtendidos,
                PromedioTicket = promedioTicket,
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
        _logger.LogInformation("🍽️ Obteniendo productos más vendidos (cantidad: {Cantidad}, período: {Periodo}, turno: {Turno})", cantidad, periodo, turno);

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
            
            var productosVendidos = comandas
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .SelectMany(c => c.Items)
                .GroupBy(i => i.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    CantidadVendida = g.Sum(i => i.Cantidad),
                    Ingresos = g.Sum(i => i.PrecioUnitario * i.Cantidad)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToList();

            var productos = await _productoRepository.ObtenerTodosAsync();
            var resultado = new List<DashboardProductoMasVendidoDto>();

            foreach (var productoVendido in productosVendidos)
            {
                var producto = productos.FirstOrDefault(p => p.Id == productoVendido.ProductoId);
                if (producto != null)
                {
                    resultado.Add(new DashboardProductoMasVendidoDto
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        CantidadVendida = productoVendido.CantidadVendida,
                        Ingresos = productoVendido.Ingresos,
                        PrecioPromedio = productoVendido.Ingresos / productoVendido.CantidadVendida
                    });
                }
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos más vendidos");
            throw;
        }
    }

    public async Task<List<DashboardVentaPorPeriodoDto>> ObtenerVentasPorPeriodoAsync(int dias = 7, string? periodo = "hoy", string? turno = "todos")
    {
        _logger.LogInformation("📅 Obteniendo ventas por período (días: {Dias}, período: {Periodo}, turno: {Turno})", dias, periodo, turno);

        try
        {
            var resultado = new List<DashboardVentaPorPeriodoDto>();
            var hoy = DateTime.Today;
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);

            for (int i = dias - 1; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);
                var inicioDia = fecha;
                var finDia = fecha.AddDays(1).AddTicks(-1);

                // Aplicar filtro de turno si no es "todos"
                if (turno != "todos")
                {
                    inicioDia = fecha.AddHours(horaInicio);
                    finDia = fecha.AddHours(horaFin);
                }

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(inicioDia, finDia);
                var ventas = facturas.Where(f => f.Estado == EstadoFactura.Pagada)
                                   .Sum(f => f.Total);

                resultado.Add(new DashboardVentaPorPeriodoDto
                {
                    Fecha = fecha,
                    Ventas = ventas,
                    CantidadFacturas = facturas.Count(f => f.Estado == EstadoFactura.Pagada)
                });
            }

            return resultado;
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
                Ocupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada),
                Disponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible),
                Reservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada),
                Mantenimiento = mesas.Count(m => m.Estado == EstadoMesa.FueraDeServicio),
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
        _logger.LogInformation("📋 Obteniendo comandas por estado (período: {Periodo}, turno: {Turno})", periodo, turno);

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
                Creadas = comandas.Count(c => c.Estado == EstadoComanda.Creada),
                EnProceso = comandas.Count(c => c.Estado == EstadoComanda.EnProceso),
                Lista = comandas.Count(c => c.Estado == EstadoComanda.Lista),
                Entregada = comandas.Count(c => c.Estado == EstadoComanda.Entregada),
                Finalizada = comandas.Count(c => c.Estado == EstadoComanda.Finalizada),
                Cancelada = comandas.Count(c => c.Estado == EstadoComanda.Cancelada),
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
        _logger.LogInformation("⏰ Obteniendo ingresos por hora (período: {Periodo}, turno: {Turno})", periodo, turno);

        try
        {
            var (fechaInicio, fechaFin) = CalcularRangoFechas(periodo);
            var (horaInicio, horaFin) = CalcularRangoHoras(turno);
            var resultado = new List<DashboardIngresosPorHoraDto>();

            // Si hay filtro de turno, solo mostrar las horas del turno
            var horasAProcesar = turno != "todos" 
                ? Enumerable.Range(horaInicio, horaFin - horaInicio).ToList()
                : Enumerable.Range(0, 24).ToList();

            foreach (var hora in horasAProcesar)
            {
                var inicioHora = fechaInicio.Date.AddHours(hora);
                var finHora = inicioHora.AddHours(1).AddTicks(-1);

                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(inicioHora, finHora);
                var ingresos = facturas.Where(f => f.Estado == EstadoFactura.Pagada)
                                     .Sum(f => f.Total);

                resultado.Add(new DashboardIngresosPorHoraDto
                {
                    Hora = hora,
                    Ingresos = ingresos,
                    CantidadFacturas = facturas.Count(f => f.Estado == EstadoFactura.Pagada)
                });
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingresos por hora");
            throw;
        }
    }

    /// <summary>
    /// Calcula el rango de fechas según el período seleccionado
    /// </summary>
    private (DateTime inicio, DateTime fin) CalcularRangoFechas(string? periodo)
    {
        var hoy = DateTime.Today;
        
        return periodo?.ToLower() switch
        {
            "ayer" => (hoy.AddDays(-1), hoy.AddDays(-1).AddDays(1).AddTicks(-1)),
            "semana" => (hoy.AddDays(-(int)hoy.DayOfWeek), hoy.AddDays(1).AddTicks(-1)),
            "mes" => (new DateTime(hoy.Year, hoy.Month, 1), hoy.AddDays(1).AddTicks(-1)),
            _ => (hoy, hoy.AddDays(1).AddTicks(-1)) // hoy por defecto
        };
    }

    /// <summary>
    /// Calcula el rango de horas según el turno seleccionado
    /// </summary>
    private (int inicio, int fin) CalcularRangoHoras(string? turno)
    {
        return turno?.ToLower() switch
        {
            "mañana" => (6, 12),
            "tarde" => (12, 18),
            "noche" => (18, 24),
            "madrugada" => (0, 6),
            _ => (0, 24) // todos por defecto
        };
    }

    /// <summary>
    /// Calcula el rango de fechas del período anterior para comparación
    /// </summary>
    private (DateTime inicio, DateTime fin) CalcularRangoFechasAnterior(string? periodo)
    {
        var hoy = DateTime.Today;
        
        return periodo?.ToLower() switch
        {
            "hoy" => (hoy.AddDays(-1), hoy.AddDays(-1).AddDays(1).AddTicks(-1)),
            "ayer" => (hoy.AddDays(-2), hoy.AddDays(-2).AddDays(1).AddTicks(-1)),
            "semana" => (hoy.AddDays(-(int)hoy.DayOfWeek - 7), hoy.AddDays(-(int)hoy.DayOfWeek).AddTicks(-1)),
            "mes" => (new DateTime(hoy.Year, hoy.Month - 1, 1), new DateTime(hoy.Year, hoy.Month, 1).AddTicks(-1)),
            _ => (hoy.AddDays(-1), hoy.AddDays(-1).AddDays(1).AddTicks(-1))
        };
    }
}
