using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;

/// <summary>
/// Trabajo para gestionar la expiración de puntos de fidelización de clientes
/// </summary>
public class LoyaltyPointsExpirationJob : BackgroundJobBase
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly IClienteRepository _clienteRepository;
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
        IClienteRepository clienteRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<LoyaltyPointsExpirationOptions> options) : base(logger)
    {
        _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        // Fecha de expiración (normalmente puntos que expiran en los próximos días)
        var fechaExpiracion = DateTime.UtcNow.AddDays(_options.DaysBeforeExpirationForNotification);
        _logger.LogInformation("Procesando puntos de fidelización que expirarán en {Dias} días", _options.DaysBeforeExpirationForNotification);

        // Obtener tarjetas con puntos próximos a expirar
        var tarjetas = await _tarjetaRepository.ObtenerConPuntosProximosAExpirarAsync(fechaExpiracion, cancellationToken);
        
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
                // Obtener información del cliente
                var cliente = await _clienteRepository.ObtenerPorIdAsync(tarjeta.ClienteId, cancellationToken);
                if (cliente == null)
                {
                    _logger.LogWarning("No se encontró el cliente {ClienteId} para la tarjeta {TarjetaId}", tarjeta.ClienteId, tarjeta.Id);
                    continue;
                }

                // Procesar expiración o enviar notificación previa
                if (_options.AutomaticExpiration)
                {
                    // Expirar puntos automáticamente
                    int puntosAExpirar = tarjeta.PuntosDisponibles;
                    tarjeta.ExpirarPuntos(puntosAExpirar, "Expiración automática de puntos");
                    
                    _unitOfWork.Set<TarjetaFidelizacion>().Update(tarjeta);
                    
                    _logger.LogInformation("Puntos expirados automáticamente para la tarjeta {TarjetaId}", tarjeta.Id);
                    
                    // Notificar al cliente sobre la expiración
                    if (_options.SendExpirationNotifications && !string.IsNullOrEmpty(cliente.Email))
                    {
                        await _emailService.SendEmailAsync(
                            cliente.Email,
                            "Tus puntos de fidelización han expirado",
                            $"Estimado/a {cliente.Nombre}, lamentamos informarte que tus puntos de fidelización han expirado. ¡Sigue acumulando nuevos puntos en tu próxima visita!");
                    }
                }
                else if (_options.SendExpirationWarnings && !string.IsNullOrEmpty(cliente.Email))
                {
                    // Enviar advertencia previa a la expiración
                    await _emailService.SendEmailAsync(
                        cliente.Email,
                        "¡Tus puntos están a punto de expirar!",
                        $"Estimado/a {cliente.Nombre}, te recordamos que tienes {tarjeta.PuntosDisponibles} puntos que expirarán pronto. ¡Visítanos pronto para canjearlos!");
                    
                    _logger.LogInformation("Notificación de expiración enviada al cliente {ClienteId} para la tarjeta {TarjetaId}", 
                        cliente.Id, tarjeta.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar tarjeta {TarjetaId}: {Error}", tarjeta.Id, ex.Message);
            }
        }
        
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);
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