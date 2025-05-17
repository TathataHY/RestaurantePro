namespace RestaurantePro.Domain.Inventario.Ingredientes.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de ingredientes
    /// </summary>
    public interface IIngredienteRepository : IRepository<Ingrediente>
    {
        /// <summary>
        /// Obtiene un ingrediente por su ID
        /// </summary>
        /// <param name="id">ID del ingrediente a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El ingrediente si existe, null en caso contrario</returns>
        Task<Ingrediente> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un ingrediente por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente a buscar</param>
        /// <returns>El ingrediente si existe, null en caso contrario</returns>
        Task<Ingrediente> ObtenerPorNombreAsync(string nombre);
        
        /// <summary>
        /// Obtiene todos los ingredientes activos
        /// </summary>
        /// <returns>Lista de ingredientes activos</returns>
        Task<List<Ingrediente>> ObtenerActivosAsync();
        
        /// <summary>
        /// Obtiene los ingredientes que tienen stock por debajo de su mínimo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        Task<List<Ingrediente>> ObtenerConStockBajoAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Busca ingredientes por unidad de medida
        /// </summary>
        /// <param name="unidadMedida">Unidad de medida a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes que coinciden con la unidad de medida</returns>
        Task<IEnumerable<Ingrediente>> BuscarPorUnidadMedidaAsync(RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida unidadMedida, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene ingredientes utilizados en un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes utilizados en el producto</returns>
        Task<IEnumerable<Ingrediente>> ObtenerIngredientesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un ingrediente existente
        /// </summary>
        /// <param name="ingrediente">Ingrediente a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task ActualizarAsync(Ingrediente ingrediente, CancellationToken cancellationToken = default);
    }
}
