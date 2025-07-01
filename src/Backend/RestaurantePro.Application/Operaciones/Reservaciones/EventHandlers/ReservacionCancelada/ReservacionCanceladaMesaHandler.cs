using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCancelada;

/// <summary>
/// 🪑 Handler para liberar la mesa cuando se cancela una reservación
/// </summary>
public class ReservacionCanceladaMesaHandler : IDomainEventHandler<RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCancelada>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReservacionCanceladaMesaHandler> _logger;

    public ReservacionCanceladaMesaHandler(
        IMesaRepository mesaRepository,
        IReservacionRepository reservacionRepository,
        IApplicationDbContext context,
        ILogger<ReservacionCanceladaMesaHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _reservacionRepository = reservacionRepository;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCancelada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MesaHandler] INICIO Handle: ReservacionId={ReservacionId}", evento.ReservacionId);

        try
        {
            // Buscar la reservación para obtener la mesa asociada
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(evento.ReservacionId, cancellationToken);
            
            if (reservacion?.MesaId == null)
            {
                _logger.LogWarning("❌ No se encontró la reservación {ReservacionId} o no tiene mesa asociada", evento.ReservacionId);
                return;
            }

            var mesaId = reservacion.MesaId;
            _logger.LogInformation("🔄 Liberando mesa {MesaId} para Reservación cancelada {ReservacionId}", mesaId, evento.ReservacionId);

            // Obtener la mesa
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId, cancellationToken);
            if (mesa == null)
            {
                _logger.LogError("❌ No se encontró la mesa {MesaId} para liberar", mesaId);
                return;
            }

            // Liberar la mesa (cambiar de Reservada a Disponible)
            if (mesa.Estado == EstadoMesa.Reservada)
            {
                mesa.MarcarComoDisponible();
                
                // Guardar los cambios en el mismo contexto
                _context.Mesas.Update(mesa);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("✅ Mesa {MesaId} liberada exitosamente para Reservación cancelada {ReservacionId}", 
                    mesaId, evento.ReservacionId);
            }
            else
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} no está en estado Reservada (estado actual: {Estado})", 
                    mesaId, mesa.Estado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error liberando mesa para reservación cancelada {ReservacionId}", evento.ReservacionId);
            throw;
        }

        _logger.LogInformation("[MesaHandler] FIN Handle: ReservacionId={ReservacionId}", evento.ReservacionId);
    }
} 