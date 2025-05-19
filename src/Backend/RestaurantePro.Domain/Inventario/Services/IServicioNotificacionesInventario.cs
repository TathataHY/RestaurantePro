namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones específico para el módulo de Inventario
    /// </summary>
    public interface IServicioNotificacionesInventario : IServicioNotificaciones
    {
        /// <summary>
        /// Notifica stock bajo de un ingrediente a los administradores
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="stockMinimo">Stock mínimo establecido</param>
        /// <returns>ID de la notificación generada</returns>
        new Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo);
        
        /// <summary>
        /// Notifica la generación de una orden de compra automática
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <returns>ID de la notificación generada</returns>
        new Task<Guid> NotificarOrdenCompraGenerada(Guid ordenCompraId, Guid proveedorId, string nombreProveedor);
    }
} 