using MediatR;
using RestaurantePro.Application.Features.Productos.Dtos;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Productos.Queries.ObtenerProductos
{
    public class ObtenerProductosQuery : IRequest<List<ProductoDto>>
    {
        public int? CategoriaId { get; set; }
        public bool? Disponible { get; set; }
    }
} 