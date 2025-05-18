namespace RestaurantePro.Domain.Core.Productos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio que gestiona la relación entre productos e ingredientes
    /// </summary>
    public interface IProductoIngredienteRepository : IRepository<ProductoIngrediente>
    {
        /// <summary>
        /// Obtiene la relación entre un producto y un ingrediente específicos
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La relación entre el producto y el ingrediente, o null si no existe</returns>
        Task<ProductoIngrediente> ObtenerPorProductoEIngredienteAsync(Guid productoId, Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las relaciones para un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones producto-ingrediente</returns>
        Task<IEnumerable<ProductoIngrediente>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las relaciones para un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones producto-ingrediente</returns>
        Task<IEnumerable<ProductoIngrediente>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una nueva relación producto-ingrediente
        /// </summary>
        /// <param name="productoIngrediente">Relación a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AgregarAsync(ProductoIngrediente productoIngrediente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza una relación producto-ingrediente existente
        /// </summary>
        /// <param name="productoIngrediente">Relación a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task ActualizarAsync(ProductoIngrediente productoIngrediente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una relación producto-ingrediente
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task EliminarAsync(Guid productoId, Guid ingredienteId, CancellationToken cancellationToken = default);
    }
} 