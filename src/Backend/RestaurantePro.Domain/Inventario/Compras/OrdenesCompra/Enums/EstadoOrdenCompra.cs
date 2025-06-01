namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums
{
    /// <summary>
    /// Estados posibles de una orden de compra
    /// </summary>
    public enum EstadoOrdenCompra
    {
        /// <summary>
        /// La orden está creada pero aún no ha sido enviada al proveedor
        /// </summary>
        Pendiente = 0,
        
        /// <summary>
        /// La orden ha sido enviada al proveedor y se está esperando su recepción
        /// </summary>
        Enviada = 1,
        
        /// <summary>
        /// La orden ha sido recibida completamente
        /// </summary>
        Recibida = 2,
        
        /// <summary>
        /// La orden ha sido recibida parcialmente
        /// </summary>
        RecibidaParcial = 3,
        
        /// <summary>
        /// La orden ha sido cancelada
        /// </summary>
        Cancelada = 4,
        
        /// <summary>
        /// La orden ha sido confirmada por el proveedor
        /// </summary>
        Confirmada = 5,
        
        /// <summary>
        /// La orden está en tránsito hacia el destino
        /// </summary>
        EnTransito = 6,
        
        /// <summary>
        /// Alias de Pendiente - La orden está en estado borrador
        /// </summary>
        Borrador = Pendiente,
        
        /// <summary>
        /// Alias de Recibida - La orden está completada
        /// </summary>
        Completada = Recibida
    }
} 
