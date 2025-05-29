namespace RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;

public class ContactoProveedorDto : BaseDto
{
    public Guid ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; }
    public string? Notas { get; set; }
} 