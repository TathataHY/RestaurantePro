using System;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Core.SharedKernel.Specifications
{
    /// <summary>
    /// Interfaz base para el patrón Specification
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a evaluar</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Expresión que define la especificación
        /// </summary>
        Expression<Func<T, bool>> ToExpression();
        
        /// <summary>
        /// Evalúa si una entidad satisface la especificación
        /// </summary>
        /// <param name="entity">Entidad a evaluar</param>
        /// <returns>True si satisface la especificación, False en caso contrario</returns>
        bool IsSatisfiedBy(T entity);
    }
    
    /// <summary>
    /// Clase base para implementar especificaciones
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a evaluar</typeparam>
    public abstract class Specification<T> : ISpecification<T>
    {
        /// <summary>
        /// Expresión que define la especificación
        /// </summary>
        public abstract Expression<Func<T, bool>> ToExpression();
        
        /// <summary>
        /// Evalúa si una entidad satisface la especificación
        /// </summary>
        /// <param name="entity">Entidad a evaluar</param>
        /// <returns>True si satisface la especificación, False en caso contrario</returns>
        public virtual bool IsSatisfiedBy(T entity)
        {
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }
        
        /// <summary>
        /// Combina esta especificación con otra usando el operador AND
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Una nueva especificación que representa la combinación AND</returns>
        public Specification<T> And(Specification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }
        
        /// <summary>
        /// Combina esta especificación con otra usando el operador OR
        /// </summary>
        /// <param name="other">Otra especificación</param>
        /// <returns>Una nueva especificación que representa la combinación OR</returns>
        public Specification<T> Or(Specification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }
        
        /// <summary>
        /// Niega esta especificación
        /// </summary>
        /// <returns>Una nueva especificación que representa la negación</returns>
        public Specification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }
    
    /// <summary>
    /// Especificación compuesta AND
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a evaluar</typeparam>
    internal sealed class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;
        
        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }
        
        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            
            var paramExpr = Expression.Parameter(typeof(T));
            var leftVisitor = new ParameterReplacerVisitor(leftExpression.Parameters[0], paramExpr);
            var rightVisitor = new ParameterReplacerVisitor(rightExpression.Parameters[0], paramExpr);
            
            var leftBody = leftVisitor.Visit(leftExpression.Body);
            var rightBody = rightVisitor.Visit(rightExpression.Body);
            
            var andExpression = Expression.AndAlso(leftBody, rightBody);
            
            return Expression.Lambda<Func<T, bool>>(andExpression, paramExpr);
        }
    }
    
    /// <summary>
    /// Especificación compuesta OR
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a evaluar</typeparam>
    internal sealed class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;
        
        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }
        
        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            
            var paramExpr = Expression.Parameter(typeof(T));
            var leftVisitor = new ParameterReplacerVisitor(leftExpression.Parameters[0], paramExpr);
            var rightVisitor = new ParameterReplacerVisitor(rightExpression.Parameters[0], paramExpr);
            
            var leftBody = leftVisitor.Visit(leftExpression.Body);
            var rightBody = rightVisitor.Visit(rightExpression.Body);
            
            var orExpression = Expression.OrElse(leftBody, rightBody);
            
            return Expression.Lambda<Func<T, bool>>(orExpression, paramExpr);
        }
    }
    
    /// <summary>
    /// Especificación de negación
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a evaluar</typeparam>
    internal sealed class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _specification;
        
        public NotSpecification(Specification<T> specification)
        {
            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }
        
        public override Expression<Func<T, bool>> ToExpression()
        {
            var expression = _specification.ToExpression();
            var notExpression = Expression.Not(expression.Body);
            
            return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters);
        }
    }
    
    /// <summary>
    /// Visitante de expresiones que reemplaza parámetros
    /// </summary>
    internal class ParameterReplacerVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        
        public ParameterReplacerVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
} 