namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;

public class ActualizarProveedorCommand : IRequest<Result<ProveedorDto>>
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? PaginaWeb { get; set; }
    public TipoProveedor Tipo { get; set; }
    public CondicionPago CondicionesPago { get; set; }
    public int DiasEntrega { get; set; }
    public CalificacionProveedor Calificacion { get; set; }
    public string? Notas { get; set; }
} 