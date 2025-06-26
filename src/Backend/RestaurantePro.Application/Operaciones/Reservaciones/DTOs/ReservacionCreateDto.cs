namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO para crear una nueva reservación
/// </summary>
public class ReservacionCreateDto
{
    /// <summary>
    /// ID del cliente que solicita la reservación
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente (para clientes no registrados)
    /// </summary>
    public string? NombreCliente { get; set; }

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
    /// Tipo de ocasión especial
    /// </summary>
    public string? TipoOcasion { get; set; }

    /// <summary>
    /// Mesa específica solicitada (opcional)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Solicitudes especiales del cliente
    /// </summary>
    public string? SolicitudesEspeciales { get; set; }

    /// <summary>
    /// Restricciones alimentarias o alergias
    /// </summary>
    public List<string>? RestriccionesAlimentarias { get; set; }

    /// <summary>
    /// Canal a través del cual se realizó la reservación
    /// </summary>
    public string Canal { get; set; } = "Web";

    /// <summary>
    /// Sucursal donde se realizará la reservación
    /// </summary>
    public string? Sucursal { get; set; }

    /// <summary>
    /// Código de promoción para descuentos
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// Indica si requiere confirmación automática
    /// </summary>
    public bool RequiereConfirmacion { get; set; } = true;

    /// <summary>
    /// Tiempo máximo de espera en minutos
    /// </summary>
    public int? TiempoMaximoEspera { get; set; }

    /// <summary>
    /// Método de confirmación preferido
    /// </summary>
    public string MetodoConfirmacion { get; set; } = "Email";

    /// <summary>
    /// Notas internas del empleado
    /// </summary>
    public string? NotasInternas { get; set; }

    /// <summary>
    /// ID del empleado que registra la reservación
    /// </summary>
    public Guid? EmpleadoId { get; set; }

    /// <summary>
    /// Prioridad de la reservación
    /// </summary>
    public string Prioridad { get; set; } = "Normal";

    /// <summary>
    /// Indica si el cliente acepta lista de espera
    /// </summary>
    public bool AceptaListaEspera { get; set; } = true;

    /// <summary>
    /// Duración estimada de la reservación en minutos
    /// </summary>
    public int? DuracionEstimada { get; set; }

    /// <summary>
    /// ID del usuario que realiza la reservación
    /// </summary>
    public Guid? UsuarioId { get; set; }
} 