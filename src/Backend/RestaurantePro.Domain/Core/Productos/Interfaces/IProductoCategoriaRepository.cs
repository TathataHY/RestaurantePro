namespace RestaurantePro.Domain.Core.Productos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de categorías de productos
    /// </summary>
    public interface IProductoCategoriaRepository
    {
        /// <summary>
        /// Obtiene una categoría por su ID
        /// </summary>
        Task<ProductoCategoria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las categorías activas
        /// </summary>
        Task<List<ProductoCategoria>> ObtenerActivasAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las categorías (activas e inactivas)
        /// </summary>
        Task<List<ProductoCategoria>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una nueva categoría
        /// </summary>
        Task AgregarAsync(ProductoCategoria categoria, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        Task ActualizarAsync(ProductoCategoria categoria, CancellationToken cancellationToken = default);
    }
} 