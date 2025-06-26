using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.QuitarProductos;

/// <summary>
/// Handler para quitar productos de una promoción
/// </summary>
public class QuitarProductosHandler : IRequestHandler<QuitarProductosCommand, Result<PromocionDto>>
{
    private readonly IPromocionRepository _promocionRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuitarProductosHandler> _logger;
    private readonly IApplicationDbContext _context;

    public QuitarProductosHandler(
        IPromocionRepository promocionRepository,
        IProductoRepository productoRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IMapper mapper,
        ILogger<QuitarProductosHandler> logger,
        IApplicationDbContext context)
    {
        _promocionRepository = promocionRepository;
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _mapper = mapper;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<PromocionDto>> Handle(QuitarProductosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🗑️ Quitando {Count} productos de promoción: {PromocionId}", 
                request.ProductosIds.Count, request.PromocionId);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.PromocionId, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {PromocionId}", request.PromocionId);
                return Result.Failure<PromocionDto>("Promoción no encontrada");
            }

            // Validar que los productos existan
            var productosExistentes = await _context.Productos
                .Where(p => request.ProductosIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var productosNoExistentes = request.ProductosIds.Except(productosExistentes).ToList();
            if (productosNoExistentes.Any())
            {
                return Result.Failure<PromocionDto>("Algunos productos no existen");
            }

            // Quitar productos de la promoción
            foreach (var productoId in request.ProductosIds)
            {
                promocion.EliminarProductoAplicable(productoId);
            }

            // Forzar detección de cambios para colecciones privadas serializadas
            if (_context is DbContext dbContext)
            {
                dbContext.Entry(promocion).State = EntityState.Modified;
            }

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);

            _logger.LogInformation("✅ Productos quitados exitosamente de promoción: {PromocionId}", request.PromocionId);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error quitando productos de promoción: {PromocionId}", request.PromocionId);
            return Result.Failure<PromocionDto>($"Error quitando productos de la promoción: {ex.Message}");
        }
    }
} 