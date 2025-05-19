namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones específico para inventario
    /// utilizando el servicio central de notificaciones
    /// </summary>
    public class ServicioNotificacionesInventario : IServicioNotificacionesInventario
    {
        private readonly Core.Notificaciones.Services.IServicioNotificaciones _servicioNotificacionesCore;
        private readonly IProveedorRepository _proveedorRepository;
        
        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public ServicioNotificacionesInventario(
            Core.Notificaciones.Services.IServicioNotificaciones servicioNotificacionesCore,
            IProveedorRepository proveedorRepository)
        {
            _servicioNotificacionesCore = servicioNotificacionesCore ?? throw new ArgumentNullException(nameof(servicioNotificacionesCore));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
        }
        
        /// <inheritdoc />
        public async Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo)
        {
            // Obtener los destinatarios (administradores del sistema)
            var administradores = await ObtenerAdministradoresAsync();
            
            if (!administradores.Any())
                throw new InvalidOperationException("No hay administradores configurados para recibir notificaciones");
            
            var titulo = $"Stock bajo: {nombre}";
            var mensaje = $"El ingrediente {nombre} tiene un stock actual de {stockActual} unidades, por debajo del mínimo recomendado ({stockMinimo} unidades).";
            
            // Usar el servicio Core para enviar la notificación masiva
            var notificaciones = await _servicioNotificacionesCore.EnviarNotificacionMasivaAsync(
                titulo,
                mensaje,
                TipoNotificacion.StockBajo,
                administradores,
                ingredienteId);
                
            // Devolver el ID de la primera notificación
            return notificaciones.First().Id;
        }
        
        /// <inheritdoc />
        public async Task<Guid> NotificarOrdenCompraGenerada(Guid ordenCompraId, Guid proveedorId, string nombreProveedor)
        {
            // Obtener los destinatarios (administradores del sistema)
            var administradores = await ObtenerAdministradoresAsync();
            
            if (!administradores.Any())
                throw new InvalidOperationException("No hay administradores configurados para recibir notificaciones");
            
            var titulo = $"Orden de compra generada: {nombreProveedor}";
            var mensaje = $"Se ha generado automáticamente una orden de compra para el proveedor {nombreProveedor} debido a stock bajo.";
            
            // Usar el servicio Core para enviar la notificación masiva
            var notificaciones = await _servicioNotificacionesCore.EnviarNotificacionMasivaAsync(
                titulo,
                mensaje,
                TipoNotificacion.OrdenCompraGenerada,
                administradores,
                ordenCompraId);
                
            // Devolver el ID de la primera notificación
            return notificaciones.First().Id;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesPendientes(Guid destinatarioId)
        {
            return await _servicioNotificacionesCore.ObtenerNotificacionesAsync(destinatarioId, true);
        }
        
        /// <inheritdoc />
        public async Task<bool> MarcarNotificacionComoLeida(Guid notificacionId)
        {
            return await _servicioNotificacionesCore.MarcarComoLeidaAsync(notificacionId);
        }
        
        // Implementación de IServicioNotificaciones
        
        /// <inheritdoc />
        public async Task<Notificacion> EnviarNotificacionAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            Guid destinatarioId,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            return await _servicioNotificacionesCore.EnviarNotificacionAsync(
                titulo,
                mensaje,
                tipo,
                destinatarioId,
                entidadRelacionadaId,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Notificacion>> EnviarNotificacionMasivaAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            IEnumerable<Guid> destinatarioIds,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            return await _servicioNotificacionesCore.EnviarNotificacionMasivaAsync(
                titulo,
                mensaje,
                tipo,
                destinatarioIds,
                entidadRelacionadaId,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<bool> MarcarComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default)
        {
            return await _servicioNotificacionesCore.MarcarComoLeidaAsync(
                notificacionId, 
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(
            Guid destinatarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default)
        {
            return await _servicioNotificacionesCore.ObtenerNotificacionesAsync(
                destinatarioId, 
                soloNoLeidas, 
                cancellationToken);
        }
        
        // Método privado para obtener los IDs de los administradores
        private async Task<List<Guid>> ObtenerAdministradoresAsync()
        {
            // En un entorno real, se obtendría de la base de datos o de un servicio de usuarios
            // Por ahora, devolvemos un ID fijo para simular al menos un administrador
            return new List<Guid> { Guid.Parse("11111111-1111-1111-1111-111111111111") };
        }
    }
} 