using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;

/// <summary>
/// Handler para activar una promoción
/// </summary>
public class ActivarPromocionHandler : IRequestHandler<ActivarPromocionCommand, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivarPromocionHandler> _logger;

    public ActivarPromocionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ActivarPromocionHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PromocionDto>> Handle(ActivarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✅ Activando promoción: {Id}", request.Id);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {Id}", request.Id);
                return Result.Failure<PromocionDto>("La promoción no existe");
            }

            // Validar que la promoción no esté ya activa
            if (promocion.Estado == EstadoPromocion.Activa)
            {
                return Result.Failure<PromocionDto>("La promoción ya está activa");
            }

            // Validar que la promoción no esté cancelada
            if (promocion.Estado == EstadoPromocion.Cancelada)
            {
                return Result.Failure<PromocionDto>("No se puede activar una promoción cancelada");
            }

            // Validar que la fecha de inicio no sea futura
            if (promocion.FechaInicio > DateTime.Today)
            {
                return Result.Failure<PromocionDto>("No se puede activar una promoción antes de su fecha de inicio");
            }

            // Validar que la fecha de fin no haya pasado
            if (promocion.FechaFin < DateTime.Today)
            {
                return Result.Failure<PromocionDto>("No se puede activar una promoción que ya expiró");
            }

            // Activar la promoción
            promocion.Activar();

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);
            promocionDto.EstaVigente = true;

            _logger.LogInformation("✅ Promoción activada exitosamente: {Id}", request.Id);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error activando promoción: {Id}", request.Id);
            return Result.Failure<PromocionDto>($"Error activando promoción: {ex.Message}");
        }
    }
} 