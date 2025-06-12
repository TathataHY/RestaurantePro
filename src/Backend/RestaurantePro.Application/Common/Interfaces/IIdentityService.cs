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
    /// Asigna un rol a un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="role">Rol a asignar</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> AddUserToRoleAsync(string userId, string role);
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