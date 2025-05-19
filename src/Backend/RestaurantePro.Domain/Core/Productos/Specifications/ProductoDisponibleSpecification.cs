using RestaurantePro.Domain.Core.SharedKernel.Specifications;

namespace RestaurantePro.Domain.Core.Productos.Specifications
{
    /// <summary>
    /// Especificación que verifica si un producto está disponible para ser incluido en una comanda.
    /// Un producto está disponible si:
    /// 1. Está activo
    /// 2. Tiene un precio válido mayor que cero
    /// 3. Pertenece a una categoría activa (opcional, según configuración)
    /// </summary>
    public class ProductoDisponibleSpecification : SpecificationBase<Producto>
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
        /// Verifica si un producto cumple con los criterios de disponibilidad
        /// </summary>
        /// <param name="producto">Producto a evaluar</param>
        /// <returns>True si el producto está disponible, False en caso contrario</returns>
        public override bool IsSatisfiedBy(Producto producto)
        {
            // Verificación básica de producto
            if (producto == null)
                return false;
                
            // Debe estar activo
            if (!producto.EstaActivo)
                return false;
                
            // Debe tener un precio válido
            if (producto.Precio == null || producto.Precio.Valor <= 0)
                return false;
                
            // Verificación de categoría activa (si se solicitó)
            // Nota: En un contexto real, esto requeriría verificar la categoría 
            // desde un repositorio, pero aquí asumimos que tenemos la información
            // Esto es intencional para mantener la especificación simple y como demo
            if (_verificarCategoriaActiva)
            {
                // En un contexto real, esto requeriría una consulta adicional o
                // incluir la información de la categoría en el producto
                // Para este ejemplo, simulamos esta verificación
                if (producto.CategoriaNombre == "Sin categoría")
                    return false;
            }
            
            return true;
        }
    }
} 