using RestaurantePro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con usuarios
    /// </summary>
    public interface IUsuarioRepository
    {
        /// <summary>
        /// Busca un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado o null si no existe</returns>
        Task<Usuario> GetByIdAsync(int id);

        /// <summary>
        /// Busca un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Usuario encontrado o null si no existe</returns>
        Task<Usuario> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        Task<List<Usuario>> GetAllAsync();

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="rol">Rol del usuario</param>
        /// <returns>Lista de usuarios</returns>
        Task<List<Usuario>> GetByRolAsync(string rol);

        /// <summary>
        /// Agrega un nuevo usuario
        /// </summary>
        /// <param name="usuario">Usuario a agregar</param>
        Task AddAsync(Usuario usuario);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="usuario">Usuario a actualizar</param>
        Task UpdateAsync(Usuario usuario);

        /// <summary>
        /// Verifica si un email ya está registrado
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <returns>True si el email ya está registrado, false en caso contrario</returns>
        Task<bool> EmailExistsAsync(string email);
    }
} 