using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.AsignarProductos;

/// <summary>
/// Handler para asignar productos a una promoción
/// </summary>
public class AsignarProductosHandler : IRequestHandler<AsignarProductosCommand, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AsignarProductosHandler> _logger;

    public AsignarProductosHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AsignarProductosHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PromocionDto>> Handle(AsignarProductosCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📦 Asignando {Count} productos a promoción: {PromocionId}", 
                request.ProductosIds.Count, request.PromocionId);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.PromocionId, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {PromocionId}", request.PromocionId);
                return Result.Failure<PromocionDto>("La promoción no existe");
            }

            // Validar que los productos existan
            var productosExistentes = await _context.Productos
                .Where(p => request.ProductosIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var productosNoExistentes = request.ProductosIds.Except(productosExistentes).ToList();
            if (productosNoExistentes.Any())
            {
                return Result.Failure<PromocionDto>($"Los siguientes productos no existen: {string.Join(", ", productosNoExistentes)}");
            }

            // Asignar productos a la promoción
            foreach (var productoId in request.ProductosIds)
            {
                promocion.AgregarProductoAplicable(productoId);
            }

            // Forzar detección de cambios para colecciones privadas serializadas
            if (_context is DbContext dbContext)
            {
                dbContext.Entry(promocion).State = EntityState.Modified;
                dbContext.Update(promocion);
            }

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);

            _logger.LogInformation("✅ Productos asignados exitosamente a promoción: {PromocionId}", request.PromocionId);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error asignando productos a promoción: {PromocionId}", request.PromocionId);
            return Result.Failure<PromocionDto>($"Error asignando productos: {ex.Message}");
        }
    }
}