namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se canjean puntos de una tarjeta
    /// </summary>
    public class PuntosCanjeados : DomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Puntos canjeados
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Concepto del canje
        /// </summary>
        public string Concepto { get; }

        /// <summary>
        /// Puntos disponibles después del canje
        /// </summary>
        public int PuntosDisponibles { get; }

        
        public PuntosCanjeados(Guid tarjetaId, int puntosCanjeados, string concepto, int puntosDisponibles)
        {
            TarjetaId = tarjetaId;
            Cantidad = puntosCanjeados;
            Concepto = concepto;
            PuntosDisponibles = puntosDisponibles;
        }
    }
} 

