using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Application.Core.Recetas.Queries.ObtenerRecetas;

/// <summary>
/// Handler para obtener recetas paginadas con filtros avanzados
/// </summary>
public class ObtenerRecetasQueryHandler : IRequestHandler<ObtenerRecetasQuery, Result<PaginatedList<RecetaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerRecetasQueryHandler> _logger;

    public ObtenerRecetasQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerRecetasQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<RecetaDto>>> Handle(
        ObtenerRecetasQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo recetas paginadas: Página {Page}/{Size} - Filtros: SoloActivas={SoloActivas}, ProductoId={ProductoId}, FiltroTexto={FiltroTexto}",
                request.PageNumber, request.PageSize, request.SoloActivas, request.ProductoId, request.FiltroTexto);

            // 1. Construir query base
            var query = _context.Recetas
                .Include(r => r.Producto)
                .AsQueryable();

            // 2. Filtrar solo recetas no eliminadas
            query = query.Where(r => !r.RecetaEliminada);

            // 3. Aplicar filtros
            query = AplicarFiltros(query, request);

            // 4. Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // 5. Contar total de elementos (antes de paginar)
            var totalCount = await query.CountAsync(cancellationToken);

            // 6. Aplicar paginación
            var recetas = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // 7. Mapear a DTOs y poblar información adicional
            var recetasDto = await EnriquecerRecetasDto(recetas, request, cancellationToken);

            // 8. Crear resultado paginado
            var resultado = new PaginatedList<RecetaDto>(
                recetasDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Se obtuvieron {Count} recetas de {Total} - Página {Page}/{TotalPages}", 
                recetasDto.Count, totalCount, request.PageNumber, resultado.TotalPages);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener recetas paginadas");
            return Result.Failure<PaginatedList<RecetaDto>>("Error al obtener las recetas");
        }
    }

    /// <summary>
    /// Aplica filtros a la query
    /// </summary>
    private IQueryable<Domain.Core.Productos.Entities.Receta> AplicarFiltros(
        IQueryable<Domain.Core.Productos.Entities.Receta> query,
        ObtenerRecetasQuery request)
    {
        // Filtro por estado activo (no hay propiedad Activa en la entidad, usamos !RecetaEliminada)
        if (request.SoloActivas.HasValue && request.SoloActivas.Value)
        {
            query = query.Where(r => !r.RecetaEliminada);
        }

        // Filtro por producto específico
        if (request.ProductoId.HasValue)
        {
            query = query.Where(r => r.ProductoId == request.ProductoId.Value);
        }

        // Filtro de texto libre
        if (!string.IsNullOrWhiteSpace(request.FiltroTexto))
        {
            var filtroLower = request.FiltroTexto.ToLower();
            query = query.Where(r => 
                r.Preparacion.ToLower().Contains(filtroLower) ||
                r.Producto.Nombre.ToLower().Contains(filtroLower));
        }

        return query;
    }

    /// <summary>
    /// Aplica ordenamiento a la query
    /// </summary>
    private IQueryable<Domain.Core.Productos.Entities.Receta> AplicarOrdenamiento(
        IQueryable<Domain.Core.Productos.Entities.Receta> query,
        ObtenerRecetasQuery request)
    {
        var esDescendente = request.DireccionOrden.Equals("Desc", StringComparison.OrdinalIgnoreCase);

        return request.OrdenarPor.ToLower() switch
        {
            "fechacreacion" => esDescendente 
                ? query.OrderByDescending(r => r.FechaCreacion)
                : query.OrderBy(r => r.FechaCreacion),
            
            "preparacion" => esDescendente 
                ? query.OrderByDescending(r => r.Preparacion)
                : query.OrderBy(r => r.Preparacion),
            
            "tiempopreparacion" => esDescendente 
                ? query.OrderByDescending(r => r.TiempoPreparacionMinutos)
                : query.OrderBy(r => r.TiempoPreparacionMinutos),
            
            "productoid" => esDescendente 
                ? query.OrderByDescending(r => r.ProductoId)
                : query.OrderBy(r => r.ProductoId),
            
            _ => esDescendente 
                ? query.OrderByDescending(r => r.FechaCreacion)
                : query.OrderBy(r => r.FechaCreacion)
        };
    }

    /// <summary>
    /// Enriquece los DTOs con información adicional
    /// </summary>
    private async Task<List<RecetaDto>> EnriquecerRecetasDto(
        List<Domain.Core.Productos.Entities.Receta> recetas,
        ObtenerRecetasQuery request,
        CancellationToken cancellationToken)
    {
        var recetasDto = _mapper.Map<List<RecetaDto>>(recetas);

        // Poblar NombreProducto si se requiere
        if (request.IncluirProducto)
        {
            var productoIds = recetas.Select(r => r.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Nombre, cancellationToken);

            foreach (var recetaDto in recetasDto)
            {
                recetaDto.NombreProducto = productos.TryGetValue(recetaDto.ProductoId, out var nombre) 
                    ? nombre 
                    : string.Empty;
            }
        }

        // Poblar información de ingredientes si se requiere
        if (request.IncluirIngredientes)
        {
            foreach (var recetaDto in recetasDto)
            {
                var receta = recetas.First(r => r.Id == recetaDto.Id);
                recetaDto.Ingredientes = receta.Ingredientes.Select(ir => new IngredienteRecetaDto
                {
                    IngredienteId = ir.IngredienteId,
                    Nombre = ir.Nombre,
                    Cantidad = ir.Cantidad,
                    UnidadMedida = ir.UnidadMedida.ToString(),
                    EsOpcional = ir.EsOpcional,
                    CostoUnitario = 0, // TODO: Calcular costo unitario desde el ingrediente
                    CostoTotal = 0 // TODO: Calcular costo total
                }).ToList();
            }
        }

        return recetasDto;
    }
} 

