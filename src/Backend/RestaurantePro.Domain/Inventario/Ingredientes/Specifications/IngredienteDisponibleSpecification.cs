using System;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Inventario.Ingredientes.Specifications
{
    /// <summary>
    /// Especificación que verifica si un ingrediente está disponible para ser utilizado
    /// en la preparación de productos.
    /// 
    /// Un ingrediente está disponible si:
    /// 1. Está activo (no eliminado lógicamente)
    /// 2. Tiene stock mayor que cero
    /// 3. No está bloqueado por control de calidad (opcional, según configuración)
    /// </summary>
    public class IngredienteDisponibleSpecification : Core.SharedKernel.Specifications.Specification<Entities.Ingrediente>
    {
        private readonly bool _verificarControlCalidad;
        private readonly decimal _cantidadMinima;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="verificarControlCalidad">Indica si debe verificarse que el ingrediente no esté bloqueado por control de calidad</param>
        /// <param name="cantidadMinima">Cantidad mínima de stock para considerar que el ingrediente está disponible</param>
        public IngredienteDisponibleSpecification(bool verificarControlCalidad = false, decimal cantidadMinima = 0.01m)
        {
            _verificarControlCalidad = verificarControlCalidad;
            _cantidadMinima = cantidadMinima;
        }
        
        /// <summary>
        /// Verifica si un ingrediente está disponible para uso
        /// </summary>
        public override Expression<Func<Entities.Ingrediente, bool>> ToExpression()
        {
            return ingrediente => 
                ingrediente != null &&
                !ingrediente.EstaEliminado && 
                ingrediente.Stock > _cantidadMinima &&
                (!_verificarControlCalidad || !ingrediente.BloqueadoControlCalidad);
        }

        /// <summary>
        /// Sobrescribe el método IsSatisfiedBy de la clase base para manejar el caso null explícitamente
        /// </summary>
        public override bool IsSatisfiedBy(Entities.Ingrediente entity)
        {
            if (entity == null)
                return false;
                
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }
    }
} 