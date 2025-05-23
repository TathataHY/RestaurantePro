

namespace RestaurantePro.Domain.Comercial.Pagos.Events.Pago
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se registra un nuevo pago
    /// </summary>
    public class PagoRegistrado : DomainEvent
    {
        /// <summary>
        /// Identificador único del pago
        /// </summary>
        public Guid PagoId { get; }

        /// <summary>
        /// Identificador de la comanda asociada al pago
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Método de pago utilizado
        /// </summary>
        public MetodoPago MetodoPago { get; }

        /// <summary>
        /// Monto total del pago
        /// </summary>
        public decimal Monto { get; }

        /// <summary>
        /// Fecha y hora en que se realizó el pago
        /// </summary>
        public DateTime FechaPago { get; }

        /// <summary>
        /// Constructor para el evento PagoRegistrado
        /// </summary>
        /// <param name="pagoId">Identificador del pago</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="fechaPago">Fecha y hora del pago</param>
        public PagoRegistrado(Guid pagoId, Guid comandaId, MetodoPago metodoPago, decimal monto, DateTime fechaPago)
        {
            PagoId = pagoId;
            ComandaId = comandaId;
            MetodoPago = metodoPago;
            Monto = monto;
            FechaPago = fechaPago;
        }
    }
} 