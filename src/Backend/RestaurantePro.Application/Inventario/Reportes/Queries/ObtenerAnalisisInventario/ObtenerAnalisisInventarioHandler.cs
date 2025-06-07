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
        try
        {
            _logger.LogInformation("Iniciando análisis de inventario para el período {FechaInicio} - {FechaFin}", 
                request.FechaInicio, request.FechaFin);

            // Validar parámetros de entrada
            var validacionResult = ValidarParametros(request);
            if (!validacionResult.Succeeded)
            {
                _logger.LogWarning("Validación falló: {Error}", validacionResult.Error);
                return Result.Failure<AnalisisInventarioDto>(validacionResult.Error ?? "Error de validación");
            }

            // Obtener datos del inventario
            var datos = await ObtenerDatosInventario(request, cancellationToken);
            _logger.LogDebug("Obtenidos {IngredientesCount} ingredientes y {MovimientosCount} movimientos", 
                datos.Ingredientes.Count, datos.Movimientos.Count);

            // Calcular métricas
            var metricas = await CalcularMetricas(datos, request, cancellationToken);
            _logger.LogDebug("Métricas calculadas: {TotalIngredientes} ingredientes, {ValorTotal} valor total", 
                metricas.TotalIngredientes, metricas.ValorTotalInventario);

            // Identificar alertas
            var alertas = IdentificarAlertas(datos, request, cancellationToken);
            _logger.LogDebug("Identificadas {AlertasCount} alertas", alertas.Count);

            // Generar recomendaciones
            var recomendaciones = GenerarRecomendaciones(datos, metricas, alertas, cancellationToken);
            _logger.LogDebug("Generadas {RecomendacionesCount} recomendaciones", recomendaciones.Count);

            // Calcular tendencias si se solicitan
            var tendencias = request.IncluirTendencias ? 
                await CalcularTendencias(request, cancellationToken) : new TendenciasInventario();

            // Construir análisis completo
            var analisis = ConstruirAnalisisCompleto(datos, metricas, alertas, recomendaciones, tendencias, request);

            _logger.LogInformation("Análisis de inventario completado exitosamente");
            return Result.Success(analisis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al generar el análisis de inventario: {Message}. StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            return Result.Failure<AnalisisInventarioDto>($"Error interno al generar el análisis de inventario: {ex.Message}");
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

        // Rotación de inventario con validaciones para división por cero
        if (metricas.ValorTotalInventario > 0 && datos.Ingredientes.Any())
        {
            var diasPeriodo = (datos.FechaFin - datos.FechaInicio).TotalDays;
            
            // Asegurar que tenemos al menos 1 día para evitar división por cero
            if (diasPeriodo <= 0)
                diasPeriodo = 1;

            var consumoDiarioPromedio = metricas.CantidadTotalSalida / (decimal)diasPeriodo;
            
            // Calcular stock promedio solo si hay ingredientes
            var stockPromedio = datos.Ingredientes.Any() ? datos.Ingredientes.Average(i => i.Stock) : 0;
            
            if (stockPromedio > 0)
            {
                metricas.RotacionInventario = consumoDiarioPromedio / stockPromedio;
            }
            else
            {
                metricas.RotacionInventario = 0;
            }
        }
        else
        {
            metricas.RotacionInventario = 0;
        }

        // TODO: Análisis de compras cuando OrdenesCompra esté disponible
        metricas.TotalOrdenesCompra = 0;
        metricas.ValorTotalCompras = 0;
        metricas.PromedioOrdenCompra = 0;

        // Eficiencia del inventario (simplificado) con validación
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

        // Recomendaciones basadas en alertas de alta prioridad pero no críticas
        var alertasAlta = alertas.Where(a => a.Prioridad == "Alta").ToList();
        
        foreach (var alerta in alertasAlta.Where(a => a.IngredienteId.HasValue))
        {
            var ingrediente = datos.Ingredientes.FirstOrDefault(i => i.Id == alerta.IngredienteId);
            if (ingrediente != null)
            {
                recomendaciones.Add(new RecomendacionCompraDto
                {
                    IngredienteId = ingrediente.Id,
                    NombreIngrediente = ingrediente.Nombre,
                    CantidadRecomendada = ingrediente.StockMinimo * 1.5m, // Reabastecer al 150% del mínimo
                    UnidadMedida = ingrediente.UnidadMedida.ToString(),
                    CostoEstimado = (ingrediente.StockMinimo * 1.5m) * ingrediente.CostoPromedio,
                    PrioridadCompra = "MEDIA",
                    FechaRecomendadaPedido = _dateTimeService.Now.AddDays(3),
                    Justificacion = "Stock bajo - Programar reabastecimiento pronto",
                    ImpactoSinCompra = "Posible interrupción del servicio"
                });
            }
        }

        // Si no hay recomendaciones críticas ni de alta prioridad, agregar recomendación general
        if (recomendaciones.Count == 0 && datos.Ingredientes.Any())
        {
            // Tomar el ingrediente con menor relación stock/stockMinimo que no esté en cero
            var ingredienteBajoStock = datos.Ingredientes
                .Where(i => i.Stock > 0 && i.StockMinimo > 0)
                .OrderBy(i => i.Stock / i.StockMinimo)
                .FirstOrDefault();

            // Si no hay ninguno con las condiciones anteriores, tomar el primero
            ingredienteBajoStock ??= datos.Ingredientes.FirstOrDefault();

            if (ingredienteBajoStock != null)
            {
                recomendaciones.Add(new RecomendacionCompraDto
                {
                    IngredienteId = ingredienteBajoStock.Id,
                    NombreIngrediente = ingredienteBajoStock.Nombre,
                    CantidadRecomendada = ingredienteBajoStock.StockMinimo,
                    UnidadMedida = ingredienteBajoStock.UnidadMedida.ToString(),
                    CostoEstimado = ingredienteBajoStock.StockMinimo * ingredienteBajoStock.CostoPromedio,
                    PrioridadCompra = "BAJA",
                    FechaRecomendadaPedido = _dateTimeService.Now.AddDays(7),
                    Justificacion = "Optimización de inventario",
                    ImpactoSinCompra = "Ninguno inmediato"
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
        var analisis = new AnalisisInventarioDto
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
            },
            // Agregar análisis detallado por ingrediente
            AnalisisIngredientes = GenerarAnalisisIngredientes(datos, metricas),
            // Agregar análisis por categorías
            AnalisisCategorias = GenerarAnalisisCategorias(datos, metricas)
        };

        // Agregar predicciones si se solicitan
        if (request.IncluirTendencias)
        {
            analisis.Predicciones = GenerarPrediccionesInventario(datos, tendencias, metricas);
        }

        // Agregar análisis financiero si se solicita
        if (request.NivelDetalle == "Financiero" || request.NivelDetalle == "Completo")
        {
            analisis.AnalisisFinanciero = GenerarAnalisisFinanciero(datos, metricas);
        }

        return analisis;
    }

    private List<RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisIngredienteDto> GenerarAnalisisIngredientes(DatosInventarioAnalisis datos, MetricasInventario metricas)
    {
        return datos.Ingredientes.Select(ingrediente =>
        {
            var movimientosIngrediente = datos.Movimientos.Where(m => m.IngredienteId == ingrediente.Id).ToList();
            var totalConsumo = movimientosIngrediente
                .Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
                .Sum(m => m.Cantidad);
            var totalIngreso = movimientosIngrediente
                .Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso)
                .Sum(m => m.Cantidad);

            return new RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisIngredienteDto
            {
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                CodigoIngrediente = ingrediente.Codigo,
                StockActual = ingrediente.Stock,
                StockMinimo = ingrediente.StockMinimo,
                UnidadMedida = ingrediente.UnidadMedida.ToString(),
                CostoUnitarioPromedio = ingrediente.CostoPromedio,
                ValorInventario = ingrediente.Stock * ingrediente.CostoPromedio,
                PorcentajeStockOptimo = ingrediente.StockMinimo > 0 ? (ingrediente.Stock / ingrediente.StockMinimo) * 100 : 0,
                EstadoStock = DeterminarEstadoStock(ingrediente),
                TotalMovimientos = movimientosIngrediente.Count,
                ConsumoTotal = totalConsumo,
                IngresoTotal = totalIngreso,
                RotacionIngrediente = ingrediente.Stock > 0 ? totalConsumo / ingrediente.Stock : 0,
                DiasStockRestante = totalConsumo > 0 ? (int)(ingrediente.Stock / (totalConsumo / (decimal)(datos.FechaFin - datos.FechaInicio).TotalDays)) : 999,
                RequiereAtencion = ingrediente.Stock <= ingrediente.StockMinimo,
                SugerenciaAccion = GenerarSugerenciaAccion(ingrediente, totalConsumo)
            };
        }).ToList();
    }

    private List<RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisCategoriaDto> GenerarAnalisisCategorias(DatosInventarioAnalisis datos, MetricasInventario metricas)
    {
        // Agrupar por unidad de medida como proxy para categorías hasta que tengamos categorías reales
        var categorias = datos.Ingredientes
            .GroupBy(i => i.UnidadMedida)
            .Select(g => new RestaurantePro.Application.Inventario.Reportes.DTOs.AnalisisCategoriaDto
            {
                CategoriaId = Guid.NewGuid(), // Temporal
                NombreCategoria = g.Key.ToString(),
                TotalIngredientes = g.Count(),
                IngredientesEnStock = g.Count(i => i.Stock > 0),
                IngredientesBajoStock = g.Count(i => i.Stock <= i.StockMinimo),
                ValorTotalCategoria = g.Sum(i => i.Stock * i.CostoPromedio),
                PorcentajeValorTotal = metricas.ValorTotalInventario > 0 ? 
                    (g.Sum(i => i.Stock * i.CostoPromedio) / metricas.ValorTotalInventario) * 100 : 0,
                RotacionPromedio = g.Any(i => i.Stock > 0) ? 
                    g.Where(i => i.Stock > 0).Average(i => datos.Movimientos
                        .Where(m => m.IngredienteId == i.Id && m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
                        .Sum(m => m.Cantidad) / i.Stock) : 0,
                AlertasActivas = g.Count(i => i.Stock <= i.StockMinimo),
                TendenciaCategoria = "Estable" // Simplificado por ahora
            }).ToList();

        return categorias.OrderByDescending(c => c.ValorTotalCategoria).ToList();
    }

    private PrediccionesInventarioDto GenerarPrediccionesInventario(DatosInventarioAnalisis datos, TendenciasInventario tendencias, MetricasInventario metricas)
    {
        return new PrediccionesInventarioDto
        {
            FechaPrediccion = _dateTimeService.Now,
            TipoPrediccion = "BasadaEnHistorico",
            NivelConfianza = 75, // Nivel medio por ahora
            
            // Predicciones de consumo
            ConsumoProximoMes = datos.Movimientos
                .Where(m => m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
                .Sum(m => m.Cantidad) * 1.2m, // Proyección simple +20%
            
            IngredientesEnRiesgo = datos.Ingredientes
                .Where(i => i.Stock <= i.StockMinimo * 1.5m)
                .Select(i => new PrediccionIngredienteDto
                {
                    IngredienteId = i.Id,
                    NombreIngrediente = i.Nombre,
                    FechaAgotamientoEstimada = _dateTimeService.Now.AddDays(CalcularDiasHastaAgotamiento(i, datos)),
                    CantidadRecomendadaCompra = i.StockMinimo * 2,
                    NivelRiesgo = i.Stock <= 0 ? "Alto" : i.Stock <= i.StockMinimo ? "Medio" : "Bajo"
                }).ToList(),
            
            TendenciaGeneralConsumo = tendencias.TendenciaConsumo > 0 ? "Ascendente" : 
                                   tendencias.TendenciaConsumo < 0 ? "Descendente" : "Estable",
            
            RecomendacionesAutomaticas = new List<string>
            {
                "Revisar stock de ingredientes críticos",
                "Considerar aumento de pedidos para ingredientes de alta rotación",
                "Optimizar inventario según patrones de consumo"
            }
        };
    }

    private AnalisisFinancieroDto GenerarAnalisisFinanciero(DatosInventarioAnalisis datos, MetricasInventario metricas)
    {
        var costosDetallados = datos.Ingredientes.Select(i => new CostoDetalladoDto
        {
            IngredienteId = i.Id,
            NombreIngrediente = i.Nombre,
            CostoUnitario = i.CostoPromedio,
            CantidadStock = i.Stock,
            ValorTotal = i.Stock * i.CostoPromedio,
            PorcentajeDelTotal = metricas.ValorTotalInventario > 0 ? 
                (i.Stock * i.CostoPromedio / metricas.ValorTotalInventario) * 100 : 0
        }).OrderByDescending(c => c.ValorTotal).ToList();

        return new AnalisisFinancieroDto
        {
            FechaAnalisis = _dateTimeService.Now,
            ValorTotalInventario = metricas.ValorTotalInventario,
            CostosDetallados = costosDetallados,
            Top10IngredientesMasCaros = costosDetallados.Take(10).ToList(),
            DistribucionCostos = new DistribucionCostosDto
            {
                IngredientesAltoCosto = costosDetallados.Where(c => c.PorcentajeDelTotal >= 10).Sum(c => c.ValorTotal),
                IngredientesCostoMedio = costosDetallados.Where(c => c.PorcentajeDelTotal >= 5 && c.PorcentajeDelTotal < 10).Sum(c => c.ValorTotal),
                IngredientesBajoCosto = costosDetallados.Where(c => c.PorcentajeDelTotal < 5).Sum(c => c.ValorTotal)
            },
            AnalisisROI = new AnalisisROIDto
            {
                RotacionCapital = metricas.RotacionInventario,
                DiasInventarioPromedio = metricas.RotacionInventario > 0 ? 365m / metricas.RotacionInventario : 0,
                EficienciaCapital = CalcularEficienciaCapital(metricas)
            },
            RecomendacionesFinancieras = GenerarRecomendacionesFinancieras(costosDetallados, metricas)
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

    private string DeterminarEstadoStock(Ingrediente ingrediente)
    {
        if (ingrediente.Stock <= 0)
            return "SinStock";
        if (ingrediente.Stock <= ingrediente.StockMinimo * 0.5m)
            return "Crítico";
        if (ingrediente.Stock <= ingrediente.StockMinimo)
            return "Bajo";
        if (ingrediente.Stock <= ingrediente.StockMinimo * 2)
            return "Normal";
        return "Alto";
    }

    private string GenerarSugerenciaAccion(Ingrediente ingrediente, decimal totalConsumo)
    {
        if (ingrediente.Stock <= 0)
            return "Comprar urgentemente";
        if (ingrediente.Stock <= ingrediente.StockMinimo)
            return "Reabastecer pronto";
        if (totalConsumo == 0)
            return "Revisar necesidad";
        return "Monitoreando";
    }

    private int CalcularDiasHastaAgotamiento(Ingrediente ingrediente, DatosInventarioAnalisis datos)
    {
        var totalDias = (decimal)(datos.FechaFin - datos.FechaInicio).TotalDays;
        
        // Asegurar que tenemos al menos 1 día para evitar división por cero
        if (totalDias <= 0)
            totalDias = 1;
            
        var consumoPromedioDiario = datos.Movimientos
            .Where(m => m.IngredienteId == ingrediente.Id && 
                       m.TipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso)
            .Sum(m => m.Cantidad) / totalDias;

        return consumoPromedioDiario > 0 ? (int)(ingrediente.Stock / consumoPromedioDiario) : 999;
    }

    private decimal CalcularEficienciaCapital(MetricasInventario metricas)
    {
        // Fórmula simplificada de eficiencia de capital
        if (metricas.ValorTotalInventario > 0 && metricas.RotacionInventario > 0)
        {
            return Math.Min(100, metricas.RotacionInventario * 10);
        }
        return 0;
    }

    private List<string> GenerarRecomendacionesFinancieras(List<CostoDetalladoDto> costosDetallados, MetricasInventario metricas)
    {
        var recomendaciones = new List<string>();

        if (costosDetallados.Any(c => c.PorcentajeDelTotal > 20))
        {
            recomendaciones.Add("Considerar negociar mejores precios para ingredientes de alto valor");
        }

        if (metricas.RotacionInventario < 1)
        {
            recomendaciones.Add("Mejorar la rotación de inventario para optimizar el capital de trabajo");
        }

        if (metricas.PorcentajeStockOptimo < 80)
        {
            recomendaciones.Add("Revisar niveles de stock para evitar desperdicios");
        }

        return recomendaciones;
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