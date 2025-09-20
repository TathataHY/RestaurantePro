using MediatR;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerMenuDelDia
{
    /// <summary>
    /// Query para obtener el menú del día (solo preparaciones disponibles, de hoy, no vencidas)
    /// </summary>
    public class ObtenerMenuDelDiaQuery : IRequest<Result<List<PreparacionDiariaDto>>>
    {
        /// <summary>
        /// Fecha específica para el menú (opcional, por defecto hoy)
        /// </summary>
        public DateTime? Fecha { get; set; }

        /// <summary>
        /// Límite de elementos a devolver (opcional, por defecto 10 para dashboard)
        /// </summary>
        public int? Limite { get; set; } = 10;
    }
}
