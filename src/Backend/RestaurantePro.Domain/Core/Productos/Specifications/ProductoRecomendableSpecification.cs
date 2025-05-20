namespace RestaurantePro.Domain.Core.Productos.Specifications
{
    /// <summary>
    /// Especificación que determina si un producto puede ser recomendado
    /// </summary>
    public class ProductoRecomendableSpecification : SpecificationBase<Producto>
    {
        /// <summary>
        /// Verifica si un producto cumple con los criterios para ser recomendable:
        /// - Debe estar activo
        /// - No debe estar marcado como no recomendable
        /// - Debe tener stock disponible (si aplica)
        /// </summary>
        /// <param name="producto">Producto a evaluar</param>
        /// <returns>True si el producto puede ser recomendado</returns>
        public override bool IsSatisfiedBy(Producto producto)
        {
            // Un producto es recomendable si:
            // 1. Está activo
            // 2. Tiene precio válido
            // 3. Pertenece a una categoría válida
            return producto.EstaActivo && 
                   producto.Precio != null && 
                   producto.Precio.Valor > 0 &&
                   producto.CategoriaId != Guid.Empty;
        }
    }
} 