using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;

public class EliminarProductoHandler : IRequestHandler<EliminarProductoCommand, Result<bool>>
{
    private readonly IProductoRepository _repository;
    private readonly ILogger<EliminarProductoHandler> _logger;
    private readonly ICacheService _cache;

    public EliminarProductoHandler(
        IProductoRepository repository,
        ILogger<EliminarProductoHandler> logger,
        ICacheService cache)
    {
        _repository = repository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(EliminarProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🗑️ Iniciando eliminación (desactivación) de producto: {Id}", request.Id);

        try
        {
            // Verificar que el producto existe
            var producto = await _repository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (producto == null)
            {
                _logger.LogWarning("⚠️ Producto no encontrado: {Id}", request.Id);
                return Result.Failure<bool>($"Producto con ID {request.Id} no encontrado");
            }

            // Verificar si ya está desactivado
            if (!producto.EstaActivo)
            {
                _logger.LogInformation("ℹ️ El producto ya está desactivado: {Id}", request.Id);
                return Result.Success(true);
            }

            // Desactivar el producto (soft delete)
            producto.Desactivar();
            await _repository.ActualizarAsync(producto, cancellationToken);
            await _repository.GuardarCambiosAsync(cancellationToken);

            try
            {
                _cache.InvalidatePattern("productos:lista:*");
                _cache.Remove($"productos:detalle:{request.Id}");
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "No se pudo invalidar caché de productos tras desactivar {Id}", request.Id);
            }

            _logger.LogInformation("✅ Producto desactivado exitosamente: {Id}", request.Id);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al desactivar producto: {Id}", request.Id);
            return Result.Failure<bool>($"Error interno al eliminar el producto: {ex.Message}");
        }
    }
} 