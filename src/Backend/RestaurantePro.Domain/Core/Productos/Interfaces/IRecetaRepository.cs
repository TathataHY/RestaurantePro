namespace RestaurantePro.Domain.Core.Productos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de recetas
    /// </summary>
    public interface IRecetaRepository : IRepository<Entities.Receta>
    {
        /// <summary>
        /// Obtiene una receta por el ID del producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Receta del producto o null si no existe</returns>
        Task<Entities.Receta?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancellationToken = default);
    }
} 