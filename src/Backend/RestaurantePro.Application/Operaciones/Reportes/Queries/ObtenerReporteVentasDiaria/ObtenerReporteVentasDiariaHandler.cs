namespace RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;

/// <summary>
/// 📊 Handler para ObtenerReporteVentasDiariaQuery
/// Procesa análisis detallado de ventas diarias con diferentes niveles de detalle
/// </summary>
public class ObtenerReporteVentasDiariaHandler : IRequestHandler<ObtenerReporteVentasDiariaQuery, Result<ReporteVentasDiariaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerReporteVentasDiariaHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ObtenerReporteVentasDiariaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerReporteVentasDiariaHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ReporteVentasDiariaDto>> Handle(
        ObtenerReporteVentasDiariaQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📊 Generando reporte de ventas diarias - Fecha: {Fecha}, Nivel: {Nivel}", 
            request.FechaReporte.ToShortDateString(), request.NivelDetalle);

        try
        {
            // 1. Obtener comandas del día
            var comandasDia = await ObtenerComandasDelDia(request, cancellationToken);
            if (!comandasDia.Any())
            {
                _logger.LogWarning("⚠️ No se encontraron comandas para la fecha {Fecha}", request.FechaReporte);
                return Result.Success(CrearReporteVacio(request));
            }

            // 2. Calcular métricas básicas
            var metricasBasicas = CalcularMetricasBasicas(comandasDia);

            // 3. Generar análisis según nivel de detalle
            var reporte = new ReporteVentasDiariaDto
            {
                FechaReporte = request.FechaReporte,
                NivelDetalle = request.NivelDetalle,
                MetricasBasicas = metricasBasicas,
                FechaGeneracion = DateTime.UtcNow
            };

            // 4. Agregar análisis específicos según configuración
            await AgregarAnalisisEspecificos(reporte, request, comandasDia, cancellationToken);

            // 5. Agregar comparativos si se solicita
            if (request.IncluirComparativoPeriodoAnterior)
            {
                await AgregarComparativoPeriodoAnterior(reporte, request, cancellationToken);
            }

            // 6. Agregar tendencias semanales si se solicita
            if (request.IncluirTendenciasSemana)
            {
                await AgregarTendenciasSemana(reporte, request, cancellationToken);
            }

            _logger.LogInformation("✅ Reporte de ventas diarias generado exitosamente - {TotalComandas} comandas, ${MontoTotal}", 
                metricasBasicas.TotalComandas, metricasBasicas.MontoTotalVentas);

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de ventas diarias para fecha {Fecha}", request.FechaReporte);
            return Result.Failure<ReporteVentasDiariaDto>($"Error interno generando reporte: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene las comandas del día según los filtros especificados
    /// </summary>
    private async Task<List<Comanda>> ObtenerComandasDelDia(
        ObtenerReporteVentasDiariaQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Comandas
            .Where(c => c.FechaCreacion.Date == request.FechaReporte.Date)
            .Include(c => c.DetalleComandas)
            .ThenInclude(d => d.Producto)
            .Include(c => c.Mesa)
            .Include(c => c.Mesero)
            .AsQueryable();

        // Aplicar filtros específicos
        if (request.MesesEspecificos?.Any() == true)
        {
            query = query.Where(c => request.MesesEspecificos.Contains(c.MesaId));
        }

        if (request.MeserosEspecificos?.Any() == true)
        {
            query = query.Where(c => request.MeserosEspecificos.Contains(c.MeseroId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calcula las métricas básicas del día
    /// </summary>
    private MetricasBasicasDto CalcularMetricasBasicas(List<Comanda> comandas)
    {
        var totalComandas = comandas.Count;
        var montoTotal = comandas.Sum(c => c.MontoTotal);
        var promedioComanda = totalComandas > 0 ? montoTotal / totalComandas : 0;
        
        return new MetricasBasicasDto
        {
            TotalComandas = totalComandas,
            MontoTotalVentas = montoTotal,
            PromedioVentaPorComanda = promedioComanda,
            HoraPico = CalcularHoraPico(comandas),
            ProductoMasVendido = CalcularProductoMasVendido(comandas)
        };
    }

    /// <summary>
    /// Agrega análisis específicos según la configuración
    /// </summary>
    private async Task AgregarAnalisisEspecificos(
        ReporteVentasDiariaDto reporte,
        ObtenerReporteVentasDiariaQuery request,
        List<Comanda> comandas,
        CancellationToken cancellationToken)
    {
        // Análisis por mesa
        if (request.IncluirAnalisisPorMesa)
        {
            reporte.AnalisisPorMesa = GenerarAnalisisPorMesa(comandas);
        }

        // Análisis por mesero
        if (request.IncluirAnalisisPorMesero)
        {
            reporte.AnalisisPorMesero = GenerarAnalisisPorMesero(comandas);
        }

        // Análisis de productos
        if (request.IncluirAnalisisProductos)
        {
            reporte.AnalisisProductos = GenerarAnalisisProductos(comandas);
        }

        // Distribución horaria
        reporte.DistribucionHoraria = GenerarDistribucionHoraria(comandas);
    }

    /// <summary>
    /// Genera análisis por mesa
    /// </summary>
    private List<AnalisisMesaDto> GenerarAnalisisPorMesa(List<Comanda> comandas)
    {
        return comandas
            .GroupBy(c => new { c.MesaId, c.Mesa?.Numero })
            .Select(g => new AnalisisMesaDto
            {
                MesaId = g.Key.MesaId,
                NumeroMesa = g.Key.Numero ?? 0,
                TotalComandas = g.Count(),
                MontoTotal = g.Sum(c => c.MontoTotal),
                PromedioComanda = g.Average(c => c.MontoTotal),
                TiempoPromedioOcupacion = CalcularTiempoPromedioMesa(g.ToList())
            })
            .OrderByDescending(m => m.MontoTotal)
            .ToList();
    }

    /// <summary>
    /// Genera análisis por mesero
    /// </summary>
    private List<AnalisisMeseroDto> GenerarAnalisisPorMesero(List<Comanda> comandas)
    {
        return comandas
            .GroupBy(c => new { c.MeseroId, c.Mesero?.Nombre })
            .Select(g => new AnalisisMeseroDto
            {
                MeseroId = g.Key.MeseroId,
                NombreMesero = g.Key.Nombre ?? "Desconocido",
                TotalComandas = g.Count(),
                MontoTotal = g.Sum(c => c.MontoTotal),
                PromedioComanda = g.Average(c => c.MontoTotal),
                EficienciaVentas = CalcularEficienciaVentas(g.ToList())
            })
            .OrderByDescending(m => m.MontoTotal)
            .ToList();
    }

    /// <summary>
    /// Genera análisis de productos
    /// </summary>
    private List<AnalisisProductoDto> GenerarAnalisisProductos(List<Comanda> comandas)
    {
        var detalles = comandas.SelectMany(c => c.DetalleComandas).ToList();
        
        return detalles
            .GroupBy(d => new { d.ProductoId, d.Producto?.Nombre })
            .Select(g => new AnalisisProductoDto
            {
                ProductoId = g.Key.ProductoId,
                NombreProducto = g.Key.Nombre ?? "Desconocido",
                CantidadVendida = g.Sum(d => d.Cantidad),
                MontoTotal = g.Sum(d => d.PrecioUnitario * d.Cantidad),
                PromedioVenta = g.Average(d => d.PrecioUnitario),
                PorcentajeVentas = 0 // Se calculará después
            })
            .OrderByDescending(p => p.CantidadVendida)
            .ToList();
    }

    /// <summary>
    /// Genera distribución horaria de ventas
    /// </summary>
    private List<DistribucionHorariaDto> GenerarDistribucionHoraria(List<Comanda> comandas)
    {
        return comandas
            .GroupBy(c => c.FechaCreacion.Hour)
            .Select(g => new DistribucionHorariaDto
            {
                Hora = g.Key,
                TotalComandas = g.Count(),
                MontoTotal = g.Sum(c => c.MontoTotal),
                PromedioComanda = g.Average(c => c.MontoTotal)
            })
            .OrderBy(d => d.Hora)
            .ToList();
    }

    /// <summary>
    /// Agrega comparativo con período anterior
    /// </summary>
    private async Task AgregarComparativoPeriodoAnterior(
        ReporteVentasDiariaDto reporte,
        ObtenerReporteVentasDiariaQuery request,
        CancellationToken cancellationToken)
    {
        var fechaAnterior = request.FechaReporte.AddDays(-1);
        var comandasAnteriores = await _context.Comandas
            .Where(c => c.FechaCreacion.Date == fechaAnterior.Date)
            .ToListAsync(cancellationToken);

        if (comandasAnteriores.Any())
        {
            var metricasAnteriores = CalcularMetricasBasicas(comandasAnteriores);
            reporte.ComparativoPeriodoAnterior = new ComparativoPeriodoDto
            {
                FechaAnterior = fechaAnterior,
                MetricasAnteriores = metricasAnteriores,
                VariacionComandas = CalcularVariacionPorcentual(reporte.MetricasBasicas.TotalComandas, metricasAnteriores.TotalComandas),
                VariacionVentas = CalcularVariacionPorcentual(reporte.MetricasBasicas.MontoTotalVentas, metricasAnteriores.MontoTotalVentas)
            };
        }
    }

    /// <summary>
    /// Agrega tendencias de la semana
    /// </summary>
    private async Task AgregarTendenciasSemana(
        ReporteVentasDiariaDto reporte,
        ObtenerReporteVentasDiariaQuery request,
        CancellationToken cancellationToken)
    {
        var fechaInicio = request.FechaReporte.AddDays(-6);
        var comandasSemana = await _context.Comandas
            .Where(c => c.FechaCreacion.Date >= fechaInicio.Date && 
                       c.FechaCreacion.Date <= request.FechaReporte.Date)
            .ToListAsync(cancellationToken);

        reporte.TendenciasSemana = comandasSemana
            .GroupBy(c => c.FechaCreacion.Date)
            .Select(g => new TendenciaDiariaDto
            {
                Fecha = g.Key,
                TotalComandas = g.Count(),
                MontoTotal = g.Sum(c => c.MontoTotal)
            })
            .OrderBy(t => t.Fecha)
            .ToList();
    }

    /// <summary>
    /// Crea un reporte vacío cuando no hay datos
    /// </summary>
    private ReporteVentasDiariaDto CrearReporteVacio(ObtenerReporteVentasDiariaQuery request)
    {
        return new ReporteVentasDiariaDto
        {
            FechaReporte = request.FechaReporte,
            NivelDetalle = request.NivelDetalle,
            MetricasBasicas = new MetricasBasicasDto(),
            FechaGeneracion = DateTime.UtcNow
        };
    }

    // Métodos auxiliares de cálculo
    private TimeSpan CalcularHoraPico(List<Comanda> comandas)
    {
        if (!comandas.Any()) return TimeSpan.Zero;
        
        var horaPico = comandas
            .GroupBy(c => c.FechaCreacion.Hour)
            .OrderByDescending(g => g.Count())
            .First().Key;
            
        return TimeSpan.FromHours(horaPico);
    }

    private string CalcularProductoMasVendido(List<Comanda> comandas)
    {
        var producto = comandas
            .SelectMany(c => c.DetalleComandas)
            .GroupBy(d => d.Producto?.Nombre)
            .OrderByDescending(g => g.Sum(d => d.Cantidad))
            .FirstOrDefault();
            
        return producto?.Key ?? "N/A";
    }

    private TimeSpan CalcularTiempoPromedioMesa(List<Comanda> comandasMesa)
    {
        // Simulación - en realidad necesitaríamos datos de inicio/fin de ocupación
        return TimeSpan.FromMinutes(45);
    }

    private decimal CalcularEficienciaVentas(List<Comanda> comandasMesero)
    {
        // Simulación de cálculo de eficiencia basado en ventas/tiempo
        return comandasMesero.Average(c => c.MontoTotal) / 100;
    }

    private decimal CalcularVariacionPorcentual(decimal valorActual, decimal valorAnterior)
    {
        if (valorAnterior == 0) return 0;
        return ((valorActual - valorAnterior) / valorAnterior) * 100;
    }
} 