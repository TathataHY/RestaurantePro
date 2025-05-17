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
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="stockMinimo">Stock mínimo</param>
        /// <returns>Identificador de la notificación enviada</returns>
        Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo);
        
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
        
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        /// <param name="destinatarioId">ID del usuario destinatario</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Contenido de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EnviarNotificacionAsync(Guid destinatarioId, string titulo, string mensaje, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Envía una notificación a múltiples usuarios
        /// </summary>
        /// <param name="destinatariosIds">Lista de IDs de los usuarios destinatarios</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Contenido de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EnviarNotificacionMasivaAsync(IEnumerable<Guid> destinatariosIds, string titulo, string mensaje, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task MarcarComoLeidaAsync(Guid notificacionId, CancellationToken cancellationToken = default);
    }
} 