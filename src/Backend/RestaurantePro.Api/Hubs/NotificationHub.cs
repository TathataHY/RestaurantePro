using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System.Security.Claims;

namespace RestaurantePro.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly ISignalRService _signalRService;

    public NotificationHub(ILogger<NotificationHub> logger, ISignalRService signalRService)
    {
        _logger = logger;
        _signalRService = signalRService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} con rol {UserRole} conectado al NotificationHub", userId, userRole);
        
        // Unir al usuario a su grupo de rol
        if (!string.IsNullOrEmpty(userRole))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userRole);
            _logger.LogInformation("Usuario {UserId} agregado al grupo {Group}", userId, userRole);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} con rol {UserRole} desconectado del NotificationHub", userId, userRole);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envía una notificación global a todos los usuarios conectados
    /// </summary>
    public async Task EnviarNotificacionGlobal(string titulo, string mensaje, string tipo = "info")
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            throw new HubException("El título y el mensaje no pueden estar vacíos.");
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando notificación global: {Titulo}", userId, userRole, titulo);
        
        try
        {
            var notificacion = new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                FechaHora = DateTime.UtcNow,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            await Clients.All.SendAsync("RecibirNotificacion", notificacion);
            
            _logger.LogInformation("Notificación global enviada exitosamente: {Titulo}", titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación global: {Titulo}", titulo);
            throw;
        }
    }

    /// <summary>
    /// Envía una notificación a todos los usuarios de un rol específico
    /// </summary>
    public async Task EnviarNotificacionARol(string rol, string titulo, string mensaje, string tipo = "info")
    {
        if (string.IsNullOrWhiteSpace(rol) || string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            throw new HubException("El rol, el título y el mensaje no pueden estar vacíos.");
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando notificación al rol {RolDestino}: {Titulo}", 
            userId, userRole, rol, titulo);
        
        try
        {
            var notificacion = new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                FechaHora = DateTime.UtcNow,
                RolDestinatario = rol,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            await Clients.Group(rol).SendAsync("RecibirNotificacion", notificacion);
            
            _logger.LogInformation("Notificación enviada al rol {Rol} exitosamente: {Titulo}", rol, titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación al rol {Rol}: {Titulo}", rol, titulo);
            throw;
        }
    }

    /// <summary>
    /// Envía una notificación a un usuario específico
    /// </summary>
    public async Task EnviarNotificacionAUsuario(Guid usuarioId, string titulo, string mensaje, string tipo = "info")
    {
        if (usuarioId == Guid.Empty || string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            throw new HubException("El usuario, el título y el mensaje no pueden estar vacíos.");
        var senderId = GetUserIdFromClaims();
        var senderRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {SenderId} ({SenderRole}) enviando notificación al usuario {UsuarioId}: {Titulo}", 
            senderId, senderRole, usuarioId, titulo);
        
        try
        {
            var notificacion = new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                FechaHora = DateTime.UtcNow,
                DestinatarioId = usuarioId,
                EnviadoPor = senderId,
                RolEnviadoPor = senderRole
            };
            
            // Enviar al usuario específico si está conectado
            await Clients.User(usuarioId.ToString()).SendAsync("RecibirNotificacion", notificacion);
            
            _logger.LogInformation("Notificación enviada al usuario {UsuarioId} exitosamente: {Titulo}", usuarioId, titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación al usuario {UsuarioId}: {Titulo}", usuarioId, titulo);
            throw;
        }
    }

    /// <summary>
    /// Envía un mensaje administrativo a todos los administradores
    /// </summary>
    public async Task EnviarMensajeAdmin(string titulo, string mensaje, string tipo = "warning")
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            throw new HubException("El título y el mensaje no pueden estar vacíos.");
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        // Verificar que el usuario sea administrador
        if (userRole != "Administrador" && userRole != "Admin")
        {
            _logger.LogWarning("Usuario {UserId} ({UserRole}) intentó enviar mensaje administrativo sin autorización", userId, userRole);
            throw new HubException("UnauthorizedAccessException: Solo los administradores pueden enviar mensajes administrativos");
        }
        
        _logger.LogInformation("Administrador {UserId} enviando mensaje administrativo: {Titulo}", userId, titulo);
        
        try
        {
            var mensajeAdmin = new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                FechaHora = DateTime.UtcNow,
                EsMensajeAdmin = true,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            await Clients.Group("Administrador").SendAsync("RecibirMensajeAdmin", mensajeAdmin);
            
            _logger.LogInformation("Mensaje administrativo enviado exitosamente: {Titulo}", titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje administrativo: {Titulo}", titulo);
            throw;
        }
    }

    /// <summary>
    /// Envía una alerta del sistema a todos los usuarios
    /// </summary>
    public async Task EnviarAlertaSistema(string titulo, string mensaje, string tipo = "error")
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            throw new HubException("El título y el mensaje no pueden estar vacíos.");
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        // Verificar que el usuario tenga permisos para enviar alertas del sistema
        if (userRole != "Administrador" && userRole != "Admin" && userRole != "Sistema")
        {
            _logger.LogWarning("Usuario {UserId} ({UserRole}) intentó enviar alerta del sistema sin autorización", userId, userRole);
            throw new HubException("UnauthorizedAccessException: Solo los administradores pueden enviar alertas del sistema");
        }
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando alerta del sistema: {Titulo}", userId, userRole, titulo);
        
        try
        {
            var alertaSistema = new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                FechaHora = DateTime.UtcNow,
                EsAlertaSistema = true,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            await Clients.All.SendAsync("RecibirAlertaSistema", alertaSistema);
            
            _logger.LogInformation("Alerta del sistema enviada exitosamente: {Titulo}", titulo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar alerta del sistema: {Titulo}", titulo);
            throw;
        }
    }

    /// <summary>
    /// Unirse a un grupo específico para recibir notificaciones
    /// </summary>
    public async Task UnirseAGrupo(string nombreGrupo)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) uniéndose al grupo {Grupo}", userId, userRole, nombreGrupo);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, nombreGrupo);
        
        _logger.LogInformation("Usuario {UserId} agregado exitosamente al grupo {Grupo}", userId, nombreGrupo);
    }

    /// <summary>
    /// Salir de un grupo específico
    /// </summary>
    public async Task SalirDeGrupo(string nombreGrupo)
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) saliendo del grupo {Grupo}", userId, userRole, nombreGrupo);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, nombreGrupo);
        
        _logger.LogInformation("Usuario {UserId} removido exitosamente del grupo {Grupo}", userId, nombreGrupo);
    }

    /// <summary>
    /// Ping/Pong para verificar conectividad
    /// </summary>
    public async Task Ping()
    {
        var userId = GetUserIdFromClaims();
        _logger.LogDebug("Ping recibido del usuario {UserId}", userId);
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    private Guid GetUserIdFromClaims()
    {
        // Intentar obtener el ID del usuario desde diferentes tipos de claims
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? Context.User?.FindFirst("sub")?.Value
                         ?? Context.User?.FindFirst("nameid")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("No se pudo obtener el ID del usuario desde los claims");
            return Guid.Empty;
        }
        
        // Si es un email, generar un GUID basado en el email para consistencia
        if (userIdClaim.Contains("@"))
        {
            // Para tests, usar un GUID consistente basado en el email
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(userIdClaim));
            return new Guid(hash);
        }
        
        // Intentar parsear como GUID
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        _logger.LogWarning("No se pudo parsear el ID del usuario como GUID: {UserIdClaim}", userIdClaim);
        return Guid.Empty;
    }

    private string GetUserRoleFromClaims()
    {
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaim ?? "Usuario";
    }
} 