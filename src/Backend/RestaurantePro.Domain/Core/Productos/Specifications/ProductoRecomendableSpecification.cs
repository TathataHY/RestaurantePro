
namespace RestaurantePro.Domain.Core.Productos.Specifications
{
    /// <summary>
    /// Especificación que determina si un producto puede ser recomendado
    /// </summary>
    public class ProductoRecomendableSpecification : Specification<Producto>
    {
        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Producto, bool>> ToExpression()
        {
            return producto => 
                producto.EstaActivo &&
                producto.Precio != null &&
                producto.Precio.Valor > 0 &&
                (!string.IsNullOrEmpty(producto.CategoriaNombre) && producto.CategoriaId != Guid.Empty);
        }
    }
} 
