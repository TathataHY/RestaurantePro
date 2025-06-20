namespace RestaurantePro.Application.Core.Usuarios.DTOs;

/// <summary>
/// DTO para representar la configuración de notificaciones de un usuario
/// </summary>
public class ConfiguracionNotificacionesDto
{
    /// <summary>
    /// ID de la configuración
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario al que pertenece la configuración
    /// </summary>
    public Guid UsuarioId { get; set; }

    // === NOTIFICACIONES POR EMAIL ===
    
    /// <summary>
    /// Habilitar notificaciones por email
    /// </summary>
    public bool EmailHabilitado { get; set; }

    /// <summary>
    /// Email para notificaciones (puede ser diferente al email principal)
    /// </summary>
    public string? EmailNotificaciones { get; set; }

    /// <summary>
    /// Notificaciones de comandas por email
    /// </summary>
    public bool EmailComandas { get; set; }

    /// <summary>
    /// Notificaciones de facturación por email
    /// </summary>
    public bool EmailFacturacion { get; set; }

    /// <summary>
    /// Notificaciones de inventario por email
    /// </summary>
    public bool EmailInventario { get; set; }

    /// <summary>
    /// Notificaciones de usuarios/personal por email
    /// </summary>
    public bool EmailUsuarios { get; set; }

    /// <summary>
    /// Notificaciones de sistema por email
    /// </summary>
    public bool EmailSistema { get; set; }

    // === NOTIFICACIONES PUSH (EN SISTEMA) ===

    /// <summary>
    /// Habilitar notificaciones push en el sistema
    /// </summary>
    public bool PushHabilitado { get; set; }

    /// <summary>
    /// Notificaciones de comandas push
    /// </summary>
    public bool PushComandas { get; set; }

    /// <summary>
    /// Notificaciones de facturación push
    /// </summary>
    public bool PushFacturacion { get; set; }

    /// <summary>
    /// Notificaciones de inventario push
    /// </summary>
    public bool PushInventario { get; set; }

    /// <summary>
    /// Notificaciones de usuarios/personal push
    /// </summary>
    public bool PushUsuarios { get; set; }

    /// <summary>
    /// Notificaciones de sistema push
    /// </summary>
    public bool PushSistema { get; set; }

    // === NOTIFICACIONES SMS ===

    /// <summary>
    /// Habilitar notificaciones por SMS
    /// </summary>
    public bool SmsHabilitado { get; set; }

    /// <summary>
    /// Teléfono para SMS (puede ser diferente al teléfono principal)
    /// </summary>
    public string? TelefonoSms { get; set; }

    /// <summary>
    /// SMS solo para notificaciones críticas
    /// </summary>
    public bool SmsSoloCriticas { get; set; }

    /// <summary>
    /// SMS para cambios de estado de comandas
    /// </summary>
    public bool SmsComandas { get; set; }

    /// <summary>
    /// SMS para alertas de inventario
    /// </summary>
    public bool SmsInventario { get; set; }

    /// <summary>
    /// SMS para notificaciones de seguridad
    /// </summary>
    public bool SmsSeguridad { get; set; }

    // === CONFIGURACIÓN DE HORARIOS ===

    /// <summary>
    /// Hora de inicio para recibir notificaciones
    /// </summary>
    public TimeSpan HoraInicioNotificaciones { get; set; }

    /// <summary>
    /// Hora de fin para recibir notificaciones
    /// </summary>
    public TimeSpan HoraFinNotificaciones { get; set; }

    /// <summary>
    /// Días de la semana para recibir notificaciones (1=Lunes, 7=Domingo)
    /// </summary>
    public List<int> DiasHabilitados { get; set; } = new();

    /// <summary>
    /// Zona horaria para las notificaciones
    /// </summary>
    public string ZonaHoraria { get; set; } = "America/Santiago";

    // === CONFIGURACIÓN DE FRECUENCIA ===

    /// <summary>
    /// Frecuencia mínima entre notificaciones del mismo tipo (en minutos)
    /// </summary>
    public int FrecuenciaMinima { get; set; }

    /// <summary>
    /// Máximo número de notificaciones por hora
    /// </summary>
    public int MaximoNotificacionesPorHora { get; set; }

    /// <summary>
    /// Agrupar notificaciones similares
    /// </summary>
    public bool AgruparNotificaciones { get; set; }

    /// <summary>
    /// Tiempo de agrupación en minutos
    /// </summary>
    public int TiempoAgrupacionMinutos { get; set; }

    // === CONFIGURACIÓN AVANZADA ===

    /// <summary>
    /// Tipos de notificación que requieren confirmación de lectura
    /// </summary>
    public List<string> TiposRequierenConfirmacion { get; set; } = new();

    /// <summary>
    /// Plantilla de formato para emails
    /// </summary>
    public string FormatoEmail { get; set; } = "HTML";

    /// <summary>
    /// Idioma para las notificaciones
    /// </summary>
    public string Idioma { get; set; } = "es-CL";

    /// <summary>
    /// Notificaciones solo durante horario laboral
    /// </summary>
    public bool SoloHorarioLaboral { get; set; }

    /// <summary>
    /// Escalamiento automático para notificaciones críticas no leídas
    /// </summary>
    public bool EscalamientoAutomatico { get; set; }

    /// <summary>
    /// Tiempo en minutos para escalamiento automático
    /// </summary>
    public int TiempoEscalamientoMinutos { get; set; }

    /// <summary>
    /// Usuarios para escalamiento (supervisores, administradores)
    /// </summary>
    public List<Guid> UsuariosEscalamiento { get; set; } = new();

    // === METADATOS ===

    /// <summary>
    /// Fecha de creación de la configuración
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// Usuario que realizó la última actualización
    /// </summary>
    public Guid? ActualizadoPor { get; set; }

    /// <summary>
    /// Versión de la configuración
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Constructor por defecto con valores predeterminados
    /// </summary>
    public ConfiguracionNotificacionesDto()
    {
        Id = Guid.NewGuid();
        
        // Valores predeterminados para notificaciones básicas
        EmailHabilitado = true;
        EmailComandas = true;
        EmailFacturacion = true;
        EmailSistema = true;
        
        PushHabilitado = true;
        PushComandas = true;
        PushFacturacion = true;
        PushInventario = true;
        PushSistema = true;
        
        SmsHabilitado = false;
        SmsSoloCriticas = true;
        SmsSeguridad = true;
        
        // Horarios predeterminados: 7 AM a 10 PM
        HoraInicioNotificaciones = new TimeSpan(7, 0, 0);
        HoraFinNotificaciones = new TimeSpan(22, 0, 0);
        DiasHabilitados = new List<int> { 1, 2, 3, 4, 5, 6, 7 }; // Todos los días
        
        // Frecuencia predeterminada
        FrecuenciaMinima = 5; // 5 minutos
        MaximoNotificacionesPorHora = 12;
        AgruparNotificaciones = true;
        TiempoAgrupacionMinutos = 10;
        
        // Configuración avanzada
        TiposRequierenConfirmacion = new List<string> { "Critica", "Seguridad", "Facturacion" };
        SoloHorarioLaboral = false;
        EscalamientoAutomatico = true;
        TiempoEscalamientoMinutos = 60;
        
        // Metadatos
        FechaCreacion = DateTime.UtcNow;
        Version = 1;
    }

    /// <summary>
    /// Factory method para configuración básica
    /// </summary>
    public static ConfiguracionNotificacionesDto CrearBasica(Guid usuarioId, string? email = null, string? telefono = null)
    {
        var config = new ConfiguracionNotificacionesDto
        {
            UsuarioId = usuarioId,
            EmailNotificaciones = email,
            TelefonoSms = telefono
        };

        return config;
    }

    /// <summary>
    /// Factory method para configuración de administrador
    /// </summary>
    public static ConfiguracionNotificacionesDto CrearAdministrador(Guid usuarioId, string email, string telefono)
    {
        var config = CrearBasica(usuarioId, email, telefono);
        
        // Administradores reciben todas las notificaciones
        config.EmailInventario = true;
        config.EmailUsuarios = true;
        config.PushUsuarios = true;
        config.SmsHabilitado = true;
        config.SmsSoloCriticas = false;
        config.SmsComandas = true;
        config.SmsInventario = true;
        
        // Disponible 24/7 para administradores
        config.HoraInicioNotificaciones = new TimeSpan(0, 0, 0);
        config.HoraFinNotificaciones = new TimeSpan(23, 59, 59);
        config.SoloHorarioLaboral = false;
        
        return config;
    }

    /// <summary>
    /// Factory method para configuración de mesero
    /// </summary>
    public static ConfiguracionNotificacionesDto CrearMesero(Guid usuarioId, string? email = null)
    {
        var config = CrearBasica(usuarioId, email);
        
        // Meseros solo necesitan notificaciones de comandas
        config.EmailFacturacion = false;
        config.EmailInventario = false;
        config.EmailUsuarios = false;
        config.PushFacturacion = false;
        config.PushInventario = false;
        config.PushUsuarios = false;
        
        // Solo durante horario laboral
        config.SoloHorarioLaboral = true;
        config.DiasHabilitados = new List<int> { 1, 2, 3, 4, 5, 6 }; // Lunes a Sábado
        
        return config;
    }

    /// <summary>
    /// Valida si la configuración es válida
    /// </summary>
    public bool EsValida()
    {
        // Validar horarios
        if (HoraInicioNotificaciones >= HoraFinNotificaciones) return false;

        // Validar días habilitados
        if (!DiasHabilitados.Any() || DiasHabilitados.Any(d => d < 1 || d > 7)) return false;

        // Validar frecuencias
        if (FrecuenciaMinima < 1 || MaximoNotificacionesPorHora < 1) return false;

        // Validar que al menos un canal esté habilitado
        if (!EmailHabilitado && !PushHabilitado && !SmsHabilitado) return false;

        return true;
    }

    /// <summary>
    /// Determina si debe recibir notificaciones en este momento
    /// </summary>
    public bool DebeRecibirNotificacionAhora()
    {
        var ahora = DateTime.Now;
        
        // Verificar día de la semana
        var diaActual = (int)ahora.DayOfWeek;
        if (diaActual == 0) diaActual = 7; // Domingo = 7
        
        if (!DiasHabilitados.Contains(diaActual)) return false;

        // Verificar horario
        var horaActual = ahora.TimeOfDay;
        if (horaActual < HoraInicioNotificaciones || horaActual > HoraFinNotificaciones) return false;

        return true;
    }
} 