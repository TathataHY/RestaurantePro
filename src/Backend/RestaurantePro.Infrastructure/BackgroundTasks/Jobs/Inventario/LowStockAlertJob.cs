using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;

/// <summary>
/// Trabajo para detectar y alertar sobre niveles bajos de stock en ingredientes
/// </summary>
public class LowStockAlertJob : BackgroundJobBase
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly LowStockAlertOptions _options;

    /// <inheritdoc/>
    public override string JobName => "LowStockAlert";

    /// <inheritdoc/>
    public override string Description => "Detecta ingredientes con stock bajo y envía alertas a los responsables";

    public LowStockAlertJob(
        ILogger<LowStockAlertJob> logger,
        IIngredienteRepository ingredienteRepository,
        IEmailService emailService,
        INotificationService notificationService,
        IUsuarioRepository usuarioRepository,
        IOptions<LowStockAlertOptions> options) : base(logger)
    {
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verificando ingredientes con stock bajo");

        // Obtener ingredientes con stock bajo
        var ingredientesBajoStock = await _ingredienteRepository.ObtenerConStockBajoAsync(cancellationToken);
        
        if (!ingredientesBajoStock.Any())
        {
            _logger.LogInformation("No se encontraron ingredientes con stock bajo");
            return;
        }

        _logger.LogInformation("Se encontraron {Cantidad} ingredientes con stock bajo", ingredientesBajoStock.Count());

        // Agrupar ingredientes por criticidad
        var ingredientesCriticos = ingredientesBajoStock.Where(i => EsStockCritico(i)).ToList();
        var ingredientesBajo = ingredientesBajoStock.Except(ingredientesCriticos).ToList();
        
        // Notificar por email a gerentes si hay ingredientes críticos
        if (ingredientesCriticos.Any() && _options.SendEmailAlerts)
        {
            await EnviarAlertasEmailAsync(ingredientesCriticos, true, cancellationToken);
        }
        
        // Notificar en sistema a todos los responsables de inventario
        if (_options.SendSystemNotifications)
        {
            await EnviarNotificacionesSistemaAsync(ingredientesCriticos, ingredientesBajo, cancellationToken);
        }
    }
    
    private bool EsStockCritico(Ingrediente ingrediente)
    {
        // Un ingrediente es crítico si su stock está por debajo del umbral crítico definido en las opciones
        var nivelCritico = ingrediente.StockMinimo * (_options.CriticalThresholdPercentage / 100.0m);
        return ingrediente.Stock <= nivelCritico;
    }
    
    private async Task EnviarAlertasEmailAsync(List<Ingrediente> ingredientes, bool esCritico, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ObtenerPorRolAsync(RolUsuario.EncargadoInventario, cancellationToken);
        
        if (!usuarios.Any())
        {
            _logger.LogWarning("No se encontraron usuarios con rol {Rol} para enviar alertas", RolUsuario.EncargadoInventario);
            return;
        }
        
        var asunto = esCritico 
            ? "¡ALERTA CRÍTICA! Ingredientes con stock crítico" 
            : "Alerta: Ingredientes con stock bajo";
            
        var contenido = GenerarContenidoEmail(ingredientes, esCritico);
        
        foreach (var usuario in usuarios)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    usuario.Email,
                    asunto,
                    contenido);
                
                _logger.LogInformation("Alerta enviada al usuario {UsuarioId}", usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar alerta al usuario {UsuarioId}: {Error}", usuario.Id, ex.Message);
            }
        }
    }
    
    private async Task EnviarNotificacionesSistemaAsync(List<Ingrediente> ingredientesCriticos, List<Ingrediente> ingredientesBajo, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ObtenerPorRolAsync(RolUsuario.EncargadoInventario, cancellationToken);
        
        if (!usuarios.Any())
        {
            _logger.LogWarning("No se encontraron usuarios con rol {Rol} para enviar notificaciones", RolUsuario.EncargadoInventario);
            return;
        }
        
        foreach (var usuario in usuarios)
        {
            try
            {
                // Notificar ingredientes críticos
                if (ingredientesCriticos.Any())
                {
                    var mensaje = $"¡ALERTA CRÍTICA! Hay {ingredientesCriticos.Count} ingredientes con stock crítico que requieren atención inmediata.";
                    await _notificationService.EnviarNotificacionAsync(
                        usuarioId: usuario.Id,
                        titulo: "Stock Crítico",
                        mensaje: mensaje,
                        tipo: "critical");
                }
                
                // Notificar ingredientes con stock bajo
                if (ingredientesBajo.Any())
                {
                    var mensaje = $"Hay {ingredientesBajo.Count} ingredientes con stock bajo que deben reponerse pronto.";
                    await _notificationService.EnviarNotificacionAsync(
                        usuarioId: usuario.Id,
                        titulo: "Stock Bajo",
                        mensaje: mensaje,
                        tipo: "warning");
                }
                
                _logger.LogInformation("Notificaciones enviadas al usuario {UsuarioId}", usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación al usuario {UsuarioId}: {Error}", usuario.Id, ex.Message);
            }
        }
    }
    
    private string GenerarContenidoEmail(List<Ingrediente> ingredientes, bool esCritico)
    {
        var prioridad = esCritico ? "crítico" : "bajo";
        var contenido = $"<h2>Alerta de stock {prioridad}</h2>" +
                        $"<p>Los siguientes ingredientes tienen un nivel de stock {prioridad} y requieren atención:</p>" +
                        "<table border='1' cellpadding='5'>" +
                        "<tr><th>Ingrediente</th><th>Stock Actual</th><th>Stock Mínimo</th><th>Unidad</th></tr>";
                        
        foreach (var ingrediente in ingredientes)
        {
            contenido += $"<tr>" +
                         $"<td>{ingrediente.Nombre}</td>" +
                         $"<td>{ingrediente.Stock}</td>" +
                         $"<td>{ingrediente.StockMinimo}</td>" +
                         $"<td>{ingrediente.UnidadMedida}</td>" +
                         $"</tr>";
        }
        
        contenido += "</table>" +
                     "<p>Por favor, realice los pedidos necesarios para reponer el inventario lo antes posible.</p>";
                     
        if (esCritico)
        {
            contenido += "<p><strong>¡ATENCIÓN! Estos ingredientes requieren acción inmediata para evitar problemas en la operación.</strong></p>";
        }
        
        return contenido;
    }
}

/// <summary>
/// Opciones de configuración para las alertas de stock bajo
/// </summary>
public class LowStockAlertOptions
{
    /// <summary>
    /// Porcentaje del stock mínimo para considerar un nivel crítico
    /// </summary>
    public decimal CriticalThresholdPercentage { get; set; } = 50;
    
    /// <summary>
    /// Rol de usuarios encargados del inventario (obsoleto, se usa RolUsuario.EncargadoInventario)
    /// </summary>
    [Obsolete("Use RolUsuario.EncargadoInventario instead")]
    public string InventoryManagerRole { get; set; } = "Inventario.Gestor";
    
    /// <summary>
    /// Indica si se deben enviar alertas por email
    /// </summary>
    public bool SendEmailAlerts { get; set; } = true;
    
    /// <summary>
    /// Indica si se deben enviar notificaciones en el sistema
    /// </summary>
    public bool SendSystemNotifications { get; set; } = true;
} 