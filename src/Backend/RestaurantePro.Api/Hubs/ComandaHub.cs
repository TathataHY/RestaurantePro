using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using System.Security.Claims;

namespace RestaurantePro.Api.Hubs;

/// <summary>
/// Hub principal para comunicación de comandas en tiempo real entre meseros y cocina
/// Gestiona la comunicación bidireccional para el flujo de comandas del restaurante
/// </summary>
[Authorize]
public class ComandaHub : Hub
{
    private readonly ILogger<ComandaHub> _logger;
    private readonly IHubConnectionManager _connectionManager;
    
    // Grupos predefinidos para diferentes roles
    private const string GRUPO_COCINA = "Cocina";
    private const string GRUPO_MESEROS = "Meseros";
    private const string GRUPO_ADMINISTRADORES = "Administradores";

    public ComandaHub(ILogger<ComandaHub> logger, IHubConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se conecta al hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromContext();
        var userRole = GetUserRoleFromContext();
        
        _logger.LogInformation("🔌 Cliente conectado al ComandaHub - Usuario: {UserId}, Rol: {UserRole}, ConnectionId: {ConnectionId}", 
            userId, userRole, Context.ConnectionId);

        // Registrar conexión en el gestor de conexiones
        if (Guid.TryParse(userId, out var userIdGuid))
        {
            await _connectionManager.AgregarConexionAsync(userIdGuid, Context.ConnectionId, userRole);
        }

        // Agregar usuario al grupo correspondiente según su rol
        await AddUserToRoleGroup(userRole);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se desconecta del hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromContext();
        var userRole = GetUserRoleFromContext();
        
        _logger.LogInformation("🔌 Cliente desconectado del ComandaHub - Usuario: {UserId}, Rol: {UserRole}, ConnectionId: {ConnectionId}", 
            userId, userRole, Context.ConnectionId);

        // Remover conexión del gestor de conexiones
        await _connectionManager.RemoverConexionAsync(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    #region Métodos del Hub (llamados desde el servidor)

    /// <summary>
    /// Envía una nueva comanda a la cocina
    /// </summary>
    /// <param name="comanda">Datos de la comanda</param>
    public async Task NuevaComanda(ComandaSummaryDto comanda)
    {
        _logger.LogInformation("🍽️ Nueva comanda enviada a cocina - Comanda: {ComandaId}, Mesa: {MesaId}", 
            comanda.Id, comanda.MesaId);

        await Clients.Group(GRUPO_COCINA).SendAsync("RecibirNuevaComanda", comanda);
    }

    /// <summary>
    /// Actualiza el estado de una comanda y notifica a todos los interesados
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="estado">Nuevo estado</param>
    /// <param name="detalles">Detalles adicionales (opcional)</param>
    public async Task ActualizarEstadoComanda(Guid comandaId, string estado, object? detalles = null)
    {
        _logger.LogInformation("🔄 Estado de comanda actualizado - Comanda: {ComandaId}, Estado: {Estado}", 
            comandaId, estado);

        var actualizacion = new
        {
            ComandaId = comandaId,
            Estado = estado,
            Detalles = detalles,
            FechaActualizacion = DateTime.UtcNow
        };

        // Notificar a todos los grupos relevantes
        await Clients.Groups(GRUPO_COCINA, GRUPO_MESEROS).SendAsync("ComandaActualizada", actualizacion);
    }

    /// <summary>
    /// Notifica que una comanda está lista para servir
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="mesaId">ID de la mesa</param>
    public async Task ComandaLista(Guid comandaId, Guid mesaId)
    {
        _logger.LogInformation("✅ Comanda lista para servir - Comanda: {ComandaId}, Mesa: {MesaId}", 
            comandaId, mesaId);

        var notificacion = new
        {
            ComandaId = comandaId,
            MesaId = mesaId,
            FechaLista = DateTime.UtcNow
        };

        await Clients.Group(GRUPO_MESEROS).SendAsync("ComandaListaParaServir", notificacion);
    }

    /// <summary>
    /// Confirma que una comanda ha sido entregada al cliente
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="mesaId">ID de la mesa</param>
    public async Task ComandaEntregada(Guid comandaId, Guid mesaId)
    {
        _logger.LogInformation("🎯 Comanda entregada al cliente - Comanda: {ComandaId}, Mesa: {MesaId}", 
            comandaId, mesaId);

        var confirmacion = new
        {
            ComandaId = comandaId,
            MesaId = mesaId,
            FechaEntrega = DateTime.UtcNow
        };

        await Clients.Groups(GRUPO_COCINA, GRUPO_MESEROS).SendAsync("ComandaEntregadaConfirmada", confirmacion);
    }

    /// <summary>
    /// Cancela una comanda y notifica a todos los interesados
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="motivo">Motivo de la cancelación</param>
    /// <param name="canceladoPor">Usuario que cancela la comanda</param>
    public async Task CancelarComanda(Guid comandaId, string motivo, string canceladoPor)
    {
        _logger.LogInformation("❌ Comanda cancelada - Comanda: {ComandaId}, Motivo: {Motivo}, Cancelado por: {CanceladoPor}", 
            comandaId, motivo, canceladoPor);

        var cancelacion = new
        {
            ComandaId = comandaId,
            Motivo = motivo,
            CanceladoPor = canceladoPor,
            FechaCancelacion = DateTime.UtcNow
        };

        await Clients.Groups(GRUPO_COCINA, GRUPO_MESEROS, GRUPO_ADMINISTRADORES).SendAsync("ComandaCancelada", cancelacion);
    }

    /// <summary>
    /// Cambia la prioridad de una comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="prioridad">Nueva prioridad (Normal, Alta, Urgente)</param>
    /// <param name="cambiadoPor">Usuario que cambia la prioridad</param>
    public async Task AsignarPrioridad(Guid comandaId, string prioridad, string cambiadoPor)
    {
        _logger.LogInformation("⚡ Prioridad de comanda cambiada - Comanda: {ComandaId}, Prioridad: {Prioridad}, Cambiado por: {CambiadoPor}", 
            comandaId, prioridad, cambiadoPor);

        var cambioPrioridad = new
        {
            ComandaId = comandaId,
            Prioridad = prioridad,
            CambiadoPor = cambiadoPor,
            FechaCambio = DateTime.UtcNow
        };

        await Clients.Groups(GRUPO_COCINA, GRUPO_MESEROS).SendAsync("PrioridadCambiada", cambioPrioridad);
    }

    /// <summary>
    /// Notifica un retraso en la preparación de una comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="motivo">Motivo del retraso</param>
    /// <param name="minutosRetraso">Minutos de retraso estimado</param>
    public async Task NotificarRetraso(Guid comandaId, string motivo, int minutosRetraso)
    {
        _logger.LogInformation("⏰ Retraso notificado - Comanda: {ComandaId}, Motivo: {Motivo}, Retraso: {MinutosRetraso} minutos", 
            comandaId, motivo, minutosRetraso);

        var retraso = new
        {
            ComandaId = comandaId,
            Motivo = motivo,
            MinutosRetraso = minutosRetraso,
            FechaNotificacion = DateTime.UtcNow
        };

        await Clients.Group(GRUPO_MESEROS).SendAsync("RetrasoNotificado", retraso);
    }

    /// <summary>
    /// Solicita ayuda para una comanda específica
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="tipoAyuda">Tipo de ayuda solicitada</param>
    /// <param name="solicitadoPor">Usuario que solicita ayuda</param>
    public async Task SolicitarAyuda(Guid comandaId, string tipoAyuda, string solicitadoPor)
    {
        _logger.LogInformation("🆘 Ayuda solicitada - Comanda: {ComandaId}, Tipo: {TipoAyuda}, Solicitado por: {SolicitadoPor}", 
            comandaId, tipoAyuda, solicitadoPor);

        var solicitudAyuda = new
        {
            ComandaId = comandaId,
            TipoAyuda = tipoAyuda,
            SolicitadoPor = solicitadoPor,
            FechaSolicitud = DateTime.UtcNow
        };

        await Clients.Groups(GRUPO_ADMINISTRADORES, GRUPO_COCINA).SendAsync("AyudaSolicitada", solicitudAyuda);
    }

    /// <summary>
    /// Confirma la recepción de una comanda por parte de la cocina
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="confirmadoPor">Usuario que confirma la recepción</param>
    public async Task ConfirmarRecepcion(Guid comandaId, string confirmadoPor)
    {
        _logger.LogInformation("✅ Recepción confirmada - Comanda: {ComandaId}, Confirmado por: {ConfirmadoPor}", 
            comandaId, confirmadoPor);

        var confirmacion = new
        {
            ComandaId = comandaId,
            ConfirmadoPor = confirmadoPor,
            FechaConfirmacion = DateTime.UtcNow
        };

        await Clients.Group(GRUPO_MESEROS).SendAsync("RecepcionConfirmada", confirmacion);
    }

    /// <summary>
    /// Actualiza el tiempo estimado de entrega de una comanda
    /// </summary>
    /// <param name="comandaId">ID de la comanda</param>
    /// <param name="minutosEstimados">Nuevo tiempo estimado en minutos</param>
    /// <param name="actualizadoPor">Usuario que actualiza el tiempo</param>
    public async Task ActualizarTiempoEstimado(Guid comandaId, int minutosEstimados, string actualizadoPor)
    {
        _logger.LogInformation("⏱️ Tiempo estimado actualizado - Comanda: {ComandaId}, Tiempo: {MinutosEstimados} min, Actualizado por: {ActualizadoPor}", 
            comandaId, minutosEstimados, actualizadoPor);

        var actualizacionTiempo = new
        {
            ComandaId = comandaId,
            MinutosEstimados = minutosEstimados,
            FechaEstimadaEntrega = DateTime.UtcNow.AddMinutes(minutosEstimados),
            ActualizadoPor = actualizadoPor,
            FechaActualizacion = DateTime.UtcNow
        };

        await Clients.Groups(GRUPO_MESEROS, GRUPO_COCINA).SendAsync("TiempoEstimadoActualizado", actualizacionTiempo);
    }

    #endregion

    #region Métodos del Cliente (llamados desde el cliente)

    /// <summary>
    /// Permite a un cliente unirse a un grupo específico
    /// </summary>
    /// <param name="groupName">Nombre del grupo</param>
    public async Task JoinGroup(string groupName)
    {
        var userId = GetUserIdFromContext();
        
        _logger.LogInformation("👥 Usuario {UserId} se une al grupo {GroupName}", userId, groupName);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Confirmar al cliente que se unió al grupo
        await Clients.Caller.SendAsync("GrupoJoined", groupName);
    }

    /// <summary>
    /// Permite a un cliente salir de un grupo específico
    /// </summary>
    /// <param name="groupName">Nombre del grupo</param>
    public async Task LeaveGroup(string groupName)
    {
        var userId = GetUserIdFromContext();
        
        _logger.LogInformation("👥 Usuario {UserId} sale del grupo {GroupName}", userId, groupName);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        // Confirmar al cliente que salió del grupo
        await Clients.Caller.SendAsync("GrupoLeft", groupName);
    }

    /// <summary>
    /// Método de ping para verificar conectividad
    /// </summary>
    public async Task Ping()
    {
        _logger.LogInformation("🏓 Ping recibido - ConnectionId: {ConnectionId}", Context.ConnectionId);
        
        // Actualizar timestamp de la conexión para mantenerla activa
        await _connectionManager.ActualizarTimestampConexionAsync(Context.ConnectionId);
        
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    /// <summary>
    /// Obtiene estadísticas de conexiones del hub
    /// </summary>
    public async Task ObtenerEstadisticasConexiones()
    {
        var userId = GetUserIdFromContext();
        var userRole = GetUserRoleFromContext();
        
        // Solo administradores pueden ver estadísticas
        if (userRole != "Administrador")
        {
            _logger.LogWarning("🚫 Usuario {UserId} intentó obtener estadísticas sin autorización", userId);
            return;
        }

        var estadisticas = await _connectionManager.ObtenerEstadisticasConexionesAsync();
        
        _logger.LogInformation("📊 Estadísticas solicitadas por usuario {UserId}", userId);
        
        await Clients.Caller.SendAsync("EstadisticasConexiones", estadisticas);
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Obtiene el ID del usuario desde el contexto de autenticación
    /// </summary>
    private string GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value ?? "Unknown";
    }

    /// <summary>
    /// Obtiene el rol del usuario desde el contexto de autenticación
    /// </summary>
    private string GetUserRoleFromContext()
    {
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? "Usuario";
    }

    /// <summary>
    /// Agrega al usuario al grupo correspondiente según su rol
    /// </summary>
    private async Task AddUserToRoleGroup(string userRole)
    {
        string groupName = userRole.ToLower() switch
        {
            "cocinero" or "chef" => GRUPO_COCINA,
            "mesero" or "camarero" => GRUPO_MESEROS,
            "administrador" or "gerente" => GRUPO_ADMINISTRADORES,
            _ => GRUPO_MESEROS // Por defecto, grupo de meseros
        };

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("👥 Usuario agregado automáticamente al grupo {GroupName} por rol {UserRole}", 
            groupName, userRole);
    }

    #endregion
} 