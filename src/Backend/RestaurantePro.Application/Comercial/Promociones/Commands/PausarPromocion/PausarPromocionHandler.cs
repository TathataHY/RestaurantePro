using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;

/// <summary>
/// Handler para pausar una promoción
/// </summary>
public class PausarPromocionHandler : IRequestHandler<PausarPromocionCommand, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PausarPromocionHandler> _logger;

    public PausarPromocionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<PausarPromocionHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PromocionDto>> Handle(PausarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("⏸️ Pausando promoción: {Id}", request.Id);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {Id}", request.Id);
                return Result.Failure<PromocionDto>("La promoción no existe");
            }

            // Validar que la promoción esté activa
            if (promocion.Estado != EstadoPromocion.Activa)
            {
                return Result.Failure<PromocionDto>("Solo se pueden pausar promociones activas");
            }

            // Pausar la promoción
            promocion.Pausar();

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);
            promocionDto.EstaVigente = false;

            _logger.LogInformation("✅ Promoción pausada exitosamente: {Id}", request.Id);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error pausando promoción: {Id}", request.Id);
            return Result.Failure<PromocionDto>($"Error pausando promoción: {ex.Message}");
        }
    }
} 