namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente
{
    /// <summary>
    /// Evento que se dispara cuando se utilizan puntos de fidelización de un cliente
    /// </summary>
    public class PuntosFidelizacionUtilizados : DomainEvent
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
        /// Cantidad de puntos utilizados
        /// </summary>
        public int PuntosUtilizados { get; }
        
        /// <summary>
        /// Total de puntos restantes
        /// </summary>
        public int PuntosRestantes { get; }
        
        /// <summary>
        /// Motivo por el que se utilizaron los puntos
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="puntosUtilizados">Cantidad de puntos utilizados</param>
        /// <param name="puntosRestantes">Total de puntos restantes</param>
        /// <param name="motivo">Motivo</param>
        public PuntosFidelizacionUtilizados(Guid clienteId, Guid tarjetaId, int puntosUtilizados, int puntosRestantes, string motivo)
        {
            ClienteId = clienteId;
            TarjetaId = tarjetaId;
            PuntosUtilizados = puntosUtilizados;
            PuntosRestantes = puntosRestantes;
            Motivo = motivo;
        }
    }
} 