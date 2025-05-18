namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento que se dispara cuando un ítem de comanda es cancelado
    /// </summary>
    public class ItemComandaCancelado : DomainEvent
    {
        /// <summary>
        /// ID del ítem de comanda
        /// </summary>
        public Guid ItemComandaId { get; }
        
        /// <summary>
        /// ID del producto asociado al ítem
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// ID de la comanda a la que pertenece el ítem
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ItemComandaCancelado(Guid itemComandaId, Guid productoId, Guid comandaId, string motivo)
        {
            ItemComandaId = itemComandaId;
            ProductoId = productoId;
            ComandaId = comandaId;
            Motivo = motivo;
        }
    }
} 