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

        /// <summary>
        /// Aplica un descuento a una factura con validaciones de negocio
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="tipoDescuento">Tipo de descuento (Promocional, Empleado, Volumen, Cortesia)</param>
        /// <param name="montoDescuento">Monto del descuento calculado</param>
        /// <param name="concepto">Concepto o descripción del descuento</param>
        /// <param name="motivo">Motivo del descuento</param>
        /// <param name="usuarioAutorizaId">Usuario que autoriza el descuento</param>
        /// <param name="aplicarAntesDeImpuestos">Si el descuento se aplica antes de calcular impuestos</param>
        /// <param name="codigoAutorizacion">Código de autorización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el descuento aplicado</returns>
        Task<Result<Factura>> AplicarDescuentoAsync(
            Guid facturaId,
            string tipoDescuento,
            decimal montoDescuento,
            string concepto,
            string motivo,
            Guid usuarioAutorizaId,
            bool aplicarAntesDeImpuestos = false,
            string? codigoAutorizacion = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Procesa un pago para una factura
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <param name="referencia">Referencia del pago (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura actualizada</returns>
        Task<Result<Factura>> ProcesarPagoAsync(
            Guid facturaId,
            decimal monto,
            string metodoPago,
            string? referencia = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Acumula puntos por compra para un cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="montoCompra">Monto de la compra</param>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con los puntos acumulados</returns>
        Task<Result<int>> AcumularPuntosPorCompraAsync(
            Guid clienteId,
            decimal montoCompra,
            Guid facturaId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera una factura (método simplificado)
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="clienteId">Identificador del cliente (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura generada</returns>
        Task<Result<Factura>> GenerarFacturaAsync(
            Guid comandaId,
            Guid? clienteId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte un pago previamente procesado
        /// </summary>
        /// <param name="facturaId">Identificador de la factura</param>
        /// <param name="pagoId">Identificador del pago a revertir</param>
        /// <param name="motivo">Motivo de la reversión</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura actualizada</returns>
        Task<Result<Factura>> RevertirPagoAsync(
            Guid facturaId,
            Guid pagoId,
            string motivo,
            CancellationToken cancellationToken = default);
    }
} 