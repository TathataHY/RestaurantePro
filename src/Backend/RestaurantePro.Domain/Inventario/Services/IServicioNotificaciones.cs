namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones del sistema
    /// </summary>
    public interface IServicioNotificaciones
    {
        /// <summary>
        /// Envía una notificación de stock bajo para un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="stockMinimo">Stock mínimo</param>
        /// <returns>Identificador de la notificación enviada</returns>
        Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo);
        
        /// <summary>
        /// Envía una notificación de orden de compra generada
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <returns>Identificador de la notificación enviada</returns>
        Task<Guid> NotificarOrdenCompraGenerada(Guid ordenCompraId, Guid proveedorId, string nombreProveedor);
        
        /// <summary>
        /// Obtiene las notificaciones pendientes para un destinatario
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario</param>
        /// <returns>Lista de notificaciones pendientes</returns>
        Task<IEnumerable<Notificacion>> ObtenerNotificacionesPendientes(Guid destinatarioId);
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <returns>True si se marcó correctamente, false si no existe</returns>
        Task<bool> MarcarNotificacionComoLeida(Guid notificacionId);
    }
} 