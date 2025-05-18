namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se canjean puntos de fidelización de un cliente
    /// </summary>
    public class PuntosFidelizacionCanjeados : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Cantidad de puntos canjeados
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Puntos que quedan disponibles después del canje
        /// </summary>
        public int PuntosRestantes { get; }

        /// <summary>
        /// Motivo del canje de puntos
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        public PuntosFidelizacionCanjeados(Guid clienteId, int cantidad, int puntosRestantes, string motivo)
        {
            ClienteId = clienteId;
            Cantidad = cantidad;
            PuntosRestantes = puntosRestantes;
            Motivo = motivo;
        }
    }
} 