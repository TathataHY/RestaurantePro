namespace RestaurantePro.Domain.Core.Usuarios.Interfaces
{
    /// <summary>
    /// Repositorio para gestión de roles de usuario.
    /// </summary>
    public interface IRolRepository
    {
        /// <summary>
        /// Obtiene un rol por su identificador único.
        /// </summary>
        /// <param name="id">Identificador único del rol.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El rol solicitado o null si no se encuentra.</returns>
        Task<Rol?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene el rol predeterminado para nuevos usuarios (normalmente Empleado).
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El rol predeterminado o null si no existe.</returns>
        Task<Rol?> ObtenerPredeterminadoAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene el rol correspondiente a un tipo específico de usuario.
        /// </summary>
        /// <param name="tipoUsuario">Tipo de usuario (Administrador, Empleado, etc.)</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El rol correspondiente al tipo de usuario o null si no existe.</returns>
        Task<Rol?> ObtenerPorTipoUsuarioAsync(TipoUsuario tipoUsuario, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los roles disponibles en el sistema.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de todos los roles en el sistema.</returns>
        Task<IReadOnlyList<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un nuevo rol al sistema.
        /// </summary>
        /// <param name="rol">Rol a agregar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task AgregarAsync(Rol rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un rol existente.
        /// </summary>
        /// <param name="rol">Rol con los cambios a actualizar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task ActualizarAsync(Rol rol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un rol por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del rol a eliminar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los roles que cumplan con la especificación
        /// </summary>
        /// <param name="spec">Especificación a aplicar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles que cumplen la especificación</returns>
        Task<IReadOnlyList<Rol>> ObtenerPorSpecAsync(Core.SharedKernel.Interfaces.ISpecification<Rol> spec, CancellationToken cancellationToken = default);
    }
} 