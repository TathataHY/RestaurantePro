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
        
        /// <summary>
        /// Obtiene comandas abiertas (estados Creada, EnProceso, Lista)
        /// </summary>
        /// <param name="incluirItems">Indica si se deben incluir los items de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas abiertas</returns>
        Task<IEnumerable<Comanda>> ObtenerComandasAbiertas(bool incluirItems = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe una comanda con el número especificado
        /// </summary>
        /// <param name="numeroComanda">Número de comanda a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task<bool> ExisteNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el último secuencial usado en el día especificado
        /// </summary>
        /// <param name="fecha">Fecha para buscar el último secuencial</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Último secuencial usado o null si no hay comandas en esa fecha</returns>
        Task<int?> ObtenerUltimoSecuencialDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene el último secuencial usado
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Último secuencial usado o 0 si no hay comandas</returns>
        Task<int> ObtenerUltimoSecuencialAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene el último secuencial usado para una sucursal en una fecha específica
        /// </summary>
        /// <param name="sucursalId">ID de la sucursal</param>
        /// <param name="fecha">Fecha para buscar el último secuencial</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Último secuencial usado o 0 si no hay comandas</returns>
        Task<int> ObtenerUltimoSecuencialAsync(Guid sucursalId, DateTime fecha, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene comandas activas con filtros y paginación
        /// </summary>
        /// <param name="criterios">Diccionario de criterios de filtro</param>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con comandas activas y total de elementos</returns>
        Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerComandasActivasAsync(Dictionary<string, object> criterios, int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las comandas activas
        /// </summary>
        /// <returns>Lista de comandas activas</returns>
        Task<IEnumerable<Comanda>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default);
    }
}
