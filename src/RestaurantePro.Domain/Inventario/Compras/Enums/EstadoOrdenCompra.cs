namespace RestaurantePro.Domain.Enums
{
    public enum EstadoOrdenCompra
    {
        /// <summary>
        /// Orden en borrador, aún no enviada
        /// </summary>
        Borrador = 0,
        
        /// <summary>
        /// Orden enviada al proveedor
        /// </summary>
        Enviada = 1,
        
        /// <summary>
        /// Orden confirmada por el proveedor
        /// </summary>
        Confirmada = 2,
        
        /// <summary>
        /// Orden parcialmente recibida
        /// </summary>
        RecibidaParcial = 3,
        
        /// <summary>
        /// Orden completamente recibida
        /// </summary>
        Completada = 4,
        
        /// <summary>
        /// Orden cancelada
        /// </summary>
        Cancelada = 5,
        
        /// <summary>
        /// Orden devuelta (total o parcialmente)
        /// </summary>
        Devuelta = 6
    }
} 