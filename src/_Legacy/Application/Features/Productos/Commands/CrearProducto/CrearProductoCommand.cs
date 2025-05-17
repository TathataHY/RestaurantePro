using MediatR;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Productos.Commands.CrearProducto
{
    public class CrearProductoCommand : IRequest<int>
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public int TiempoPreparacionMinutos { get; set; }
        public string UnidadMedida { get; set; }
        public List<IngredienteProductoInfo> Ingredientes { get; set; } = new List<IngredienteProductoInfo>();
    }

    public class IngredienteProductoInfo
    {
        public int IngredienteId { get; set; }
        public decimal Cantidad { get; set; }
        public bool Opcional { get; set; }
    }
} 