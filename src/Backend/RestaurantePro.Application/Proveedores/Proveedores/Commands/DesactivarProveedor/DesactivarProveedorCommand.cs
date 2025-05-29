namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;

public class DesactivarProveedorCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public string? RazonDesactivacion { get; set; }
} 