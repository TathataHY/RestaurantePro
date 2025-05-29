using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;

/// <summary>
/// Handler para obtener ingredientes paginados con filtros avanzados
/// Implementa búsqueda, filtrado, ordenamiento y paginación
/// </summary>
public class ObtenerIngredientesPaginadosHandler : IRequestHandler<ObtenerIngredientesPaginadosQuery, Result<PaginatedList<IngredienteSummaryDto>>>
{
    private readonly IIngredienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerIngredientesPaginadosHandler> _logger;

    public ObtenerIngredientesPaginadosHandler(
        IIngredienteRepository repository,
        IMapper mapper,
        ILogger<ObtenerIngredientesPaginadosHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<IngredienteSummaryDto>>> Handle(
        ObtenerIngredientesPaginadosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo ingredientes paginados: Página {Page}/{Size} - Filtros: {FiltrosCount}", 
            request.PageNumber, request.PageSize, ContarFiltrosActivos(request));

        try
        {
            // 1. Obtener todos los ingredientes del repositorio
            var ingredientes = await _repository.ObtenerTodosAsync(cancellationToken);

            // 2. Aplicar filtros
            var ingredientesFiltrados = AplicarFiltros(ingredientes, request);

            // 3. Aplicar ordenamiento
            var ingredientesOrdenados = AplicarOrdenamiento(ingredientesFiltrados, request);

            // 4. Calcular paginación
            var totalCount = ingredientesOrdenados.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // 5. Aplicar paginación
            var ingredientesPaginados = ingredientesOrdenados
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 6. Mapear a DTOs
            var ingredientesDto = _mapper.Map<List<IngredienteSummaryDto>>(ingredientesPaginados);

            // 7. Enriquecer DTOs con información calculada
            foreach (var dto in ingredientesDto)
            {
                EnriquecerIngredienteSummaryDto(dto);
            }

            // 8. Crear resultado paginado
            var resultado = new PaginatedList<IngredienteSummaryDto>(
                ingredientesDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Ingredientes obtenidos: {Count} de {Total} - Página {Page}/{TotalPages}", 
                ingredientesDto.Count, totalCount, request.PageNumber, totalPages);

            // 9. Log de estadísticas
            LogearEstadisticas(ingredientesDto, request);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingredientes paginados");
            return Result.Failure<PaginatedList<IngredienteSummaryDto>>("Error interno al obtener ingredientes");
        }
    }

    /// <summary>
    /// Aplica todos los filtros especificados en la query
    /// </summary>
    private IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> AplicarFiltros(
        IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes,
        ObtenerIngredientesPaginadosQuery request)
    {
        var query = ingredientes.AsQueryable();

        // Filtro por estado activo
        if (request.SoloActivos)
        {
            query = query.Where(i => i.EstaActivo);
        }

        // Filtro por control de calidad
        if (request.ExcluirBloqueados)
        {
            query = query.Where(i => !i.BloqueadoControlCalidad);
        }

        // Filtro de texto libre (nombre, código, descripción)
        if (!string.IsNullOrWhiteSpace(request.FiltroTexto))
        {
            var filtroLower = request.FiltroTexto.ToLower();
            query = query.Where(i => 
                i.Nombre.ToLower().Contains(filtroLower) ||
                i.Codigo.ToLower().Contains(filtroLower) ||
                i.Descripcion.ToLower().Contains(filtroLower));
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

        // Filtro por unidad de medida
        if (!string.IsNullOrWhiteSpace(request.FiltroUnidadMedida))
        {
            if (Enum.TryParse<Domain.Inventario.Ingredientes.Enums.UnidadMedida>(request.FiltroUnidadMedida, true, out var unidad))
            {
                query = query.Where(i => i.UnidadMedida == unidad);
            }
        }

        // Filtro por proveedor principal
        if (request.ProveedorPrincipalId.HasValue)
        {
            query = query.Where(i => i.ProveedorPrincipalId == request.ProveedorPrincipalId.Value);
        }

        // Filtros de stock
        if (request.SoloConStock)
        {
            query = query.Where(i => i.Stock > 0);
        }

        if (request.SoloBajoStock)
        {
            query = query.Where(i => i.StockMinimo > 0 && i.Stock < i.StockMinimo);
        }

        // Filtros de rango de stock
        if (request.StockMinimo.HasValue)
        {
            query = query.Where(i => i.Stock >= request.StockMinimo.Value);
        }

        if (request.StockMaximo.HasValue)
        {
            query = query.Where(i => i.Stock <= request.StockMaximo.Value);
        }

        // Filtros de rango de costo
        if (request.CostoMinimo.HasValue)
        {
            query = query.Where(i => i.CostoPromedio >= request.CostoMinimo.Value);
        }

        if (request.CostoMaximo.HasValue)
        {
            query = query.Where(i => i.CostoPromedio <= request.CostoMaximo.Value);
        }

        return query;
    }

    /// <summary>
    /// Aplica ordenamiento según los parámetros especificados
    /// </summary>
    private IOrderedEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> AplicarOrdenamiento(
        IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes,
        ObtenerIngredientesPaginadosQuery request)
    {
        var esDescendente = request.DireccionOrden.Equals("Desc", StringComparison.OrdinalIgnoreCase);

        return request.OrdenarPor.ToLower() switch
        {
            "nombre" => esDescendente 
                ? ingredientes.OrderByDescending(i => i.Nombre)
                : ingredientes.OrderBy(i => i.Nombre),

            "codigo" => esDescendente
                ? ingredientes.OrderByDescending(i => i.Codigo)
                : ingredientes.OrderBy(i => i.Codigo),

            "stockactual" => esDescendente
                ? ingredientes.OrderByDescending(i => i.Stock)
                : ingredientes.OrderBy(i => i.Stock),

            "stockminimo" => esDescendente
                ? ingredientes.OrderByDescending(i => i.StockMinimo)
                : ingredientes.OrderBy(i => i.StockMinimo),

            "porcentajestock" => esDescendente
                ? ingredientes.OrderByDescending(i => i.StockMinimo > 0 ? (i.Stock / i.StockMinimo) * 100 : 100)
                : ingredientes.OrderBy(i => i.StockMinimo > 0 ? (i.Stock / i.StockMinimo) * 100 : 100),

            "valortotalstock" => esDescendente
                ? ingredientes.OrderByDescending(i => i.Stock * i.CostoPromedio)
                : ingredientes.OrderBy(i => i.Stock * i.CostoPromedio),

            "costopromedio" => esDescendente
                ? ingredientes.OrderByDescending(i => i.CostoPromedio)
                : ingredientes.OrderBy(i => i.CostoPromedio),

            "rotacion" => esDescendente
                ? ingredientes.OrderByDescending(i => i.Rotacion)
                : ingredientes.OrderBy(i => i.Rotacion),

            "temporada" => esDescendente
                ? ingredientes.OrderByDescending(i => i.Temporada)
                : ingredientes.OrderBy(i => i.Temporada),

            "unidadmedida" => esDescendente
                ? ingredientes.OrderByDescending(i => i.UnidadMedida)
                : ingredientes.OrderBy(i => i.UnidadMedida),

            "fechacreacion" => esDescendente
                ? ingredientes.OrderByDescending(i => i.FechaCreacion)
                : ingredientes.OrderBy(i => i.FechaCreacion),

            _ => ingredientes.OrderBy(i => i.Nombre) // Default: por nombre ascendente
        };
    }

    /// <summary>
    /// Enriquece el DTO con información calculada para UI
    /// </summary>
    private void EnriquecerIngredienteSummaryDto(IngredienteSummaryDto dto)
    {
        // Calcular estado del stock
        var porcentajeStock = dto.StockMinimo > 0 ? (dto.StockActual / dto.StockMinimo) * 100 : 100;
        dto.PorcentajeStock = porcentajeStock;
        dto.EstaBajoMinimo = dto.StockActual < dto.StockMinimo;

        // Determinar estado y color
        (dto.EstadoStock, dto.ColorEstado) = porcentajeStock switch
        {
            <= 10 => ("Crítico", "#D32F2F"),
            <= 25 => ("Muy Bajo", "#F44336"),
            <= 50 => ("Bajo", "#FF9800"),
            <= 100 => ("Normal", "#4CAF50"),
            _ => ("Alto", "#2196F3")
        };

        // Calcular valor total del stock
        dto.ValorTotalStock = dto.StockActual * dto.CostoPromedio;

        // Formatear stock con unidad
        dto.StockTexto = $"{dto.StockActual:F2} {dto.UnidadMedida}";

        // Determinar prioridad de reposición
        (dto.PrioridadReposicion, dto.ColorPrioridad) = (porcentajeStock, dto.Rotacion.ToLower()) switch
        {
            (< 10, _) => ("Urgente", "#D32F2F"),
            (< 25, "alta") => ("Alta", "#F44336"),
            (< 25, _) => ("Media", "#FF9800"),
            (< 50, "alta") => ("Media", "#FF9800"),
            _ => ("Baja", "#FFC107")
        };

        // Crear resumen del estado
        dto.ResumenEstado = dto.EstaBajoMinimo 
            ? $"🚨 {dto.EstadoStock}: {dto.StockActual:F2}/{dto.StockMinimo:F2} {dto.UnidadMedida}"
            : $"✅ {dto.EstadoStock}: {dto.StockActual:F2} {dto.UnidadMedida} - ${dto.ValorTotalStock:F2}";
    }

    /// <summary>
    /// Cuenta la cantidad de filtros activos
    /// </summary>
    private int ContarFiltrosActivos(ObtenerIngredientesPaginadosQuery request)
    {
        var filtros = 0;
        if (!string.IsNullOrWhiteSpace(request.FiltroTexto)) filtros++;
        if (!string.IsNullOrWhiteSpace(request.FiltroRotacion)) filtros++;
        if (!string.IsNullOrWhiteSpace(request.FiltroTemporada)) filtros++;
        if (!string.IsNullOrWhiteSpace(request.FiltroUnidadMedida)) filtros++;
        if (request.ProveedorPrincipalId.HasValue) filtros++;
        if (request.SoloConStock) filtros++;
        if (request.SoloBajoStock) filtros++;
        if (!request.SoloActivos) filtros++;
        if (!request.ExcluirBloqueados) filtros++;
        if (request.StockMinimo.HasValue) filtros++;
        if (request.StockMaximo.HasValue) filtros++;
        if (request.CostoMinimo.HasValue) filtros++;
        if (request.CostoMaximo.HasValue) filtros++;
        return filtros;
    }

    /// <summary>
    /// Loguea estadísticas del resultado
    /// </summary>
    private void LogearEstadisticas(List<IngredienteSummaryDto> ingredientes, ObtenerIngredientesPaginadosQuery request)
    {
        if (ingredientes.Count == 0) return;

        var estadisticas = ingredientes
            .GroupBy(i => i.EstadoStock)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var (estado, cantidad) in estadisticas)
        {
            _logger.LogInformation("📊 {Estado}: {Cantidad} ingredientes", estado, cantidad);
        }

        var valorTotal = ingredientes.Sum(i => i.ValorTotalStock);
        var stockPromedio = ingredientes.Average(i => i.PorcentajeStock);

        _logger.LogInformation("💰 Valor total mostrado: ${ValorTotal:F2} - Stock promedio: {StockPromedio:F1}%", 
            valorTotal, stockPromedio);

        // Log de filtros aplicados
        if (!string.IsNullOrWhiteSpace(request.FiltroTexto))
            _logger.LogInformation("🔍 Filtro de texto: '{FiltroTexto}'", request.FiltroTexto);

        if (request.SoloBajoStock)
            _logger.LogInformation("⚠️ Mostrando solo ingredientes bajo stock mínimo");
    }
} 