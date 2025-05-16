namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a un cliente
    /// </summary>
    public class PuntosAgregadosEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Cantidad de puntos agregados
        /// </summary>
        public int PuntosAgregados { get; }

        /// <summary>
        /// Total de puntos acumulados
        /// </summary>
        public int PuntosTotales { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public PuntosAgregadosEvent(Guid clienteId, int puntosAgregados, int puntosTotales)
        {
            ClienteId = clienteId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
        }
    }
} 