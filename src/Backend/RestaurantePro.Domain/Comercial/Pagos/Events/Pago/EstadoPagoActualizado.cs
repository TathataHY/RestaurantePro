

namespace RestaurantePro.Domain.Comercial.Pagos.Events.Pago
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el estado de un pago
    /// </summary>
    public class EstadoPagoActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador único del pago
        /// </summary>
        public Guid PagoId { get; }

        /// <summary>
        /// Estado anterior del pago
        /// </summary>
        public EstadoPago EstadoAnterior { get; }

        /// <summary>
        /// Nuevo estado del pago
        /// </summary>
        public EstadoPago NuevoEstado { get; }

        /// <summary>
        /// Motivo de la actualización del estado (opcional)
        /// </summary>
        public string? Motivo { get; }

        /// <summary>
        /// Fecha y hora de la actualización
        /// </summary>
        public DateTime FechaActualizacion { get; }

        /// <summary>
        /// Constructor para el evento EstadoPagoActualizado
        /// </summary>
        /// <param name="pagoId">Identificador del pago</param>
        /// <param name="estadoAnterior">Estado anterior del pago</param>
        /// <param name="nuevoEstado">Nuevo estado del pago</param>
        /// <param name="fechaActualizacion">Fecha y hora de la actualización</param>
        /// <param name="motivo">Motivo de la actualización (opcional)</param>
        public EstadoPagoActualizado(
            Guid pagoId,
            EstadoPago estadoAnterior,
            EstadoPago nuevoEstado,
            DateTime fechaActualizacion,
            string? motivo = null)
        {
            PagoId = pagoId;
            EstadoAnterior = estadoAnterior;
            NuevoEstado = nuevoEstado;
            FechaActualizacion = fechaActualizacion;
            Motivo = motivo;
        }
    }
} 