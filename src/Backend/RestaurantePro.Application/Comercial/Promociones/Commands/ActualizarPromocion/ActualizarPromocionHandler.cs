using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;

public class ActualizarPromocionHandler : IRequestHandler<ActualizarPromocionCommand, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarPromocionHandler> _logger;

    public ActualizarPromocionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ActualizarPromocionHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PromocionDto>> Handle(ActualizarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✏️ Actualizando promoción: {Id}", request.Id);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {Id}", request.Id);
                return Result.Failure<PromocionDto>("La promoción no existe");
            }

            promocion.ActualizarInformacion(
                request.Nombre,
                request.Descripcion,
                request.ValorDescuento,
                request.FechaInicio,
                request.FechaFin,
                request.MontoMinimo,
                request.PuntosRequeridos,
                request.MaximoUsos,
                request.EsAcumulable
            );

            if (request.DiasValidos?.Any() == true)
                promocion.EstablecerDiasValidos(request.DiasValidos.ToArray());

            // Actualizar productos aplicables
            if (request.ProductosAplicablesIds != null)
            {
                // Obtener productos actuales y remover los que ya no están en la lista
                var productosActuales = promocion.ProductosAplicablesIds.ToList();
                var productosARemover = productosActuales.Except(request.ProductosAplicablesIds);
                var productosAAgregar = request.ProductosAplicablesIds.Except(productosActuales);

                foreach (var productoId in productosARemover)
                    promocion.EliminarProductoAplicable(productoId);

                foreach (var productoId in productosAAgregar)
                    promocion.AgregarProductoAplicable(productoId);
            }

            // Actualizar categorías aplicables
            if (request.CategoriasAplicablesIds != null)
            {
                // Obtener categorías actuales y remover las que ya no están en la lista
                var categoriasActuales = promocion.CategoriasAplicablesIds.ToList();
                var categoriasARemover = categoriasActuales.Except(request.CategoriasAplicablesIds);
                var categoriasAAgregar = request.CategoriasAplicablesIds.Except(categoriasActuales);

                foreach (var categoriaId in categoriasARemover)
                    promocion.EliminarCategoriaAplicable(categoriaId);

                foreach (var categoriaId in categoriasAAgregar)
                    promocion.AgregarCategoriaAplicable(categoriaId);
            }

            await _context.SaveChangesAsync(cancellationToken);
            var dto = _mapper.Map<PromocionDto>(promocion);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando promoción: {Id}", request.Id);
            return Result.Failure<PromocionDto>($"Error actualizando promoción: {ex.Message}");
        }
    }
} 