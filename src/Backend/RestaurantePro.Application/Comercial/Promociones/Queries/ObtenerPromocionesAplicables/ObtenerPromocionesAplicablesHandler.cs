using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionesAplicables;

public class ObtenerPromocionesAplicablesHandler : IRequestHandler<ObtenerPromocionesAplicablesQuery, Result<List<PromocionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerPromocionesAplicablesHandler> _logger;

    public ObtenerPromocionesAplicablesHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerPromocionesAplicablesHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<PromocionDto>>> Handle(ObtenerPromocionesAplicablesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💡 Obteniendo promociones aplicables para ClienteId={ClienteId}, Monto={Monto}", request.ClienteId, request.Monto);

            var ahora = request.Fecha ?? DateTime.UtcNow;
            var query = _context.Promociones.AsQueryable();

            // Solo promociones activas y vigentes
            query = query.Where(p => p.Estado == EstadoPromocion.Activa && p.FechaInicio <= ahora && p.FechaFin >= ahora);

            if (request.ClienteId.HasValue)
            {
                // Filtrar por cliente si aplica (puedes agregar lógica de exclusión/inclusión por cliente)
            }
            if (request.Monto.HasValue)
            {
                query = query.Where(p => p.MontoMinimo <= request.Monto.Value);
            }
            if (request.ProductosIds != null && request.ProductosIds.Any())
            {
                query = query.Where(p => p.ProductosAplicablesIds.Any(id => request.ProductosIds.Contains(id)));
            }
            if (request.SoloAcumulables.HasValue && request.SoloAcumulables.Value)
            {
                query = query.Where(p => p.EsAcumulable);
            }
            if (request.Tipo.HasValue)
            {
                query = query.Where(p => p.Tipo == request.Tipo.Value);
            }

            var promociones = await query.ToListAsync(cancellationToken);
            var dtos = _mapper.Map<List<PromocionDto>>(promociones);
            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo promociones aplicables");
            return Result.Failure<List<PromocionDto>>($"Error obteniendo promociones aplicables: {ex.Message}");
        }
    }
} 