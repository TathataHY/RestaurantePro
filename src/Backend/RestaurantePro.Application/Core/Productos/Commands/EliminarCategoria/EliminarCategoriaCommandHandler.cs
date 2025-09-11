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
    private readonly ILogger<EliminarCategoriaCommandHandler> _logger;

    public EliminarCategoriaCommandHandler(
        IProductoCategoriaRepository categoriaRepository,
        ILogger<EliminarCategoriaCommandHandler> logger)
    {
        _categoriaRepository = categoriaRepository;
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

            // Nota: En una implementación completa, se verificaría si la categoría tiene productos asociados
            // Por ahora, asumimos que no hay productos asociados o que la verificación se hace en el dominio
            // En una implementación real, se podría inyectar IProductoRepository para verificar esto

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
