using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces.Services;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;

/// <summary>
/// Trabajo para gestionar la expiración de puntos de fidelización de clientes
/// </summary>
public class LoyaltyPointsExpirationJob : BackgroundJobBase
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoyaltyPointsExpirationOptions _options;

    /// <inheritdoc/>
    public override string JobName => "LoyaltyPointsExpiration";

    /// <inheritdoc/>
    public override string Description => "Gestiona la expiración de puntos de fidelización y notifica a los clientes";

    public LoyaltyPointsExpirationJob(
        ILogger<LoyaltyPointsExpirationJob> logger,
        ITarjetaFidelizacionRepository tarjetaRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<LoyaltyPointsExpirationOptions> options) : base(logger)
    {
        _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        // Fecha de expiración (normalmente puntos que expiran en los próximos días)
        var fechaExpiración = DateTime.UtcNow.AddDays(_options.DaysBeforeExpirationForNotification);
        _logger.LogInformation("Procesando puntos de fidelización que expirarán en {Dias} días", _options.DaysBeforeExpirationForNotification);

        // Obtener tarjetas con puntos próximos a expirar
        var tarjetas = await _tarjetaRepository.ObtenerConPuntosProximosAExpirarAsync(fechaExpiración, cancellationToken);
        
        if (!tarjetas.Any())
        {
            _logger.LogInformation("No se encontraron tarjetas con puntos próximos a expirar");
            return;
        }

        _logger.LogInformation("Se encontraron {Cantidad} tarjetas con puntos próximos a expirar", tarjetas.Count());

        foreach (var tarjeta in tarjetas)
        {
            try
            {
                // Procesar expiración o enviar notificación previa
                if (_options.AutomaticExpiration && DateTime.UtcNow >= tarjeta.FechaExpiración)
                {
                    // Expirar puntos automáticamente
                    tarjeta.ExpirarPuntos();
                    await _unitOfWork.GuardarCambiosAsync(cancellationToken);
                    
                    _logger.LogInformation("Puntos expirados automáticamente para la tarjeta {TarjetaId}", tarjeta.Id);
                    
                    // Notificar al cliente sobre la expiración
                    if (_options.SendExpirationNotifications && tarjeta.Cliente?.Email != null)
                    {
                        await _emailService.EnviarCorreoAsync(
                            destinatario: tarjeta.Cliente.Email,
                            asunto: "Tus puntos de fidelización han expirado",
                            contenido: $"Estimado/a {tarjeta.Cliente.Nombre}, lamentamos informarte que tus puntos de fidelización han expirado. ¡Sigue acumulando nuevos puntos en tu próxima visita!",
                            cancellationToken: cancellationToken);
                    }
                }
                else if (_options.SendExpirationWarnings && tarjeta.Cliente?.Email != null)
                {
                    // Enviar advertencia previa a la expiración
                    await _emailService.EnviarCorreoAsync(
                        destinatario: tarjeta.Cliente.Email,
                        asunto: "¡Tus puntos están a punto de expirar!",
                        contenido: $"Estimado/a {tarjeta.Cliente.Nombre}, te recordamos que tienes {tarjeta.PuntosProximosAExpirar} puntos que expirarán el {tarjeta.FechaExpiración:dd/MM/yyyy}. ¡Visítanos pronto para canjearlos!",
                        cancellationToken: cancellationToken);
                    
                    // Marcar como notificado
                    tarjeta.MarcarNotificacionExpiracion();
                    await _unitOfWork.GuardarCambiosAsync(cancellationToken);
                    
                    _logger.LogInformation("Notificación de expiración enviada al cliente {ClienteId} para la tarjeta {TarjetaId}", 
                        tarjeta.Cliente.Id, tarjeta.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar tarjeta {TarjetaId}: {Error}", tarjeta.Id, ex.Message);
            }
        }
    }
}

/// <summary>
/// Opciones de configuración para la expiración de puntos de fidelización
/// </summary>
public class LoyaltyPointsExpirationOptions
{
    /// <summary>
    /// Días antes de la expiración para enviar notificaciones
    /// </summary>
    public int DaysBeforeExpirationForNotification { get; set; } = 7;
    
    /// <summary>
    /// Indica si se deben expirar puntos automáticamente
    /// </summary>
    public bool AutomaticExpiration { get; set; } = true;
    
    /// <summary>
    /// Indica si se deben enviar notificaciones de expiración
    /// </summary>
    public bool SendExpirationNotifications { get; set; } = true;
    
    /// <summary>
    /// Indica si se deben enviar advertencias previas a la expiración
    /// </summary>
    public bool SendExpirationWarnings { get; set; } = true;
} 