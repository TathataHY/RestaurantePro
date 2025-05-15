using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa una personalización aplicada a un detalle de comanda
    /// </summary>
    public class ComandaDetallePersonalizacion : BaseEntity
    {
        /// <summary>
        /// ID del detalle de comanda al que se aplica esta personalización
        /// </summary>
        public int ComandaDetalleId { get; set; }

        /// <summary>
        /// Detalle de comanda al que se aplica esta personalización
        /// </summary>
        public virtual ComandaDetalle ComandaDetalle { get; set; }

        /// <summary>
        /// ID del ingrediente que se está personalizando
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Ingrediente que se está personalizando
        /// </summary>
        public virtual Ingrediente Ingrediente { get; set; }

        /// <summary>
        /// Tipo de acción de personalización
        /// </summary>
        public AccionPersonalizacion Accion { get; set; }

        /// <summary>
        /// Cantidad del ingrediente (para agregar o sustituir)
        /// </summary>
        public decimal Cantidad { get; set; }
    }
} 