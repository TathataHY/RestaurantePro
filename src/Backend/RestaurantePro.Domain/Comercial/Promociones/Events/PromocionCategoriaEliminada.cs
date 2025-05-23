namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina una categoría de una promoción
    /// </summary>
    public class PromocionCategoriaEliminada : DomainEvent
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
        /// Identificador de la categoría eliminada
        /// </summary>
        public Guid CategoriaId { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionCategoriaEliminada
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="nombre">Nombre de la promoción</param>
        /// <param name="categoriaId">Identificador de la categoría</param>
        public PromocionCategoriaEliminada(Guid promocionId, string codigo, string nombre, Guid categoriaId)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            CategoriaId = categoriaId;
        }
    }
} 