using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;

/// <summary>
/// Handler para obtener ingredientes con stock bajo
/// Implementa lógica de priorización y filtros avanzados
/// </summary>
public class ObtenerIngredientesBajoStockHandler : IRequestHandler<ObtenerIngredientesBajoStockQuery, Result<List<IngredienteSummaryDto>>>
{
    private readonly IIngredienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerIngredientesBajoStockHandler> _logger;

    public ObtenerIngredientesBajoStockHandler(
        IIngredienteRepository repository,
        IMapper mapper,
        ILogger<ObtenerIngredientesBajoStockHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<IngredienteSummaryDto>>> Handle(
        ObtenerIngredientesBajoStockQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚨 Obteniendo ingredientes bajo stock: Crítico < {PorcentajeCritico}%", 
            request.PorcentajeCritico);

        try
        {
            // 1. Obtener ingredientes con stock bajo usando el método del repositorio
            var ingredientesBajoStock = await _repository.ObtenerConStockBajoAsync(cancellationToken);

            // 2. Aplicar filtros adicionales
            var ingredientesFiltrados = AplicarFiltros(ingredientesBajoStock, request);

            // 3. Filtrar por porcentaje crítico específico
            var ingredientesCriticos = FiltrarPorPorcentajeCritico(ingredientesFiltrados, request);

            // 4. Mapear a DTOs
            var ingredientesDto = _mapper.Map<List<IngredienteSummaryDto>>(ingredientesCriticos);

            // 5. Enriquecer DTOs con información calculada
            foreach (var dto in ingredientesDto)
            {
                EnriquecerIngredienteSummaryDto(dto);
            }

            // 6. Aplicar ordenamiento
            var ingredientesOrdenados = AplicarOrdenamiento(ingredientesDto, request);

            // 7. Aplicar límite de resultados
            var resultado = ingredientesOrdenados.Take(request.LimiteResultados).ToList();

            _logger.LogInformation("✅ Ingredientes bajo stock encontrados: {Count} de {Total} total", 
                resultado.Count, ingredientesBajoStock.Count());

            // Log de estadísticas por estado
            LogearEstadisticas(resultado);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingredientes bajo stock");
            return Result.Failure<List<IngredienteSummaryDto>>("Error interno al obtener ingredientes bajo stock");
        }
    }

    /// <summary>
    /// Aplica filtros adicionales según la query
    /// </summary>
    private IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> AplicarFiltros(
        IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes,
        ObtenerIngredientesBajoStockQuery request)
    {
        var query = ingredientes.AsQueryable();

        // Filtro por control de calidad
        if (!request.IncluirBloqueados)
        {
            query = query.Where(i => !i.BloqueadoControlCalidad);
        }

        // Filtro por rotación
        if (!string.IsNullOrWhiteSpace(request.FiltroRotacion))
        {
            if (Enum.TryParse<Domain.Inventario.Ingredientes.Enums.RotacionIngrediente>(request.FiltroRotacion, true, out var rotacion))
            {
                query = query.Where(i => i.Rotacion == rotacion);
            }
        }

        // Filtro por temporada
        if (!string.IsNullOrWhiteSpace(request.FiltroTemporada))
        {
            if (Enum.TryParse<Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente>(request.FiltroTemporada, true, out var temporada))
            {
                query = query.Where(i => i.Temporada == temporada);
            }
        }

        return query;
    }

    /// <summary>
    /// Filtra ingredientes por porcentaje crítico específico
    /// </summary>
    private IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> FiltrarPorPorcentajeCritico(
        IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes,
        ObtenerIngredientesBajoStockQuery request)
    {
        return ingredientes.Where(ingrediente =>
        {
            // Si no tiene stock mínimo definido, no se considera bajo stock
            if (ingrediente.StockMinimo <= 0) return false;

            // Calcular porcentaje del stock actual respecto al mínimo
            var porcentajeStock = (ingrediente.Stock / ingrediente.StockMinimo) * 100;

            // Está bajo stock si está por debajo del porcentaje crítico
            return porcentajeStock < request.PorcentajeCritico;
        });
    }

    /// <summary>
    /// Enriquece el DTO con información calculada para UI
    /// </summary>
    private void EnriquecerIngredienteSummaryDto(IngredienteSummaryDto dto)
    {
        // Calcular estado del stock
        var porcentajeStock = dto.StockMinimo > 0 ? (dto.StockActual / dto.StockMinimo) * 100 : 100;
        
        // Las propiedades están disponibles como calculadas en el DTO
        // dto.PorcentajeStock - Es una propiedad calculada
        // dto.EstadoStock - Es una propiedad calculada
        // dto.ColorEstado - Es una propiedad calculada

        // Crear resumen del estado
        var diasFaltantes = EstimarDiasFaltantes(dto);
        
        // El DTO ya tiene ResumenEstado como propiedad calculada
        // Podemos agregar información adicional si es necesario
        _logger.LogDebug("Ingrediente {Nombre}: {Estado} - {Stock}", 
            dto.Nombre, dto.EstadoStock, dto.StockFormateado);
    }

    /// <summary>
    /// Estima días hasta agotar stock según rotación
    /// </summary>
    private int? EstimarDiasFaltantes(IngredienteSummaryDto dto)
    {
        // Usar ConsumoPromedioMensual que ya existe en el DTO
        if (dto.ConsumoPromedioMensual <= 0)
            return null;

        var consumoDiario = dto.ConsumoPromedioMensual / 30; // Convertir mensual a diario
        return consumoDiario > 0 ? (int)(dto.StockActual / consumoDiario) : null;
    }

    /// <summary>
    /// Aplica ordenamiento según los parámetros de la query
    /// </summary>
    private IEnumerable<IngredienteSummaryDto> AplicarOrdenamiento(
        List<IngredienteSummaryDto> ingredientes,
        ObtenerIngredientesBajoStockQuery request)
    {
        if (!request.OrdenarPorPrioridad)
        {
            return ingredientes.OrderBy(i => i.Nombre);
        }

        // Ordenar por prioridad usando las propiedades que SÍ existen
        return ingredientes
            .OrderBy(i => i.PrioridadAtencion)  // Usar PrioridadAtencion que ya existe
            .ThenBy(i => i.PorcentajeStock)     // Usar PorcentajeStock que ya existe
            .ThenByDescending(i => i.ValorStock); // Usar ValorStock que ya existe
    }

    /// <summary>
    /// Loguea estadísticas del resultado
    /// </summary>
    private void LogearEstadisticas(List<IngredienteSummaryDto> ingredientes)
    {
        // Usar las propiedades que SÍ existen en IngredienteSummaryDto
        var estadisticas = ingredientes
            .GroupBy(i => i.EstadoStock)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var (estado, cantidad) in estadisticas)
        {
            _logger.LogInformation("📊 {Estado}: {Cantidad} ingredientes", estado, cantidad);
        }

        var valorTotal = ingredientes.Sum(i => i.ValorStock);
        _logger.LogInformation("💰 Valor total en riesgo: ${ValorTotal:F2}", valorTotal);
        
        var ingredientesCriticos = ingredientes.Count(i => i.RequiereAtencion);
        _logger.LogInformation("🚨 Ingredientes que requieren atención inmediata: {Cantidad}", ingredientesCriticos);
    }
} 