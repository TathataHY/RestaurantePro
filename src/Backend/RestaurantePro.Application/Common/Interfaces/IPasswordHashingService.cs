namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de hashing de contraseñas del sistema
/// Proporciona métodos seguros para hashear y verificar contraseñas
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Genera un hash seguro de la contraseña con salt
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Tupla con el hash generado y el salt utilizado</returns>
    (string Hash, string Salt) HashPassword(string password);
    
    /// <summary>
    /// Verifica si una contraseña coincide con el hash almacenado
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar</param>
    /// <param name="hash">Hash almacenado en la base de datos</param>
    /// <param name="salt">Salt utilizado para generar el hash</param>
    /// <returns>True si la contraseña es correcta</returns>
    bool VerifyPassword(string password, string hash, string salt);
    
    /// <summary>
    /// Genera un salt aleatorio para hashing de contraseñas
    /// </summary>
    /// <returns>Salt generado como string</returns>
    string GenerateSalt();
    
    /// <summary>
    /// Verifica si una contraseña cumple con los requisitos de seguridad
    /// </summary>
    /// <param name="password">Contraseña a validar</param>
    /// <returns>Resultado de validación con mensajes de error si aplica</returns>
    (bool IsValid, List<string> Errors) ValidatePasswordStrength(string password);
    
    /// <summary>
    /// Genera una contraseña temporal segura
    /// </summary>
    /// <param name="length">Longitud de la contraseña (por defecto 12)</param>
    /// <returns>Contraseña temporal generada</returns>
    string GenerateTemporaryPassword(int length = 12);
} 