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
        /// <returns>Diccionario con los IDs de ingredientes como clave y las cantidades como valor.</returns>
        Task<Dictionary<Guid, decimal>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si hay suficiente stock de ingredientes para elaborar una cantidad específica de un producto.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cantidad">Cantidad de producto a elaborar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>True si hay suficiente stock, False en caso contrario.</returns>
        Task<bool> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los ingredientes que no tienen suficiente stock para elaborar una cantidad específica de un producto.
        /// </summary>
        /// <param name="productoId">ID del producto.</param>
        /// <param name="cantidad">Cantidad de producto a elaborar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de IDs de ingredientes sin stock suficiente y la cantidad faltante.</returns>
        Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default);
    }
} 