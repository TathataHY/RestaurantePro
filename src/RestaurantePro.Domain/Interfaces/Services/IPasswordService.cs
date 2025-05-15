namespace RestaurantePro.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para manejo seguro de contraseñas
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Genera un salt aleatorio para hash de contraseña
        /// </summary>
        /// <returns>Salt generado</returns>
        string GenerateSalt();

        /// <summary>
        /// Genera un hash para una contraseña usando un salt específico
        /// </summary>
        /// <param name="password">Contraseña a hashear</param>
        /// <param name="salt">Salt a utilizar</param>
        /// <returns>Hash de la contraseña</returns>
        string HashPassword(string password, string salt);

        /// <summary>
        /// Verifica si una contraseña coincide con un hash almacenado
        /// </summary>
        /// <param name="password">Contraseña a verificar</param>
        /// <param name="salt">Salt utilizado</param>
        /// <param name="hashedPassword">Hash almacenado</param>
        /// <returns>True si la contraseña coincide, false en caso contrario</returns>
        bool VerifyPassword(string password, string salt, string hashedPassword);
    }
} 