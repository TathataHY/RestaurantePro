using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Domain.Core.Usuarios.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios con caché
    /// Proporciona los mismos métodos que IUsuarioService pero con implementación de caché
    /// </summary>
    public interface IUsuarioServiceCached : IUsuarioService
    {
        /// <summary>
        /// Invalida todas las entradas de caché relacionadas con un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        void InvalidarCacheUsuario(Guid usuarioId);
        
        /// <summary>
        /// Invalida todas las entradas de caché relacionadas con usuarios
        /// </summary>
        void InvalidarCacheUsuarios();
        
        /// <summary>
        /// Obtiene las estadísticas de la caché de usuarios
        /// </summary>
        /// <returns>Diccionario con estadísticas de la caché</returns>
        Dictionary<string, object> ObtenerEstadisticasCache();
    }
} 