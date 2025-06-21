namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de movimientos de inventario
    /// </summary>
    public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
    {
        /// <summary>
        /// Obtiene todos los movimientos de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de movimientos del ingrediente</returns>
        Task<IEnumerable<MovimientoInventario>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calcula el stock actual de un ingrediente basado en sus movimientos
        /// </summary>
        Task<decimal> CalcularStockActualAsync(Guid ingredienteId);

        /// <summary>
        /// Agrega un nuevo movimiento
        /// </summary>
        Task AgregarAsync(MovimientoInventario movimiento);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync();
    }
} 

