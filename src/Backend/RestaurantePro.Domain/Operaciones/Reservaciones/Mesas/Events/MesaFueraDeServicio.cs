namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando una mesa se marca como fuera de servicio
    /// </summary>
    public class MesaFueraDeServicio : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa
        /// </summary>
        public Guid MesaId { get; }
        
        /// <summary>
        /// Motivo por el que la mesa está fuera de servicio
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaFueraDeServicio(Guid mesaId, string motivo)
        {
            MesaId = mesaId;
            Motivo = motivo;
        }
    }
} 