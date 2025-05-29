namespace RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;

public class ContactoProveedorCreateDto
{
    public Guid ProveedorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    public bool EsPrincipal { get; set; }
    public string? Notas { get; set; }
} 