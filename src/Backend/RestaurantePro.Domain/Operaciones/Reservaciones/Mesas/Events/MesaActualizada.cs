namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualizan los datos de una mesa
    /// </summary>
    public class MesaActualizada : DomainEvent
    {
        /// <summary>
        /// Identificador de la mesa actualizada
        /// </summary>
        public Guid MesaId { get; }

        /// <summary>
        /// Nuevo número asignado a la mesa
        /// </summary>
        public int Numero { get; }

        /// <summary>
        /// Nueva capacidad de personas en la mesa
        /// </summary>
        public int Capacidad { get; }

        /// <summary>
        /// Nueva ubicación de la mesa
        /// </summary>
        public string Ubicacion { get; }

        /// <summary>
        /// Constructor para crear una nueva instancia del evento
        /// </summary>
        public MesaActualizada(Guid mesaId, int numero, int capacidad, string ubicacion)
        {
            MesaId = mesaId;
            Numero = numero;
            Capacidad = capacidad;
            Ubicacion = ubicacion;
        }
    }
} 