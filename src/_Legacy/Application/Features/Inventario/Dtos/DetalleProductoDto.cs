namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class DetalleProductoDto
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto
        /// </summary>
        public decimal Cantidad { get; set; }
    }
} 