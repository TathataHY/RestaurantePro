namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Interfaz para el servicio de dominio que gestiona operaciones relacionadas con categorías de productos
    /// </summary>
    public interface IProductoCategoriaService
    {
        /// <summary>
        /// Obtiene todos los productos de una categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="soloActivos">Indica si solo se deben obtener productos activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<List<Entities.Producto>> ObtenerProductosPorCategoriaAsync(
            Guid categoriaId, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza la categoría de un conjunto de productos
        /// </summary>
        /// <param name="productosIds">IDs de los productos a actualizar</param>
        /// <param name="categoriaId">ID de la nueva categoría</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de productos actualizados</returns>
        Task<int> ActualizarCategoriaProductosAsync(
            List<Guid> productosIds, 
            Guid categoriaId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reorganiza el orden de las categorías
        /// </summary>
        /// <param name="nuevosOrdenes">Diccionario con ID de categoría como clave y nuevo orden como valor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de categorías actualizadas</returns>
        Task<int> ReorganizarCategoriasAsync(
            Dictionary<Guid, int> nuevosOrdenes, 
            CancellationToken cancellationToken = default);
    }
} 