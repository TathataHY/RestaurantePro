namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina una personalización de un ítem de comanda
    /// </summary>
    public class PersonalizacionEliminadaDeItem : DomainEvent
    {
        /// <summary>
        /// ID del ítem de comanda
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Datos de la personalización eliminada
        /// </summary>
        public PersonalizacionItemDto Personalizacion { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PersonalizacionEliminadaDeItem(Guid itemId, Guid comandaId, PersonalizacionItemDto personalizacion)
        {
            ItemId = itemId;
            ComandaId = comandaId;
            Personalizacion = personalizacion;
        }
    }
} 
