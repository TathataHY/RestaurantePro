using System.Collections.Generic;
using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDiarias
{
    /// <summary>
    /// Query para obtener todas las preparaciones diarias
    /// </summary>
    public class ObtenerPreparacionesDiariasQuery : IRequest<Result<List<PreparacionDiariaDto>>>
    {
        // No requiere parámetros adicionales para obtener todas las preparaciones diarias
    }
} 