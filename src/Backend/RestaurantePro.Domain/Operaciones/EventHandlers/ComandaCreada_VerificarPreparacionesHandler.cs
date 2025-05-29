namespace RestaurantePro.Domain.Operaciones.EventHandlers;

/// <summary>
/// Manejador de eventos que verifica y consume preparaciones cuando se crea una comanda
/// </summary>
public class ComandaCreada_VerificarPreparacionesHandler : IDomainEventHandler<ComandaCreada>
{
    private readonly IServicioPreparaciones _servicioPreparaciones;
    private readonly IComandaRepository _comandaRepository;
    private readonly ILogger<ComandaCreada_VerificarPreparacionesHandler> _logger;
    private readonly INotificationManager _notificationManager;

    public ComandaCreada_VerificarPreparacionesHandler(
        IServicioPreparaciones servicioPreparaciones,
        IComandaRepository comandaRepository,
        ILogger<ComandaCreada_VerificarPreparacionesHandler> logger,
        INotificationManager notificationManager)
    {
        _servicioPreparaciones = servicioPreparaciones ?? throw new ArgumentNullException(nameof(servicioPreparaciones));
        _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
    }

    /// <summary>
    /// Maneja el evento ComandaCreada verificando disponibilidad de preparaciones
    /// </summary>
    public async Task Handle(ComandaCreada evento, CancellationToken cancellationToken = default)
    {
        if (evento == null) throw new ArgumentNullException(nameof(evento));

        _logger.LogInformation("🍳 Comanda {ComandaId} creada - iniciando verificación de preparaciones", evento.ComandaId);

        try
        {
            // Obtener la comanda completa para analizar sus ítems
            var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comanda == null)
            {
                _logger.LogWarning("⚠️ No se encontró la comanda {ComandaId} para verificar preparaciones", evento.ComandaId);
                return;
            }

            if (!comanda.Items.Any())
            {
                _logger.LogInformation("ℹ️ Comanda {ComandaId} no tiene ítems, no hay preparaciones que verificar", evento.ComandaId);
                return;
            }

            // Verificar disponibilidad de preparaciones para cada producto
            var productosVerificados = 0;
            var productosConPreparaciones = 0;
            var productosAlMomento = 0;

            foreach (var item in comanda.Items)
            {
                productosVerificados++;

                try
                {
                    var disponibilidadResult = await _servicioPreparaciones.VerificarDisponibilidadAsync(
                        item.ProductoId, 
                        item.Cantidad);

                    if (disponibilidadResult.Succeeded && disponibilidadResult.Value)
                    {
                        productosConPreparaciones++;
                        _logger.LogInformation("✅ Producto {ProductoId} (cantidad: {Cantidad}) disponible en preparaciones", 
                            item.ProductoId, item.Cantidad);
                    }
                    else
                    {
                        productosAlMomento++;
                        _logger.LogInformation("🥘 Producto {ProductoId} (cantidad: {Cantidad}) será preparado al momento", 
                            item.ProductoId, item.Cantidad);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al verificar preparaciones para producto {ProductoId}", item.ProductoId);
                    productosAlMomento++; // En caso de error, asumir preparación al momento
                }
            }

            // Log resumen del análisis
            if (productosConPreparaciones > 0 || productosAlMomento > 0)
            {
                var porcentajePreparaciones = (productosConPreparaciones * 100.0) / productosVerificados;
                
                _logger.LogInformation(
                    "📊 Análisis de preparaciones para comanda {ComandaId}: " +
                    "{ProductosVerificados} productos | " +
                    "{ProductosConPreparaciones} preparados ({PorcentajePreparaciones:F1}%) | " +
                    "{ProductosAlMomento} al momento",
                    evento.ComandaId, 
                    productosVerificados, 
                    productosConPreparaciones, 
                    porcentajePreparaciones,
                    productosAlMomento);

                // Si la mayoría de productos requieren preparación al momento, generar alerta
                if (porcentajePreparaciones < 30.0 && productosVerificados > 1)
                {
                    _logger.LogWarning(
                        "⚠️ ALERTA: Comanda {ComandaId} tiene bajo porcentaje de preparaciones ({PorcentajePreparaciones:F1}%). " +
                        "Considerar preparar más productos anticipadamente.",
                        evento.ComandaId, porcentajePreparaciones);
                }
            }

            _logger.LogInformation("🏁 Verificación de preparaciones completada para comanda {ComandaId} - Mesa: {MesaId}, Mesero: {MeseroId}", 
                evento.ComandaId, evento.MesaId, evento.MeseroId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error crítico al procesar preparaciones para comanda {ComandaId}", evento.ComandaId);
            // No lanzamos la excepción para no interrumpir el flujo principal
        }
    }
} 