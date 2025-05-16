namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se finaliza una comanda
    /// </summary>
    public class ComandaFinalizada : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda finalizada
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Total de la comanda (incluyendo impuestos)
        /// </summary>
        public decimal Total { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaFinalizada(Guid comandaId, decimal total)
        {
            ComandaId = comandaId;
            Total = total;
            OccurredOn = DateTime.Now;
        }
    }
}
