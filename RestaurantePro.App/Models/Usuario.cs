using SQLite;

namespace RestaurantePro.App.Models;

public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string NombreUsuario { get; set; } // Cambiado de Usuario a NombreUsuario
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