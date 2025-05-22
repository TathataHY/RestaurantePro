namespace RestaurantePro.Domain.Comercial.Pagos.Events.Pago
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se reembolsa un pago
    /// </summary>
    public class PagoReembolsado : DomainEvent
    {
        /// <summary>
        /// Identificador único del pago
        /// </summary>
        public Guid PagoId { get; }

        /// <summary>
        /// Monto reembolsado
        /// </summary>
        public decimal MontoReembolsado { get; }

        /// <summary>
        /// Indica si el reembolso fue total o parcial
        /// </summary>
        public bool EsReembolsoTotal { get; }

        /// <summary>
        /// Motivo del reembolso
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha y hora del reembolso
        /// </summary>
        public DateTime FechaReembolso { get; }

        /// <summary>
        /// Identificador de la transacción de reembolso (opcional)
        /// </summary>
        public string? ReferenciaTransaccion { get; }

        /// <summary>
        /// Constructor para el evento PagoReembolsado
        /// </summary>
        /// <param name="pagoId">Identificador del pago</param>
        /// <param name="montoReembolsado">Monto reembolsado</param>
        /// <param name="esReembolsoTotal">Indica si es reembolso total</param>
        /// <param name="motivo">Motivo del reembolso</param>
        /// <param name="fechaReembolso">Fecha y hora del reembolso</param>
        /// <param name="referenciaTransaccion">Referencia de la transacción (opcional)</param>
        public PagoReembolsado(
            Guid pagoId,
            decimal montoReembolsado,
            bool esReembolsoTotal,
            string motivo,
            DateTime fechaReembolso,
            string? referenciaTransaccion = null)
        {
            PagoId = pagoId;
            MontoReembolsado = montoReembolsado;
            EsReembolsoTotal = esReembolsoTotal;
            Motivo = motivo;
            FechaReembolso = fechaReembolso;
            ReferenciaTransaccion = referenciaTransaccion;
        }
    }
} 