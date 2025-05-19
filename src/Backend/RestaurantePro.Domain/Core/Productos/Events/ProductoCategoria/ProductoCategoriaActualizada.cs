namespace RestaurantePro.Domain.Core.Productos.Events.ProductoCategoria
{
    /// <summary>
    /// Evento de dominio emitido cuando se actualiza una categoría de producto
    /// </summary>
    public class ProductoCategoriaActualizada : DomainEvent
    {
        /// <summary>
        /// Identificador de la categoría actualizada
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Nuevo nombre de la categoría
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Nueva descripción de la categoría
        /// </summary>
        public string Descripcion { get; }
        
        /// <summary>
        /// Nuevo orden de visualización
        /// </summary>
        public int Orden { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ProductoCategoriaActualizada(Guid id, string nombre, string descripcion, int orden)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Orden = orden;
        }
    }
} 