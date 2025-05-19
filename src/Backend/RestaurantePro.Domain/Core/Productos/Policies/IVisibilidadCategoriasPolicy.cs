namespace RestaurantePro.Domain.Core.Productos.Policies
{
    /// <summary>
    /// Interfaz para la política que determina qué categorías deben ser visibles según reglas de negocio específicas
    /// </summary>
    public interface IVisibilidadCategoriasPolicy
    {
        /// <summary>
        /// Determina las categorías visibles según criterios de negocio
        /// </summary>
        /// <param name="ocultarCategoriasVacias">Indica si se deben ocultar categorías sin productos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de categorías visibles, ordenadas por su propiedad Orden</returns>
        Task<List<Entities.ProductoCategoria>> ObtenerCategoriasVisiblesAsync(
            bool ocultarCategoriasVacias = true,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Determina si una categoría debe ser visible
        /// </summary>
        /// <param name="categoriaId">ID de la categoría a verificar</param>
        /// <param name="ocultarCategoriasVacias">Indica si se deben ocultar categorías sin productos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la categoría debe ser visible, False en caso contrario</returns>
        Task<bool> EsCategoriaVisibleAsync(
            Guid categoriaId,
            bool ocultarCategoriasVacias = true,
            CancellationToken cancellationToken = default);
    }
} 