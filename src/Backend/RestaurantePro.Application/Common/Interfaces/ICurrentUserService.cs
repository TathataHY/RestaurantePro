using System;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para obtener información sobre el usuario actual (versión en inglés)
    /// Mantener esta interfaz para compatibilidad con código existente
    /// </summary>
    [Obsolete("Usar IUsuarioActualService en su lugar")]
    public interface ICurrentUserService
    {
        /// <summary>
        /// ID del usuario actual
        /// </summary>
        Guid UserId { get; }
        
        /// <summary>
        /// Nombre de usuario del usuario actual
        /// </summary>
        string UserName { get; }
        
        /// <summary>
        /// Indica si el usuario está autenticado
        /// </summary>
        bool IsAuthenticated { get; }
    }
} 