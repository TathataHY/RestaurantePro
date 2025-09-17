using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Notifications;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using ComandaCreadaEvent = RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaCreada;

namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;

/// <summary>
/// 🔄 Handler que procesa el evento ComandaCreada para enviar notificaciones en tiempo real
/// </summary>
public class ComandaCreadaSignalRHandler : IDomainEventHandler<ComandaCreadaEvent>
{
    private readonly ISignalRService _signalRService;
    private readonly IComandaRepository _comandaRepository;
    private readonly ILogger<ComandaCreadaSignalRHandler> _logger;

    public ComandaCreadaSignalRHandler(
        ISignalRService signalRService,
        IComandaRepository comandaRepository,
        ILogger<ComandaCreadaSignalRHandler> logger)
    {
        _signalRService = signalRService;
        _comandaRepository = comandaRepository;
        _logger = logger;
    }

    /// <summary>
    /// 🚀 Procesa la creación de comanda enviando notificaciones en tiempo real
    /// </summary>
    public async Task Handle(ComandaCreadaEvent evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔄 Iniciando notificación SignalR para Comanda {ComandaId}", evento.ComandaId);

        try
        {
            // 🔄 Obtener la comanda completa con sus detalles
            var comandaResult = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comandaResult == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada para notificación SignalR: {ComandaId}", evento.ComandaId);
                return;
            }

            var comanda = comandaResult;

            // 📢 Enviar notificación a la cocina
            await NotificarCocina(comanda, cancellationToken);

            // 📢 Enviar notificación al mesero
            await NotificarMesero(comanda, cancellationToken);

            // 📢 Enviar notificación global del sistema
            await NotificarSistema(comanda, cancellationToken);

            _logger.LogInformation("✅ Notificaciones SignalR enviadas correctamente para Comanda {ComandaId}", evento.ComandaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar notificaciones SignalR para Comanda {ComandaId}", evento.ComandaId);
            // No relanzamos la excepción para no interrumpir el flujo principal
        }
    }

    /// <summary>
    /// 👨‍🍳 Notifica a la cocina sobre la nueva comanda
    /// </summary>
    private async Task NotificarCocina(Domain.Operaciones.Comandas.Entities.Comanda comanda, CancellationToken cancellationToken)
    {
        var notificacionCocina = new
        {
            Titulo = "🍽️ Nueva Comanda",
            Mensaje = $"Nueva comanda #{comanda.NumeroComanda} - Mesa {comanda.MesaId}",
            Tipo = "info",
            ComandaId = comanda.Id,
            NumeroComanda = comanda.NumeroComanda,
            MesaId = comanda.MesaId,
            Estado = comanda.Estado.ToString(),
            Items = comanda.Items.Select(i => new
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                Observaciones = i.Observaciones
            }).ToList(),
            FechaCreacion = comanda.FechaCreacion,
            Observaciones = comanda.Observaciones
        };

        _logger.LogInformation("👨‍🍳 Enviando notificación a cocina: {@Notificacion}", notificacionCocina);

        try
        {
            // Crear DTO para nueva comanda
            var nuevaComandaDto = new NuevaComandaNotificationDto
            {
                ComandaId = comanda.Id,
                MesaId = comanda.MesaId ?? Guid.Empty,
                Estado = comanda.Estado.ToString(),
                Items = comanda.Items.Select(i => new ComandaItemNotificationDto
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Observaciones = i.Observaciones
                }).ToList(),
                FechaCreacion = comanda.FechaCreacion
            };

            await _signalRService.NotificarNuevaComandaAsync(nuevaComandaDto);

            // También enviar actualización de estado de comanda
            await _signalRService.NotificarActualizacionComandaAsync(
                comanda.Id,
                comanda.Estado.ToString(),
                $"Comanda #{comanda.NumeroComanda} creada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al notificar a cocina para Comanda {ComandaId}", comanda.Id);
        }
    }

    /// <summary>
    /// 👨‍💼 Notifica al mesero sobre la confirmación de su comanda
    /// </summary>
    private async Task NotificarMesero(Domain.Operaciones.Comandas.Entities.Comanda comanda, CancellationToken cancellationToken)
    {
        var notificacionMesero = new
        {
            Titulo = "✅ Comanda Creada",
            Mensaje = $"Comanda #{comanda.NumeroComanda} creada exitosamente",
            Tipo = "success",
            ComandaId = comanda.Id,
            NumeroComanda = comanda.NumeroComanda,
            MesaId = comanda.MesaId,
            Estado = comanda.Estado.ToString(),
            FechaCreacion = comanda.FechaCreacion
        };

        _logger.LogInformation("👨‍💼 Enviando notificación al mesero: {@Notificacion}", notificacionMesero);

        try
        {
            // Notificar al mesero específico
            if (comanda.MeseroId != Guid.Empty)
            {
                await _signalRService.NotificarUsuarioAsync(
                    comanda.MeseroId.ToString(),
                    "ComandaCreada",
                    notificacionMesero);
            }

            // También notificar a todos los meseros
            await _signalRService.NotificarGrupoAsync(
                "Meseros",
                "ComandaCreada",
                notificacionMesero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al notificar al mesero para Comanda {ComandaId}", comanda.Id);
        }
    }

    /// <summary>
    /// 📢 Notifica al sistema sobre la nueva comanda
    /// </summary>
    private async Task NotificarSistema(Domain.Operaciones.Comandas.Entities.Comanda comanda, CancellationToken cancellationToken)
    {
        var notificacionSistema = new
        {
            Titulo = "📊 Nueva Comanda Registrada",
            Mensaje = $"Comanda #{comanda.NumeroComanda} registrada en el sistema",
            Tipo = "info",
            ComandaId = comanda.Id,
            NumeroComanda = comanda.NumeroComanda,
            MesaId = comanda.MesaId,
            MeseroId = comanda.MeseroId,
            Estado = comanda.Estado.ToString(),
            TotalItems = comanda.Items.Count,
            FechaCreacion = comanda.FechaCreacion
        };

        _logger.LogInformation("📊 Enviando notificación del sistema: {@Notificacion}", notificacionSistema);

        try
        {
            // Notificar a administradores
            await _signalRService.NotificarGrupoAsync(
                "Administradores",
                "NuevaComandaRegistrada",
                notificacionSistema);

            // También enviar notificación global para logging
            await _signalRService.NotificarEventoSistemaAsync(
                "NuevaComandaRegistrada",
                notificacionSistema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al enviar notificación del sistema para Comanda {ComandaId}", comanda.Id);
        }
    }
} 