namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz para especificaciones genéricas utilizando el patrón Specification
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a la que se aplica la especificación</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Verifica si una entidad satisface esta especificación
        /// </summary>
        /// <param name="entity">Entidad a evaluar</param>
        /// <returns>True si la entidad cumple la especificación, False en caso contrario</returns>
        bool IsSatisfiedBy(T entity);
        
        /// <summary>
        /// Combina esta especificación con otra usando AND lógico
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Nueva especificación que es AND de las dos</returns>
        ISpecification<T> And(ISpecification<T> other);
        
        /// <summary>
        /// Combina esta especificación con otra usando OR lógico
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Nueva especificación que es OR de las dos</returns>
        ISpecification<T> Or(ISpecification<T> other);
        
        /// <summary>
        /// Niega esta especificación (NOT lógico)
        /// </summary>
        /// <returns>Nueva especificación que es la negación de esta</returns>
        ISpecification<T> Not();
    }
} 