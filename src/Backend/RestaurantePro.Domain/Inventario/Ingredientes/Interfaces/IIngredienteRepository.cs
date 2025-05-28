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
        /// <param name="id">ID del ingrediente</param>
        /// <param name="incluirMovimientos">Indica si se deben incluir los movimientos del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Ingrediente encontrado o null si no existe</returns>
        Task<Ingrediente?> ObtenerPorIdAsync(Guid id, bool incluirMovimientos = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes por nombre (búsqueda parcial)
        /// </summary>
        /// <param name="nombre">Nombre completo o parcial</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes que coinciden con el criterio</returns>
        Task<IEnumerable<Ingrediente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes con stock por debajo del mínimo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        Task<IEnumerable<Ingrediente>> ObtenerConStockBajoAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes por nivel de rotación
        /// </summary>
        /// <param name="rotacion">Nivel de rotación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes con el nivel de rotación especificado</returns>
        Task<IEnumerable<Ingrediente>> ObtenerPorRotacionAsync(RotacionIngrediente rotacion, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes por temporada
        /// </summary>
        /// <param name="temporada">Temporada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes de la temporada especificada</returns>
        Task<IEnumerable<Ingrediente>> ObtenerPorTemporadaAsync(TemporadaIngrediente temporada, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes activos o inactivos
        /// </summary>
        /// <param name="activos">Indica si se deben obtener ingredientes activos o inactivos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes según el criterio</returns>
        Task<IEnumerable<Ingrediente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes que provee un determinado proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes provistos por el proveedor</returns>
        Task<IEnumerable<Ingrediente>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con ingredientes y total de elementos</returns>
        new Task<(IEnumerable<Ingrediente> Ingredientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes bloqueados por control de calidad
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes bloqueados</returns>
        Task<IEnumerable<Ingrediente>> ObtenerBloqueadosPorCalidadAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes asociados a un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes asociados al producto</returns>
        Task<IEnumerable<Ingrediente>> ObtenerIngredientesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene ingredientes con stock por debajo del mínimo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        Task<IEnumerable<Ingrediente>> ObtenerIngredientesConStockBajoAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Busca ingredientes por categoría o tipo
        /// </summary>
        /// <param name="categoria">Categoría o tipo de ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes de la categoría especificada</returns>
        Task<IEnumerable<Ingrediente>> BuscarPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    }
}

