using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionPorId;

/// <summary>
/// Handler para obtener una promoción por ID
/// </summary>
public class ObtenerPromocionPorIdHandler : IRequestHandler<ObtenerPromocionPorIdQuery, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerPromocionPorIdHandler> _logger;

    public ObtenerPromocionPorIdHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerPromocionPorIdHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PromocionDto>> Handle(ObtenerPromocionPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo promoción por ID: {Id}", request.Id);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {Id}", request.Id);
                return Result.Failure<PromocionDto>("La promoción no existe");
            }

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);

            // Calcular propiedades adicionales
            promocionDto.EstaVigente = CalcularVigencia(promocionDto);

            _logger.LogInformation("✅ Promoción obtenida exitosamente: {Id}", request.Id);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo promoción: {Id}", request.Id);
            return Result.Failure<PromocionDto>($"Error obteniendo promoción: {ex.Message}");
        }
    }

    private bool CalcularVigencia(PromocionDto promocion)
    {
        var ahora = DateTime.UtcNow;
        return promocion.Estado == EstadoPromocion.Activa && 
               promocion.FechaInicio <= ahora && 
               promocion.FechaFin >= ahora;
    }
} 