using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerEvaluacionesPorProveedor;

/// <summary>
/// Query para obtener evaluaciones de un proveedor específico
/// </summary>
public class ObtenerEvaluacionesPorProveedorQuery : IRequest<Result<List<DTOs.EvaluacionProveedorDto>>>
{
    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Indica si incluir solo evaluaciones activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Número de página para paginación
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    public int TamanoPagina { get; set; } = 20;

    public ObtenerEvaluacionesPorProveedorQuery(Guid proveedorId)
    {
        ProveedorId = proveedorId;
    }
} 