using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.CrearEvaluacionProveedor;

/// <summary>
/// Command para crear una nueva evaluación de proveedor
/// </summary>
public class CrearEvaluacionProveedorCommand : IRequest<Result<EvaluacionProveedorDto>>
{
    public Guid ProveedorId { get; set; }
    public int CalificacionGeneral { get; set; }
    public int CalificacionCalidad { get; set; }
    public int CalificacionPuntualidad { get; set; }
    public int CalificacionComunicacion { get; set; }
    public int CalificacionPrecios { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    // El EvaluadorId se puede obtener del contexto de usuario autenticado
} 