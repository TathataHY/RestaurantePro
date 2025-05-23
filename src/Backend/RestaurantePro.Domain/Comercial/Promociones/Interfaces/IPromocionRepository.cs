namespace RestaurantePro.Domain.Comercial.Promociones.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de promociones
    /// </summary>
    public interface IPromocionRepository : IRepository<Promocion>
    {
        /// <summary>
        /// Obtiene las promociones activas en una fecha determinada
        /// </summary>
        /// <param name="fecha">Fecha para verificar promociones activas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de promociones activas</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesActivasAsync(DateTime fecha, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las promociones activas y vigentes en la fecha actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones activas y vigentes</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesActivasAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las promociones por estado
        /// </summary>
        /// <param name="estado">Estado de la promoción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de promociones con el estado especificado</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesPorEstadoAsync(EstadoPromocion estado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las promociones aplicables a un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="categoriaId">ID de la categoría del producto (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de promociones aplicables al producto</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesPorProductoAsync(Guid productoId, Guid? categoriaId = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene una promoción por su código
        /// </summary>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Promoción encontrada o null si no existe</returns>
        Task<Promocion?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las promociones que un cliente ha utilizado
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones utilizadas por el cliente</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesUsadasPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las promociones que están próximas a vencer
        /// </summary>
        /// <param name="diasLimite">Número de días límite para considerar una promoción próxima a vencer</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de promociones próximas a vencer</returns>
        Task<IEnumerable<Promocion>> ObtenerPromocionesProximasAVencerAsync(int diasLimite, CancellationToken cancellationToken = default);
    }
} 