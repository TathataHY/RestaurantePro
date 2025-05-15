using RestaurantePro.Domain.Entities;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para generación y validación de tokens JWT
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        /// <param name="usuario">Usuario para el que se genera el token</param>
        /// <returns>Token JWT generado</returns>
        string GenerateToken(Usuario usuario);

        /// <summary>
        /// Genera un token de actualización para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Token de actualización generado</returns>
        string GenerateRefreshToken(int userId);

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>True si el token es válido, false en caso contrario</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Obtiene las reclamaciones (claims) de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Diccionario con las reclamaciones</returns>
        Dictionary<string, string> GetTokenClaims(string token);

        /// <summary>
        /// Obtiene el ID de usuario desde un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>ID del usuario</returns>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// Renueva un token de acceso usando un token de actualización
        /// </summary>
        /// <param name="refreshToken">Token de actualización</param>
        /// <returns>Nuevo token de acceso</returns>
        string RenewAccessToken(string refreshToken);
    }
} 