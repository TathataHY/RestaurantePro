namespace RestaurantePro.Domain.Operaciones.Comandas.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de comandas
    /// </summary>
    public interface IComandaRepository : IRepository<Comanda>
    {
        /// <summary>
        /// Obtiene una comanda por su ID
        /// </summary>
        /// <param name="id">ID de la comanda</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Comanda encontrada o null si no existe</returns>
        Task<Comanda?> ObtenerPorIdAsync(Guid id, bool incluirItems = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas por estado
        /// </summary>
        /// <param name="estado">Estado de las comandas a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas en el estado especificado</returns>
        Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas por mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas de la mesa especificada</returns>
        Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId, bool incluirItems = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas por mesero
        /// </summary>
        /// <param name="meseroId">ID del mesero</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas del mesero especificado</returns>
        Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId, bool incluirItems = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas por cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas del cliente especificado</returns>
        Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId, bool incluirItems = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas creadas en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas creadas en el rango especificado</returns>
        Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, bool incluirItems = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un item de comanda por su ID
        /// </summary>
        /// <param name="itemId">ID del item</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Item de comanda encontrado o null si no existe</returns>
        Task<ItemComanda?> ObtenerItemPorIdAsync(Guid itemId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene items de comanda por producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de items del producto especificado</returns>
        Task<IEnumerable<ItemComanda>> ObtenerItemsPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con comandas y total de elementos</returns>
        Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirItems = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las comandas que incluyen un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas que incluyen el ingrediente</returns>
        Task<IEnumerable<Comanda>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene estadísticas de comandas por período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Diccionario con estadísticas por día</returns>
        Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    }
}
