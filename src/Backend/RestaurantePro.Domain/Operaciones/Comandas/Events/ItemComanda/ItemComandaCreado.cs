namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un ítem de comanda
    /// </summary>
    public class ItemComandaCreado : DomainEvent
    {
        /// <summary>
        /// Identificador único de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Identificador único del ítem agregado
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// Identificador único del producto agregado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Nombre del producto agregado
        /// </summary>
        public string NombreProducto { get; }

        /// <summary>
        /// Cantidad de producto agregada
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ItemComandaCreado(
            Guid comandaId,
            Guid itemId,
            Guid productoId,
            string nombreProducto,
            int cantidad)
        {
            ComandaId = comandaId;
            ItemId = itemId;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
        }
    }
} 