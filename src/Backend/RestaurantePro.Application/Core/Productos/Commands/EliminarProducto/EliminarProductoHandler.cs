namespace RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;

public class EliminarProductoHandler : IRequestHandler<EliminarProductoCommand, Result<bool>>
{
    private readonly IProductoRepository _repository;
    private readonly ILogger<EliminarProductoHandler> _logger;

    public EliminarProductoHandler(
        IProductoRepository repository,
        ILogger<EliminarProductoHandler> logger)
    {
        _repository = repository;
        _logger = logger;
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

            // Realizar eliminación física para tests (en lugar de soft delete)
            await _repository.EliminarFisicamenteAsync(request.Id, cancellationToken);
            await _repository.GuardarCambiosAsync(cancellationToken); // ✅ AGREGAR: Guardar cambios

            _logger.LogInformation("✅ Producto eliminado exitosamente: {Id}", request.Id);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al eliminar producto: {Id}", request.Id);
            return Result.Failure<bool>($"Error interno al eliminar el producto: {ex.Message}");
        }
    }
} 