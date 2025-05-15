namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de categoría
    /// </summary>
    public class CategoriaDto
    {
        /// <summary>
        /// ID de la categoría
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la categoría
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción de la categoría
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// URL de la imagen de la categoría
        /// </summary>
        public string ImagenUrl { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// Indica si la categoría está activa
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// Cantidad de productos en la categoría
        /// </summary>
        public int CantidadProductos { get; set; }
    }
} 