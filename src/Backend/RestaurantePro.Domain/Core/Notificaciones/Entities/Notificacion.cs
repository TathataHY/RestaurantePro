namespace RestaurantePro.Domain.Core.Notificaciones.Entities
{
    /// <summary>
    /// Entidad que representa una notificación en el sistema
    /// </summary>
    public class Notificacion : EntityBase
    {
        /// <summary>
        /// Título de la notificación
        /// </summary>
        public string Titulo { get; private set; }
        
        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        public string Mensaje { get; private set; }
        
        /// <summary>
        /// Tipo de notificación
        /// </summary>
        public TipoNotificacion Tipo { get; private set; }
        
        /// <summary>
        /// ID del destinatario de la notificación
        /// </summary>
        public Guid DestinatarioId { get; private set; }
        
        /// <summary>
        /// Fecha de creación de la notificación
        /// </summary>
        public new DateTime FechaCreacion { get; private set; }
        
        /// <summary>
        /// Fecha en que se leyó la notificación
        /// </summary>
        public DateTime? FechaLectura { get; private set; }
        
        /// <summary>
        /// ID de la entidad relacionada con la notificación (ej. ID de ingrediente para notificación de stock bajo)
        /// Puede ser nulo para notificaciones personalizadas generales
        /// </summary>
        public Guid? EntidadRelacionadaId { get; private set; }
        
        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        public bool EstaLeida => FechaLectura.HasValue;
        
        // Constructor privado para EF Core
        private Notificacion() { }
        
        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public static Notificacion Crear(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            Guid destinatarioId,
            Guid? entidadRelacionadaId = null,
            DateTime? fechaCreacion = null)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título no puede estar vacío", nameof(titulo));
                
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío", nameof(mensaje));
            
            var notificacion = new Notificacion
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                DestinatarioId = destinatarioId,
                EntidadRelacionadaId = entidadRelacionadaId,
                FechaCreacion = fechaCreacion ?? DateTime.UtcNow
            };
            
            notificacion.AddDomainEvent(new NotificacionCreada(
                notificacion.Id,
                titulo,
                tipo,
                destinatarioId));
                
            return notificacion;
        }
        
        /// <summary>
        /// Marca la notificación como leída
        /// </summary>
        public void MarcarComoLeida()
        {
            if (EstaLeida)
                return;
                
            FechaLectura = DateTime.UtcNow;
            MarkAsModified();
            
            AddDomainEvent(new NotificacionLeida(Id, DestinatarioId));
        }
    }
} 