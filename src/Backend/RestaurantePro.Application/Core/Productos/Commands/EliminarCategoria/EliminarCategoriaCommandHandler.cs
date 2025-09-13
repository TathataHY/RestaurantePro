using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Core.Productos.Commands.EliminarCategoria;

/// <summary>
/// Handler para eliminar una categoría de productos
/// </summary>
public class EliminarCategoriaCommandHandler : IRequestHandler<EliminarCategoriaCommand, Result>
{
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly ILogger<EliminarCategoriaCommandHandler> _logger;

    public EliminarCategoriaCommandHandler(
        IProductoCategoriaRepository categoriaRepository,
        IProductoRepository productoRepository,
        ILogger<EliminarCategoriaCommandHandler> logger)
    {
        _categoriaRepository = categoriaRepository;
        _productoRepository = productoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(EliminarCategoriaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando eliminación de categoría con ID: {Id}", request.Id);

            // Buscar la categoría existente (incluyendo inactivas para poder eliminarlas)
            var categoria = await _categoriaRepository.ObtenerPorIdIncluyendoInactivasAsync(request.Id, cancellationToken);

            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada con ID: {Id}", request.Id);
                return Result.Failure("Categoría no encontrada");
            }

            // Verificar si la categoría tiene productos asociados
            _logger.LogInformation("🔍 Verificando productos asociados para categoría {Id}", request.Id);
            var productosAsociados = await _productoRepository.ObtenerPorCategoriaAsync(request.Id, false, cancellationToken);
            
            _logger.LogInformation("📊 Productos encontrados: {Count} productos asociados a categoría {Id}", 
                productosAsociados.Count, request.Id);
            
            if (productosAsociados.Any())
            {
                _logger.LogWarning("❌ No se puede eliminar la categoría {Id} porque tiene {Count} productos asociados", 
                    request.Id, productosAsociados.Count);
                return Result.Failure($"No se puede eliminar la categoría porque tiene {productosAsociados.Count} productos asociados");
            }
            
            _logger.LogInformation("✅ Categoría {Id} no tiene productos asociados, procediendo con eliminación", request.Id);

            // Desactivar la categoría (soft delete)
            categoria.Desactivar();
            await _categoriaRepository.ActualizarAsync(categoria, cancellationToken);

            _logger.LogInformation("Categoría eliminada exitosamente con ID: {Id}", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la categoría con ID: {Id}", request.Id);
            return Result.Failure("Error interno al eliminar la categoría");
        }
    }
}
