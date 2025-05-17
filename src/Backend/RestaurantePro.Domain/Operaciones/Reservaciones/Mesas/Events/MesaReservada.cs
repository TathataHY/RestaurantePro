namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una mesa se marca como reservada
    /// </summary>
    public class MesaReservada : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaReservada(Guid mesaId)
        {
            MesaId = mesaId;
                    }
    }
}


