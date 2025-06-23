namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se asigna un cliente a una mesa
    /// </summary>
    public class MesaAsignada : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa asignada
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Identificador del cliente asignado
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Observaciones de la asignación
        /// </summary>
        public string? Observaciones { get; }

        public MesaAsignada(Guid mesaId, Guid clienteId, string? observaciones = null)
        {
            MesaId = mesaId;
            ClienteId = clienteId;
            Observaciones = observaciones;
        }
    }
} 