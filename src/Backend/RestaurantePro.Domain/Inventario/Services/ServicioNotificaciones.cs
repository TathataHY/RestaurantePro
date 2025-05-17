namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones del sistema
    /// </summary>
    public class ServicioNotificaciones : IServicioNotificaciones
    {
        private readonly INotificacionRepository _notificacionRepository;
        private readonly IProveedorRepository _proveedorRepository;
        
        /// <summary>
        /// Constructor del servicio de notificaciones
        /// </summary>
        public ServicioNotificaciones(
            INotificacionRepository notificacionRepository,
            IProveedorRepository proveedorRepository)
        {
            _notificacionRepository = notificacionRepository ?? throw new ArgumentNullException(nameof(notificacionRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
        }
        
        /// <inheritdoc />
        public async Task<Guid> NotificarStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo)
        {
            // Obtener los destinatarios (administradores del sistema)
            var administradores = await ObtenerAdministradoresAsync();
            
            if (!administradores.Any())
                throw new InvalidOperationException("No hay administradores configurados para recibir notificaciones");
            
            var notificaciones = new List<Notificacion>();
            
            // Para cada administrador, crear una notificación
            foreach (var adminId in administradores)
            {
                var titulo = $"Stock bajo: {nombre}";
                var mensaje = $"El ingrediente {nombre} tiene un stock actual de {stockActual} unidades, por debajo del mínimo recomendado ({stockMinimo} unidades).";
                
                var notificacion = Notificacion.Crear(
                    titulo,
                    mensaje,
                    TipoNotificacion.StockBajo,
                    adminId,
                    ingredienteId);
                    
                notificaciones.Add(notificacion);
            }
            
            // Guardar todas las notificaciones
            foreach (var notificacion in notificaciones)
            {
                await _notificacionRepository.AddAsync(notificacion);
            }
            
            // Devolver el ID de la primera notificación (para la primera persona notificada)
            return notificaciones.First().Id;
        }
        
        /// <inheritdoc />
        public async Task<Guid> NotificarOrdenCompraGenerada(Guid ordenCompraId, Guid proveedorId, string nombreProveedor)
        {
            // Obtener los destinatarios (administradores del sistema)
            var administradores = await ObtenerAdministradoresAsync();
            
            if (!administradores.Any())
                throw new InvalidOperationException("No hay administradores configurados para recibir notificaciones");
            
            var notificaciones = new List<Notificacion>();
            
            // Para cada administrador, crear una notificación
            foreach (var adminId in administradores)
            {
                var titulo = $"Orden de compra generada: {nombreProveedor}";
                var mensaje = $"Se ha generado automáticamente una orden de compra para el proveedor {nombreProveedor} debido a stock bajo.";
                
                var notificacion = Notificacion.Crear(
                    titulo,
                    mensaje,
                    TipoNotificacion.OrdenCompraGenerada,
                    adminId,
                    ordenCompraId);
                    
                notificaciones.Add(notificacion);
            }
            
            // Guardar todas las notificaciones
            foreach (var notificacion in notificaciones)
            {
                await _notificacionRepository.AddAsync(notificacion);
            }
            
            // Devolver el ID de la primera notificación (para la primera persona notificada)
            return notificaciones.First().Id;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesPendientes(Guid destinatarioId)
        {
            return await _notificacionRepository.ObtenerPendientesPorDestinatarioAsync(destinatarioId);
        }
        
        /// <inheritdoc />
        public async Task<bool> MarcarNotificacionComoLeida(Guid notificacionId)
        {
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId);
            
            if (notificacion == null)
                return false;
                
            notificacion.MarcarComoLeida();
            await _notificacionRepository.UpdateAsync(notificacion);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task EnviarNotificacionAsync(Guid destinatarioId, string titulo, string mensaje, CancellationToken cancellationToken = default)
        {
            var notificacion = Notificacion.Crear(
                titulo,
                mensaje,
                TipoNotificacion.Personalizada,
                destinatarioId,
                null);
                
            await _notificacionRepository.AddAsync(notificacion, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task EnviarNotificacionMasivaAsync(IEnumerable<Guid> destinatariosIds, string titulo, string mensaje, CancellationToken cancellationToken = default)
        {
            var notificaciones = new List<Notificacion>();
            
            foreach (var destinatarioId in destinatariosIds)
            {
                var notificacion = Notificacion.Crear(
                    titulo,
                    mensaje,
                    TipoNotificacion.Personalizada,
                    destinatarioId,
                    null);
                    
                notificaciones.Add(notificacion);
            }
            
            foreach (var notificacion in notificaciones)
            {
                await _notificacionRepository.AddAsync(notificacion, cancellationToken);
            }
        }
        
        /// <inheritdoc />
        public async Task MarcarComoLeidaAsync(Guid notificacionId, CancellationToken cancellationToken = default)
        {
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId, cancellationToken);
            
            if (notificacion != null)
            {
                notificacion.MarcarComoLeida();
                await _notificacionRepository.UpdateAsync(notificacion, cancellationToken);
            }
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