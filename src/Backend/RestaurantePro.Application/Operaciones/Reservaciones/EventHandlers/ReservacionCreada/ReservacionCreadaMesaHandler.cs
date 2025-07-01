using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

/// <summary>
/// 🪑 Handler que procesa el evento ReservacionCreada para actualizar el estado de la mesa
/// </summary>
public class ReservacionCreadaMesaHandler : IDomainEventHandler<RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReservacionCreadaMesaHandler> _logger;

    public ReservacionCreadaMesaHandler(
        IMesaRepository mesaRepository,
        IApplicationDbContext context,
        ILogger<ReservacionCreadaMesaHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 🚀 Procesa la creación de reservación para actualizar el estado de la mesa
    /// </summary>
    public async Task Handle(RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"[MesaHandler] INICIO Handle: ReservacionId={evento.ReservacionId}, MesaId={evento.MesaId}");
        _logger.LogInformation("🪑 Actualizando estado de mesa {MesaId} a Reservada para Reservación {ReservacionId}", 
            evento.MesaId, evento.ReservacionId);

        try
        {
            // 1. Obtener la mesa
            var mesa = await _mesaRepository.ObtenerPorIdAsync(evento.MesaId, cancellationToken);
            if (mesa == null)
            {
                _logger.LogWarning("⚠️ No se encontró la mesa {MesaId} para actualizar estado", evento.MesaId);
                return;
            }

            // 2. Verificar que la mesa esté disponible
            if (mesa.Estado != EstadoMesa.Disponible)
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} no está disponible (Estado: {EstadoActual}), no se puede reservar", 
                    evento.MesaId, mesa.Estado);
                return;
            }

            // 3. Cambiar el estado de la mesa a Reservada
            mesa.MarcarComoReservada();

            // 4. Guardar los cambios en el mismo contexto de la reservación
            // Esto asegura que el cambio se refleje en la misma transacción
            _context.Mesas.Update(mesa);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Mesa {MesaId} marcada como Reservada exitosamente para Reservación {ReservacionId}", 
                evento.MesaId, evento.ReservacionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al actualizar estado de mesa {MesaId} para Reservación {ReservacionId}", 
                evento.MesaId, evento.ReservacionId);
            throw;
        }
        _logger.LogInformation($"[MesaHandler] FIN Handle: ReservacionId={evento.ReservacionId}, MesaId={evento.MesaId}");
    }
} 