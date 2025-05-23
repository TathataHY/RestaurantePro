

namespace RestaurantePro.Domain.Comercial.Pagos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de pagos
    /// </summary>
    public interface IPagoRepository : IRepository<Pago>
    {
        /// <summary>
        /// Obtiene todos los pagos asociados a una comanda
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de pagos asociados a la comanda</returns>
        Task<IEnumerable<Pago>> ObtenerPagosPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los pagos filtrados por estado
        /// </summary>
        /// <param name="estado">Estado de pago a filtrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de pagos con el estado especificado</returns>
        Task<IEnumerable<Pago>> ObtenerPagosPorEstadoAsync(EstadoPago estado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los pagos realizados en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de pagos en el rango de fechas</returns>
        Task<IEnumerable<Pago>> ObtenerPagosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los pagos por método de pago
        /// </summary>
        /// <param name="metodoPago">Método de pago a filtrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de pagos con el método de pago especificado</returns>
        Task<IEnumerable<Pago>> ObtenerPagosPorMetodoAsync(MetodoPago metodoPago, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el total de pagos por comanda
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Monto total pagado para la comanda</returns>
        Task<decimal> ObtenerTotalPagadoPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca un pago por su referencia de transacción
        /// </summary>
        /// <param name="referenciaTransaccion">Referencia de transacción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Pago encontrado o null si no existe</returns>
        Task<Pago?> ObtenerPagoPorReferenciaTransaccionAsync(string referenciaTransaccion, CancellationToken cancellationToken = default);
    }
} 