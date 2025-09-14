namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para operaciones de usuarios en Identity
/// </summary>
public interface IIdentityUserService
{
    /// <summary>
    /// Crea un nuevo usuario en Identity
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <param name="email">Email del usuario</param>
    /// <param name="nombre">Nombre del usuario</param>
    /// <param name="apellidos">Apellidos del usuario</param>
    /// <param name="password">Contraseña del usuario</param>
    /// <param name="rol">Rol del usuario</param>
    /// <param name="activo">Si el usuario está activo</param>
    /// <param name="emailConfirmed">Si el email está confirmado</param>
    /// <returns>ID del usuario creado</returns>
    Task<string> CreateUserAsync(
        string userName, 
        string email, 
        string nombre, 
        string apellidos, 
        string password, 
        string rol, 
        bool activo = true, 
        bool emailConfirmed = true);

    /// <summary>
    /// Verifica si una contraseña es válida para un usuario
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <param name="password">Contraseña a verificar</param>
    /// <returns>True si la contraseña es válida</returns>
    Task<bool> VerifyPasswordAsync(string userName, string password);

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <param name="newPassword">Nueva contraseña</param>
    /// <returns>True si se cambió exitosamente</returns>
    Task<bool> ChangePasswordAsync(string userName, string newPassword);
}
