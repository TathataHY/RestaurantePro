namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;

/// <summary>
/// Comando para cambiar la contraseña de un usuario con validaciones de seguridad empresariales
/// Incluye validación de contraseña actual, generación de hash seguro y auditoría completa
/// </summary>
public class CambiarPasswordUsuarioCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID único del usuario que cambia su contraseña
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Contraseña actual del usuario (requerida para verificación)
    /// </summary>
    public string PasswordActual { get; set; } = string.Empty;

    /// <summary>
    /// Nueva contraseña que el usuario desea establecer
    /// </summary>
    public string PasswordNueva { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña
    /// </summary>
    public string ConfirmarPasswordNueva { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que autoriza el cambio (puede ser el mismo usuario o un administrador)
    /// </summary>
    public Guid UsuarioAutorizaId { get; set; }

    /// <summary>
    /// Motivo del cambio de contraseña
    /// </summary>
    public string MotivosCambio { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es un cambio forzado por política de seguridad
    /// </summary>
    public bool EsCambioForzado { get; set; } = false;

    /// <summary>
    /// Indica si es el primer cambio después de creación (reseteo de contraseña temporal)
    /// </summary>
    public bool EsPrimerCambio { get; set; } = false;

    /// <summary>
    /// Indica si invalidar todas las sesiones activas después del cambio
    /// </summary>
    public bool InvalidarSesionesActivas { get; set; } = true;

    /// <summary>
    /// Fecha de expiración de la nueva contraseña
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }

    /// <summary>
    /// Indica si enviar notificación por email sobre el cambio
    /// </summary>
    public bool NotificarPorEmail { get; set; } = true;

    /// <summary>
    /// Indica si registrar evento de auditoría
    /// </summary>
    public bool RegistrarAuditoria { get; set; } = true;

    /// <summary>
    /// Observaciones adicionales sobre el cambio
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Dirección IP desde donde se realiza el cambio
    /// </summary>
    public string? DireccionIP { get; set; }

    /// <summary>
    /// User-Agent del navegador/aplicación
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Prioridad del cambio: Baja=1, Normal=2, Alta=3, Crítica=4
    /// </summary>
    public int Prioridad { get; set; } = 2;

    // Factory Methods para diferentes tipos de cambio de contraseña

    /// <summary>
    /// Cambio normal de contraseña por el propio usuario
    /// </summary>
    public static CambiarPasswordUsuarioCommand CambioNormal(
        Guid usuarioId,
        string passwordActual,
        string passwordNueva,
        string confirmarPassword,
        string motivo = "Cambio voluntario de contraseña")
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = passwordNueva,
            ConfirmarPasswordNueva = confirmarPassword,
            UsuarioAutorizaId = usuarioId,
            MotivosCambio = motivo,
            EsCambioForzado = false,
            EsPrimerCambio = false,
            InvalidarSesionesActivas = false,
            NotificarPorEmail = true,
            Prioridad = 2
        };
    }

    /// <summary>
    /// Primer cambio de contraseña (reseteo de temporal)
    /// </summary>
    public static CambiarPasswordUsuarioCommand PrimerCambio(
        Guid usuarioId,
        string passwordTemporal,
        string passwordNueva,
        string confirmarPassword)
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordTemporal,
            PasswordNueva = passwordNueva,
            ConfirmarPasswordNueva = confirmarPassword,
            UsuarioAutorizaId = usuarioId,
            MotivosCambio = "Primer cambio de contraseña - Activación de cuenta",
            EsCambioForzado = true,
            EsPrimerCambio = true,
            InvalidarSesionesActivas = true,
            NotificarPorEmail = true,
            Prioridad = 3,
            FechaExpiracion = DateTime.UtcNow.AddDays(90) // 90 días por defecto
        };
    }

    /// <summary>
    /// Cambio forzado por política de seguridad
    /// </summary>
    public static CambiarPasswordUsuarioCommand CambioForzadoPolitica(
        Guid usuarioId,
        string passwordActual,
        string passwordNueva,
        string confirmarPassword,
        Guid administradorId,
        string motivo)
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = passwordNueva,
            ConfirmarPasswordNueva = confirmarPassword,
            UsuarioAutorizaId = administradorId,
            MotivosCambio = motivo,
            EsCambioForzado = true,
            EsPrimerCambio = false,
            InvalidarSesionesActivas = true,
            NotificarPorEmail = true,
            Prioridad = 4,
            FechaExpiracion = DateTime.UtcNow.AddDays(60) // 60 días para cambios forzados
        };
    }

    /// <summary>
    /// Reset de contraseña por administrador
    /// </summary>
    public static CambiarPasswordUsuarioCommand ResetPorAdministrador(
        Guid usuarioId,
        string passwordTemporal,
        Guid administradorId,
        string motivo)
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = string.Empty, // No se requiere contraseña actual en reset administrativo
            PasswordNueva = passwordTemporal,
            ConfirmarPasswordNueva = passwordTemporal,
            UsuarioAutorizaId = administradorId,
            MotivosCambio = motivo,
            EsCambioForzado = true,
            EsPrimerCambio = false,
            InvalidarSesionesActivas = true,
            NotificarPorEmail = true,
            Prioridad = 4,
            FechaExpiracion = DateTime.UtcNow.AddDays(7) // Temporal por 7 días
        };
    }

    /// <summary>
    /// Cambio por expiración de contraseña
    /// </summary>
    public static CambiarPasswordUsuarioCommand CambioPorExpiracion(
        Guid usuarioId,
        string passwordActual,
        string passwordNueva,
        string confirmarPassword)
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = passwordNueva,
            ConfirmarPasswordNueva = confirmarPassword,
            UsuarioAutorizaId = usuarioId,
            MotivosCambio = "Cambio obligatorio por expiración de contraseña",
            EsCambioForzado = true,
            EsPrimerCambio = false,
            InvalidarSesionesActivas = true,
            NotificarPorEmail = true,
            Prioridad = 3,
            FechaExpiracion = DateTime.UtcNow.AddDays(90)
        };
    }

    /// <summary>
    /// Cambio de emergencia por posible compromiso de seguridad
    /// </summary>
    public static CambiarPasswordUsuarioCommand CambioEmergencia(
        Guid usuarioId,
        string passwordActual,
        string passwordNueva,
        string confirmarPassword,
        Guid administradorId,
        string detallesIncidente)
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = passwordNueva,
            ConfirmarPasswordNueva = confirmarPassword,
            UsuarioAutorizaId = administradorId,
            MotivosCambio = $"Cambio de emergencia - Posible compromiso de seguridad: {detallesIncidente}",
            EsCambioForzado = true,
            EsPrimerCambio = false,
            InvalidarSesionesActivas = true,
            NotificarPorEmail = true,
            Prioridad = 4,
            FechaExpiracion = DateTime.UtcNow.AddDays(30), // Más corto por seguridad
            ObservacionesAdicionales = $"Incidente de seguridad: {detallesIncidente}"
        };
    }

    /// <summary>
    /// Validación básica del comando
    /// </summary>
    public bool EsValido()
    {
        return UsuarioId != Guid.Empty &&
               UsuarioAutorizaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(PasswordNueva) &&
               !string.IsNullOrWhiteSpace(ConfirmarPasswordNueva) &&
               !string.IsNullOrWhiteSpace(MotivosCambio) &&
               Prioridad >= 1 && Prioridad <= 4;
    }

    /// <summary>
    /// Verifica si las contraseñas nuevas coinciden
    /// </summary>
    public bool PasswordsNuevasCoinciden()
    {
        return PasswordNueva == ConfirmarPasswordNueva;
    }

    /// <summary>
    /// Verifica si es un cambio administrativo (no requiere contraseña actual)
    /// </summary>
    public bool EsCambioAdministrativo()
    {
        return UsuarioId != UsuarioAutorizaId;
    }

    /// <summary>
    /// Verifica si el cambio es crítico (requiere validaciones adicionales)
    /// </summary>
    public bool EsCambioCritico()
    {
        return EsCambioForzado || Prioridad >= 3 || EsPrimerCambio;
    }

    /// <summary>
    /// Obtiene el tipo de cambio como texto
    /// </summary>
    public string ObtenerTipoCambio()
    {
        if (EsPrimerCambio) return "Primer Cambio";
        if (EsCambioForzado && Prioridad == 4) return "Cambio Forzado Crítico";
        if (EsCambioForzado) return "Cambio Forzado";
        if (EsCambioAdministrativo()) return "Cambio Administrativo";
        return "Cambio Normal";
    }

    /// <summary>
    /// Obtiene resumen del comando para logging
    /// </summary>
    public string ObtenerResumen()
    {
        return $"CambiarPassword: Usuario {UsuarioId} - Tipo: {ObtenerTipoCambio()} - Motivo: {MotivosCambio} - Prioridad: {Prioridad} - Crítico: {EsCambioCritico()}";
    }

    /// <summary>
    /// Establece información de contexto de seguridad
    /// </summary>
    public void EstablecerContextoSeguridad(string direccionIP, string userAgent)
    {
        DireccionIP = direccionIP;
        UserAgent = userAgent;
    }

    /// <summary>
    /// Establece fecha de expiración basada en tipo de cambio
    /// </summary>
    public void EstablecerExpiracionSegunTipo()
    {
        FechaExpiracion = ObtenerTipoCambio() switch
        {
            "Primer Cambio" => DateTime.UtcNow.AddDays(90),
            "Cambio Forzado Crítico" => DateTime.UtcNow.AddDays(30),
            "Cambio Forzado" => DateTime.UtcNow.AddDays(60),
            "Cambio Administrativo" => DateTime.UtcNow.AddDays(7),
            _ => DateTime.UtcNow.AddDays(120)
        };
    }
} 