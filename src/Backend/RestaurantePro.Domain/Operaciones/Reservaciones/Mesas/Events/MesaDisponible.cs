namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una mesa se marca como disponible
    /// </summary>
    public class MesaDisponible : IDomainEvent
    {
        /// <summary>
        /// Identificador de la mesa
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaDisponible(Guid mesaId)
        {
            MesaId = mesaId;
            OccurredOn = DateTime.Now;
        }
    }
}
