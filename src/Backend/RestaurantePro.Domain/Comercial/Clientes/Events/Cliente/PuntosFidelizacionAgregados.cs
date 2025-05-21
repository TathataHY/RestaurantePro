namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se dispara cuando se agregan puntos de fidelización a un cliente
    /// </summary>
    public class PuntosFidelizacionAgregados : DomainEvent
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// ID de la tarjeta de fidelización
        /// </summary>
        public Guid TarjetaId { get; }
        
        /// <summary>
        /// Cantidad de puntos agregados
        /// </summary>
        public int PuntosAgregados { get; }
        
        /// <summary>
        /// Total de puntos acumulados
        /// </summary>
        public int TotalPuntos { get; }
        
        /// <summary>
        /// Motivo por el que se agregaron los puntos
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="puntosAgregados">Cantidad de puntos agregados</param>
        /// <param name="totalPuntos">Total de puntos acumulados</param>
        /// <param name="motivo">Motivo</param>
        public PuntosFidelizacionAgregados(Guid clienteId, Guid tarjetaId, int puntosAgregados, int totalPuntos, string motivo)
        {
            ClienteId = clienteId;
            TarjetaId = tarjetaId;
            PuntosAgregados = puntosAgregados;
            TotalPuntos = totalPuntos;
            Motivo = motivo;
        }
    }
} 