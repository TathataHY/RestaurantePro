
namespace RestaurantePro.Domain.Core.Productos.Specifications
{
    /// <summary>
    /// Especificación que verifica si un producto está disponible para ser incluido en una comanda.
    /// Un producto está disponible si:
    /// 1. Está activo
    /// 2. Tiene un precio válido mayor que cero
    /// 3. Pertenece a una categoría activa (opcional, según configuración)
    /// </summary>
    public class ProductoDisponibleSpecification : Specification<Producto>
    {
        private readonly bool _verificarCategoriaActiva;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="verificarCategoriaActiva">Indica si debe verificarse también que la categoría del producto esté activa</param>
        public ProductoDisponibleSpecification(bool verificarCategoriaActiva = true)
        {
            _verificarCategoriaActiva = verificarCategoriaActiva;
        }
        
        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Producto, bool>> ToExpression()
        {
            return producto => 
                producto.EstaActivo &&
                producto.Precio != null &&
                producto.Precio.Valor > 0 &&
                (!_verificarCategoriaActiva || (producto.CategoriaNombre != null && producto.CategoriaId != Guid.Empty));
        }
    }
} 
