using System;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para obtener información sobre el usuario actual
    /// </summary>
    public interface IUsuarioActualService
    {
        /// <summary>
        /// Obtiene el ID del usuario actual
        /// </summary>
        /// <returns>ID del usuario</returns>
        Guid ObtenerUsuarioId();
        
        /// <summary>
        /// Obtiene el email del usuario actual
        /// </summary>
        /// <returns>Email del usuario</returns>
        string ObtenerUsuarioEmail();
        
        /// <summary>
        /// Obtiene el rol del usuario actual
        /// </summary>
        /// <returns>Rol del usuario</returns>
        string ObtenerUsuarioRol();
        
        /// <summary>
        /// Verifica si el usuario actual está autenticado
        /// </summary>
        /// <returns>True si está autenticado, False en caso contrario</returns>
        bool EstaAutenticado();
    }
} 