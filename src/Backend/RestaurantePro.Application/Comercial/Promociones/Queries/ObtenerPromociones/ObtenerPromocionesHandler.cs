using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromociones;

/// <summary>
/// Handler para obtener promociones con filtros
/// </summary>
public class ObtenerPromocionesHandler : IRequestHandler<ObtenerPromocionesQuery, Result<List<PromocionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerPromocionesHandler> _logger;

    public ObtenerPromocionesHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerPromocionesHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<PromocionDto>>> Handle(ObtenerPromocionesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo promociones con filtros: Estado={Estado}, Tipo={Tipo}, SoloVigentes={SoloVigentes}", 
                request.Estado, request.Tipo, request.SoloVigentes);

            var query = _context.Promociones.AsQueryable();

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Aplicar paginación
            var promociones = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var promocionesDto = _mapper.Map<List<PromocionDto>>(promociones);

            // Calcular propiedades adicionales
            foreach (var promocionDto in promocionesDto)
            {
                promocionDto.EstaVigente = CalcularVigencia(promocionDto);
            }

            _logger.LogInformation("✅ Se obtuvieron {Count} promociones", promocionesDto.Count);

            return Result.Success(promocionesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo promociones");
            return Result.Failure<List<PromocionDto>>($"Error obteniendo promociones: {ex.Message}");
        }
    }

    private IQueryable<Domain.Comercial.Promociones.Entities.Promocion> AplicarFiltros(
        IQueryable<Domain.Comercial.Promociones.Entities.Promocion> query, 
        ObtenerPromocionesQuery request)
    {
        if (request.Estado.HasValue)
        {
            query = query.Where(p => p.Estado == request.Estado.Value);
        }

        if (request.Tipo.HasValue)
        {
            query = query.Where(p => p.Tipo == request.Tipo.Value);
        }

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(p => p.Codigo.Contains(request.Codigo));
        }

        if (!string.IsNullOrEmpty(request.Nombre))
        {
            query = query.Where(p => p.Nombre.Contains(request.Nombre));
        }

        if (request.FechaInicioDesde.HasValue)
        {
            query = query.Where(p => p.FechaInicio >= request.FechaInicioDesde.Value);
        }

        if (request.FechaFinHasta.HasValue)
        {
            query = query.Where(p => p.FechaFin <= request.FechaFinHasta.Value);
        }

        if (request.SoloVigentes.HasValue && request.SoloVigentes.Value)
        {
            var ahora = DateTime.UtcNow;
            query = query.Where(p => p.Estado == EstadoPromocion.Activa && 
                                   p.FechaInicio <= ahora && 
                                   p.FechaFin >= ahora);
        }

        if (request.EsAcumulable.HasValue)
        {
            query = query.Where(p => p.EsAcumulable == request.EsAcumulable.Value);
        }

        return query;
    }

    private IQueryable<Domain.Comercial.Promociones.Entities.Promocion> AplicarOrdenamiento(
        IQueryable<Domain.Comercial.Promociones.Entities.Promocion> query, 
        ObtenerPromocionesQuery request)
    {
        var ordenarPor = request.OrdenarPor?.ToLower() ?? "fechacreacion";
        var direccion = request.DireccionOrdenamiento?.ToLower() ?? "desc";

        query = ordenarPor switch
        {
            "nombre" => direccion == "asc" ? query.OrderBy(p => p.Nombre) : query.OrderByDescending(p => p.Nombre),
            "codigo" => direccion == "asc" ? query.OrderBy(p => p.Codigo) : query.OrderByDescending(p => p.Codigo),
            "fechainicio" => direccion == "asc" ? query.OrderBy(p => p.FechaInicio) : query.OrderByDescending(p => p.FechaInicio),
            "fechafin" => direccion == "asc" ? query.OrderBy(p => p.FechaFin) : query.OrderByDescending(p => p.FechaFin),
            "prioridad" => direccion == "asc" ? query.OrderBy(p => p.Prioridad) : query.OrderByDescending(p => p.Prioridad),
            "vecesusada" => direccion == "asc" ? query.OrderBy(p => p.VecesUsada) : query.OrderByDescending(p => p.VecesUsada),
            "estado" => direccion == "asc" ? query.OrderBy(p => p.Estado) : query.OrderByDescending(p => p.Estado),
            "tipo" => direccion == "asc" ? query.OrderBy(p => p.Tipo) : query.OrderByDescending(p => p.Tipo),
            _ => direccion == "asc" ? query.OrderBy(p => p.FechaCreacion) : query.OrderByDescending(p => p.FechaCreacion)
        };

        return query;
    }

    private bool CalcularVigencia(PromocionDto promocion)
    {
        var ahora = DateTime.UtcNow;
        return promocion.Estado == EstadoPromocion.Activa && 
               promocion.FechaInicio <= ahora && 
               promocion.FechaFin >= ahora;
    }
} 