namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// 🪑 Handler que procesa el evento ComandaFinalizada para liberar mesa automáticamente
/// </summary>
public class ComandaFinalizadaMesaHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<ComandaFinalizadaMesaHandler> _logger;
    private readonly IMediator _mediator;

    public ComandaFinalizadaMesaHandler(
        IComandaRepository comandaRepository,
        IMesaRepository mesaRepository,
        ILogger<ComandaFinalizadaMesaHandler> logger,
        IMediator mediator)
    {
        _comandaRepository = comandaRepository;
        _mesaRepository = mesaRepository;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Maneja el evento de comanda finalizada liberando la mesa asociada
    /// </summary>
    public async Task Handle(Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🍽️ Comanda {ComandaId} finalizada con monto {Monto:C}. Procesando liberación de mesa.", notification.ComandaId, notification.Total);

            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(notification.ComandaId, cancellationToken);
            
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda {ComandaId} no encontrada para liberación de mesa.", notification.ComandaId);
                return;
            }

            // Verificar si tiene mesa asignada
            if (comanda.MesaId == Guid.Empty)
            {
                _logger.LogInformation("ℹ️ Comanda {ComandaId} no tiene mesa asociada. Nada que liberar.", notification.ComandaId);
                return;
            }

            // Obtener la mesa asociada
            var mesaId = comanda.MesaId.Value; // Ya validamos que no es null arriba
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId, cancellationToken);

            if (mesa == null)
            {
                _logger.LogWarning("❓ Mesa {MesaId} asociada a la comanda {ComandaId} no encontrada.", mesaId, notification.ComandaId);
                return;
            }

            // Verificar si la mesa debe ser liberada
            if (mesa.Estado == EstadoMesa.Disponible || mesa.Estado == EstadoMesa.FueraDeServicio)
            {
                _logger.LogInformation("ℹ️ Mesa {MesaId} ya está {EstadoMesa}, no es necesario liberarla.", mesaId, mesa.Estado);
                return;
            }

            // Crear comando para liberar la mesa
            var liberarMesaCommand = new LiberarMesaCommand
            {
                MesaId = mesaId
            };

            // Registrar que se va a liberar la mesa
            LogDescripcionMesa(mesa);

            // Liberar mesa
            var resultado = await _mediator.Send(liberarMesaCommand, cancellationToken);
            
            if (resultado.Succeeded)
            {
                _logger.LogInformation("✅ Mesa {MesaId} liberada exitosamente tras finalizar comanda {ComandaId}.", mesaId, notification.ComandaId);
            }
            else
            {
                _logger.LogError("❌ Error liberando mesa {MesaId}: {Error}", mesaId, resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al procesar liberación de mesa para comanda {ComandaId}: {ErrorMessage}", notification.ComandaId, ex.Message);
            throw; // Propagamos la excepción para que pueda ser manejada por un nivel superior
        }
    }

    // Método helper para registrar descripción de mesa según capacidad
    private void LogDescripcionMesa(Mesa mesa)
    {
        string descripcion = ObtenerDescripcionMesa(mesa.Capacidad);
        _logger.LogInformation("🪑 Liberando {Descripcion} (ID: {MesaId}, Capacidad: {Capacidad})", 
            descripcion, mesa.Id, mesa.Capacidad);
    }

    // Método helper para obtener descripción de mesa según capacidad
    private string ObtenerDescripcionMesa(int capacidad)
    {
        return capacidad switch
        {
            <= 2 => "👥 Mesa pequeña",
            <= 4 => "🪑 Mesa estándar",
            <= 6 => "👥 Mesa familiar",
            <= 10 => "🎉 Mesa grande",
            _ => "👑 Mesa VIP"
        };
    }
} 