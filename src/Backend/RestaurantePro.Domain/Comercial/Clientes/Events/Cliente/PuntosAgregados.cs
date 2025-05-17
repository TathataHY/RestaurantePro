namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a un cliente
    /// </summary>
    public class PuntosAgregados : DomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Cantidad de puntos agregados
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Total de puntos acumulados
        /// </summary>
        public int PuntosTotales { get; }

        
        public PuntosAgregados(Guid clienteId, int puntosAgregados, int puntosTotales)
        {
            ClienteId = clienteId;
            Cantidad = puntosAgregados;
            PuntosTotales = puntosTotales;
        }
    }
} 

