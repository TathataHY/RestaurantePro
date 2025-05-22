using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación para filtrar promociones aplicables a un producto específico
    /// </summary>
    public class PromocionParaProductoSpecification : Specification<Promocion>
    {
        private readonly Guid _productoId;
        private readonly Guid? _categoriaId;
        
        /// <summary>
        /// Constructor de la especificación
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="categoriaId">ID de la categoría del producto (opcional)</param>
        public PromocionParaProductoSpecification(Guid productoId, Guid? categoriaId = null)
        {
            _productoId = productoId;
            _categoriaId = categoriaId;
        }
        
        /// <summary>
        /// Retorna la expresión que verifica si una promoción es aplicable a un producto específico
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion => 
                // Si no hay productos ni categorías especificadas, la promoción aplica a todos
                (promocion.ProductosAplicablesIds.Count == 0 && promocion.CategoriasAplicablesIds.Count == 0) ||
                // O si el producto está en la lista de productos aplicables
                promocion.ProductosAplicablesIds.Contains(_productoId) ||
                // O si la categoría está en la lista de categorías aplicables
                (_categoriaId.HasValue && promocion.CategoriasAplicablesIds.Contains(_categoriaId.Value));
        }
    }
} 