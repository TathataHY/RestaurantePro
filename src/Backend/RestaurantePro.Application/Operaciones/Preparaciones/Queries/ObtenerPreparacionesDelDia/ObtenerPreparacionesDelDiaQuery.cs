using System;
using System.Collections.Generic;
using MediatR;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDelDia
{
    /// <summary>
    /// Consulta para obtener las preparaciones del día actual
    /// </summary>
    public class ObtenerPreparacionesDelDiaQuery : IRequest<List<PreparacionDto>>
    {
        /// <summary>
        /// Filtro por estado de preparación (opcional)
        /// </summary>
        public int? EstadoId { get; set; }
        
        /// <summary>
        /// Filtro por producto (opcional)
        /// </summary>
        public Guid? ProductoId { get; set; }
        
        /// <summary>
        /// Filtro por chef (opcional)
        /// </summary>
        public Guid? ChefId { get; set; }
        
        /// <summary>
        /// Indica si se deben incluir preparaciones vencidas
        /// </summary>
        public bool IncluirVencidas { get; set; } = false;
        
        /// <summary>
        /// Campo por el cual ordenar (fecha, disponibilidad, producto, estado)
        /// </summary>
        public string OrderBy { get; set; } = "fecha";
        
        /// <summary>
        /// Dirección del ordenamiento (Asc, Desc)
        /// </summary>
        public string OrderDirection { get; set; } = "Desc";
    }
} 