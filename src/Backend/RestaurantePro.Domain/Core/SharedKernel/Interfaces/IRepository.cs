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
        Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de entidades</returns>
        Task<IEnumerable<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarRangoAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene entidades con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con entidades y conteo total</returns>
        Task<(IEnumerable<T> Items, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene entidades que cumplen una condición
        /// </summary>
        /// <param name="predicado">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de entidades que cumplen la condición</returns>
        Task<IEnumerable<T>> BuscarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si existe alguna entidad que cumple una condición
        /// </summary>
        /// <param name="predicado">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumple la condición, False en caso contrario</returns>
        Task<bool> ExisteAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Cuenta la cantidad de entidades que cumplen una condición
        /// </summary>
        /// <param name="predicado">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cantidad de entidades que cumplen la condición</returns>
        Task<int> ContarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene la primera entidad que cumple una condición
        /// </summary>
        /// <param name="predicado">Predicado con la condición</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La primera entidad que cumple la condición o null si no existe</returns>
        Task<T?> PrimeroODefaultAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las entidades que cumplan con la especificación
        /// </summary>
        /// <param name="specification">Especificación a aplicar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Colección de entidades que cumplen la especificación</returns>
        Task<IEnumerable<T>> ObtenerPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Cuenta cuántas entidades cumplen con la especificación
        /// </summary>
        /// <param name="specification">Especificación a aplicar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades que cumplen la especificación</returns>
        Task<int> ContarPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene la primera entidad que cumple con la especificación
        /// </summary>
        /// <param name="specification">Especificación a aplicar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Primera entidad que cumple la especificación o null si no hay ninguna</returns>
        Task<T?> PrimeroODefaultPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda los cambios y publica eventos de dominio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades modificadas</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    }
} 