namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO para la respuesta de reservaciones
/// Contiene información completa de la reservación creada o consultada
/// </summary>
public class ReservacionDto
{
    /// <summary>
    /// ID único de la reservación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de confirmación de la reservación
    /// </summary>
    public string NumeroConfirmacion { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente que realizó la reservación
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Información básica del cliente
    /// </summary>
    public ClienteReservacionDto? Cliente { get; set; }

    /// <summary>
    /// Nombre del cliente (para reservaciones sin registro)
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Email de contacto
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    public DateTime FechaHoraReservacion { get; set; }

    /// <summary>
    /// Número de personas para la reservación
    /// </summary>
    public int NumeroPersonas { get; set; }

    /// <summary>
    /// Estado actual de la reservación
    /// </summary>
    public EstadoReservacion Estado { get; set; }

    /// <summary>
    /// Tipo de ocasión especial
    /// </summary>
    public string? TipoOcasion { get; set; }

    /// <summary>
    /// Mesa asignada a la reservación
    /// </summary>
    public MesaReservacionDto? Mesa { get; set; }

    /// <summary>
    /// Área del restaurante asignada
    /// </summary>
    public string? AreaAsignada { get; set; }

    /// <summary>
    /// Solicitudes especiales del cliente
    /// </summary>
    public string? SolicitudesEspeciales { get; set; }

    /// <summary>
    /// Restricciones alimentarias
    /// </summary>
    public List<string> RestriccionesAlimentarias { get; set; } = new();

    /// <summary>
    /// Canal de origen de la reservación
    /// </summary>
    public string Canal { get; set; } = string.Empty;

    /// <summary>
    /// Sucursal donde se realizará la reservación
    /// </summary>
    public string? Sucursal { get; set; }

    /// <summary>
    /// Promoción aplicada
    /// </summary>
    public PromocionReservacionDto? PromocionAplicada { get; set; }

    /// <summary>
    /// Indica si está confirmada
    /// </summary>
    public bool EstaConfirmada { get; set; }

    /// <summary>
    /// Fecha de confirmación
    /// </summary>
    public DateTime? FechaConfirmacion { get; set; }

    /// <summary>
    /// Método de confirmación utilizado
    /// </summary>
    public string? MetodoConfirmacion { get; set; }

    /// <summary>
    /// Tiempo máximo de espera en minutos
    /// </summary>
    public int? TiempoMaximoEspera { get; set; }

    /// <summary>
    /// Servicios adicionales contratados
    /// </summary>
    public List<ServicioAdicionalDto> ServiciosAdicionales { get; set; } = new();

    /// <summary>
    /// Prioridad de la reservación
    /// </summary>
    public string Prioridad { get; set; } = string.Empty;

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    public int? DuracionEstimada { get; set; }

    /// <summary>
    /// Fecha de creación de la reservación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Empleado que registró la reservación
    /// </summary>
    public EmpleadoReservacionDto? EmpleadoRegistro { get; set; }

    /// <summary>
    /// Historial de estados de la reservación
    /// </summary>
    public List<HistorialEstadoDto> HistorialEstados { get; set; } = new();

    /// <summary>
    /// Recordatorios programados
    /// </summary>
    public List<RecordatorioDto> Recordatorios { get; set; } = new();

    /// <summary>
    /// Hora de llegada del cliente
    /// </summary>
    public DateTime? HoraLlegada { get; set; }

    /// <summary>
    /// Hora de salida del cliente
    /// </summary>
    public DateTime? HoraSalida { get; set; }

    /// <summary>
    /// Calificación de la experiencia (1-5)
    /// </summary>
    public int? CalificacionExperiencia { get; set; }

    /// <summary>
    /// Comentarios del cliente sobre la experiencia
    /// </summary>
    public string? ComentariosExperiencia { get; set; }

    /// <summary>
    /// Notas internas del personal
    /// </summary>
    public string? NotasInternas { get; set; }

    /// <summary>
    /// Costo total estimado
    /// </summary>
    public decimal? CostoEstimado { get; set; }

    /// <summary>
    /// Anticipo pagado
    /// </summary>
    public decimal? AnticipoPagado { get; set; }

    /// <summary>
    /// Indica si acepta lista de espera
    /// </summary>
    public bool AceptaListaEspera { get; set; }

    /// <summary>
    /// Posición en lista de espera (si aplica)
    /// </summary>
    public int? PosicionListaEspera { get; set; }

    /// <summary>
    /// URL para gestionar la reservación online
    /// </summary>
    public string? UrlGestion { get; set; }

    /// <summary>
    /// Código QR para check-in rápido
    /// </summary>
    public string? QrCheckIn { get; set; }

    /// <summary>
    /// Datos adicionales
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Estados posibles de una reservación
/// </summary>
public enum EstadoReservacion
{
    /// <summary>
    /// Reservación solicitada
    /// </summary>
    Solicitada = 1,

    /// <summary>
    /// Reservación confirmada
    /// </summary>
    Confirmada = 2,

    /// <summary>
    /// Cliente llegó al restaurante
    /// </summary>
    Presente = 3,

    /// <summary>
    /// Mesa asignada
    /// </summary>
    MesaAsignada = 4,

    /// <summary>
    /// En progreso
    /// </summary>
    EnProgreso = 5,

    /// <summary>
    /// Completada
    /// </summary>
    Completada = 6,

    /// <summary>
    /// Cancelada por el cliente
    /// </summary>
    CanceladaCliente = 7,

    /// <summary>
    /// Cancelada por el restaurante
    /// </summary>
    CanceladaRestaurante = 8,

    /// <summary>
    /// No show - cliente no apareció
    /// </summary>
    NoShow = 9,

    /// <summary>
    /// En lista de espera
    /// </summary>
    ListaEspera = 10,

    /// <summary>
    /// Reprogramada
    /// </summary>
    Reprogramada = 11
}

/// <summary>
/// DTO para información del cliente en reservaciones
/// </summary>
public class ClienteReservacionDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Email principal
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono principal
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de fidelización
    /// </summary>
    public string? NivelFidelizacion { get; set; }

    /// <summary>
    /// Número de reservaciones previas
    /// </summary>
    public int ReservacionesPrevias { get; set; }

    /// <summary>
    /// Preferencias conocidas
    /// </summary>
    public List<string> Preferencias { get; set; } = new();
}

/// <summary>
/// DTO para información de mesa en reservaciones
/// </summary>
public class MesaReservacionDto
{
    /// <summary>
    /// ID de la mesa
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad máxima
    /// </summary>
    public int Capacidad { get; set; }

    /// <summary>
    /// Área donde está ubicada
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de mesa
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Características especiales
    /// </summary>
    public List<string> Caracteristicas { get; set; } = new();

    /// <summary>
    /// Ubicación específica (ventana, terraza, etc.)
    /// </summary>
    public string? Ubicacion { get; set; }
}

/// <summary>
/// DTO para promociones aplicadas a reservaciones
/// </summary>
public class PromocionReservacionDto
{
    /// <summary>
    /// ID de la promoción
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código de la promoción
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la promoción
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del beneficio
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Descuento aplicado
    /// </summary>
    public decimal? DescuentoAplicado { get; set; }

    /// <summary>
    /// Tipo de descuento
    /// </summary>
    public string? TipoDescuento { get; set; }
}

/// <summary>
/// DTO para servicios adicionales
/// </summary>
public class ServicioAdicionalDto
{
    /// <summary>
    /// Tipo de servicio
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Costo del servicio
    /// </summary>
    public decimal? Costo { get; set; }

    /// <summary>
    /// Estado del servicio
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Notas específicas
    /// </summary>
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para empleados en reservaciones
/// </summary>
public class EmpleadoReservacionDto
{
    /// <summary>
    /// ID del empleado
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Cargo o puesto
    /// </summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Sucursal del empleado
    /// </summary>
    public string Sucursal { get; set; } = string.Empty;
}

/// <summary>
/// DTO para historial de estados
/// </summary>
public class HistorialEstadoDto
{
    /// <summary>
    /// Estado anterior
    /// </summary>
    public string EstadoAnterior { get; set; } = string.Empty;

    /// <summary>
    /// Estado nuevo
    /// </summary>
    public string EstadoNuevo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del cambio
    /// </summary>
    public DateTime FechaCambio { get; set; }

    /// <summary>
    /// Usuario que realizó el cambio
    /// </summary>
    public string UsuarioCambio { get; set; } = string.Empty;

    /// <summary>
    /// Motivo del cambio
    /// </summary>
    public string? MotivoCambio { get; set; }
}

/// <summary>
/// DTO para recordatorios
/// </summary>
public class RecordatorioDto
{
    /// <summary>
    /// Tipo de recordatorio
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha programada
    /// </summary>
    public DateTime FechaProgramada { get; set; }

    /// <summary>
    /// Mensaje del recordatorio
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Estado del recordatorio
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Método de envío
    /// </summary>
    public string MetodoEnvio { get; set; } = string.Empty;
} 