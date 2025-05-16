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
        /// <returns>La entidad si existe, null en caso contrario</returns>
        Task<T> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Obtiene todas las entidades del repositorio
        /// </summary>
        /// <returns>Lista de entidades</returns>
        Task<IReadOnlyList<T>> GetAllAsync();
        
        /// <summary>
        /// Obtiene entidades que cumplen con una especificación
        /// </summary>
        /// <param name="spec">Especificación a aplicar</param>
        /// <returns>Lista de entidades que cumplen la especificación</returns>
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> spec);
        
        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <returns>La entidad agregada</returns>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad con los cambios aplicados</param>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Cuenta el número de entidades que cumplen con una especificación
        /// </summary>
        /// <param name="spec">Especificación a aplicar</param>
        /// <returns>Cantidad de entidades que cumplen la especificación</returns>
        Task<int> CountAsync(ISpecification<T> spec);
    }
} 