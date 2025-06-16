using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces.Services;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;

/// <summary>
/// Trabajo para detectar y notificar usuarios inactivos en el sistema
/// </summary>
public class UserInactivityJob : BackgroundJobBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserInactivityOptions _options;

    /// <inheritdoc/>
    public override string JobName => "UserInactivity";

    /// <inheritdoc/>
    public override string Description => "Detecta usuarios inactivos y envía notificaciones automáticas";

    public UserInactivityJob(
        ILogger<UserInactivityJob> logger,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<UserInactivityOptions> options) : base(logger)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        var fechaInactividad = DateTime.UtcNow.AddDays(-_options.InactivityThresholdDays);
        _logger.LogInformation("Buscando usuarios inactivos desde {FechaInactividad}", fechaInactividad);

        var usuariosInactivos = await _usuarioRepository.ObtenerInactivosDesdeAsync(fechaInactividad, cancellationToken);
        
        if (!usuariosInactivos.Any())
        {
            _logger.LogInformation("No se encontraron usuarios inactivos");
            return;
        }

        _logger.LogInformation("Se encontraron {Cantidad} usuarios inactivos", usuariosInactivos.Count());

        foreach (var usuario in usuariosInactivos)
        {
            try
            {
                // Enviar email recordatorio
                if (_options.SendReminderEmails)
                {
                    await _emailService.EnviarCorreoAsync(
                        destinatario: usuario.Email,
                        asunto: "Te extrañamos en RestaurantePro",
                        contenido: $"Hola {usuario.Nombre}, hace tiempo que no accedes a RestaurantePro. ¡Te esperamos pronto!",
                        cancellationToken: cancellationToken);
                }

                // Marcar usuario como notificado
                usuario.MarcarComoNotificadoInactividad();
                await _unitOfWork.GuardarCambiosAsync(cancellationToken);
                
                _logger.LogInformation("Usuario {UsuarioId} notificado por inactividad", usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar usuario inactivo {UsuarioId}: {Error}", usuario.Id, ex.Message);
            }
        }
    }
}

/// <summary>
/// Opciones de configuración para la detección de usuarios inactivos
/// </summary>
public class UserInactivityOptions
{
    /// <summary>
    /// Número de días de inactividad para considerar a un usuario como inactivo
    /// </summary>
    public int InactivityThresholdDays { get; set; } = 30;
    
    /// <summary>
    /// Indica si se deben enviar emails recordatorios a los usuarios inactivos
    /// </summary>
    public bool SendReminderEmails { get; set; } = true;
} 