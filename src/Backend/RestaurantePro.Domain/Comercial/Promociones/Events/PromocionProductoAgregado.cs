namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agrega un producto a una promoción
    /// </summary>
    public class PromocionProductoAgregado : DomainEvent
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
        /// Identificador del producto agregado
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionProductoAgregado
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="nombre">Nombre de la promoción</param>
        /// <param name="productoId">Identificador del producto</param>
        public PromocionProductoAgregado(Guid promocionId, string codigo, string nombre, Guid productoId)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            ProductoId = productoId;
        }
    }
} 