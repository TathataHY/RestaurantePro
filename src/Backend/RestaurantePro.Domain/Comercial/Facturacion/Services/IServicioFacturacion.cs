namespace RestaurantePro.Domain.Comercial.Facturacion.Services
{
    /// <summary>
    /// Interfaz para el servicio de facturación
    /// </summary>
    public interface IServicioFacturacion
    {
        /// <summary>
        /// Genera una factura para una comanda
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="tipoFactura">Tipo de factura a generar</param>
        /// <param name="nombreCliente">Nombre del cliente o razón social</param>
        /// <param name="clienteId">Identificador del cliente (opcional)</param>
        /// <param name="identificacionFiscal">RFC o identificador fiscal (opcional)</param>
        /// <param name="direccionCliente">Dirección del cliente (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura generada</returns>
        Task<Result<Factura>> GenerarFacturaParaComandaAsync(
            Guid comandaId,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera una factura para varias comandas
        /// </summary>
        /// <param name="comandasIds">Identificadores de las comandas</param>
        /// <param name="tipoFactura">Tipo de factura a generar</param>
        /// <param name="nombreCliente">Nombre del cliente o razón social</param>
        /// <param name="clienteId">Identificador del cliente (opcional)</param>
        /// <param name="identificacionFiscal">RFC o identificador fiscal (opcional)</param>
        /// <param name="direccionCliente">Dirección del cliente (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura generada</returns>
        Task<Result<Factura>> GenerarFacturaParaComandasAsync(
            IEnumerable<Guid> comandasIds,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Emite una factura previamente creada en estado borrador
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="diasVencimiento">Días para vencimiento (0 para pago inmediato)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura emitida</returns>
        Task<Result<Factura>> EmitirFacturaAsync(
            Guid facturaId,
            int diasVencimiento = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Anula una factura
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="motivo">Motivo de la anulación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura anulada</returns>
        Task<Result<Factura>> AnularFacturaAsync(
            Guid facturaId,
            string motivo,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un pago para una factura
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="pagoId">Identificador del pago</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura actualizada</returns>
        Task<Result<Factura>> RegistrarPagoFacturaAsync(
            Guid facturaId,
            Guid pagoId,
            decimal monto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera el siguiente número de factura
        /// </summary>
        /// <param name="prefijo">Prefijo para el número de factura (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el siguiente número de factura disponible</returns>
        Task<Result<string>> GenerarSiguienteNumeroFacturaAsync(
            string? prefijo = null,
            CancellationToken cancellationToken = default);
    }
} 