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
    private readonly IInventarioServiceFacade _inventarioService;
    private readonly ICurrentUserService _currentUserService;

    public ObtenerAnalisisInventarioHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerAnalisisInventarioHandler> logger,
        IDateTimeService dateTimeService,
        IInventarioServiceFacade inventarioService,
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
                return Result.Failure<AnalisisInventarioDto>(validacionResult.Error ?? "Error de validación");
            }

            // 2. Obtener datos base del inventario
            var datosInventario = await ObtenerDatosInventario(request, cancellationToken);

            // 3. Calcular métricas principales
            var metricas = await CalcularMetricas(datosInventario, request, cancellationToken);

            // 4. Identificar alertas y problemas
            var alertas = IdentificarAlertas(datosInventario, request, cancellationToken);

            // 5. Generar recomendaciones
            var recomendaciones = GenerarRecomendaciones(datosInventario, metricas, alertas, cancellationToken);

            // 6. Obtener tendencias
            var tendencias = await CalcularTendencias(request, cancellationToken);

            // 7. Construir análisis completo
            var analisis = ConstruirAnalisisCompleto(datosInventario, metricas, alertas, recomendaciones, tendencias, request);

            _logger.LogInformation("✅ Análisis de inventario completado - {TotalIngredientes} ingredientes analizados, {TotalAlertas} alertas generadas",
                analisis.ResumenExecutivo.TotalIngredientes, analisis.Alertas.Count);

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
            .Include(i => i.Movimientos)
            .AsQueryable();

        // Aplicar filtros opcionales
        if (request.CategoriaId.HasValue)
        {
            // TODO: Agregar filtro por categoría cuando exista la propiedad
            // ingredientesQuery = ingredientesQuery.Where(i => i.CategoriaId == request.CategoriaId.Value);
        }

        if (request.SoloAlertaStock)
        {
            ingredientesQuery = ingredientesQuery.Where(i => i.Stock <= i.StockMinimo);
        }

        if (request.SoloCriticos)
        {
            // TODO: Implementar lógica de ingredientes críticos cuando esté disponible
            // ingredientesQuery = ingredientesQuery.Where(i => i.EsCritico);
        }

        var ingredientes = await ingredientesQuery.ToListAsync(cancellationToken);

        // Obtener movimientos del período
        var movimientos = await _context.MovimientosInventario
            .Where(m => m.Fecha >= request.FechaInicio && 
                       m.Fecha <= request.FechaFin)
            .ToListAsync(cancellationToken);

        // TODO: Obtener órdenes de compra cuando estén disponibles
        var ordenesCompra = new List<dynamic>(); // Placeholder temporal

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
        metricas.IngredientesBajoStock = datos.Ingredientes.Count(i => i.Stock <= i.StockMinimo);
        metricas.IngredientesCriticos = 0; // TODO: Implementar cuando esté disponible EsCritico
        metricas.IngredientesSinStock = datos.Ingredientes.Count(i => i.Stock <= 0);

        // Valor total del inventario
        metricas.ValorTotalInventario = datos.Ingredientes.Sum(i => i.Stock * i.CostoPromedio);
        metricas.ValorIngredientesCriticos = 0; // TODO: Calcular cuando EsCritico esté disponible

        // Análisis de movimientos
        var movimientosEntrada = datos.Movimientos.Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso);
        var movimientosSalida = datos.Movimientos.Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso);

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
            var stockPromedio = datos.Ingredientes.Average(i => i.Stock);
            
            if (stockPromedio > 0)
            {
                metricas.RotacionInventario = consumoDiarioPromedio / stockPromedio;
            }
        }

        // TODO: Análisis de compras cuando OrdenesCompra esté disponible
        metricas.TotalOrdenesCompra = 0;
        metricas.ValorTotalCompras = 0;
        metricas.PromedioOrdenCompra = 0;

        // Eficiencia del inventario (simplificado)
        metricas.PorcentajeStockOptimo = datos.Ingredientes.Count > 0 ?
            (decimal)datos.Ingredientes.Count(i => i.Stock >= i.StockMinimo) / datos.Ingredientes.Count * 100 : 0;

        return metricas;
    }

    private List<AlertaInventarioDto> IdentificarAlertas(DatosInventarioAnalisis datos, ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var alertas = new List<AlertaInventarioDto>();

        // Alertas de stock bajo
        foreach (var ingrediente in datos.Ingredientes.Where(i => i.Stock <= i.StockMinimo))
        {
            var tipoAlerta = ingrediente.Stock <= 0 ? "SinStock" :
                            ingrediente.Stock <= (ingrediente.StockMinimo * 0.5m) ? "StockCritico" :
                            "StockBajo";

            var prioridad = ingrediente.Stock <= 0 ? "Critica" :
                           ingrediente.Stock <= (ingrediente.StockMinimo * 0.5m) ? "Alta" :
                           "Media";

            alertas.Add(new AlertaInventarioDto
            {
                TipoAlerta = tipoAlerta,
                Prioridad = prioridad,
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                Titulo = tipoAlerta == "SinStock" ? "Sin Stock" : "Stock Bajo",
                Descripcion = $"Stock bajo: {ingrediente.Stock} {ingrediente.UnidadMedida} (Mínimo: {ingrediente.StockMinimo})",
                ValorActual = ingrediente.Stock,
                ValorEsperado = ingrediente.StockMinimo,
                FechaDeteccion = _dateTimeService.Now,
                AccionRecomendada = "Reabastecer inmediatamente"
            });
        }

        // TODO: Alertas de stock excesivo cuando StockMaximo esté disponible
        // TODO: Alertas de ingredientes sin movimiento
        
        return alertas.OrderByDescending(a => a.Prioridad).ToList();
    }

    private List<RecomendacionCompraDto> GenerarRecomendaciones(
        DatosInventarioAnalisis datos, 
        MetricasInventario metricas, 
        List<AlertaInventarioDto> alertas, 
        CancellationToken cancellationToken)
    {
        var recomendaciones = new List<RecomendacionCompraDto>();

        // Recomendaciones basadas en alertas críticas
        var alertasCriticas = alertas.Where(a => a.Prioridad == "Critica").ToList();
        
        foreach (var alerta in alertasCriticas.Where(a => a.IngredienteId.HasValue))
        {
            var ingrediente = datos.Ingredientes.FirstOrDefault(i => i.Id == alerta.IngredienteId);
            if (ingrediente != null)
            {
                recomendaciones.Add(new RecomendacionCompraDto
                {
                    IngredienteId = ingrediente.Id,
                    NombreIngrediente = ingrediente.Nombre,
                    CantidadRecomendada = ingrediente.StockMinimo * 2, // Reabastecer al doble del mínimo
                    UnidadMedida = ingrediente.UnidadMedida.ToString(),
                    CostoEstimado = (ingrediente.StockMinimo * 2) * ingrediente.CostoPromedio,
                    PrioridadCompra = "ALTA",
                    FechaRecomendadaPedido = _dateTimeService.Now.AddDays(1),
                    Justificacion = "Stock crítico - Reabastecer urgentemente",
                    ImpactoSinCompra = "Interrupción del servicio"
                });
            }
        }

        return recomendaciones.OrderByDescending(r => r.PrioridadCompra).ToList();
    }

    private async Task<TendenciasInventario> CalcularTendencias(ObtenerAnalisisInventarioQuery request, CancellationToken cancellationToken)
    {
        var tendencias = new TendenciasInventario();

        // Calcular tendencias de los últimos meses para comparar
        var fechaInicioComparacion = request.FechaInicio.AddMonths(-3);
        
        var movimientosPeriodoComparacion = await _context.MovimientosInventario
            .Where(m => m.Fecha >= fechaInicioComparacion && m.Fecha < request.FechaInicio)
            .ToListAsync(cancellationToken);

        var movimientosPeriodoActual = await _context.MovimientosInventario
            .Where(m => m.Fecha >= request.FechaInicio && m.Fecha <= request.FechaFin)
            .ToListAsync(cancellationToken);

        // Tendencia de consumo
        var consumoAnterior = movimientosPeriodoComparacion
            .Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
            .Sum(m => m.Cantidad);

        var consumoActual = movimientosPeriodoActual
            .Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
            .Sum(m => m.Cantidad);

        tendencias.TendenciaConsumo = consumoAnterior > 0 ? 
            ((consumoActual - consumoAnterior) / consumoAnterior) * 100 : 0;

        // TODO: Tendencia de compras cuando OrdenesCompra esté disponible
        tendencias.TendenciaCompras = 0;

        return tendencias;
    }

    private AnalisisInventarioDto ConstruirAnalisisCompleto(
        DatosInventarioAnalisis datos,
        MetricasInventario metricas,
        List<AlertaInventarioDto> alertas,
        List<RecomendacionCompraDto> recomendaciones,
        TendenciasInventario tendencias,
        ObtenerAnalisisInventarioQuery request)
    {
        return new AnalisisInventarioDto
        {
            InfoAnalisis = new InfoAnalisisDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                FechaGeneracion = _dateTimeService.Now,
                NivelDetalle = request.NivelDetalle,
                UsuarioSolicitante = _currentUserService.UserId?.ToString() ?? "Sistema"
            },
            ResumenExecutivo = new ResumenInventarioDto
            {
                TotalIngredientes = metricas.TotalIngredientes,
                IngredientesEnStock = metricas.TotalIngredientes - metricas.IngredientesSinStock,
                IngredientesBajoStock = metricas.IngredientesBajoStock,
                IngredientesStockCritico = metricas.IngredientesCriticos,
                IngredientesSinStock = metricas.IngredientesSinStock,
                ValorTotalInventario = metricas.ValorTotalInventario,
                ValorPromedioIngrediente = metricas.TotalIngredientes > 0 ? metricas.ValorTotalInventario / metricas.TotalIngredientes : 0,
                TotalMovimientos = metricas.TotalMovimientos,
                TasaRotacionInventario = metricas.RotacionInventario,
                EstadoGeneralInventario = GenerarEstadoGeneral(metricas),
                AlertasActivas = alertas.Count,
                TendenciaGeneral = GenerarTendenciaGeneral(tendencias)
            },
            Alertas = alertas,
            Recomendaciones = recomendaciones,
            MetricasEficiencia = new MetricasEficienciaDto
            {
                PorcentajeStockOptimo = metricas.PorcentajeStockOptimo,
                TasaRotacionGlobal = metricas.RotacionInventario,
                EficienciaGeneralInventario = CalcularEficienciaGeneral(metricas),
                ClasificacionEficiencia = GenerarClasificacionEficiencia(metricas.PorcentajeStockOptimo)
            }
        };
    }

    private string GenerarEstadoGeneral(MetricasInventario metricas)
    {
        if (metricas.IngredientesSinStock > 0)
            return "Crítico";
        if (metricas.PorcentajeStockOptimo < 70)
            return "Atención";
        return "Óptimo";
    }

    private string GenerarTendenciaGeneral(TendenciasInventario tendencias)
    {
        if (Math.Abs(tendencias.TendenciaConsumo) < 5)
            return "Estable";
        return tendencias.TendenciaConsumo > 0 ? "Ascendente" : "Descendente";
    }

    private decimal CalcularEficienciaGeneral(MetricasInventario metricas)
    {
        // Calcular eficiencia basada en múltiples factores
        decimal eficienciaStock = metricas.PorcentajeStockOptimo;
        decimal eficienciaRotacion = metricas.RotacionInventario > 0 ? Math.Min(100, metricas.RotacionInventario * 10) : 0;
        decimal eficienciaSinStock = metricas.TotalIngredientes > 0 ? 
            (100 - ((decimal)metricas.IngredientesSinStock / metricas.TotalIngredientes * 100)) : 100;

        // Promedio ponderado de las eficiencias
        return (eficienciaStock * 0.4m + eficienciaRotacion * 0.3m + eficienciaSinStock * 0.3m);
    }

    private string GenerarClasificacionEficiencia(decimal porcentajeStockOptimo)
    {
        return porcentajeStockOptimo switch
        {
            >= 90 => "Excelente",
            >= 80 => "Buena",
            >= 70 => "Regular",
            >= 60 => "Deficiente",
            _ => "Crítica"
        };
    }

    // DTOs internos para el análisis
    private class DatosInventarioAnalisis
    {
        public List<Ingrediente> Ingredientes { get; set; } = new();
        public List<MovimientoInventario> Movimientos { get; set; } = new();
        public List<dynamic> OrdenesCompra { get; set; } = new(); // TODO: Cambiar por OrdenCompra cuando esté disponible
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