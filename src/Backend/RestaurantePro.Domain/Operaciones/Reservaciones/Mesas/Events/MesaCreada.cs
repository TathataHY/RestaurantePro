namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una nueva mesa
    /// </summary>
    public class MesaCreada : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa creada
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Número asignado a la mesa
        /// </summary>
        public int Numero { get; }

        /// <summary>
        /// Capacidad de personas en la mesa
        /// </summary>
        public int Capacidad { get; }

        /// <summary>
        /// Ubicación de la mesa
        /// </summary>
        public string Ubicacion { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaCreada(Guid mesaId, int numero, int capacidad, string ubicacion)
        {
            MesaId = mesaId;
            Numero = numero;
            Capacidad = capacidad;
            Ubicacion = ubicacion;
                    }
    }
}


