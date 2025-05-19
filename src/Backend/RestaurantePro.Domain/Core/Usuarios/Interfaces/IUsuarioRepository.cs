namespace RestaurantePro.Domain.Core.Usuarios.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de usuarios
    /// </summary>
    public interface IUsuarioRepository
    {
        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario si existe, null en caso contrario</returns>
        Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario si existe, null en caso contrario</returns>
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario si existe, null en caso contrario</returns>
        Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un usuario por su ID de Identity
        /// </summary>
        /// <param name="identityId">ID de Identity</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario si existe, null en caso contrario</returns>
        Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <param name="soloActivos">True para obtener solo usuarios activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios</returns>
        Task<IEnumerable<Usuario>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="rol">Rol a filtrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios con el rol especificado</returns>
        Task<IEnumerable<Usuario>> ObtenerPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un nuevo usuario
        /// </summary>
        /// <param name="usuario">Usuario a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="usuario">Usuario con los cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si existe un usuario con el email especificado
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    }
} 