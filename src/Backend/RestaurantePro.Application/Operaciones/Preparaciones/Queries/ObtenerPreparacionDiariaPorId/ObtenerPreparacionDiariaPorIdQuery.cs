using System;
using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionDiariaPorId
{
    /// <summary>
    /// Query para obtener una preparación diaria específica por ID
    /// </summary>
    public class ObtenerPreparacionDiariaPorIdQuery : IRequest<Result<PreparacionDiariaDto>>
    {
        /// <summary>
        /// ID de la preparación diaria a obtener
        /// </summary>
        public Guid Id { get; set; }
    }
} 