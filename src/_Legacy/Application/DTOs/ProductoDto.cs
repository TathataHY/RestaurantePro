using System.Collections.Generic;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de producto
    /// </summary>
    public class ProductoDto
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la categoría
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Nombre de la categoría
        /// </summary>
        public string CategoriaNombre { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Precio del producto
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// URL de la imagen del producto
        /// </summary>
        public string ImagenUrl { get; set; }

        /// <summary>
        /// Indica si el producto está disponible
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>
        /// Tiempo estimado de preparación en minutos
        /// </summary>
        public int TiempoPreparacion { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Indica si el producto es personalizable
        /// </summary>
        public bool Personalizable { get; set; }

        /// <summary>
        /// Lista de ingredientes del producto
        /// </summary>
        public List<IngredienteProductoDto> Ingredientes { get; set; } = new List<IngredienteProductoDto>();
    }

    /// <summary>
    /// DTO para ingrediente en un producto
    /// </summary>
    public class IngredienteProductoDto
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Cantidad requerida
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Indica si es opcional
        /// </summary>
        public bool Opcional { get; set; }
    }
} 