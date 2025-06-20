namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Command para crear una nueva reservación en el restaurante
/// Gestiona la solicitud, validación y asignación de mesas
/// </summary>
public class CrearReservacionCommand : IRequest<Result<ReservacionDto>>
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
    /// Alias para FechaHoraReservacion (compatibilidad con tests)
    /// </summary>
    public DateTime FechaHora 
    { 
        get => FechaHoraReservacion; 
        set => FechaHoraReservacion = value; 
    }

    /// <summary>
    /// Alias para Telefono (compatibilidad con tests)
    /// </summary>
    public string TelefonoContacto 
    { 
        get => Telefono; 
        set => Telefono = value; 
    }

    /// <summary>
    /// Alias para MesaEspecificaId (compatibilidad con tests)
    /// </summary>
    public Guid? MesaId 
    { 
        get => MesaEspecificaId; 
        set => MesaEspecificaId = value; 
    }

    /// <summary>
    /// Alias para SolicitudesEspeciales (compatibilidad con tests)
    /// </summary>
    public string? Observaciones 
    { 
        get => SolicitudesEspeciales; 
        set => SolicitudesEspeciales = value; 
    }

    /// <summary>
    /// Número de personas para la reservación
    /// </summary>
    public int NumeroPersonas { get; set; }

    /// <summary>
    /// Tipo de ocasión especial
    /// </summary>
    public TipoOcasionEspecial? TipoOcasion { get; set; }

    /// <summary>
    /// Preferencias de mesa específicas
    /// </summary>
    public PreferenciasMesa? PreferenciasMesa { get; set; }

    /// <summary>
    /// Área del restaurante preferida
    /// </summary>
    public string? AreaPreferida { get; set; }

    /// <summary>
    /// Mesa específica solicitada (opcional)
    /// </summary>
    public Guid? MesaEspecificaId { get; set; }

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
    /// Servicios adicionales solicitados
    /// </summary>
    public List<ServicioAdicional>? ServiciosAdicionales { get; set; }

    /// <summary>
    /// Método de confirmación preferido
    /// </summary>
    public MetodoConfirmacion MetodoConfirmacion { get; set; } = MetodoConfirmacion.Email;

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
    public PrioridadReservacion Prioridad { get; set; } = PrioridadReservacion.Normal;

    /// <summary>
    /// Indica si el cliente acepta lista de espera
    /// </summary>
    public bool AceptaListaEspera { get; set; } = true;

    /// <summary>
    /// Duración estimada de la reservación en minutos
    /// </summary>
    public int? DuracionEstimada { get; set; }

    /// <summary>
    /// Datos adicionales de la reservación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Fecha de la reservación
    /// </summary>
    public DateTime FechaReservacion { get; set; }

    /// <summary>
    /// Hora de la reservación
    /// </summary>
    public TimeSpan HoraReservacion { get; set; }

    /// <summary>
    /// ID del usuario que realiza la reservación
    /// </summary>
    public Guid? UsuarioId { get; set; }
}

/// <summary>
/// Tipos de ocasiones especiales
/// </summary>
public enum TipoOcasionEspecial
{
    /// <summary>
    /// Cumpleaños
    /// </summary>
    Cumpleanos = 1,

    /// <summary>
    /// Aniversario
    /// </summary>
    Aniversario = 2,

    /// <summary>
    /// Cita romántica
    /// </summary>
    CitaRomantica = 3,

    /// <summary>
    /// Reunión de negocios
    /// </summary>
    ReunionNegocios = 4,

    /// <summary>
    /// Celebración familiar
    /// </summary>
    CelebracionFamiliar = 5,

    /// <summary>
    /// Graduación
    /// </summary>
    Graduacion = 6,

    /// <summary>
    /// Reunión de amigos
    /// </summary>
    ReunionAmigos = 7,

    /// <summary>
    /// Pedida de matrimonio
    /// </summary>
    PedidaMatrimonio = 8,

    /// <summary>
    /// Evento corporativo
    /// </summary>
    EventoCorporativo = 9,

    /// <summary>
    /// Otra ocasión
    /// </summary>
    Otra = 10
}

/// <summary>
/// Preferencias de mesa
/// </summary>
public class PreferenciasMesa
{
    /// <summary>
    /// Preferencia por terraza
    /// </summary>
    public bool PrefiereTerraza { get; set; }

    /// <summary>
    /// Preferencia por mesa junto a ventana
    /// </summary>
    public bool PrefiereVentana { get; set; }

    /// <summary>
    /// Área tranquila preferida
    /// </summary>
    public bool PrefiereAreaTranquila { get; set; }

    /// <summary>
    /// Acceso para silla de ruedas
    /// </summary>
    public bool RequiereAccesoSillaRuedas { get; set; }

    /// <summary>
    /// Mesa redonda preferida
    /// </summary>
    public bool PrefiereMesaRedonda { get; set; }

    /// <summary>
    /// Mesa con banqueta
    /// </summary>
    public bool PrefiereBanqueta { get; set; }

    /// <summary>
    /// Área para niños
    /// </summary>
    public bool RequiereAreaNinos { get; set; }

    /// <summary>
    /// Mesa privada o reservada
    /// </summary>
    public bool PrefiereMesaPrivada { get; set; }
}

/// <summary>
/// Servicios adicionales disponibles
/// </summary>
public class ServicioAdicional
{
    /// <summary>
    /// Tipo de servicio
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del servicio
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Costo adicional del servicio
    /// </summary>
    public decimal? Costo { get; set; }

    /// <summary>
    /// Notas específicas del servicio
    /// </summary>
    public string? Notas { get; set; }
}

/// <summary>
/// Métodos de confirmación de reservación
/// </summary>
public enum MetodoConfirmacion
{
    /// <summary>
    /// Confirmación por email
    /// </summary>
    Email = 1,

    /// <summary>
    /// Confirmación por SMS
    /// </summary>
    Sms = 2,

    /// <summary>
    /// Confirmación por WhatsApp
    /// </summary>
    WhatsApp = 3,

    /// <summary>
    /// Confirmación telefónica
    /// </summary>
    Telefonica = 4,

    /// <summary>
    /// Confirmación por aplicación
    /// </summary>
    App = 5,

    /// <summary>
    /// Sin confirmación requerida
    /// </summary>
    SinConfirmacion = 6
}

/// <summary>
/// Prioridades de reservación
/// </summary>
public enum PrioridadReservacion
{
    /// <summary>
    /// Prioridad baja
    /// </summary>
    Baja = 1,

    /// <summary>
    /// Prioridad normal
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Prioridad alta
    /// </summary>
    Alta = 3,

    /// <summary>
    /// Prioridad crítica (VIP)
    /// </summary>
    Critica = 4,

    /// <summary>
    /// Prioridad de emergencia
    /// </summary>
    Emergencia = 5
} 