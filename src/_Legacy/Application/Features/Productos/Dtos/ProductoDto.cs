using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Productos.Dtos
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public int TiempoPreparacionMinutos { get; set; }
        public string UnidadMedida { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
        public List<IngredienteProductoDto> Ingredientes { get; set; }
    }
} 