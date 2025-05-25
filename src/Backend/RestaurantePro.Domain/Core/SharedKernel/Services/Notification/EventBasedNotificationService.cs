namespace RestaurantePro.Domain.Core.SharedKernel.Services.Notification
{
    /// <summary>
    /// Servicio que maneja notificaciones basadas en eventos de dominio
    /// </summary>
    public class EventBasedNotificationService : IEventBasedNotificationService
    {
        private readonly IEventSubscriptionManager _subscriptionManager;
        private readonly IServicioNotificaciones _servicioNotificaciones;
        private readonly IUsuarioRepository _usuarioRepository;
        
        public EventBasedNotificationService(
            IEventSubscriptionManager subscriptionManager,
            IServicioNotificaciones servicioNotificaciones,
            IUsuarioRepository usuarioRepository)
        {
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        }
        
        /// <inheritdoc />
        public void SuscribirseAEvento<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificacion>> generadorNotificacion, 
            Func<TEvent, Task<IEnumerable<Guid>>> selectorDestinatarios) 
            where TEvent : DomainEvent
        {
            // Crear suscripción al evento
            _subscriptionManager.Subscribe<TEvent>(async (evento, cancellationToken) =>
            {
                try
                {
                    // Solo procesar eventos del tipo esperado
                    if (evento is TEvent eventoTipado)
                    {
                        // Generar notificación a partir del evento
                        var notificacion = await generadorNotificacion(eventoTipado);
                        
                        // Obtener destinatarios para esta notificación
                        var destinatariosIds = await selectorDestinatarios(eventoTipado);
                        var destinatarios = new List<string>();
                        
                        // Convertir IDs de usuario a canales de notificación
                        foreach (var destinatarioId in destinatariosIds)
                        {
                            var destinatario = await _usuarioRepository.ObtenerPorIdAsync(destinatarioId);
                            if (destinatario != null)
                            {
                                destinatarios.Add(ObtenerCanal(nombreCanal, destinatario.Email));
                            }
                        }
                        
                        // No hay destinatarios, no enviar notificación
                        if (destinatarios.Count == 0)
                        {
                            return;
                        }
                        
                        // Enviar notificación a los destinatarios
                        foreach (var destinatarioId in destinatariosIds)
                        {
                            await _servicioNotificaciones.EnviarNotificacionAsync(
                                notificacion.Titulo,
                                notificacion.Mensaje,
                                notificacion.Tipo,
                                destinatarioId,
                                notificacion.EntidadRelacionadaId,
                                cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Loguear error pero no interrumpir el flujo del evento
                    Console.WriteLine($"Error al procesar notificación para evento {typeof(TEvent).Name}: {ex.Message}");
                }
            });
        }
        
        /// <inheritdoc />
        public void SuscribirseAEventoPorRol<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificacion>> generadorNotificacion, 
            RolUsuario rolNecesario) 
            where TEvent : DomainEvent
        {
            _subscriptionManager.Subscribe<TEvent>(async (evento, cancellationToken) =>
            {
                try
                {
                    // Solo procesar eventos del tipo esperado
                    if (evento is TEvent eventoTipado)
                    {
                        // Generar notificación a partir del evento
                        var notificacion = await generadorNotificacion(eventoTipado);
                        
                        // Obtener todos los usuarios con el rol especificado
                        var usuariosConRol = await _usuarioRepository.ObtenerPorRolAsync(rolNecesario);
                        var destinatarios = new List<string>();
                        
                        // Crear canales de notificación para cada usuario
                        foreach (var usuario in usuariosConRol)
                        {
                            destinatarios.Add(ObtenerCanal(nombreCanal, usuario.Email));
                        }
                        
                        // No hay destinatarios, no enviar notificación
                        if (destinatarios.Count == 0)
                        {
                            return;
                        }
                        
                        // Enviar notificación a los destinatarios
                        foreach (var usuario in usuariosConRol)
                        {
                            await _servicioNotificaciones.EnviarNotificacionAsync(
                                notificacion.Titulo,
                                notificacion.Mensaje,
                                notificacion.Tipo,
                                usuario.Id,
                                notificacion.EntidadRelacionadaId,
                                cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Loguear error pero no interrumpir el flujo del evento
                    Console.WriteLine($"Error al procesar notificación para evento {typeof(TEvent).Name}: {ex.Message}");
                }
            });
        }
        
        /// <inheritdoc />
        public async Task<bool> EnviarNotificacionAUsuarioAsync(
            Guid usuarioId, 
            string titulo, 
            string contenido, 
            TipoNotificacion tipo, 
            CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
            {
                return false;
            }
            
            await _servicioNotificaciones.EnviarNotificacionAsync(
                titulo,
                contenido,
                tipo,
                usuarioId,
                null,
                cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task EnviarNotificacionAUsuariosConRolAsync(
            RolUsuario rol, 
            string titulo, 
            string contenido, 
            TipoNotificacion tipo, 
            CancellationToken cancellationToken = default)
        {
            var usuarios = await _usuarioRepository.ObtenerPorRolAsync(rol);
            if (!usuarios.Any())
            {
                return;
            }
            
            var destinatarioIds = usuarios.Select(u => u.Id).ToList();
            
            await _servicioNotificaciones.EnviarNotificacionMasivaAsync(
                titulo,
                contenido,
                tipo,
                destinatarioIds,
                null,
                cancellationToken);
        }
        
        /// <summary>
        /// Obtiene el canal de notificación para un usuario
        /// </summary>
        private string ObtenerCanal(string nombreCanal, string destinatario)
        {
            return $"{nombreCanal}:{destinatario}";
        }
    }
    
    // La interfaz IEventBasedNotificationService se ha movido a un archivo separado
} 
