namespace RestaurantePro.Domain.Inventario.Results
{
    /// <summary>
    /// Clase que representa el resultado de la verificación de stock
    /// </summary>
    public class ResultadoVerificacionStock
    {
        /// <summary>
        /// Órdenes de compra generadas durante la verificación
        /// </summary>
        public List<OrdenCompra> OrdenesGeneradas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Órdenes de compra existentes que fueron actualizadas
        /// </summary>
        public List<OrdenCompra> OrdenesActualizadas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Errores ocurridos durante la verificación
        /// </summary>
        public List<string> Errores { get; } = new List<string>();
    }
} 