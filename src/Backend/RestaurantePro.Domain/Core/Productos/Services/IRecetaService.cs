namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Servicio para gestionar recetas de productos y sus ingredientes.
    /// </summary>
    public interface IRecetaService
    {
        /// <summary>
        /// Obtiene los ingredientes necesarios para un producto y sus cantidades requeridas.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado con diccionario de IDs de ingredientes como clave y las cantidades como valor.</returns>
        Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si hay suficiente stock de ingredientes para elaborar una cantidad específica de un producto.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cantidad">Cantidad de producto a elaborar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado con True si hay suficiente stock, False en caso contrario.</returns>
        Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los ingredientes que no tienen suficiente stock para elaborar una cantidad específica de un producto.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cantidad">Cantidad de producto a elaborar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado con lista de IDs de ingredientes sin stock suficiente y la cantidad faltante.</returns>
        Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcula el costo total de los ingredientes necesarios para elaborar un producto según su receta.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado con el costo total de los ingredientes en la receta.</returns>
        Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcula la rentabilidad de un producto basado en su precio de venta y el costo de sus ingredientes.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resultado con objeto ValueObject con información detallada sobre la rentabilidad.</returns>
        Task<Result<ValueObjects.RentabilidadProducto>> CalcularRentabilidadProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
    }
} 