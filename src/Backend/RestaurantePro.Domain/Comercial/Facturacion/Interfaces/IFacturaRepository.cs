namespace RestaurantePro.Domain.Comercial.Facturacion.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de facturas
    /// </summary>
    public interface IFacturaRepository : IRepository<Factura>
    {
        /// <summary>
        /// Obtiene una factura por su número
        /// </summary>
        /// <param name="numeroFactura">Número de la factura a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Factura encontrada o null si no existe</returns>
        Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las facturas asociadas a una comanda
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas asociadas a la comanda</returns>
        Task<IEnumerable<Factura>> ObtenerPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las facturas de un cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas del cliente</returns>
        Task<IEnumerable<Factura>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las facturas por estado
        /// </summary>
        /// <param name="estado">Estado de factura a filtrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas con el estado especificado</returns>
        Task<IEnumerable<Factura>> ObtenerPorEstadoAsync(EstadoFactura estado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las facturas en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas en el rango de fechas</returns>
        Task<IEnumerable<Factura>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las facturas pendientes de pago
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de facturas pendientes de pago</returns>
        Task<IEnumerable<Factura>> ObtenerPendientesPagoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el siguiente número de factura disponible
        /// </summary>
        /// <param name="prefijo">Prefijo para el número de factura (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Siguiente número de factura disponible</returns>
        Task<string> ObtenerSiguienteNumeroFacturaAsync(string? prefijo = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe una factura con el número especificado
        /// </summary>
        /// <param name="numeroFactura">Número de factura a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task<bool> ExisteNumeroFacturaAsync(string numeroFactura, CancellationToken cancellationToken = default);
    }
} 