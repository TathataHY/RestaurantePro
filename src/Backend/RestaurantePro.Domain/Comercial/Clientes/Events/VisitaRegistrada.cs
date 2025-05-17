namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se registra una visita de un cliente
    /// </summary>
    public class VisitaRegistrada : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Cantidad total de visitas del cliente
        /// </summary>
        public int TotalVisitas { get; }

        /// <summary>
        /// Constructor para evento de visita registrada
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="totalVisitas">Total de visitas acumuladas</param>
        public VisitaRegistrada(Guid clienteId, int totalVisitas)
        {
            ClienteId = clienteId;
            TotalVisitas = totalVisitas;
        }
    }
} 