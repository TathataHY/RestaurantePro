namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

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
    /// 🚀 Procesa la finalización de comanda para liberar mesa automáticamente
    /// </summary>
    public async Task Handle(Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🪑 Iniciando liberación automática de mesa para Comanda {ComandaId}", evento.ComandaId);

        try
        {
            // 🔄 Obtener la comanda
            var comandaResult = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comandaResult == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada para liberación de mesa: {ComandaId}", evento.ComandaId);
                return;
            }

            var comanda = comandaResult;

            // 🔍 Verificar si la comanda tiene mesa asignada
            if (comanda.MesaId == Guid.Empty)
            {
                _logger.LogInformation("ℹ️ Comanda {ComandaId} no tiene mesa asignada, omitiendo liberación", evento.ComandaId);
                return;
            }

            // 3. Obtener información de la mesa
            var mesaResult = await _mesaRepository.ObtenerPorIdAsync(comanda.MesaId, cancellationToken);
            if (mesaResult == null)
            {
                _logger.LogWarning("⚠️ No se encontró la mesa {MesaId} para liberar", comanda.MesaId);
                return;
            }

            var mesa = mesaResult;

            // 4. Verificar si la mesa necesita ser liberada
            // TODO: Revisar el estado de la mesa cuando esté implementado
            _logger.LogInformation("ℹ️ Verificando estado de Mesa {MesaNumero}", mesa.Numero);

            // 🪑 Liberar la mesa usando el command existente
            var liberarMesaCommand = new LiberarMesaCommand
            {
                MesaId = comanda.MesaId,
                Observaciones = "Comanda finalizada automáticamente por el sistema"
            };

            var resultadoLiberacion = await _mediator.Send(liberarMesaCommand, cancellationToken);
            if (resultadoLiberacion == null || !EsResultadoExitoso(resultadoLiberacion))
            {
                _logger.LogWarning("⚠️ No se pudo liberar la mesa {MesaId} para comanda {ComandaId}", 
                    comanda.MesaId, evento.ComandaId);
                return;
            }

            // 6. Calcular duración del servicio para métricas
            await CalcularDuracionServicio(comanda, evento.FechaFinalizacion, cancellationToken);

            // 7. Notificar al personal sobre la mesa disponible
            await NotificarMesaDisponible(mesa, comanda, cancellationToken);

            // 8. Verificar si hay reservaciones pendientes para esta mesa
            await VerificarReservacionesPendientes(mesa.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al procesar liberación de mesa para Comanda {ComandaId}", evento.ComandaId);
            throw;
        }
    }

    /// <summary>
    /// ⏱️ Calcula la duración del servicio para métricas
    /// </summary>
    private async Task CalcularDuracionServicio(
        Comanda comanda, 
        DateTime fechaFinalizacion,
        CancellationToken cancellationToken)
    {
        var duracionServicio = fechaFinalizacion - comanda.FechaCreacion;
        
        var metricas = new
        {
            ComandaId = comanda.Id,
            MesaId = comanda.MesaId,
            DuracionMinutos = (int)duracionServicio.TotalMinutes,
            FechaInicio = comanda.FechaCreacion,
            FechaFin = fechaFinalizacion,
            TotalComanda = comanda.Total,
            CantidadItems = comanda.Items.Count
        };

        _logger.LogInformation("⏱️ Duración del servicio calculada: {@Metricas}", metricas);

        // Alertar si el servicio fue excepcionalmente largo
        if (duracionServicio.TotalHours > 3)
        {
            _logger.LogWarning("⚠️ Servicio excepcionalmente largo detectado: {Duracion:hh\\:mm} para Comanda {ComandaId}", 
                duracionServicio, comanda.Id);
        }

        // TODO: Almacenar métricas para analytics
        // await _analyticsService.RecordServiceDurationAsync(metricas, cancellationToken);
    }

    /// <summary>
    /// 📢 Notifica al personal sobre la mesa disponible
    /// </summary>
    private async Task NotificarMesaDisponible(
        Mesa mesa,
        Comanda comanda,
        CancellationToken cancellationToken)
    {
        var notificacion = new
        {
            Tipo = "MesaDisponible",
            MesaId = mesa.Id,
            MesaNumero = mesa.Numero,
            Capacidad = mesa.Capacidad,
            ComandaAnterior = comanda.Id,
            TotalComandaAnterior = comanda.Total,
            Mensaje = $"🪑 Mesa {mesa.Numero} está disponible (Capacidad: {mesa.Capacidad} personas)",
            FechaHora = DateTime.UtcNow,
            RequiereLimpieza = true // Siempre requerir limpieza después de uso
        };

        _logger.LogInformation("📢 Enviando notificación de mesa disponible: {@Notificacion}", notificacion);
        
        // TODO: Implementar envío real de notificación al personal (SignalR, etc.)
        // await _notificationService.SendTableAvailableNotificationAsync(notificacion, cancellationToken);
    }

    /// <summary>
    /// 🔍 Verifica si hay reservaciones pendientes para esta mesa
    /// </summary>
    private async Task VerificarReservacionesPendientes(Guid mesaId, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar reservaciones para las próximas 2 horas
            var fechaLimite = DateTime.UtcNow.AddHours(2);
            
            // TODO: Implementar query para buscar reservaciones pendientes
            // var reservacionesPendientes = await _mediator.Send(
            //     ObtenerReservacionesPendientesPorMesaQuery.Create(mesaId, fechaLimite), 
            //     cancellationToken);

            _logger.LogInformation("🔍 Verificando reservaciones pendientes para Mesa {MesaId} hasta {FechaLimite}", 
                mesaId, fechaLimite);

            // Si hay reservaciones próximas, notificar al personal
            // if (reservacionesPendientes.IsSuccess && reservacionesPendientes.Value.Any())
            // {
            //     await NotificarReservacionProxima(mesaId, reservacionesPendientes.Value.First(), cancellationToken);
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al verificar reservaciones pendientes para Mesa {MesaId}", mesaId);
        }
    }

    /// <summary>
    /// 📊 Registra estadísticas de uso de mesa
    /// </summary>
    private async Task RegistrarEstadisticasUsoMesa(
        Mesa mesa,
        Comanda comanda,
        Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento,
        CancellationToken cancellationToken)
    {
        var estadisticas = new
        {
            FechaHora = DateTime.UtcNow,
            MesaId = mesa.Id,
            MesaNumero = mesa.Numero,
            CapacidadMesa = mesa.Capacidad,
            ComandaId = comanda.Id,
            DuracionUso = evento.FechaFinalizacion - comanda.FechaCreacion,
            TotalFacturado = evento.Total,
            CantidadItems = evento.Items.Count,
            IngresosPorHora = evento.Total / Math.Max((decimal)(evento.FechaFinalizacion - comanda.FechaCreacion).TotalHours, 0.1m)
        };

        _logger.LogInformation("📊 Registrando estadísticas de uso de mesa: {@Estadisticas}", estadisticas);
        
        // TODO: Implementar almacenamiento de estadísticas para analytics
        // await _analyticsService.RecordTableUsageStatsAsync(estadisticas, cancellationToken);
    }

    /// <summary>
    /// ⚡ Calcula métricas de eficiencia de la mesa
    /// </summary>
    private async Task CalcularMetricasEficienciaMesa(Guid mesaId, TimeSpan duracionServicio, decimal totalComanda)
    {
        // 📊 Calcular eficiencia (ventas por minuto)
        var ventasPorMinuto = duracionServicio.TotalMinutes > 0 
            ? totalComanda / (decimal)duracionServicio.TotalMinutes 
            : 0;

        _logger.LogInformation("📊 Métricas de eficiencia - Mesa {MesaId}: {VentasPorMinuto:C}/min", 
            mesaId, ventasPorMinuto);

        // TODO: Implementar cálculo de métricas de eficiencia
    }

    /// <summary>
    /// ✅ Verifica si el resultado es exitoso (helper para compatibilidad)
    /// </summary>
    private bool EsResultadoExitoso(object resultado)
    {
        // Para compatibilidad con diferentes tipos de Result
        if (resultado == null) return false;
        
        var type = resultado.GetType();
        var isSuccessProperty = type.GetProperty("IsSuccess");
        if (isSuccessProperty != null)
        {
            return (bool)(isSuccessProperty.GetValue(resultado) ?? false);
        }
        
        // Si no tiene IsSuccess, asumimos que es exitoso si no es null
        return true;
    }
} 