namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Command para crear una nueva reservación en el restaurante
/// Gestiona reservaciones con validación de disponibilidad y capacidad
/// </summary>
public class CrearReservacionCommand : IRequest<Result<ReservacionDto>>
{
    /// <summary>
    /// ID del cliente que realiza la reservación (opcional si es cliente nuevo)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre completo de la persona que reserva
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto para la reservación
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Email de contacto (opcional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Fecha y hora deseada para la reservación
    /// </summary>
    public DateTime FechaHoraReservacion { get; set; }

    /// <summary>
    /// Número de personas para la reservación
    /// </summary>
    public int NumeroPersonas { get; set; }

    /// <summary>
    /// ID de mesa específica (opcional, si no se especifica se asigna automáticamente)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Tipo de mesa preferida (Interior, Terraza, VIP, etc.)
    /// </summary>
    public string? TipoMesaPreferida { get; set; }

    /// <summary>
    /// Zona preferida del restaurante
    /// </summary>
    public string? ZonaPreferida { get; set; }

    /// <summary>
    /// Comentarios o solicitudes especiales
    /// </summary>
    public string? Comentarios { get; set; }

    /// <summary>
    /// Ocasión especial (Cumpleaños, Aniversario, Cita de Negocios, etc.)
    /// </summary>
    public string? OcasionEspecial { get; set; }

    /// <summary>
    /// Duración estimada de la reservación en minutos (por defecto 120 min)
    /// </summary>
    public int DuracionEstimadaMinutos { get; set; } = 120;

    /// <summary>
    /// Canal por el cual se realizó la reservación (Web, Telefono, App, etc.)
    /// </summary>
    public string Canal { get; set; } = "Web";

    /// <summary>
    /// Indica si requiere confirmación previa
    /// </summary>
    public bool RequiereConfirmacion { get; set; } = true;

    /// <summary>
    /// Anticipo requerido para la reservación
    /// </summary>
    public decimal? MontoAnticipo { get; set; }

    /// <summary>
    /// Método de pago del anticipo
    /// </summary>
    public string? MetodoPagoAnticipo { get; set; }

    /// <summary>
    /// Preferencias alimentarias o alergias
    /// </summary>
    public string? PreferenciasAlimentarias { get; set; }

    /// <summary>
    /// Indica si es una reservación recurrente
    /// </summary>
    public bool EsRecurrente { get; set; } = false;

    /// <summary>
    /// Patrón de recurrencia (si aplica)
    /// </summary>
    public string? PatronRecurrencia { get; set; }

    /// <summary>
    /// Notas internas del staff
    /// </summary>
    public string? NotasInternas { get; set; }

    /// <summary>
    /// Indica si se debe notificar al cliente
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la reservación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
} 