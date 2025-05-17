using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Comandas.Dtos
{
    public class ComandaDetalleDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public List<ComandaDetallePersonalizacionDto> Personalizaciones { get; set; } = new List<ComandaDetallePersonalizacionDto>();
    }
} 