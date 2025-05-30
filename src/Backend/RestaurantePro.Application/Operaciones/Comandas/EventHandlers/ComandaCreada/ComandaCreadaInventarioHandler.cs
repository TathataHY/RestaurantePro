namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;

/// <summary>
/// 🔄 Handler que procesa el evento ComandaCreada para verificar inventario disponible
/// </summary>
public class ComandaCreadaInventarioHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaCreada>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly ILogger<ComandaCreadaInventarioHandler> _logger;
    private readonly IMediator _mediator;

    public ComandaCreadaInventarioHandler(
        IComandaRepository comandaRepository,
        IIngredienteRepository ingredienteRepository,
        ILogger<ComandaCreadaInventarioHandler> logger,
        IMediator mediator)
    {
        _comandaRepository = comandaRepository;
        _ingredienteRepository = ingredienteRepository;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// 🚀 Procesa la creación de comanda verificando disponibilidad de inventario
    /// </summary>
    public async Task Handle(Domain.Operaciones.Comandas.Events.Comanda.ComandaCreada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔄 Iniciando verificación de inventario para Comanda {ComandaId}", evento.ComandaId);

        try
        {
            // 🔄 Obtener la comanda completa
            var comandaResult = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comandaResult == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada para verificación de inventario: {ComandaId}", evento.ComandaId);
                return;
            }

            var comanda = comandaResult;
            var alertasBajoStock = new List<string>();

            // 2. Verificar stock para cada item de la comanda
            foreach (var item in comanda.Items)
            {
                var stockResult = await VerificarStockItem(item, cancellationToken);
                if (stockResult.IsFailure)
                {
                    alertasBajoStock.Add($"❌ {item.Producto?.Nombre ?? "Producto desconocido"}: {stockResult.Error}");
                }
            }

            // 3. Si hay alertas de stock, notificar
            if (alertasBajoStock.Any())
            {
                _logger.LogWarning("⚠️ Alertas de inventario para Comanda {ComandaId}: {Alertas}", 
                    evento.ComandaId, string.Join(", ", alertasBajoStock));

                // Enviar comando para notificar al personal
                await NotificarBajoStock(evento.ComandaId, evento.MeseroId, alertasBajoStock, cancellationToken);
            }
            else
            {
                _logger.LogInformation("✅ Inventario verificado correctamente para Comanda {ComandaId}", evento.ComandaId);
            }

            // 4. Actualizar métricas de uso de inventario
            await ActualizarMetricasInventario(comanda.Items, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al verificar inventario para Comanda {ComandaId}", evento.ComandaId);
            throw;
        }
    }

    /// <summary>
    /// 🔍 Verifica el stock disponible para un item específico
    /// </summary>
    private async Task<Result> VerificarStockItem(
        ItemComanda item, 
        CancellationToken cancellationToken)
    {
        // Obtener ingredientes necesarios para el producto
        var ingredientesProducto = await _ingredienteRepository.ObtenerPorProductoAsync(item.ProductoId, cancellationToken);
        
        _logger.LogInformation("🔍 Verificando stock para producto {ProductoNombre} (Cantidad: {Cantidad})",
            item.Producto?.Nombre ?? "Producto desconocido", item.Cantidad);

        if (ingredientesProducto == null || !ingredientesProducto.Any())
        {
            return Result.Failure("No se pudieron obtener ingredientes para el producto");
        }

        var ingredientes = ingredientesProducto;
        foreach (var ingrediente in ingredientes)
        {
            var stockNecesario = ingrediente.CantidadPorUnidad * item.Cantidad;
            if (ingrediente.StockActual < stockNecesario)
            {
                return Result.Failure($"Stock insuficiente: necesario {stockNecesario}, disponible {ingrediente.StockActual}");
            }

            // Verificar si está cerca del stock mínimo
            if (ingrediente.StockActual - stockNecesario <= ingrediente.StockMinimo)
            {
                _logger.LogWarning("⚠️ Ingrediente {IngredienteNombre} quedará cerca del stock mínimo después de usar {CantidadUsar}", 
                    ingrediente.Nombre, stockNecesario);
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// 📢 Notifica al personal sobre problemas de stock
    /// </summary>
    private async Task NotificarBajoStock(
        Guid comandaId, 
        Guid meseroId, 
        List<string> alertas, 
        CancellationToken cancellationToken)
    {
        var notificacion = new
        {
            Tipo = "BajoStock",
            ComandaId = comandaId,
            MeseroId = meseroId,
            Mensaje = $"⚠️ Alertas de inventario para Comanda {comandaId}",
            Detalles = alertas,
            FechaHora = DateTime.UtcNow
        };

        _logger.LogInformation("📢 Enviando notificación de bajo stock: {@Notificacion}", notificacion);
        
        // TODO: Implementar envío real de notificación (SignalR, email, etc.)
        // await _notificationService.SendAsync(notificacion, cancellationToken);
    }

    /// <summary>
    /// 📊 Actualiza métricas de uso de inventario
    /// </summary>
    private async Task ActualizarMetricasInventario(
        IReadOnlyCollection<ItemComanda> items,
        CancellationToken cancellationToken)
    {
        var metricas = new
        {
            FechaHora = DateTime.UtcNow,
            TotalItems = items.Count,
            ProductosUsados = items.Select(i => new { i.ProductoId, i.Producto?.Nombre, i.Cantidad }).ToList()
        };

        _logger.LogInformation("📊 Actualizando métricas de inventario: {@Metricas}", metricas);
        
        // TODO: Implementar almacenamiento de métricas
        // await _metricsService.RecordInventoryUsageAsync(metricas, cancellationToken);
    }

    /// <summary>
    /// ⚠️ Registra alerta específica para un item con stock bajo
    /// </summary>
    private async Task RegistrarAlertaStockItem(ItemComanda item, List<dynamic> ingredientes, List<dynamic> stockBajo, CancellationToken cancellationToken)
    {
        var alertaItem = new
        {
            FechaHora = DateTime.UtcNow,
            ComandaId = item.ComandaId,
            ItemId = item.Id,
            ProductoId = item.ProductoId,
            ProductoNombre = item.Producto?.Nombre ?? "Producto desconocido",
            CantidadSolicitada = item.Cantidad,
            IngredientesAfectados = ingredientes.Count,
            IngredientesCriticos = stockBajo.Count,
            RequiereAtencionInmediata = stockBajo.Any()
        };

        _logger.LogInformation("⚠️ Alerta de stock registrada para item: {@AlertaItem}", alertaItem);
        
        // TODO: Enviar alerta al personal correspondiente
        // await _alertService.SendStockAlertAsync(alertaItem, cancellationToken);
    }
} 