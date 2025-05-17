using System;

namespace RestaurantePro.Application.Features.Categorias.Dtos
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public int Orden { get; set; }
        public int CantidadProductos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
    }
} 