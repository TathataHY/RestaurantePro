using MediatR;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerEstadisticasPreparaciones
{
    /// <summary>
    /// Consulta para obtener estadísticas de preparaciones
    /// </summary>
    public class ObtenerEstadisticasPreparacionesQuery : IRequest<EstadisticasPreparacionesDto>
    {
        // No requiere parámetros adicionales
        // Se calculan las estadísticas para el día actual
    }
} 