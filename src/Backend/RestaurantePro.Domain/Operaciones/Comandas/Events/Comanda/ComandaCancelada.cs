namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se cancela una comanda
    /// </summary>
    public class ComandaCancelada : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda cancelada
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCancelada(Guid comandaId, string motivo)
        {
            ComandaId = comandaId;
            Motivo = motivo;
            OccurredOn = DateTime.Now;
        }
    }
}
