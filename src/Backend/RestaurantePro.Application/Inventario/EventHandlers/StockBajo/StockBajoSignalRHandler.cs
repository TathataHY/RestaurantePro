using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.EventHandlers.StockBajo
{
    /// <summary>
    /// Evento de stock bajo detectado para SignalR
    /// </summary>
    public class StockBajoDetectadoEvent : INotification
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler para enviar notificación SignalR cuando se detecta stock bajo
    /// Sigue las mejores prácticas de DDD y arquitectura limpia
    /// </summary>
    public class StockBajoSignalRHandler : INotificationHandler<StockBajoDetectadoEvent>
    {
        private readonly ISignalRService _signalRService;
        private readonly ILogger<StockBajoSignalRHandler> _logger;

        public StockBajoSignalRHandler(
            ISignalRService signalRService,
            ILogger<StockBajoSignalRHandler> logger)
        {
            _signalRService = signalRService ?? throw new ArgumentNullException(nameof(signalRService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maneja el evento de stock bajo detectado
        /// Envía notificaciones a administradores y personal de inventario
        /// </summary>
        /// <param name="notification">Evento de stock bajo detectado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task completado</returns>
        public async Task Handle(StockBajoDetectadoEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Enviando alerta SignalR por stock bajo: Ingrediente {IngredienteId} - {NombreIngrediente}",
                    notification.IngredienteId,
                    notification.NombreIngrediente);

                // Enviar alerta a administradores usando notificación de grupo
                await _signalRService.NotificarGrupoAsync(
                    "Administradores",
                    "AlertaStockBajo",
                    new
                    {
                        IngredienteId = notification.IngredienteId,
                        NombreIngrediente = notification.NombreIngrediente,
                        StockActual = notification.StockActual,
                        StockMinimo = notification.StockMinimo,
                        UnidadMedida = notification.UnidadMedida,
                        Mensaje = $"El ingrediente '{notification.NombreIngrediente}' tiene stock bajo. Stock actual: {notification.StockActual} {notification.UnidadMedida}",
                        Tipo = "warning",
                        Timestamp = DateTime.UtcNow
                    });

                // Enviar alerta a personal de inventario
                await _signalRService.NotificarGrupoAsync(
                    "Inventario",
                    "StockBajoDetectado",
                    new
                    {
                        IngredienteId = notification.IngredienteId,
                        NombreIngrediente = notification.NombreIngrediente,
                        StockActual = notification.StockActual,
                        StockMinimo = notification.StockMinimo,
                        UnidadMedida = notification.UnidadMedida,
                        Mensaje = $"Ingrediente: {notification.NombreIngrediente} | Stock: {notification.StockActual} {notification.UnidadMedida} | Mínimo: {notification.StockMinimo} {notification.UnidadMedida}",
                        Tipo = "warning",
                        Timestamp = DateTime.UtcNow
                    });

                // Notificación global del sistema para casos críticos
                if (notification.StockActual <= notification.StockMinimo * 0.5m)
                {
                    await _signalRService.NotificarEventoSistemaAsync(
                        "AlertaCriticaInventario",
                        new
                        {
                            IngredienteId = notification.IngredienteId,
                            NombreIngrediente = notification.NombreIngrediente,
                            StockActual = notification.StockActual,
                            UnidadMedida = notification.UnidadMedida,
                            Mensaje = $"Stock críticamente bajo: {notification.NombreIngrediente} - {notification.StockActual} {notification.UnidadMedida}",
                            Tipo = "error",
                            Timestamp = DateTime.UtcNow
                        });
                }

                _logger.LogInformation(
                    "Alerta SignalR enviada exitosamente para ingrediente {IngredienteId}",
                    notification.IngredienteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error enviando alerta SignalR por stock bajo para ingrediente {IngredienteId}",
                    notification.IngredienteId);
                
                // Re-lanzar la excepción para que el sistema pueda manejarla apropiadamente
                throw;
            }
        }
    }
} 