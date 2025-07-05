using System.Threading.Tasks;

namespace RestaurantePro.Mobile.Core.Services.Authentication
{
    /// <summary>
    /// Servicio para gestionar tokens de autenticación
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Obtiene el token de autenticación almacenado
        /// </summary>
        /// <returns>El token o null si no hay token disponible</returns>
        Task<string> GetTokenAsync();
        
        /// <summary>
        /// Guarda un token de autenticación
        /// </summary>
        /// <param name="token">Token a guardar</param>
        Task SaveTokenAsync(string token);
        
        /// <summary>
        /// Elimina el token almacenado
        /// </summary>
        Task DeleteTokenAsync();
        
        /// <summary>
        /// Verifica si hay un token válido almacenado
        /// </summary>
        /// <returns>True si hay un token válido</returns>
        Task<bool> HasValidTokenAsync();
    }
} 