using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionPorId;

/// <summary>
/// Query para obtener una evaluación de proveedor por ID
/// </summary>
public class ObtenerEvaluacionPorIdQuery : IRequest<Result<DTOs.EvaluacionProveedorDto>>
{
    /// <summary>
    /// ID de la evaluación a obtener
    /// </summary>
    public Guid Id { get; set; }

    public ObtenerEvaluacionPorIdQuery(Guid id)
    {
        Id = id;
    }
} 