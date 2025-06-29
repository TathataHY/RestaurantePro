using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.ActualizarEvaluacionProveedor;

/// <summary>
/// Command para actualizar una evaluación de proveedor existente
/// </summary>
public class ActualizarEvaluacionProveedorCommand : IRequest<Result<EvaluacionProveedorDto>>
{
    public Guid Id { get; set; }
    public int CalificacionGeneral { get; set; }
    public int CalificacionCalidad { get; set; }
    public int CalificacionPuntualidad { get; set; }
    public int CalificacionComunicacion { get; set; }
    public int CalificacionPrecios { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    // El usuario actual se obtiene del contexto
} 