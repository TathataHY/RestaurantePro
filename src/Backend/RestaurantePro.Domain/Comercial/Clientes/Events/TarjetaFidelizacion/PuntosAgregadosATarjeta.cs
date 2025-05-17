namespace RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a una tarjeta
    /// </summary>
    public class PuntosAgregadosATarjeta : DomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Puntos agregados
        /// </summary>
        public int PuntosAgregados { get; }

        /// <summary>
        /// Total de puntos acumulados
        /// </summary>
        public int PuntosTotales { get; }

        
        public PuntosAgregadosATarjeta(Guid tarjetaId, int puntosAgregados, int puntosTotales)
        {
            TarjetaId = tarjetaId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
        }
    }
} 

