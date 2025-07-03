using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System.Security.Claims;

namespace RestaurantePro.Api.Hubs;

[Authorize]
public class InventarioHub : Hub
{
    private readonly ILogger<InventarioHub> _logger;
    private readonly ISignalRService _signalRService;

    public InventarioHub(ILogger<InventarioHub> logger, ISignalRService signalRService)
    {
        _logger = logger;
        _signalRService = signalRService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} con rol {UserRole} conectado al InventarioHub", userId, userRole);
        
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
        
        _logger.LogInformation("Usuario {UserId} con rol {UserRole} desconectado del InventarioHub", userId, userRole);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envía una alerta de stock bajo para un ingrediente específico
    /// </summary>
    public async Task AlertaStockBajo(Guid ingredienteId, string nombre, decimal stockActual, decimal stockMinimo, string unidadMedida)
    {
        if (ingredienteId == Guid.Empty || string.IsNullOrWhiteSpace(nombre))
            throw new HubException("El ID del ingrediente y el nombre no pueden estar vacíos.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando alerta de stock bajo para {Ingrediente}: {StockActual} {UnidadMedida}", 
            userId, userRole, nombre, stockActual, unidadMedida);
        
        try
        {
            var alerta = new
            {
                IngredienteId = ingredienteId,
                NombreIngrediente = nombre,
                StockActual = stockActual,
                StockMinimo = stockMinimo,
                UnidadMedida = unidadMedida,
                TipoAlerta = "StockBajo",
                FechaAlerta = DateTime.UtcNow,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            // Enviar a todos los usuarios con roles de inventario y administradores
            await Clients.Groups("Administrador", "Gerente", "EncargadoInventario").SendAsync("RecibirAlertaStock", alerta);
            
            _logger.LogInformation("Alerta de stock bajo enviada exitosamente para {Ingrediente}", nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar alerta de stock bajo para {Ingrediente}", nombre);
            throw;
        }
    }

    /// <summary>
    /// Notifica la recepción de mercancía de una orden de compra
    /// </summary>
    public async Task RecepcionMercancia(Guid ordenCompraId, string numeroOrden, List<object> items, DateTime fechaRecepcion)
    {
        if (ordenCompraId == Guid.Empty || string.IsNullOrWhiteSpace(numeroOrden) || items == null || !items.Any())
            throw new HubException("Los datos de la orden de compra y los items no pueden estar vacíos.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) notificando recepción de mercancía para orden {NumeroOrden}", 
            userId, userRole, numeroOrden);
        
        try
        {
            var recepcion = new
            {
                OrdenCompraId = ordenCompraId,
                NumeroOrden = numeroOrden,
                Items = items,
                FechaRecepcion = fechaRecepcion,
                CantidadItems = items.Count,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            // Enviar a todos los usuarios con roles de inventario y administradores
            await Clients.Groups("Administrador", "Gerente", "EncargadoInventario").SendAsync("RecibirActualizacionInventario", recepcion);
            
            _logger.LogInformation("Notificación de recepción de mercancía enviada exitosamente para orden {NumeroOrden}", numeroOrden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar recepción de mercancía para orden {NumeroOrden}", numeroOrden);
            throw;
        }
    }

    /// <summary>
    /// Envía una alerta de producto próximo a vencer
    /// </summary>
    public async Task ProductoPorVencer(Guid ingredienteId, string nombre, DateTime fechaVencimiento, int diasRestantes)
    {
        if (ingredienteId == Guid.Empty || string.IsNullOrWhiteSpace(nombre))
            throw new HubException("El ID del ingrediente y el nombre no pueden estar vacíos.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando alerta de vencimiento para {Ingrediente}: vence en {DiasRestantes} días", 
            userId, userRole, nombre, diasRestantes);
        
        try
        {
            var alerta = new
            {
                IngredienteId = ingredienteId,
                NombreIngrediente = nombre,
                FechaVencimiento = fechaVencimiento,
                DiasRestantes = diasRestantes,
                TipoAlerta = "PorVencer",
                FechaAlerta = DateTime.UtcNow,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            // Enviar a todos los usuarios con roles de inventario y administradores
            await Clients.Groups("Administrador", "Gerente", "EncargadoInventario").SendAsync("RecibirAlertaVencimiento", alerta);
            
            _logger.LogInformation("Alerta de vencimiento enviada exitosamente para {Ingrediente}", nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar alerta de vencimiento para {Ingrediente}", nombre);
            throw;
        }
    }

    /// <summary>
    /// Envía una alerta de stock agotado
    /// </summary>
    public async Task AlertaStockAgotado(Guid ingredienteId, string nombre, string unidadMedida)
    {
        if (ingredienteId == Guid.Empty || string.IsNullOrWhiteSpace(nombre))
            throw new HubException("El ID del ingrediente y el nombre no pueden estar vacíos.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) enviando alerta de stock agotado para {Ingrediente}", 
            userId, userRole, nombre);
        
        try
        {
            var alerta = new
            {
                IngredienteId = ingredienteId,
                NombreIngrediente = nombre,
                UnidadMedida = unidadMedida,
                TipoAlerta = "StockAgotado",
                FechaAlerta = DateTime.UtcNow,
                EnviadoPor = userId,
                RolEnviadoPor = userRole
            };
            
            // Enviar a todos los usuarios con roles de inventario y administradores
            await Clients.Groups("Administrador", "Gerente", "EncargadoInventario").SendAsync("RecibirAlertaStock", alerta);
            
            _logger.LogInformation("Alerta de stock agotado enviada exitosamente para {Ingrediente}", nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar alerta de stock agotado para {Ingrediente}", nombre);
            throw;
        }
    }

    /// <summary>
    /// Permite a un usuario unirse a un grupo específico
    /// </summary>
    public async Task UnirseAGrupo(string nombreGrupo)
    {
        if (string.IsNullOrWhiteSpace(nombreGrupo))
            throw new HubException("El nombre del grupo no puede estar vacío.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) uniéndose al grupo {Grupo}", userId, userRole, nombreGrupo);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, nombreGrupo);
        
        _logger.LogInformation("Usuario {UserId} agregado exitosamente al grupo {Grupo}", userId, nombreGrupo);
    }

    /// <summary>
    /// Permite a un usuario salir de un grupo específico
    /// </summary>
    public async Task SalirDeGrupo(string nombreGrupo)
    {
        if (string.IsNullOrWhiteSpace(nombreGrupo))
            throw new HubException("El nombre del grupo no puede estar vacío.");
        
        var userId = GetUserIdFromClaims();
        var userRole = GetUserRoleFromClaims();
        
        _logger.LogInformation("Usuario {UserId} ({UserRole}) saliendo del grupo {Grupo}", userId, userRole, nombreGrupo);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, nombreGrupo);
        
        _logger.LogInformation("Usuario {UserId} removido exitosamente del grupo {Grupo}", userId, nombreGrupo);
    }

    /// <summary>
    /// Método de ping para verificar conectividad
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    /// <summary>
    /// Obtiene el ID del usuario desde los claims de autenticación
    /// </summary>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // Fallback: intentar obtener del email si el NameIdentifier no está disponible
            var emailClaim = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(emailClaim))
            {
                // Generar un GUID determinístico basado en el email para tests
                var hash = System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(emailClaim));
                return new Guid(hash.Take(16).ToArray());
            }
            
            _logger.LogWarning("No se pudo obtener el ID del usuario desde los claims");
            return Guid.Empty;
        }
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        _logger.LogWarning("No se pudo parsear el ID del usuario: {UserIdClaim}", userIdClaim);
        return Guid.Empty;
    }

    /// <summary>
    /// Obtiene el rol del usuario desde los claims de autenticación
    /// </summary>
    private string GetUserRoleFromClaims()
    {
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(roleClaim))
        {
            _logger.LogWarning("No se pudo obtener el rol del usuario desde los claims");
            return "Empleado"; // Rol por defecto
        }
        
        return roleClaim;
    }
} 