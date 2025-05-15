using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Features.Inventario.Dtos;
using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerMovimientos
{
    public class ObtenerMovimientosQuery : IRequest<PaginatedList<MovimientoDto>>
    {
        /// <summary>
        /// Filtrar por ingrediente específico
        /// </summary>
        public int? IngredienteId { get; set; }

        /// <summary>
        /// Filtrar por tipo de movimiento
        /// </summary>
        public TipoMovimiento? TipoMovimiento { get; set; }

        /// <summary>
        /// Fecha de inicio para filtrar
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Fecha final para filtrar
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>
        /// Referencia (número de orden, comanda, etc.)
        /// </summary>
        public string Referencia { get; set; }

        /// <summary>
        /// Usuario que realizó el movimiento
        /// </summary>
        public string UsuarioId { get; set; }

        /// <summary>
        /// Ordenar por campo específico
        /// </summary>
        public string OrderBy { get; set; } = "Fecha";

        /// <summary>
        /// Dirección de ordenamiento (asc, desc)
        /// </summary>
        public string OrderDirection { get; set; } = "desc";

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