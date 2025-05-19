namespace RestaurantePro.Domain.Core.Notificaciones.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones del sistema
    /// </summary>
    public class ServicioNotificaciones : IServicioNotificaciones
    {
        private readonly INotificacionRepository _notificacionRepository;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public ServicioNotificaciones(
            INotificacionRepository notificacionRepository)
        {
            _notificacionRepository = notificacionRepository ?? throw new ArgumentNullException(nameof(notificacionRepository));
        }

        /// <summary>
        /// Envía una notificación al destinatario especificado
        /// </summary>
        public async Task<Notificacion> EnviarNotificacionAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            Guid destinatarioId,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            var notificacion = Notificacion.Crear(
                titulo,
                mensaje,
                tipo,
                destinatarioId,
                entidadRelacionadaId);
                
            await _notificacionRepository.AddAsync(notificacion, cancellationToken);
            return notificacion;
        }
        
        /// <summary>
        /// Envía una notificación a múltiples destinatarios
        /// </summary>
        public async Task<IEnumerable<Notificacion>> EnviarNotificacionMasivaAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            IEnumerable<Guid> destinatarioIds,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            if (destinatarioIds == null || !destinatarioIds.Any())
                throw new ArgumentException("Debe especificar al menos un destinatario", nameof(destinatarioIds));
                
            var notificaciones = new List<Notificacion>();
            
            foreach (var destinatarioId in destinatarioIds)
            {
                var notificacion = await EnviarNotificacionAsync(
                    titulo,
                    mensaje,
                    tipo,
                    destinatarioId,
                    entidadRelacionadaId,
                    cancellationToken);
                    
                notificaciones.Add(notificacion);
            }
            
            return notificaciones;
        }
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<bool> MarcarComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default)
        {
            var notificacion = await _notificacionRepository.GetByIdAsync(notificacionId, cancellationToken);
            if (notificacion == null)
                return false;
                
            if (notificacion.EstaLeida)
                return true;
                
            notificacion.MarcarComoLeida();
            await _notificacionRepository.UpdateAsync(notificacion, cancellationToken);
            
            return true;
        }
        
        /// <summary>
        /// Obtiene las notificaciones de un destinatario
        /// </summary>
        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(
            Guid destinatarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default)
        {
            if (soloNoLeidas)
            {
                return await _notificacionRepository.GetUnreadByRecipientIdAsync(
                    destinatarioId, 
                    cancellationToken);
            }
            else
            {
                return await _notificacionRepository.GetByRecipientIdAsync(
                    destinatarioId, 
                    cancellationToken);
            }
        }
    }
} 