using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para gestionar la autenticación y autorización de usuarios
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <param name="email">Email</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Resultado con el ID del usuario creado si es exitoso, o errores en caso contrario</returns>
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password);
    
    /// <summary>
    /// Registra un nuevo usuario con nombre, apellidos y rol
    /// </summary>
    Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol);
    
    /// <summary>
    /// Crea un nuevo rol en el sistema.
    /// </summary>
    Task<Result> CreateRoleAsync(string roleName, string description, bool isSystemRole);
    
    /// <summary>
    /// Valida las credenciales de un usuario e inicia sesión
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Respuesta de autenticación con token si las credenciales son válidas</returns>
    Task<AuthResponse> LoginAsync(string email, string password);
    
    /// <summary>
    /// Obtiene la lista de usuarios del sistema
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    Task<List<UserDto>> GetUsersAsync();
    
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<UserDto> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Actualiza la información de un usuario existente
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="nombre">Nombre del usuario</param>
    /// <param name="apellidos">Apellidos del usuario</param>
    /// <param name="email">Email del usuario</param>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> UpdateUserAsync(string id, string nombre, string apellidos, string email, string username);

    /// <summary>
    /// Elimina un usuario del sistema
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> DeleteUserAsync(string userId);

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="currentPassword">Contraseña actual</param>
    /// <param name="newPassword">Nueva contraseña</param>
    /// <param name="confirmNewPassword">Confirmación de la nueva contraseña (opcional)</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? confirmNewPassword = null);

    /// <summary>
    /// Autentica un usuario y obtiene una respuesta de autenticación
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Resultado de la autenticación</returns>
    Task<Result<AuthResponse>> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Actualiza el token de autenticación de un usuario
    /// </summary>
    /// <param name="token">Token actual</param>
    /// <param name="refreshToken">Token de actualización</param>
    /// <returns>Resultado de la actualización</returns>
    Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken);
}

/// <summary>
/// Respuesta de autenticación
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Indica si la autenticación fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Token JWT generado
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// Fecha y hora de expiración del token
    /// </summary>
    public DateTime Expiration { get; set; }
    
    /// <summary>
    /// ID del usuario autenticado
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Nombre del usuario autenticado
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Roles del usuario autenticado
    /// </summary>
    public List<string> Roles { get; set; }
}

/// <summary>
/// DTO de usuario
/// </summary>
public class UserDto
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Indica si el email está confirmado
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// Roles del usuario
    /// </summary>
    public List<string> Roles { get; set; }
} 