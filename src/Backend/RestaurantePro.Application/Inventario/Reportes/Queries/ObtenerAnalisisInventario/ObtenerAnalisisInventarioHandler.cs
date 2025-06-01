namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;

/// <summary>
/// Handler para generar análisis completo de inventario
/// Proporciona métricas, alertas y recomendaciones de inventario
/// </summary>
public class ObtenerAnalisisInventarioHandler : IRequestHandler<ObtenerAnalisisInventarioQuery, Result<AnalisisInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerAnalisisInventarioHandler> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IInventarioService _inventarioService;
    private readonly ICurrentUserService _currentUserService;

    public ObtenerAnalisisInventarioHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerAnalisisInventarioHandler> logger,
        IDateTimeService dateTimeService,
        IInventarioService inventarioService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _inventarioService = inventarioService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AnalisisInventarioDto>> Handle(ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📊 Iniciando análisis de inventario - Período: {FechaInicio} a {FechaFin}, Usuario: {UserId}",
            request.FechaInicio, request.FechaFin, _currentUserService.UserId);

        try
        {
            // 1. Validar parámetros
            var validacionResult = ValidarParametros(request);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure<AnalisisInventarioDto>(validacionResult.Error);
            }

            // 2. Obtener datos base del inventario
            var datosInventario = await ObtenerDatosInventario(request, cancellationToken);

            // 3. Calcular métricas principales
            var metricas = await CalcularMetricas(datosInventario, request, cancellationToken);

            // 4. Identificar alertas y problemas
            var alertas = await IdentificarAlertas(datosInventario, request, cancellationToken);

            // 5. Generar recomendaciones
            var recomendaciones = await GenerarRecomendaciones(datosInventario, metricas, alertas, cancellationToken);

            // 6. Obtener tendencias
            var tendencias = await CalcularTendencias(request, cancellationToken);

            // 7. Construir análisis completo
            var analisis = ConstruirAnalisisCompleto(datosInventario, metricas, alertas, recomendaciones, tendencias, request);

            _logger.LogInformation("✅ Análisis de inventario completado - {TotalIngredientes} ingredientes analizados, {TotalAlertas} alertas generadas",
                analisis.TotalIngredientes, analisis.Alertas.Count);

            return Result.Success(analisis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando análisis de inventario: {Error}", ex.Message);
            return Result.Failure<AnalisisInventarioDto>("Error interno al generar el análisis de inventario");
        }
    }

    private Result ValidarParametros(ObtenerAnalisisInventarioQuery request)
    {
        if (request.FechaInicio > request.FechaFin)
        {
            return Result.Failure("La fecha de inicio no puede ser posterior a la fecha de fin");
        }

        if (request.FechaFin > _dateTimeService.Now)
        {
            return Result.Failure("La fecha de fin no puede ser futura");
        }

        var diferenciaDias = (request.FechaFin - request.FechaInicio).TotalDays;
        if (diferenciaDias > 365)
        {
            return Result.Failure("El período de análisis no puede exceder 365 días");
        }

        return Result.Success();
    }

    private async Task<DatosInventarioAnalisis> ObtenerDatosInventario(ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var ingredientesQuery = _context.Ingredientes
            .Include(i => i.MovimientosStock)
            .AsQueryable();

        // Aplicar filtros opcionales
        if (request.CategoriaId.HasValue)
        {
            ingredientesQuery = ingredientesQuery.Where(i => i.CategoriaId == request.CategoriaId.Value);
        }

        if (request.SoloAlertaStock)
        {
            ingredientesQuery = ingredientesQuery.Where(i => i.StockActual <= i.StockMinimo);
        }

        if (request.SoloCriticos)
        {
            ingredientesQuery = ingredientesQuery.Where(i => i.EsCritico);
        }

        var ingredientes = await ingredientesQuery.ToListAsync(cancellationToken);

        // Obtener movimientos del período
        var movimientos = await _context.MovimientosStock
            .Where(m => m.FechaMovimiento >= request.FechaInicio && 
                       m.FechaMovimiento <= request.FechaFin)
            .Include(m => m.Ingrediente)
            .ToListAsync(cancellationToken);

        // Obtener órdenes de compra del período
        var ordenesCompra = await _context.OrdenesCompra
            .Where(o => o.FechaCreacion >= request.FechaInicio && 
                       o.FechaCreacion <= request.FechaFin)
            .Include(o => o.DetallesOrden)
            .ThenInclude(d => d.Ingrediente)
            .ToListAsync(cancellationToken);

        return new DatosInventarioAnalisis
        {
            Ingredientes = ingredientes,
            Movimientos = movimientos,
            OrdenesCompra = ordenesCompra,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin
        };
    }

    private async Task<MetricasInventario> CalcularMetricas(DatosInventarioAnalisis datos, ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var metricas = new MetricasInventario();

        // Métricas básicas
        metricas.TotalIngredientes = datos.Ingredientes.Count;
        metricas.IngredientesBajoStock = datos.Ingredientes.Count(i => i.StockActual <= i.StockMinimo);
        metricas.IngredientesCriticos = datos.Ingredientes.Count(i => i.EsCritico);
        metricas.IngredientesSinStock = datos.Ingredientes.Count(i => i.StockActual <= 0);

        // Valor total del inventario
        metricas.ValorTotalInventario = datos.Ingredientes.Sum(i => i.StockActual * i.CostoUnitario);
        metricas.ValorIngredientesCriticos = datos.Ingredientes
            .Where(i => i.EsCritico)
            .Sum(i => i.StockActual * i.CostoUnitario);

        // Análisis de movimientos
        var movimientosEntrada = datos.Movimientos.Where(m => m.TipoMovimiento == TipoMovimientoStock.Entrada);
        var movimientosSalida = datos.Movimientos.Where(m => m.TipoMovimiento == TipoMovimientoStock.Salida);

        metricas.TotalMovimientos = datos.Movimientos.Count;
        metricas.MovimientosEntrada = movimientosEntrada.Count();
        metricas.MovimientosSalida = movimientosSalida.Count();
        metricas.CantidadTotalEntrada = movimientosEntrada.Sum(m => m.Cantidad);
        metricas.CantidadTotalSalida = movimientosSalida.Sum(m => m.Cantidad);

        // Rotación de inventario
        if (metricas.ValorTotalInventario > 0)
        {
            var diasPeriodo = (datos.FechaFin - datos.FechaInicio).TotalDays;
            var consumoDiarioPromedio = metricas.CantidadTotalSalida / (decimal)diasPeriodo;
            var stockPromedio = datos.Ingredientes.Average(i => i.StockActual);
            
            if (stockPromedio > 0)
            {
                metricas.RotacionInventario = consumoDiarioPromedio / stockPromedio;
            }
        }

        // Análisis de compras
        metricas.TotalOrdenesCompra = datos.OrdenesCompra.Count;
        metricas.ValorTotalCompras = datos.OrdenesCompra.Sum(o => o.Total);
        metricas.PromedioOrdenCompra = metricas.TotalOrdenesCompra > 0 ? 
            metricas.ValorTotalCompras / metricas.TotalOrdenesCompra : 0;

        // Eficiencia del inventario
        metricas.PorcentajeStockOptimo = datos.Ingredientes.Count > 0 ?
            (decimal)datos.Ingredientes.Count(i => i.StockActual >= i.StockMinimo && i.StockActual <= i.StockMaximo) / datos.Ingredientes.Count * 100 : 0;

        return metricas;
    }

    private async Task<List<AlertaInventario>> IdentificarAlertas(DatosInventarioAnalisis datos, ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var alertas = new List<AlertaInventario>();

        // Alertas de stock bajo
        foreach (var ingrediente in datos.Ingredientes.Where(i => i.StockActual <= i.StockMinimo))
        {
            var criticidad = ingrediente.EsCritico ? "CRÍTICA" : 
                           ingrediente.StockActual <= 0 ? "ALTA" : "MEDIA";

            alertas.Add(new AlertaInventario
            {
                Tipo = "STOCK_BAJO",
                Criticidad = criticidad,
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                Mensaje = $"Stock bajo: {ingrediente.StockActual} {ingrediente.UnidadMedida} (Mínimo: {ingrediente.StockMinimo})",
                ValorActual = ingrediente.StockActual,
                ValorEsperado = ingrediente.StockMinimo,
                FechaDeteccion = _dateTimeService.Now
            });
        }

        // Alertas de stock excesivo
        foreach (var ingrediente in datos.Ingredientes.Where(i => i.StockActual > i.StockMaximo))
        {
            alertas.Add(new AlertaInventario
            {
                Tipo = "STOCK_EXCESIVO",
                Criticidad = "BAJA",
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                Mensaje = $"Stock excesivo: {ingrediente.StockActual} {ingrediente.UnidadMedida} (Máximo: {ingrediente.StockMaximo})",
                ValorActual = ingrediente.StockActual,
                ValorEsperado = ingrediente.StockMaximo,
                FechaDeteccion = _dateTimeService.Now
            });
        }

        // Alertas de ingredientes sin movimiento
        var diasSinMovimiento = 30;
        var fechaLimite = _dateTimeService.Now.AddDays(-diasSinMovimiento);

        foreach (var ingrediente in datos.Ingredientes)
        {
            var ultimoMovimiento = datos.Movimientos
                .Where(m => m.IngredienteId == ingrediente.Id)
                .OrderByDescending(m => m.FechaMovimiento)
                .FirstOrDefault();

            if (ultimoMovimiento == null || ultimoMovimiento.FechaMovimiento < fechaLimite)
            {
                alertas.Add(new AlertaInventario
                {
                    Tipo = "SIN_MOVIMIENTO",
                    Criticidad = "MEDIA",
                    IngredienteId = ingrediente.Id,
                    NombreIngrediente = ingrediente.Nombre,
                    Mensaje = $"Sin movimiento por {diasSinMovimiento}+ días",
                    FechaDeteccion = _dateTimeService.Now
                });
            }
        }

        return alertas.OrderByDescending(a => a.Criticidad).ToList();
    }

    private async Task<List<RecomendacionInventario>> GenerarRecomendaciones(
        DatosInventarioAnalisis datos, 
        MetricasInventario metricas, 
        List<AlertaInventario> alertas, 
        CancellationToken cancellationToken)
    {
        var recomendaciones = new List<RecomendacionInventario>();

        // Recomendaciones basadas en alertas críticas
        var alertasCriticas = alertas.Where(a => a.Criticidad == "CRÍTICA").ToList();
        if (alertasCriticas.Any())
        {
            recomendaciones.Add(new RecomendacionInventario
            {
                Tipo = "URGENTE",
                Prioridad = "ALTA",
                Titulo = "Reabastecer ingredientes críticos",
                Descripcion = $"Se requiere reabastecer {alertasCriticas.Count} ingredientes críticos de forma inmediata",
                Accion = "Generar órdenes de compra de emergencia",
                ImpactoEstimado = "Evitar interrupción del servicio"
            });
        }

        // Recomendación de optimización de stock
        if (metricas.PorcentajeStockOptimo < 70)
        {
            recomendaciones.Add(new RecomendacionInventario
            {
                Tipo = "OPTIMIZACIÓN",
                Prioridad = "MEDIA",
                Titulo = "Optimizar niveles de stock",
                Descripcion = $"Solo el {metricas.PorcentajeStockOptimo:F1}% del inventario está en niveles óptimos",
                Accion = "Revisar y ajustar niveles mínimos y máximos de stock",
                ImpactoEstimado = "Reducir costos de almacenamiento y mejorar rotación"
            });
        }

        // Recomendación de rotación
        if (metricas.RotacionInventario < 0.1m)
        {
            recomendaciones.Add(new RecomendacionInventario
            {
                Tipo = "ROTACIÓN",
                Prioridad = "MEDIA",
                Titulo = "Mejorar rotación de inventario",
                Descripcion = "La rotación de inventario es baja, indicando posible sobrestock",
                Accion = "Implementar estrategias FIFO y revisar frecuencia de pedidos",
                ImpactoEstimado = "Reducir desperdicio y liberar capital de trabajo"
            });
        }

        // Recomendaciones para ingredientes sin movimiento
        var ingredientesSinMovimiento = alertas.Where(a => a.Tipo == "SIN_MOVIMIENTO").ToList();
        if (ingredientesSinMovimiento.Count > 5)
        {
            recomendaciones.Add(new RecomendacionInventario
            {
                Tipo = "REVISIÓN",
                Prioridad = "BAJA",
                Titulo = "Revisar ingredientes sin rotación",
                Descripcion = $"{ingredientesSinMovimiento.Count} ingredientes sin movimiento reciente",
                Accion = "Evaluar si estos ingredientes siguen siendo necesarios",
                ImpactoEstimado = "Optimizar espacio de almacenamiento"
            });
        }

        return recomendaciones.OrderByDescending(r => r.Prioridad).ToList();
    }

    private async Task<TendenciasInventario> CalcularTendencias(ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var tendencias = new TendenciasInventario();

        // Calcular tendencias de los últimos meses para comparar
        var fechaInicioComparacion = request.FechaInicio.AddMonths(-3);
        
        var movimientosPeriodoComparacion = await _context.MovimientosStock
            .Where(m => m.FechaMovimiento >= fechaInicioComparacion && m.FechaMovimiento < request.FechaInicio)
            .ToListAsync(cancellationToken);

        var movimientosPeriodoActual = await _context.MovimientosStock
            .Where(m => m.FechaMovimiento >= request.FechaInicio && m.FechaMovimiento <= request.FechaFin)
            .ToListAsync(cancellationToken);

        // Tendencia de consumo
        var consumoAnterior = movimientosPeriodoComparacion
            .Where(m => m.TipoMovimiento == TipoMovimientoStock.Salida)
            .Sum(m => m.Cantidad);

        var consumoActual = movimientosPeriodoActual
            .Where(m => m.TipoMovimiento == TipoMovimientoStock.Salida)
            .Sum(m => m.Cantidad);

        tendencias.TendenciaConsumo = consumoAnterior > 0 ? 
            ((consumoActual - consumoAnterior) / consumoAnterior) * 100 : 0;

        // Tendencia de compras
        var comprasAnteriores = await _context.OrdenesCompra
            .Where(o => o.FechaCreacion >= fechaInicioComparacion && o.FechaCreacion < request.FechaInicio)
            .SumAsync(o => o.Total, cancellationToken);

        var comprasActuales = await _context.OrdenesCompra
            .Where(o => o.FechaCreacion >= request.FechaInicio && o.FechaCreacion <= request.FechaFin)
            .SumAsync(o => o.Total, cancellationToken);

        tendencias.TendenciaCompras = comprasAnteriores > 0 ? 
            ((comprasActuales - comprasAnteriores) / comprasAnteriores) * 100 : 0;

        return tendencias;
    }

    private AnalisisInventarioDto ConstruirAnalisisCompleto(
        DatosInventarioAnalisis datos,
        MetricasInventario metricas,
        List<AlertaInventario> alertas,
        List<RecomendacionInventario> recomendaciones,
        TendenciasInventario tendencias,
        ObtenerAnalisisInventarioQuery request)
    {
        return new AnalisisInventarioDto
        {
            FechaGeneracion = _dateTimeService.Now,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            UsuarioGeneradorId = _currentUserService.UserId,
            
            // Métricas principales
            TotalIngredientes = metricas.TotalIngredientes,
            IngredientesBajoStock = metricas.IngredientesBajoStock,
            IngredientesCriticos = metricas.IngredientesCriticos,
            IngredientesSinStock = metricas.IngredientesSinStock,
            ValorTotalInventario = metricas.ValorTotalInventario,
            PorcentajeStockOptimo = metricas.PorcentajeStockOptimo,
            RotacionInventario = metricas.RotacionInventario,
            
            // Análisis de movimientos
            TotalMovimientos = metricas.TotalMovimientos,
            MovimientosEntrada = metricas.MovimientosEntrada,
            MovimientosSalida = metricas.MovimientosSalida,
            
            // Análisis de compras
            TotalOrdenesCompra = metricas.TotalOrdenesCompra,
            ValorTotalCompras = metricas.ValorTotalCompras,
            PromedioOrdenCompra = metricas.PromedioOrdenCompra,
            
            // Alertas y recomendaciones
            Alertas = alertas,
            Recomendaciones = recomendaciones,
            
            // Tendencias
            TendenciaConsumo = tendencias.TendenciaConsumo,
            TendenciaCompras = tendencias.TendenciaCompras,
            
            // Resumen ejecutivo
            ResumenEjecutivo = GenerarResumenEjecutivo(metricas, alertas, recomendaciones, tendencias)
        };
    }

    private string GenerarResumenEjecutivo(
        MetricasInventario metricas, 
        List<AlertaInventario> alertas, 
        List<RecomendacionInventario> recomendaciones, 
        TendenciasInventario tendencias)
    {
        var resumen = new List<string>();

        // Estado general
        var alertasCriticas = alertas.Count(a => a.Criticidad == "CRÍTICA");
        if (alertasCriticas > 0)
        {
            resumen.Add($"⚠️ ATENCIÓN: {alertasCriticas} alertas críticas requieren acción inmediata.");
        }

        // Eficiencia del inventario
        if (metricas.PorcentajeStockOptimo >= 80)
        {
            resumen.Add($"✅ Inventario eficiente: {metricas.PorcentajeStockOptimo:F1}% en niveles óptimos.");
        }
        else if (metricas.PorcentajeStockOptimo < 70)
        {
            resumen.Add($"📊 Oportunidad de mejora: Solo {metricas.PorcentajeStockOptimo:F1}% en niveles óptimos.");
        }

        // Valor del inventario
        resumen.Add($"💰 Valor total inventario: ${metricas.ValorTotalInventario:N2}");

        // Tendencias
        if (Math.Abs(tendencias.TendenciaConsumo) > 10)
        {
            var direccion = tendencias.TendenciaConsumo > 0 ? "incremento" : "disminución";
            resumen.Add($"📈 Tendencia consumo: {direccion} del {Math.Abs(tendencias.TendenciaConsumo):F1}%");
        }

        // Recomendaciones principales
        var recomendacionesAltas = recomendaciones.Count(r => r.Prioridad == "ALTA");
        if (recomendacionesAltas > 0)
        {
            resumen.Add($"🎯 {recomendacionesAltas} recomendaciones de alta prioridad pendientes.");
        }

        return string.Join(" ", resumen);
    }

    // DTOs internos para el análisis
    private class DatosInventarioAnalisis
    {
        public List<Ingrediente> Ingredientes { get; set; } = new();
        public List<MovimientoStock> Movimientos { get; set; } = new();
        public List<OrdenCompra> OrdenesCompra { get; set; } = new();
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    private class MetricasInventario
    {
        public int TotalIngredientes { get; set; }
        public int IngredientesBajoStock { get; set; }
        public int IngredientesCriticos { get; set; }
        public int IngredientesSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal ValorIngredientesCriticos { get; set; }
        public int TotalMovimientos { get; set; }
        public int MovimientosEntrada { get; set; }
        public int MovimientosSalida { get; set; }
        public decimal CantidadTotalEntrada { get; set; }
        public decimal CantidadTotalSalida { get; set; }
        public decimal RotacionInventario { get; set; }
        public int TotalOrdenesCompra { get; set; }
        public decimal ValorTotalCompras { get; set; }
        public decimal PromedioOrdenCompra { get; set; }
        public decimal PorcentajeStockOptimo { get; set; }
    }

    private class TendenciasInventario
    {
        public decimal TendenciaConsumo { get; set; }
        public decimal TendenciaCompras { get; set; }
    }
} 