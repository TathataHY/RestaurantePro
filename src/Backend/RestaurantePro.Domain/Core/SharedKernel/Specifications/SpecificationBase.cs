namespace RestaurantePro.Domain.Core.SharedKernel.Specifications
{
    /// <summary>
    /// Clase base abstracta para todas las especificaciones que implementa
    /// las operaciones básicas (AND, OR, NOT) definidas en ISpecification.
    /// 
    /// Patrón: Specification
    /// 
    /// El patrón Specification permite encapsular reglas de negocio como objetos
    /// que pueden combinarse mediante operaciones lógicas. Cada especificación
    /// evalúa si una entidad cumple con ciertos criterios.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a la que se aplica la especificación</typeparam>
    public abstract class SpecificationBase<T> : ISpecification<T>
    {
        /// <summary>
        /// Verifica si una entidad satisface esta especificación
        /// </summary>
        /// <param name="entity">Entidad a evaluar</param>
        /// <returns>True si la entidad cumple la especificación, False en caso contrario</returns>
        public abstract bool IsSatisfiedBy(T entity);

        /// <summary>
        /// Combina esta especificación con otra usando AND lógico
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Nueva especificación que es AND de las dos</returns>
        public ISpecification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        /// <summary>
        /// Combina esta especificación con otra usando OR lógico
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Nueva especificación que es OR de las dos</returns>
        public ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        /// <summary>
        /// Niega esta especificación (NOT lógico)
        /// </summary>
        /// <returns>Nueva especificación que es la negación de esta</returns>
        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }

    /// <summary>
    /// Especificación que representa la operación AND entre dos especificaciones
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a la que se aplica la especificación</typeparam>
    public class AndSpecification<T> : SpecificationBase<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T entity)
        {
            return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
        }
    }

    /// <summary>
    /// Especificación que representa la operación OR entre dos especificaciones
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a la que se aplica la especificación</typeparam>
    public class OrSpecification<T> : SpecificationBase<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T entity)
        {
            return _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
        }
    }

    /// <summary>
    /// Especificación que representa la negación (NOT) de otra especificación
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a la que se aplica la especificación</typeparam>
    public class NotSpecification<T> : SpecificationBase<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification;
        }

        public override bool IsSatisfiedBy(T entity)
        {
            return !_specification.IsSatisfiedBy(entity);
        }
    }
} 