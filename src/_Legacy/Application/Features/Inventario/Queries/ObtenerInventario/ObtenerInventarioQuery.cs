using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Features.Inventario.Dtos;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerInventario
{
    public class ObtenerInventarioQuery : IRequest<PaginatedList<InventarioDto>>
    {
        /// <summary>
        /// Filtro por nombre o categoría del ingrediente
        /// </summary>
        public string Filtro { get; set; }

        /// <summary>
        /// Filtrar por categoría del ingrediente
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Mostrar solo elementos que requieren reposición (CantidadDisponible <= CantidadMinima)
        /// </summary>
        public bool SoloConAlertas { get; set; } = false;

        /// <summary>
        /// Ordenar por campo específico
        /// </summary>
        public string OrderBy { get; set; } = "NombreIngrediente";

        /// <summary>
        /// Dirección de ordenamiento (asc, desc)
        /// </summary>
        public string OrderDirection { get; set; } = "asc";

        /// <summary>
        /// Página a recuperar (1-indexed)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Tamaño de página a recuperar
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
} 