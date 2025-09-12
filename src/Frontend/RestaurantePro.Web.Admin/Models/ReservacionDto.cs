using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO base para reservaciones del restaurante
/// </summary>
public class ReservacionDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El número de reservación es obligatorio")]
    [StringLength(20, ErrorMessage = "El número de reservación no puede exceder 20 caracteres")]
    public string NumeroReservacion { get; set; } = string.Empty;
    
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaReservacion { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    public DateTime FechaHoraCompleta => FechaReservacion.Date.Add(HoraReservacion);
    
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public int MesaNumero { get; set; }
    public string MesaUbicacion { get; set; } = string.Empty;
    public int MesaCapacidad { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "El número de personas debe ser mayor a 0")]
    public int NumeroPersonas { get; set; }
    
    [Required(ErrorMessage = "El estado es obligatorio")]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, EnProceso, Completada, Cancelada, NoShow
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [StringLength(500, ErrorMessage = "Las notas especiales no pueden exceder 500 caracteres")]
    public string? NotasEspeciales { get; set; }
    
    [StringLength(500, ErrorMessage = "Los requerimientos especiales no pueden exceder 500 caracteres")]
    public string? RequerimientosEspeciales { get; set; }
    
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaCancelacion { get; set; }
    public DateTime? FechaLlegada { get; set; }
    public DateTime? FechaSalida { get; set; }
    
    public string? MotivoCancelacion { get; set; }
    public string? CanalReservacion { get; set; } = "Web"; // Web, Telefono, Presencial, App
    public string? FuenteReservacion { get; set; } = "Directa"; // Directa, Google, Facebook, TripAdvisor, etc.
    
    public bool EsRecurrente { get; set; } = false;
    public string? FrecuenciaRecurrencia { get; set; } // Diaria, Semanal, Mensual
    public DateTime? FechaFinRecurrencia { get; set; }
    public Guid? ReservacionPadreId { get; set; }
    
    public bool RequiereConfirmacion { get; set; } = true;
    public bool EsUrgente { get; set; } = false;
    public bool EsVIP { get; set; } = false;
    public bool EsGrupo { get; set; } = false;
    
    // Información de contacto adicional
    public string? ContactoAlternativo { get; set; }
    public string? TelefonoAlternativo { get; set; }
    public string? EmailAlternativo { get; set; }
    
    // Información de grupo (si aplica)
    public string? NombreGrupo { get; set; }
    public string? EmpresaGrupo { get; set; }
    public int? TamañoGrupo { get; set; }
    
    // Propiedades calculadas
    public TimeSpan TiempoTranscurrido => DateTime.Now - FechaCreacion;
    public TimeSpan TiempoHastaReservacion => FechaHoraCompleta - DateTime.Now;
    public bool EstaPendiente => Estado == "Pendiente";
    public bool EstaConfirmada => Estado == "Confirmada";
    public bool EstaEnProceso => Estado == "EnProceso";
    public bool EstaCompletada => Estado == "Completada";
    public bool EstaCancelada => Estado == "Cancelada";
    public bool EsNoShow => Estado == "NoShow";
    public bool EsHoy => FechaReservacion.Date == DateTime.Today;
    public bool EsPasada => FechaHoraCompleta < DateTime.Now;
    public bool EsFutura => FechaHoraCompleta > DateTime.Now;
    public bool RequiereAtencion => (TiempoHastaReservacion.TotalHours < 2 && TiempoHastaReservacion.TotalHours > 0) || EsUrgente;
    
    public string EstadoVisual => Estado switch
    {
        "Pendiente" => "Pendiente",
        "Confirmada" => "Confirmada",
        "EnProceso" => "En Proceso",
        "Completada" => "Completada",
        "Cancelada" => "Cancelada",
        "NoShow" => "No Show",
        _ => "Desconocido"
    };
    
    public string ClaseEstado => Estado switch
    {
        "Pendiente" => "warning",
        "Confirmada" => "success",
        "EnProceso" => "info",
        "Completada" => "primary",
        "Cancelada" => "danger",
        "NoShow" => "secondary",
        _ => "secondary"
    };
    
    public string ClaseUrgencia => EsUrgente ? "danger" : RequiereAtencion ? "warning" : "success";
    public int MinutosHastaReservacion => (int)TiempoHastaReservacion.TotalMinutes;
    public bool EsPronto => MinutosHastaReservacion > 0 && MinutosHastaReservacion <= 120; // 2 horas
    public bool EsMuyPronto => MinutosHastaReservacion > 0 && MinutosHastaReservacion <= 30; // 30 minutos
}

/// <summary>
/// DTO para crear una nueva reservación
/// </summary>
public class CrearReservacionRequest
{
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public Guid ClienteId { get; set; }
    
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    
    [Required(ErrorMessage = "La fecha de reservación es obligatoria")]
    public DateTime FechaReservacion { get; set; }
    
    [Required(ErrorMessage = "La hora de reservación es obligatoria")]
    public TimeSpan HoraReservacion { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "El número de personas debe ser mayor a 0")]
    public int NumeroPersonas { get; set; }
    
    public string? Observaciones { get; set; }
    public string? NotasEspeciales { get; set; }
    public string? RequerimientosEspeciales { get; set; }
    public string CanalReservacion { get; set; } = "Web";
    public string? FuenteReservacion { get; set; } = "Directa";
    public bool EsRecurrente { get; set; } = false;
    public string? FrecuenciaRecurrencia { get; set; }
    public DateTime? FechaFinRecurrencia { get; set; }
    public bool RequiereConfirmacion { get; set; } = true;
    public bool EsUrgente { get; set; } = false;
    public bool EsVIP { get; set; } = false;
    public bool EsGrupo { get; set; } = false;
    public string? ContactoAlternativo { get; set; }
    public string? TelefonoAlternativo { get; set; }
    public string? EmailAlternativo { get; set; }
    public string? NombreGrupo { get; set; }
    public string? EmpresaGrupo { get; set; }
    public int? TamañoGrupo { get; set; }
}

/// <summary>
/// DTO para actualizar una reservación existente
/// </summary>
public class ActualizarReservacionRequest
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public Guid Id { get; set; }
    
    public DateTime? FechaReservacion { get; set; }
    public TimeSpan? HoraReservacion { get; set; }
    public Guid? MesaId { get; set; }
    public int? NumeroPersonas { get; set; }
    public string? Observaciones { get; set; }
    public string? NotasEspeciales { get; set; }
    public string? RequerimientosEspeciales { get; set; }
    public string? Estado { get; set; }
    public bool? EsUrgente { get; set; }
    public bool? EsVIP { get; set; }
    public string? ContactoAlternativo { get; set; }
    public string? TelefonoAlternativo { get; set; }
    public string? EmailAlternativo { get; set; }

    [Required(ErrorMessage = "El ID del cliente es obligatorio")]
    public Guid ClienteId { get; set; }

    [StringLength(50, ErrorMessage = "El canal de reservación no puede exceder 50 caracteres")]
    public string CanalReservacion { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notas { get; set; }

    public bool RequiereAtencion { get; set; } = false;

    [StringLength(100, ErrorMessage = "El motivo de atención no puede exceder 100 caracteres")]
    public string MotivoAtencion { get; set; } = string.Empty;

    public bool EsGrupo { get; set; } = false;
    public bool EsRecurrente { get; set; } = false;

    [Required(ErrorMessage = "La fecha de creación es obligatoria")]
    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para confirmar una reservación
/// </summary>
public class ConfirmarReservacionRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    public string? Observaciones { get; set; }
    public string? NotasEspeciales { get; set; }
}

/// <summary>
/// DTO para cancelar una reservación
/// </summary>
public class CancelarReservacionRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    [Required(ErrorMessage = "El motivo de cancelación es obligatorio")]
    public string MotivoCancelacion { get; set; } = string.Empty;
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para marcar llegada
/// </summary>
public class MarcarLlegadaRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para marcar salida
/// </summary>
public class MarcarSalidaRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para verificar disponibilidad
/// </summary>
public class VerificarDisponibilidadRequest
{
    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; }
    
    [Required(ErrorMessage = "La hora es obligatoria")]
    public TimeSpan Hora { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "El número de personas debe ser mayor a 0")]
    public int NumeroPersonas { get; set; }
    
    public Guid? MesaIdExcluir { get; set; }
    public TimeSpan DuracionEstimada { get; set; } = TimeSpan.FromHours(2);
}

/// <summary>
/// DTO para respuesta de disponibilidad
/// </summary>
public class DisponibilidadResponse
{
    public bool Disponible { get; set; }
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();
    public List<ReservacionConflictoDto> Conflictos { get; set; } = new();
    public string? Mensaje { get; set; }
}

/// <summary>
/// DTO para mesa disponible
/// </summary>
public class MesaDisponibleDto
{
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public int MesaNumero { get; set; }
    public string MesaUbicacion { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public int CapacidadRecomendada { get; set; }
    public bool EsIdeal { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para conflicto de reservación
/// </summary>
public class ReservacionConflictoDto
{
    public Guid ReservacionId { get; set; }
    public string NumeroReservacion { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para estadísticas de reservaciones
/// </summary>
public class ReservacionEstadisticasDto
{
    public int TotalReservaciones { get; set; }
    public int ReservacionesPendientes { get; set; }
    public int ReservacionesConfirmadas { get; set; }
    public int ReservacionesEnProceso { get; set; }
    public int ReservacionesCompletadas { get; set; }
    public int ReservacionesCanceladas { get; set; }
    public int ReservacionesNoShow { get; set; }
    public int ReservacionesHoy { get; set; }
    public int ReservacionesUrgentes { get; set; }
    public int ReservacionesVIP { get; set; }
    public int ReservacionesGrupo { get; set; }
    public decimal TasaConfirmacion { get; set; }
    public decimal TasaNoShow { get; set; }
    public decimal TasaCancelacion { get; set; }
    
    // Propiedades adicionales para el dashboard
    public decimal IngresosPorReservaciones { get; set; }
    public decimal OcupacionPromedio { get; set; }
    public decimal? CambioTotalReservaciones { get; set; }
    public decimal? CambioIngresos { get; set; }
    public decimal? CambioOcupacion { get; set; }
    
    public List<ReservacionEstadoEstadisticasDto> Estados { get; set; } = new();
    public List<ReservacionCanalEstadisticasDto> Canales { get; set; } = new();
    public List<ReservacionHoraEstadisticasDto> HorasPico { get; set; } = new();
    public List<ReservacionMesaEstadisticasDto> MesasMasReservadas { get; set; } = new();
}

/// <summary>
/// DTO para estadísticas por estado
/// </summary>
public class ReservacionEstadoEstadisticasDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para estadísticas por canal
/// </summary>
public class ReservacionCanalEstadisticasDto
{
    public string Canal { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal TasaConfirmacion { get; set; }
}

/// <summary>
/// DTO para estadísticas por hora
/// </summary>
public class ReservacionHoraEstadisticasDto
{
    public string Hora { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para estadísticas por mesa
/// </summary>
public class ReservacionMesaEstadisticasDto
{
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public int CantidadReservaciones { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal TasaOcupacion { get; set; }
}

/// <summary>
/// DTO para filtros de reservaciones
/// </summary>
public class ReservacionFiltrosDto
{
    public string? Busqueda { get; set; }
    public string? Estado { get; set; }
    public string? CanalReservacion { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MesaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
    public int? NumeroPersonasMinimo { get; set; }
    public int? NumeroPersonasMaximo { get; set; }
    public bool? EsUrgente { get; set; }
    public bool? EsVIP { get; set; }
    public bool? EsGrupo { get; set; }
    public bool? EsRecurrente { get; set; }
    public bool? EsHoy { get; set; }
    public bool? EsFutura { get; set; }
    public bool? RequiereAtencion { get; set; }
    public string? OrdenarPor { get; set; } = "FechaHoraCompleta";
    public string? DireccionOrden { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para calendario de reservaciones
/// </summary>
public class CalendarioReservacionesDto
{
    public DateTime Fecha { get; set; }
    public List<ReservacionDto> Reservaciones { get; set; } = new();
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();
    public int TotalReservaciones { get; set; }
    public int ReservacionesConfirmadas { get; set; }
    public int ReservacionesPendientes { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponiblesCount { get; set; }
    public decimal TasaOcupacion { get; set; }
}

/// <summary>
/// DTO para reasignar mesa
/// </summary>
public class ReasignarMesaReservacionRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    [Required(ErrorMessage = "El ID de nueva mesa es obligatorio")]
    public Guid NuevaMesaId { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para cambiar hora
/// </summary>
public class CambiarHoraReservacionRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    [Required(ErrorMessage = "La nueva hora es obligatoria")]
    public TimeSpan NuevaHora { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para duplicar reservación
/// </summary>
public class DuplicarReservacionRequest
{
    [Required(ErrorMessage = "El ID de reservación es obligatorio")]
    public Guid ReservacionId { get; set; }
    
    [Required(ErrorMessage = "La nueva fecha es obligatoria")]
    public DateTime NuevaFecha { get; set; }
    
    [Required(ErrorMessage = "La nueva hora es obligatoria")]
    public TimeSpan NuevaHora { get; set; }
    
    public Guid? NuevaMesaId { get; set; }
    public string? Observaciones { get; set; }
}
