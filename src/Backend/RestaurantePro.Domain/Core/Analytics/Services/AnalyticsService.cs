using RestaurantePro.Domain.Core.Analytics.DTOs;
using RestaurantePro.Domain.Core.Analytics.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;

namespace RestaurantePro.Domain.Core.Analytics.Services;

/// <summary>
/// Implementación del servicio de analytics y métricas operativas con consultas reales a la base de datos
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMesaRepository mesaRepository,
        ILogger<AnalyticsService> logger)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mesaRepository = mesaRepository;
        _logger = logger;
    }

    public async Task<MetricasDiaDto> ObtenerMetricasDiaAsync()
    {
        try
        {
            var fecha = DateTime.Today;
            var fechaInicio = fecha.Date;
            var fechaFin = fecha.Date.AddDays(1).AddTicks(-1);

            _logger.LogInformation("Obteniendo métricas del día {Fecha}", fecha);

            // Obtener comandas del día
            var comandasDelDia = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, true);

            // Calcular métricas básicas
            var totalVentas = comandasDelDia
                .Where(c => c.Estado == EstadoComanda.Finalizada && c.Total != null)
                .Sum(c => c.Total!.Total);

            var totalComandas = comandasDelDia.Count();
            var totalProductosVendidos = comandasDelDia
                .SelectMany(c => c.Items)
                .Sum(i => i.Cantidad);

            var clientesAtendidos = comandasDelDia
                .Where(c => c.ClienteId.HasValue)
                .Select(c => c.ClienteId!.Value)
                .Distinct()
                .Count();

            // Calcular tiempo promedio de preparación (simulado por ahora)
            var tiempoPromedioPreparacion = 15; // TODO: Implementar cálculo real basado en timestamps

            // Obtener ocupación de mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
            var mesasReservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada);
            var mesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible);
            var totalMesas = mesas.Count();

            var porcentajeOcupacion = totalMesas > 0 ? (decimal)(mesasOcupadas + mesasReservadas) / totalMesas * 100 : 0;

            // Obtener top productos del día
            var topProductos = await ObtenerTopProductosDelDiaAsync(comandasDelDia);

            var metricas = new MetricasDiaDto
            {
                Fecha = fecha,
                TotalVentas = totalVentas,
                TotalComandas = totalComandas,
                TotalProductosVendidos = totalProductosVendidos,
                TiempoPromedioPreparacion = tiempoPromedioPreparacion,
                PorcentajeOcupacionMesas = porcentajeOcupacion,
                ClientesAtendidos = clientesAtendidos,
                TopProductos = topProductos
            };

            _logger.LogInformation("Métricas del día calculadas: Ventas={TotalVentas}, Comandas={TotalComandas}, Productos={TotalProductos}", 
                totalVentas, totalComandas, totalProductosVendidos);

            return metricas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas del día");
            throw;
        }
    }

    public async Task<MetricasRangoDto> ObtenerMetricasRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            _logger.LogInformation("Obteniendo métricas del rango {FechaDesde} - {FechaHasta}", fechaDesde, fechaHasta);

            var fechaInicio = fechaDesde.Date;
            var fechaFin = fechaHasta.Date.AddDays(1).AddTicks(-1);

            // Obtener comandas del rango
            var comandasDelRango = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, true);

            // Calcular métricas del rango
            var totalVentas = comandasDelRango
                .Where(c => c.Estado == EstadoComanda.Finalizada && c.Total != null)
                .Sum(c => c.Total!.Total);

            var totalComandas = comandasDelRango.Count();
            var diasEnRango = (fechaHasta - fechaDesde).Days + 1;
            var promedioVentasDiarias = diasEnRango > 0 ? totalVentas / diasEnRango : 0;
            var promedioComandasDiarias = diasEnRango > 0 ? (decimal)totalComandas / diasEnRango : 0;

            // Generar métricas por día
            var metricasPorDia = new List<MetricasDiaDto>();
            for (var fecha = fechaDesde; fecha <= fechaHasta; fecha = fecha.AddDays(1))
            {
                var comandasDelDia = comandasDelRango.Where(c => c.FechaCreacion.Date == fecha.Date).ToList();
                var ventasDelDia = comandasDelDia
                    .Where(c => c.Estado == EstadoComanda.Finalizada && c.Total != null)
                    .Sum(c => c.Total!.Total);

                metricasPorDia.Add(new MetricasDiaDto
                {
                    Fecha = fecha,
                    TotalVentas = ventasDelDia,
                    TotalComandas = comandasDelDia.Count,
                    TotalProductosVendidos = comandasDelDia.SelectMany(c => c.Items).Sum(i => i.Cantidad),
                    TiempoPromedioPreparacion = 15, // TODO: Implementar cálculo real
                    PorcentajeOcupacionMesas = 0, // TODO: Implementar cálculo real por día
                    ClientesAtendidos = comandasDelDia.Where(c => c.ClienteId.HasValue).Select(c => c.ClienteId!.Value).Distinct().Count(),
                    TopProductos = new List<TopProductoDto>() // TODO: Implementar top productos por día
                });
            }

            var metricas = new MetricasRangoDto
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                TotalVentas = totalVentas,
                TotalComandas = totalComandas,
                PromedioVentasDiarias = promedioVentasDiarias,
                PromedioComandasDiarias = promedioComandasDiarias,
                MetricasPorDia = metricasPorDia
            };

            _logger.LogInformation("Métricas del rango calculadas: Ventas={TotalVentas}, Comandas={TotalComandas}", 
                totalVentas, totalComandas);

            return metricas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas del rango");
            throw;
        }
    }

    public async Task<List<TopProductoDto>> ObtenerTopProductosAsync(int limite, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        try
        {
            _logger.LogInformation("Obteniendo top {Limite} productos", limite);

            var fechaInicio = fechaDesde?.Date ?? DateTime.Today.AddDays(-30);
            var fechaFin = fechaHasta?.Date ?? DateTime.Today;

            // Obtener comandas del período
            var comandasDelPeriodo = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin.AddDays(1).AddTicks(-1), true);

            // Agrupar productos por cantidad vendida y total de ventas
            var productosAgrupados = comandasDelPeriodo
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .SelectMany(c => c.Items)
                .GroupBy(i => i.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    CantidadVendida = g.Sum(i => i.Cantidad),
                    TotalVentas = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(limite)
                .ToList();

            // Obtener información de productos
            var productos = await _productoRepository.ObtenerTodosAsync(true);
            var productosDict = productos.ToDictionary(p => p.Id, p => p);

            var topProductos = new List<TopProductoDto>();
            var posicion = 1;
            var totalVentasPeriodo = productosAgrupados.Sum(p => p.TotalVentas);

            foreach (var producto in productosAgrupados)
            {
                if (productosDict.TryGetValue(producto.ProductoId, out var productoInfo))
                {
                    var porcentajeTotalVentas = totalVentasPeriodo > 0 ? (producto.TotalVentas / totalVentasPeriodo) * 100 : 0;
                    var precioPromedio = producto.CantidadVendida > 0 ? producto.TotalVentas / producto.CantidadVendida : 0;

                    topProductos.Add(new TopProductoDto
                    {
                        Posicion = posicion++,
                        ProductoId = producto.ProductoId,
                        NombreProducto = productoInfo.Nombre ?? "Producto desconocido",
                        Categoria = productoInfo.CategoriaNombre ?? "Sin categoría",
                        CantidadVendida = producto.CantidadVendida,
                        TotalVentas = producto.TotalVentas,
                        PorcentajeTotalVentas = porcentajeTotalVentas,
                        PrecioPromedio = precioPromedio
                    });
                }
            }

            _logger.LogInformation("Top {Cantidad} productos obtenidos", topProductos.Count);
            return topProductos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top de productos");
            throw;
        }
    }

    public async Task<OcupacionMesasDto> ObtenerOcupacionMesasAsync(DateTime fecha)
    {
        try
        {
            _logger.LogInformation("Obteniendo ocupación de mesas para {Fecha}", fecha);

            var mesas = await _mesaRepository.ObtenerTodasAsync();
            var mesasOcupadas = mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
            var mesasReservadas = mesas.Count(m => m.Estado == EstadoMesa.Reservada);
            var mesasDisponibles = mesas.Count(m => m.Estado == EstadoMesa.Disponible);
            var totalMesas = mesas.Count();

            var ocupacion = new OcupacionMesasDto
            {
                Fecha = fecha,
                TotalMesas = totalMesas,
                MesasOcupadas = mesasOcupadas,
                MesasDisponibles = mesasDisponibles,
                MesasReservadas = mesasReservadas,
                TiempoPromedioOcupacion = 85, // TODO: Implementar cálculo real
                RotacionesMesas = 8 // TODO: Implementar cálculo real
            };

            _logger.LogInformation("Ocupación de mesas: Total={Total}, Ocupadas={Ocupadas}, Disponibles={Disponibles}, Reservadas={Reservadas}", 
                totalMesas, mesasOcupadas, mesasDisponibles, mesasReservadas);

            return ocupacion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ocupación de mesas");
            throw;
        }
    }

    public async Task<TiempoPreparacionDto> ObtenerTiempoPreparacionAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        try
        {
            _logger.LogInformation("Obteniendo tiempo de preparación");

            // TODO: Implementar cálculo real basado en timestamps de cambio de estado
            // Por ahora retornamos datos simulados
            var tiempoPreparacion = new TiempoPreparacionDto
            {
                TiempoPromedioMinutos = 15,
                TiempoMinimoMinutos = 5,
                TiempoMaximoMinutos = 45,
                TotalPreparaciones = 150,
                PreparacionesEnTiempo = 135,
                PreparacionesFueraTiempo = 15,
                TiempoEstandarMinutos = 20
            };

            _logger.LogInformation("Tiempo de preparación obtenido: Promedio={Promedio}min, En tiempo={EnTiempo}%", 
                tiempoPreparacion.TiempoPromedioMinutos, tiempoPreparacion.PorcentajeEnTiempo);

            return tiempoPreparacion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tiempo de preparación");
            throw;
        }
    }

    public async Task<List<VentasHoraDto>> ObtenerVentasPorHoraAsync(DateTime fecha)
    {
        try
        {
            _logger.LogInformation("Obteniendo ventas por hora para {Fecha}", fecha);

            var fechaInicio = fecha.Date;
            var fechaFin = fecha.Date.AddDays(1).AddTicks(-1);

            // Obtener comandas del día
            var comandasDelDia = await _comandaRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, true);

            // Agrupar por hora
            var ventasPorHora = new List<VentasHoraDto>();
            var totalVentasDia = comandasDelDia
                .Where(c => c.Estado == EstadoComanda.Finalizada && c.Total != null)
                .Sum(c => c.Total!.Total);

            for (int hora = 11; hora <= 22; hora++) // Horario de restaurante
            {
                var comandasEnHora = comandasDelDia
                    .Where(c => c.FechaCreacion.Hour == hora && c.Estado == EstadoComanda.Finalizada && c.Total != null)
                    .ToList();

                var ventasEnHora = comandasEnHora.Sum(c => c.Total!.Total);
                var numeroComandas = comandasEnHora.Count;
                var porcentajeTotalVentas = totalVentasDia > 0 ? (ventasEnHora / totalVentasDia) * 100 : 0;

                ventasPorHora.Add(new VentasHoraDto
                {
                    Hora = hora,
                    TotalVentas = ventasEnHora,
                    NumeroComandas = numeroComandas,
                    PorcentajeTotalVentas = porcentajeTotalVentas
                });
            }

            _logger.LogInformation("Ventas por hora obtenidas para {Fecha}: {TotalHoras} horas", fecha, ventasPorHora.Count);
            return ventasPorHora;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por hora");
            throw;
        }
    }

    private async Task<List<TopProductoDto>> ObtenerTopProductosDelDiaAsync(IEnumerable<Comanda> comandasDelDia)
    {
        try
        {
            // Agrupar productos por cantidad vendida
            var productosAgrupados = comandasDelDia
                .Where(c => c.Estado == EstadoComanda.Finalizada)
                .SelectMany(c => c.Items)
                .GroupBy(i => i.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    CantidadVendida = g.Sum(i => i.Cantidad),
                    TotalVentas = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(5)
                .ToList();

            // Obtener información de productos
            var productos = await _productoRepository.ObtenerTodosAsync(true);
            var productosDict = productos.ToDictionary(p => p.Id, p => p);

            var topProductos = new List<TopProductoDto>();
            var posicion = 1;
            var totalVentasDia = productosAgrupados.Sum(p => p.TotalVentas);

            foreach (var producto in productosAgrupados)
            {
                if (productosDict.TryGetValue(producto.ProductoId, out var productoInfo))
                {
                    var porcentajeTotalVentas = totalVentasDia > 0 ? (producto.TotalVentas / totalVentasDia) * 100 : 0;
                    var precioPromedio = producto.CantidadVendida > 0 ? producto.TotalVentas / producto.CantidadVendida : 0;

                    topProductos.Add(new TopProductoDto
                    {
                        Posicion = posicion++,
                        ProductoId = producto.ProductoId,
                        NombreProducto = productoInfo.Nombre ?? "Producto desconocido",
                        Categoria = productoInfo.CategoriaNombre ?? "Sin categoría",
                        CantidadVendida = producto.CantidadVendida,
                        TotalVentas = producto.TotalVentas,
                        PorcentajeTotalVentas = porcentajeTotalVentas,
                        PrecioPromedio = precioPromedio
                    });
                }
            }

            return topProductos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top productos del día");
            return new List<TopProductoDto>();
        }
    }
}