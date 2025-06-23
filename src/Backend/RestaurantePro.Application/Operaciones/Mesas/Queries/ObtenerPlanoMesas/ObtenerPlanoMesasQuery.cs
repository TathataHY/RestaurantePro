using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerPlanoMesas;

/// <summary>
/// Query para obtener el plano de mesas del restaurante
/// </summary>
public class ObtenerPlanoMesasQuery : IRequest<Result<PlanoMesasDto>>
{
    /// <summary>
    /// Filtrar por estado de mesa (opcional)
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Filtrar por ubicación (opcional)
    /// </summary>
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Incluir mesas reservadas (por defecto true)
    /// </summary>
    public bool IncluirReservadas { get; set; } = true;

    /// <summary>
    /// Incluir mesas ocupadas (por defecto true)
    /// </summary>
    public bool IncluirOcupadas { get; set; } = true;
} 