namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando una comanda es dividida
    /// </summary>
    public class ComandaDividida : DomainEvent
    {
        /// <summary>
        /// ID de la comanda dividida
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comandaId">ID de la comanda dividida</param>
        public ComandaDividida(Guid comandaId)
        {
            ComandaId = comandaId;
        }
    }
} 