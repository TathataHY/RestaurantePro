namespace RestaurantePro.Domain.Inventario.Ingredientes.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de ingredientes
    /// </summary>
    public interface IIngredienteRepository : IRepository<Ingrediente>
    {
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
    }
}
