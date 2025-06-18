using RestaurantePro.Domain.Core.SharedKernel.Validation;

namespace RestaurantePro.Domain.Core.Productos.Services;

/// <summary>
/// Define los calculos relacionados con las recetas de los productos.
/// </summary>
public interface ICalculoRecetaService
{
    /// <summary>
    /// Obtiene los ingredientes y cantidades requeridas para un producto.
    /// </summary>
    Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si hay suficientes ingredientes en el inventario para preparar una cantidad de un producto.
    /// </summary>
    Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula el costo total de los ingredientes para una receta.
    /// </summary>
    Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken = default);
} 