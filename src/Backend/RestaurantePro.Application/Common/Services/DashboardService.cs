using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Application.Common.Models.Dashboard;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

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

    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        _logger.LogInformation("📊 Obteniendo resumen completo del dashboard");

        try
        {
            var metricas = await ObtenerMetricasAsync();
            var productosMasVendidos = await ObtenerProductosMasVendidosAsync(5);
            var ventasPorPeriodo = await ObtenerVentasPorPeriodoAsync(7);
            var estadoMesas = await ObtenerEstadoMesasAsync();
            var comandasPorEstado = await ObtenerComandasPorEstadoAsync();
            var ingresosPorHora = await ObtenerIngresosPorHoraAsync();

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

    public async Task<DashboardMetricasDto> ObtenerMetricasAsync()
    {
        _logger.LogInformation("📈 Obteniendo métricas del dashboard");

        try
        {
            var hoy = DateTime.Today;
            var ayer = hoy.AddDays(-1);
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Obtener facturas del día
            var facturasHoy = await _facturaRepository.ObtenerPorRangoFechasAsync(hoy, hoy.AddDays(1).AddTicks(-1));
            var facturasAyer = await _facturaRepository.ObtenerPorRangoFechasAsync(ayer, ayer.AddDays(1).AddTicks(-1));
            var facturasSemana = await _facturaRepository.ObtenerPorRangoFechasAsync(inicioSemana, hoy.AddDays(1).AddTicks(-1));
            var facturasMes = await _facturaRepository.ObtenerPorRangoFechasAsync(inicioMes, hoy.AddDays(1).AddTicks(-1));

            // Calcular ventas
            var ventasHoy = facturasHoy.Where(f => f.Estado == EstadoFactura.Pagada)
                                     .Sum(f => f.Total);
            var ventasAyer = facturasAyer.Where(f => f.Estado == EstadoFactura.Pagada)
                                        .Sum(f => f.Total);
            var ventasSemana = facturasSemana.Where(f => f.Estado == EstadoFactura.Pagada)
                                            .Sum(f => f.Total);
            var ventasMes = facturasMes.Where(f => f.Estado == EstadoFactura.Pagada)
                                      .Sum(f => f.Total);

            // Obtener mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
            var mesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible);

            // Obtener comandas
            var comandasHoy = await _comandaRepository.ObtenerPorRangoFechasAsync(hoy, hoy.AddDays(1).AddTicks(-1));
            var comandasActivas = comandasHoy.Count(c => c.Estado != EstadoComanda.Finalizada && 
                                                         c.Estado != EstadoComanda.Cancelada);
            var comandasCompletadas = comandasHoy.Count(c => c.Estado == EstadoComanda.Finalizada);

            // Calcular productos vendidos hoy
            var productosVendidosHoy = comandasHoy
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .SelectMany(c => c.Items)
                .Sum(i => i.Cantidad);

            // Calcular clientes atendidos hoy
            var clientesAtendidosHoy = comandasHoy
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .Select(c => c.ClienteId)
                .Distinct()
                .Count();

            // Calcular promedio de ticket
            var promedioTicket = comandasCompletadas > 0 
                ? ventasHoy / comandasCompletadas 
                : 0;

            // Calcular crecimiento de ventas
            var crecimientoVentas = ventasAyer > 0 
                ? ((ventasHoy - ventasAyer) / ventasAyer) * 100 
                : 0;

            return new DashboardMetricasDto
            {
                VentasHoy = ventasHoy,
                VentasAyer = ventasAyer,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                MesasOcupadas = mesasOcupadas,
                MesasDisponibles = mesasDisponibles,
                TotalMesas = mesas.Count(),
                ComandasActivas = comandasActivas,
                ComandasCompletadas = comandasCompletadas,
                ProductosVendidosHoy = productosVendidosHoy,
                ClientesAtendidosHoy = clientesAtendidosHoy,
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

    public async Task<List<DashboardProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(int cantidad = 5)
    {
        _logger.LogInformation("🍽️ Obteniendo productos más vendidos (cantidad: {Cantidad})", cantidad);

        try
        {
            var hoy = DateTime.Today;
            var comandasHoy = await _comandaRepository.ObtenerPorRangoFechasAsync(hoy, hoy.AddDays(1).AddTicks(-1));
            
            var productosVendidos = comandasHoy
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

    public async Task<List<DashboardVentaPorPeriodoDto>> ObtenerVentasPorPeriodoAsync(int dias = 7)
    {
        _logger.LogInformation("📅 Obteniendo ventas por período (días: {Dias})", dias);

        try
        {
            var resultado = new List<DashboardVentaPorPeriodoDto>();
            var hoy = DateTime.Today;

            for (int i = dias - 1; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);
                var inicioDia = fecha;
                var finDia = fecha.AddDays(1).AddTicks(-1);

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

    public async Task<DashboardComandasPorEstadoDto> ObtenerComandasPorEstadoAsync()
    {
        _logger.LogInformation("📋 Obteniendo comandas por estado");

        try
        {
            var hoy = DateTime.Today;
            var comandasHoy = await _comandaRepository.ObtenerPorRangoFechasAsync(hoy, hoy.AddDays(1).AddTicks(-1));

            return new DashboardComandasPorEstadoDto
            {
                Creadas = comandasHoy.Count(c => c.Estado == EstadoComanda.Creada),
                EnProceso = comandasHoy.Count(c => c.Estado == EstadoComanda.EnProceso),
                Lista = comandasHoy.Count(c => c.Estado == EstadoComanda.Lista),
                Entregada = comandasHoy.Count(c => c.Estado == EstadoComanda.Entregada),
                Finalizada = comandasHoy.Count(c => c.Estado == EstadoComanda.Finalizada),
                Cancelada = comandasHoy.Count(c => c.Estado == EstadoComanda.Cancelada),
                Total = comandasHoy.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener comandas por estado");
            throw;
        }
    }

    public async Task<List<DashboardIngresosPorHoraDto>> ObtenerIngresosPorHoraAsync()
    {
        _logger.LogInformation("⏰ Obteniendo ingresos por hora del día actual");

        try
        {
            var hoy = DateTime.Today;
            var resultado = new List<DashboardIngresosPorHoraDto>();

            for (int hora = 0; hora < 24; hora++)
            {
                var inicioHora = hoy.AddHours(hora);
                var finHora = hoy.AddHours(hora + 1).AddTicks(-1);

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
}
