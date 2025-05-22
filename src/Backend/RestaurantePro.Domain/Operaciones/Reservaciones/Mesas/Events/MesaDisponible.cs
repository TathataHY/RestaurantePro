namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una mesa se marca como disponible
    /// </summary>
    public class MesaDisponible : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaDisponible(Guid mesaId)
        {
            MesaId = mesaId;
        }
    }
}


