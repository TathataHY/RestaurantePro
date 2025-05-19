namespace RestaurantePro.Domain.Inventario.Ingredientes.Enums
{
    /// <summary>
    /// Representa el nivel de rotación de un ingrediente en inventario.
    /// Se utiliza para priorizar ingredientes en las políticas de stock
    /// y para la generación de órdenes de compra.
    /// </summary>
    public enum RotacionIngrediente
    {
        /// <summary>
        /// Ingrediente con rotación baja (movimiento lento en inventario)
        /// </summary>
        Baja = 0,
        
        /// <summary>
        /// Ingrediente con rotación media
        /// </summary>
        Media = 1,
        
        /// <summary>
        /// Ingrediente con rotación alta (movimiento rápido en inventario)
        /// </summary>
        Alta = 2,
        
        /// <summary>
        /// Ingrediente crítico con rotación muy alta
        /// </summary>
        Critica = 3
    }
} 