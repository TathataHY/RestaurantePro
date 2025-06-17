namespace RestaurantePro.Domain.Proveedores.ValueObjects
{
    /// <summary>
    /// Value Object que representa una categoría asignada a un proveedor
    /// </summary>
    public class ProveedorCategoria : ValueObject
    {
        /// <summary>
        /// Categoría del proveedor
        /// </summary>
        public CategoriaProveedor Categoria { get; private set; }
        
        /// <summary>
        /// Porcentaje de descuento acordado para esta categoría
        /// </summary>
        public decimal PorcentajeDescuento { get; private set; }
        
        /// <summary>
        /// Indica si este proveedor es considerado el principal para esta categoría
        /// </summary>
        public bool EsProveedorPrincipal { get; private set; }

        private ProveedorCategoria() {}
        
        /// <summary>
        /// Constructor para crear una categoría de proveedor
        /// </summary>
        /// <param name="categoria">Categoría asignada</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento (0-100)</param>
        /// <param name="esProveedorPrincipal">Si es el proveedor principal para esta categoría</param>
        private ProveedorCategoria(CategoriaProveedor categoria, decimal porcentajeDescuento, bool esProveedorPrincipal)
        {
            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 100", nameof(porcentajeDescuento));
                
            Categoria = categoria;
            PorcentajeDescuento = porcentajeDescuento;
            EsProveedorPrincipal = esProveedorPrincipal;
        }
        
        /// <summary>
        /// Crea una nueva categoría para un proveedor
        /// </summary>
        /// <param name="categoria">Categoría asignada</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento (0-100)</param>
        /// <param name="esProveedorPrincipal">Si es el proveedor principal para esta categoría</param>
        /// <returns>Un nuevo objeto ProveedorCategoria</returns>
        public static ProveedorCategoria Crear(
            CategoriaProveedor categoria, 
            decimal porcentajeDescuento = 0, 
            bool esProveedorPrincipal = false)
        {
            return new ProveedorCategoria(categoria, porcentajeDescuento, esProveedorPrincipal);
        }
        
        /// <summary>
        /// Crea una nueva categoría para un proveedor principal
        /// </summary>
        /// <param name="categoria">Categoría asignada</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento (0-100)</param>
        /// <returns>Un nuevo objeto ProveedorCategoria marcado como principal</returns>
        public static ProveedorCategoria CrearPrincipal(CategoriaProveedor categoria, decimal porcentajeDescuento = 0)
        {
            return new ProveedorCategoria(categoria, porcentajeDescuento, true);
        }
        
        /// <summary>
        /// Crea una copia de esta categoría actualizando si es principal
        /// </summary>
        /// <param name="esProveedorPrincipal">Nuevo valor para esProveedorPrincipal</param>
        /// <returns>Una nueva instancia de ProveedorCategoria</returns>
        public ProveedorCategoria ConEstadoPrincipal(bool esProveedorPrincipal)
        {
            return new ProveedorCategoria(Categoria, PorcentajeDescuento, esProveedorPrincipal);
        }
        
        /// <summary>
        /// Crea una copia de esta categoría actualizando el porcentaje de descuento
        /// </summary>
        /// <param name="porcentajeDescuento">Nuevo porcentaje de descuento</param>
        /// <returns>Una nueva instancia de ProveedorCategoria</returns>
        public ProveedorCategoria ConPorcentajeDescuento(decimal porcentajeDescuento)
        {
            return new ProveedorCategoria(Categoria, porcentajeDescuento, EsProveedorPrincipal);
        }
        
        /// <summary>
        /// Determina la igualdad comparando las propiedades de los objetos
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Categoria;
            yield return PorcentajeDescuento;
            yield return EsProveedorPrincipal;
        }
    }
} 