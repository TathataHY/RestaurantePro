namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz genérica para repositorios de entidades
    /// Esta interfaz define operaciones comunes que cualquier repositorio debe implementar.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad que maneja el repositorio</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La entidad si existe, null en caso contrario</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de entidades</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene entidades con paginación
        /// </summary>
        /// <param name="page">Número de página (base 0)</param>
        /// <param name="pageSize">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con entidades y conteo total</returns>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de entidades que cumplen la condición</returns>
        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si existe alguna entidad que cumple una condición
        /// </summary>
        /// <param name="predicate">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumple la condición, False en caso contrario</returns>
        Task<bool> AnyAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Cuenta la cantidad de entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cantidad de entidades que cumplen la condición</returns>
        Task<int> CountAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene la primera entidad que cumple una condición
        /// </summary>
        /// <param name="predicate">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La primera entidad que cumple la condición o null si no existe</returns>
        Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda los cambios y publica eventos de dominio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades modificadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 