namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento que se genera cuando se agrega una observación a una comanda.
    /// </summary>
    public class ObservacionAgregada : DomainEvent
    {
        /// <summary>
        /// ID de la comanda a la que se agregó la observación
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Observación que se agregó
        /// </summary>
        public string Observacion { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="observacion">Observación agregada</param>
        public ObservacionAgregada(Guid comandaId, string observacion) : base()
        {
            ComandaId = comandaId;
            Observacion = observacion;
        }
    }
} 