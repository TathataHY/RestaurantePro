namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se actualiza el estado de una comanda
    /// </summary>
    public class EstadoComandaActualizado : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Estado anterior de la comanda
        /// </summary>
        public EstadoComanda EstadoAnterior { get; }

        /// <summary>
        /// Nuevo estado de la comanda
        /// </summary>
        public EstadoComanda NuevoEstado { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EstadoComandaActualizado(Guid comandaId, EstadoComanda estadoAnterior, EstadoComanda nuevoEstado)
        {
            ComandaId = comandaId;
            EstadoAnterior = estadoAnterior;
            NuevoEstado = nuevoEstado;
            OccurredOn = DateTime.Now;
        }
    }
}
