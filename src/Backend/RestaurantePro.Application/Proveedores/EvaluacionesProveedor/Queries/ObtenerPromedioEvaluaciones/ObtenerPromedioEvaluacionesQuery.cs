using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerPromedioEvaluaciones;

/// <summary>
/// Query para obtener el promedio de evaluaciones de un proveedor
/// </summary>
public class ObtenerPromedioEvaluacionesQuery : IRequest<Result<DTOs.PromedioEvaluacionesDto>>
{
    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Indica si incluir solo evaluaciones activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    public ObtenerPromedioEvaluacionesQuery(Guid proveedorId)
    {
        ProveedorId = proveedorId;
    }
} 