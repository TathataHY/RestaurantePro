using System;

namespace RestaurantePro.Infrastructure.DTOs.SignalR
{
    /// <summary>
    /// DTO para notificaciones por SignalR
    /// Optimizado para comunicación en tiempo real
    /// </summary>
    public class NotificacionSignalRDto
    {
        /// <summary>
        /// Título de la notificación
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de notificación (info, success, warning, error)
        /// </summary>
        public string Tipo { get; set; } = "info";

        /// <summary>
        /// Timestamp de la notificación
        /// </summary>
        public DateTime FechaHora { get; set; }

        /// <summary>
        /// ID del usuario destinatario (null para notificaciones globales)
        /// </summary>
        public Guid? DestinatarioId { get; set; }

        /// <summary>
        /// Rol destinatario (null para notificaciones individuales)
        /// </summary>
        public string? Rol { get; set; }

        /// <summary>
        /// Datos adicionales específicos del tipo de notificación
        /// </summary>
        public object? Datos { get; set; }

        /// <summary>
        /// ID único de la notificación
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Indica si la notificación requiere confirmación
        /// </summary>
        public bool RequiereConfirmacion { get; set; }

        /// <summary>
        /// Duración de la notificación en segundos (0 = permanente)
        /// </summary>
        public int DuracionSegundos { get; set; }

        /// <summary>
        /// Constructor por defecto requerido para serialización
        /// </summary>
        public NotificacionSignalRDto()
        {
            Id = Guid.NewGuid();
            FechaHora = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor para notificación global
        /// </summary>
        public NotificacionSignalRDto(string titulo, string mensaje, string tipo = "info")
        {
            Id = Guid.NewGuid();
            Titulo = titulo;
            Mensaje = mensaje;
            Tipo = tipo;
            FechaHora = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor para notificación por rol
        /// </summary>
        public NotificacionSignalRDto(string titulo, string mensaje, string rol, string tipo = "info")
        {
            Id = Guid.NewGuid();
            Titulo = titulo;
            Mensaje = mensaje;
            Rol = rol;
            Tipo = tipo;
            FechaHora = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor para notificación individual
        /// </summary>
        public NotificacionSignalRDto(string titulo, string mensaje, Guid destinatarioId, string tipo = "info")
        {
            Id = Guid.NewGuid();
            Titulo = titulo;
            Mensaje = mensaje;
            DestinatarioId = destinatarioId;
            Tipo = tipo;
            FechaHora = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor completo con todos los parámetros
        /// </summary>
        public NotificacionSignalRDto(
            string titulo,
            string mensaje,
            string tipo,
            Guid? destinatarioId = null,
            string? rol = null,
            object? datos = null,
            bool requiereConfirmacion = false,
            int duracionSegundos = 0)
        {
            Id = Guid.NewGuid();
            Titulo = titulo;
            Mensaje = mensaje;
            Tipo = tipo;
            DestinatarioId = destinatarioId;
            Rol = rol;
            Datos = datos;
            RequiereConfirmacion = requiereConfirmacion;
            DuracionSegundos = duracionSegundos;
            FechaHora = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tipos de notificación disponibles
    /// </summary>
    public static class TiposNotificacion
    {
        public const string Info = "info";
        public const string Success = "success";
        public const string Warning = "warning";
        public const string Error = "error";
        public const string System = "system";
        public const string Alert = "alert";
    }

    /// <summary>
    /// Datos específicos para notificaciones de comanda
    /// </summary>
    public class DatosNotificacionComanda
    {
        public Guid ComandaId { get; set; }
        public string NumeroComanda { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Mesa { get; set; } = string.Empty;
        public string? Accion { get; set; }
    }

    /// <summary>
    /// Datos específicos para notificaciones de inventario
    /// </summary>
    public class DatosNotificacionInventario
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public string TipoAlerta { get; set; } = string.Empty;
    }
} 