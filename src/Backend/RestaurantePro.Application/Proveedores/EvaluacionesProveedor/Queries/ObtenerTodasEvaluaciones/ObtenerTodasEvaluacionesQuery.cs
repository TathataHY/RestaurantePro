using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Queries.ObtenerTodasEvaluaciones;

/// <summary>
/// Query para obtener todas las evaluaciones de proveedores
/// </summary>
public class ObtenerTodasEvaluacionesQuery : IRequest<Result<List<DTOs.EvaluacionProveedorDto>>>
{
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

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaEvaluacion";

    /// <summary>
    /// Indica si ordenar de forma descendente
    /// </summary>
    public bool OrdenDescendente { get; set; } = true;
} 