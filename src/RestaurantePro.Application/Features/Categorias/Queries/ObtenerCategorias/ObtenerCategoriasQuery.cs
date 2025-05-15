using MediatR;
using RestaurantePro.Application.Features.Categorias.Dtos;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategorias
{
    public class ObtenerCategoriasQuery : IRequest<List<CategoriaDto>>
    {
        public bool? SoloActivas { get; set; }
    }
} 