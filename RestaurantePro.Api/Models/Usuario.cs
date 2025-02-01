namespace RestaurantePro.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; }
    public string Contraseña { get; set; }
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; }
}

public enum RolUsuario
{
    Administrador,
    Mesero,
    Cocinero
} 