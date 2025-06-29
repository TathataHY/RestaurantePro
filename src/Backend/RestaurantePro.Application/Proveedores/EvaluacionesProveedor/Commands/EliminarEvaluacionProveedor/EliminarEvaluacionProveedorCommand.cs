using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.EliminarEvaluacionProveedor;

/// <summary>
/// Command para eliminar (soft delete) una evaluación de proveedor
/// </summary>
public class EliminarEvaluacionProveedorCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
} 