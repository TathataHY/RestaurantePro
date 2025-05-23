namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina un producto de una promoción
    /// </summary>
    public class PromocionProductoEliminado : DomainEvent
    {
        /// <summary>
        /// Identificador único de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Código de la promoción
        /// </summary>
        public string Codigo { get; }
        
        /// <summary>
        /// Nombre de la promoción
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Identificador del producto eliminado
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionProductoEliminado
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="nombre">Nombre de la promoción</param>
        /// <param name="productoId">Identificador del producto</param>
        public PromocionProductoEliminado(Guid promocionId, string codigo, string nombre, Guid productoId)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            ProductoId = productoId;
        }
    }
} 