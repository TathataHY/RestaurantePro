namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Interfaz para el servicio de notificaciones específico para el módulo de Inventario
    /// </summary>
    public interface IServicioNotificacionesInventario : Core.Notificaciones.Services.IServicioNotificaciones
    {
        /// <summary>
        /// Notifica stock bajo de un ingrediente a los administradores
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="stockMinimo">Stock mínimo establecido</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID de la notificación generada</returns>
        Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Notifica la generación de una orden de compra automática
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID de la notificación generada</returns>
        Task<Guid> NotificarOrdenCompraGenerada(Guid ordenCompraId, Guid proveedorId, string nombreProveedor, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las notificaciones pendientes (no leídas) de un destinatario
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de notificaciones pendientes</returns>
        Task<IEnumerable<Core.Notificaciones.Entities.Notificacion>> ObtenerNotificacionesPendientes(Guid destinatarioId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        Task<bool> MarcarNotificacionComoLeida(Guid notificacionId, CancellationToken cancellationToken = default);
    }
} 