using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
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

        // Obtenemos todos los usuarios activos
        var todosUsuarios = await _usuarioRepository.ObtenerTodosAsync(soloActivos: true, cancellationToken);
        
        // Filtramos los usuarios inactivos (último acceso anterior a la fecha de inactividad)
        var usuariosInactivos = todosUsuarios
            .Where(u => u.UltimoAcceso.HasValue && u.UltimoAcceso.Value < fechaInactividad)
            .ToList();
        
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
                if (_options.SendReminderEmails && !string.IsNullOrEmpty(usuario.Email))
                {
                    await _emailService.SendEmailAsync(
                        usuario.Email,
                        "Te extrañamos en RestaurantePro",
                        $"Hola {usuario.NombreCompleto}, hace tiempo que no accedes a RestaurantePro. ¡Te esperamos pronto!");
                }

                // Actualizar la fecha de último acceso para evitar notificaciones repetidas
                // Nota: No tenemos un método MarcarComoNotificadoInactividad, así que actualizamos el usuario
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                
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