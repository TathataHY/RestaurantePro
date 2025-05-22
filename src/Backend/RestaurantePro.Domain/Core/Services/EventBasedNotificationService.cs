
namespace RestaurantePro.Domain.Core.Services
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
    
    /// <summary>
    /// Interfaz del servicio de notificaciones basadas en eventos de dominio
    /// </summary>
    public interface IEventBasedNotificationService
    {
        /// <summary>
        /// Suscribe un manejador de eventos para generar notificaciones cuando ocurra un evento específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento al que suscribirse</typeparam>
        /// <param name="nombreCanal">Nombre del canal de notificación (email, sms, push, etc.)</param>
        /// <param name="generadorNotificacion">Función que genera una notificación a partir del evento</param>
        /// <param name="selectorDestinatarios">Función que selecciona los destinatarios a partir del evento</param>
        void SuscribirseAEvento<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificacion>> generadorNotificacion, 
            Func<TEvent, Task<IEnumerable<Guid>>> selectorDestinatarios) 
            where TEvent : DomainEvent;
        
        /// <summary>
        /// Suscribe un manejador de eventos para generar notificaciones para usuarios con un rol específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento al que suscribirse</typeparam>
        /// <param name="nombreCanal">Nombre del canal de notificación (email, sms, push, etc.)</param>
        /// <param name="generadorNotificacion">Función que genera una notificación a partir del evento</param>
        /// <param name="rolNecesario">Rol que deben tener los usuarios para recibir la notificación</param>
        void SuscribirseAEventoPorRol<TEvent>(
            string nombreCanal, 
            Func<TEvent, Task<Notificacion>> generadorNotificacion, 
            RolUsuario rolNecesario) 
            where TEvent : DomainEvent;
        
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario destinatario</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="contenido">Contenido de la notificación</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se envió correctamente, False si no se encontró el usuario</returns>
        Task<bool> EnviarNotificacionAUsuarioAsync(
            Guid usuarioId, 
            string titulo, 
            string contenido, 
            TipoNotificacion tipo, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Envía una notificación a todos los usuarios con un rol específico
        /// </summary>
        /// <param name="rol">Rol de los usuarios destinatarios</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="contenido">Contenido de la notificación</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EnviarNotificacionAUsuariosConRolAsync(
            RolUsuario rol, 
            string titulo, 
            string contenido, 
            TipoNotificacion tipo, 
            CancellationToken cancellationToken = default);
    }
} 
