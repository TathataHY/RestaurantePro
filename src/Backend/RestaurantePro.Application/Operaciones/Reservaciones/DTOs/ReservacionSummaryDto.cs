namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO resumido para mostrar información básica de una reservación
/// Se usa en listados y como información adicional en otros DTOs
/// </summary>
public class ReservacionSummaryDto
{
    /// <summary>
    /// ID único de la reservación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Número de comensales
    /// </summary>
    public int NumeroComensales { get; set; }

    /// <summary>
    /// Estado actual de la reservación
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Número de la mesa asignada (si aplica)
    /// </summary>
    public int? MesaNumero { get; set; }

    /// <summary>
    /// Zona de la mesa (si aplica)
    /// </summary>
    public string? ZonaMesa { get; set; }

    /// <summary>
    /// Nombre del cliente que hizo la reservación
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto para la reservación
    /// </summary>
    public string? TelefonoContacto { get; set; }

    /// <summary>
    /// Notas especiales de la reservación
    /// </summary>
    public string? NotasEspeciales { get; set; }

    /// <summary>
    /// Monto estimado para la reservación
    /// </summary>
    public decimal MontoEstimado { get; set; }

    /// <summary>
    /// Indica si la reservación requiere confirmación
    /// </summary>
    public bool RequiereConfirmacion { get; set; }

    /// <summary>
    /// Fecha límite para confirmación (si aplica)
    /// </summary>
    public DateTime? FechaLimiteConfirmacion { get; set; }

    /// <summary>
    /// Tiempo restante hasta la reservación en minutos
    /// </summary>
    public int MinutosHastaReservacion 
    {
        get
        {
            var diferencia = FechaHora - DateTime.UtcNow;
            return diferencia.TotalMinutes > 0 ? (int)diferencia.TotalMinutes : 0;
        }
    }

    /// <summary>
    /// Indica si la reservación es para hoy
    /// </summary>
    public bool EsParaHoy => FechaHora.Date == DateTime.UtcNow.Date;

    /// <summary>
    /// Indica si la reservación está próxima (menos de 2 horas)
    /// </summary>
    public bool EsProxima => MinutosHastaReservacion <= 120 && MinutosHastaReservacion > 0;

    /// <summary>
    /// Obtiene una descripción corta de la reservación
    /// </summary>
    public string DescripcionCorta => $"{NombreCliente} - {NumeroComensales} personas - Mesa {MesaNumero?.ToString() ?? "Por asignar"}";
} 