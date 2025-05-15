using RestaurantePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz genérica para operaciones básicas de repositorio
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que hereda de BaseEntity</typeparam>
    public interface IBaseRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null si no existe</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de entidades</returns>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Obtiene entidades que cumplan con una condición específica
        /// </summary>
        /// <param name="predicate">Expresión de filtrado</param>
        /// <returns>Lista de entidades que cumplen la condición</returns>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <returns>Entidad agregada</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <returns>Entidad actualizada</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Elimina lógicamente una entidad (cambia IsDeleted a true)
        /// </summary>
        /// <param name="id">ID de la entidad a eliminar</param>
        /// <returns>True si se eliminó correctamente, false si no se encontró</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Elimina físicamente una entidad de la base de datos
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> HardDeleteAsync(T entity);
    }
} 