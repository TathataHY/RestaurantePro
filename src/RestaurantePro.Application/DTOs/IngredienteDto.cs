namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de ingrediente
    /// </summary>
    public class IngredienteDto
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción del ingrediente
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Stock actual
        /// </summary>
        public decimal StockActual { get; set; }

        /// <summary>
        /// Stock mínimo
        /// </summary>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Costo unitario
        /// </summary>
        public decimal CostoUnitario { get; set; }

        /// <summary>
        /// Indica si el ingrediente está activo
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Indica si el nivel de stock es bajo
        /// </summary>
        public bool StockBajo => StockActual < StockMinimo;
    }
} 