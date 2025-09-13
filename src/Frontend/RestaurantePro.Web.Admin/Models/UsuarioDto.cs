namespace RestaurantePro.Web.Admin.Models;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int NivelAcceso { get; set; }
    public bool EsAdministrador { get; set; }
    public string? Telefono { get; set; }
    public string Estado { get; set; } = "Activo";
}


