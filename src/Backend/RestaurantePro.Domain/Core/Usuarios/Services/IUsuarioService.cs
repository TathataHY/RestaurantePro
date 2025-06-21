using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Domain.Core.Usuarios.Services
{
    /// <summary>
    /// Servicio para la gestión de usuarios a nivel de dominio
    /// </summary>
    public interface IUsuarioService
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
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="rol">Rol a filtrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios con el rol especificado</returns>
        Task<IEnumerable<Usuario>> ObtenerPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <param name="soloActivos">True para obtener solo usuarios activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios</returns>
        Task<IEnumerable<Usuario>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario para login</param>
        /// <param name="nombreCompleto">Nombre completo del usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="rol">Rol inicial del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario creado</returns>
        Task<Usuario> CrearUsuarioAsync(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza los datos de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="nombreCompleto">Nuevo nombre completo (null para no modificar)</param>
        /// <param name="email">Nuevo email (null para no modificar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario actualizado o null si no existe</returns>
        Task<Usuario?> ActualizarUsuarioAsync(Guid id, string? nombreCompleto, string? email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Activa un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se activó correctamente, False si no se encontró el usuario</returns>
        Task<bool> ActivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Desactiva un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se desactivó correctamente, False si no se encontró el usuario</returns>
        Task<bool> DesactivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Bloquea un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="motivo">Motivo del bloqueo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se bloqueó correctamente, False si no se encontró el usuario</returns>
        Task<bool> BloquearUsuarioAsync(Guid id, string motivo, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Desbloquea un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se desbloqueó correctamente, False si no se encontró el usuario</returns>
        Task<bool> DesbloquearUsuarioAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="rol">Rol a asignar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se asignó correctamente, False si no se encontró el usuario</returns>
        Task<bool> AsignarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un rol de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="rol">Rol a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se eliminó correctamente, False si no se encontró el usuario o el rol</returns>
        Task<bool> EliminarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default);
        
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

        /// <summary>
        /// Elimina un usuario (soft delete)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se eliminó correctamente, False si no se encontró el usuario</returns>
        Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cambia el rol de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="nuevoRol">Nuevo rol a asignar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se cambió correctamente, False si no se encontró el usuario</returns>
        Task<bool> CambiarRolAsync(Guid id, RolUsuario nuevoRol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resetea la contraseña de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="nuevaPassword">Nueva contraseña temporal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se reseteó correctamente, False si no se encontró el usuario</returns>
        Task<bool> ResetearPasswordAsync(Guid id, string nuevaPassword, CancellationToken cancellationToken = default);
    }
} 