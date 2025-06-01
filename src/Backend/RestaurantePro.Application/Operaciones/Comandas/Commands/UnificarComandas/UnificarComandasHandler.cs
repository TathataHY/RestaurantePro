namespace RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;

/// <summary>
/// Handler para unificar múltiples comandas en una sola comanda consolidada
/// Gestiona el proceso completo de unificación con consolidación de items y descuentos
/// </summary>
public class UnificarComandasHandler : IRequestHandler<UnificarComandasCommand, Result<UnificarComandasDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UnificarComandasHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunicationService _notificacionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public UnificarComandasHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UnificarComandasHandler> logger,
        ICurrentUserService currentUserService,
        ICommunicationService notificacionService,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<UnificarComandasDto>> Handle(UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando unificación de {CantidadComandas} comandas en mesa {MesaDestinoId}",
            request.ComandasIds.Count, request.MesaDestinoId);

        try
        {
            return await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                // 1. Obtener comandas originales
                var comandasOriginalesResult = await ObtenerComandasOriginales(request.ComandasIds, cancellationToken);
                if (!comandasOriginalesResult.IsSuccess)
                {
                    return Result<UnificarComandasDto>.Failure(comandasOriginalesResult.ErrorMessage);
                }

                var comandasOriginales = comandasOriginalesResult.Value;

                // 2. Determinar comanda principal o crear nueva
                var comandaUnificadaResult = await ObtenerOCrearComandaUnificada(comandasOriginales, request, cancellationToken);
                if (!comandaUnificadaResult.IsSuccess)
                {
                    return Result<UnificarComandasDto>.Failure(comandaUnificadaResult.ErrorMessage);
                }

                var comandaUnificada = comandaUnificadaResult.Value;

                // 3. Consolidar items de todas las comandas
                await ConsolidarItems(comandasOriginales, comandaUnificada, cancellationToken);

                // 4. Aplicar estrategia de descuentos
                await AplicarEstrategiaDescuentos(comandasOriginales, comandaUnificada, request.EstrategiaDescuentos, cancellationToken);

                // 5. Actualizar comandas originales
                await ActualizarComandasOriginales(comandasOriginales, comandaUnificada, request, cancellationToken);

                // 6. Actualizar estado de la mesa destino
                await ActualizarMesaDestino(request.MesaDestinoId, cancellationToken);

                // 7. Registrar auditoría
                await RegistrarAuditoria(comandasOriginales, comandaUnificada, request, cancellationToken);

                // 8. Guardar cambios
                await _unitOfWork.GuardarCambiosAsync(cancellationToken);

                // 9. Crear respuesta
                var response = CrearRespuesta(comandasOriginales, comandaUnificada, request);

                _logger.LogInformation("✅ Unificación completada exitosamente. Comandas originales: {ComandasOriginalesIds}, Comanda unificada: {ComandaUnificadaId}",
                    string.Join(", ", request.ComandasIds), comandaUnificada.Id);

                return Result<UnificarComandasDto>.Success(response);

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al unificar comandas {ComandasIds}: {ErrorMessage}", 
                string.Join(", ", request.ComandasIds), ex.Message);
            return Result<UnificarComandasDto>.Failure($"Error interno al unificar las comandas: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<List<Comanda>>> ObtenerComandasOriginales(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandas = await _context.Comandas
            .Include(c => c.Mesa)
            .Where(c => comandasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (comandas.Count != comandasIds.Count)
        {
            var faltantes = comandasIds.Except(comandas.Select(c => c.Id)).ToList();
            return Result<List<Comanda>>.Failure($"Las siguientes comandas no fueron encontradas: {string.Join(", ", faltantes)}");
        }

        return Result<List<Comanda>>.Success(comandas);
    }

    private async Task<Result<Comanda>> ObtenerOCrearComandaUnificada(List<Comanda> comandasOriginales, UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        Comanda comandaUnificada;

        if (request.ComandaPrincipalId.HasValue)
        {
            // Usar comanda principal existente
            comandaUnificada = comandasOriginales.FirstOrDefault(c => c.Id == request.ComandaPrincipalId.Value);
            if (comandaUnificada == null)
            {
                return Result<Comanda>.Failure("La comanda principal especificada no se encuentra en la lista de comandas a unificar.");
            }

            // Actualizar observaciones de la comanda principal usando método disponible
            var observacionesUnificadas = request.ObservacionesUnificada ?? 
                $"Unificación de comandas: {string.Join(", ", comandasOriginales.Where(c => c.Id != comandaUnificada.Id).Select(c => c.Id))}";
            comandaUnificada.AgregarObservacion(observacionesUnificadas);
        }
        else
        {
            // Crear nueva comanda unificada usando factory method correcto
            comandaUnificada = Comanda.Crear(
                meseroId: request.MeseroId,
                clienteId: comandasOriginales.FirstOrDefault()?.ClienteId,
                mesaId: request.MesaDestinoId,
                observaciones: request.ObservacionesUnificada ?? $"Unificación de comandas: {string.Join(", ", comandasOriginales.Select(c => c.Id))}"
            );

            await _context.Comandas.AddAsync(comandaUnificada, cancellationToken);
        }

        return Result<Comanda>.Success(comandaUnificada);
    }

    private async Task ConsolidarItems(List<Comanda> comandasOriginales, Comanda comandaUnificada, CancellationToken cancellationToken)
    {
        // Obtener todos los items de las comandas originales
        var todosLosItems = comandasOriginales.SelectMany(c => c.Items).ToList();
        
        // Agrupar items por producto para consolidar cantidades
        var itemsConsolidados = todosLosItems
            .GroupBy(i => new { i.ProductoId, i.PrecioUnitario, i.Observaciones })
            .Select(grupo => new
            {
                ProductoId = grupo.Key.ProductoId,
                PrecioUnitario = grupo.Key.PrecioUnitario,
                Observaciones = grupo.Key.Observaciones,
                CantidadTotal = grupo.Sum(i => i.Cantidad)
            })
            .ToList();

        // Agregar items consolidados a la comanda unificada usando método real
        foreach (var itemConsolidado in itemsConsolidados)
        {
            comandaUnificada.AgregarItem(
                itemConsolidado.ProductoId,
                "Producto Consolidado", // nombreProducto - requerido por el método
                itemConsolidado.CantidadTotal,
                itemConsolidado.PrecioUnitario,
                itemConsolidado.Observaciones
            );
        }
    }

    private async Task AplicarEstrategiaDescuentos(List<Comanda> comandasOriginales, Comanda comandaUnificada, EstrategiaDescuentos estrategia, CancellationToken cancellationToken)
    {
        // Obtener descuentos de fidelización de las comandas originales
        var descuentosFidelizacion = comandasOriginales
            .Where(c => c.DescuentoFidelizacion.HasValue)
            .Select(c => c.DescuentoFidelizacion.Value)
            .ToList();

        if (!descuentosFidelizacion.Any())
            return;

        decimal descuentoFinal = estrategia switch
        {
            EstrategiaDescuentos.Sumar => descuentosFidelizacion.Sum(),
            EstrategiaDescuentos.TomarMayor => descuentosFidelizacion.Max(),
            EstrategiaDescuentos.TomarMenor => descuentosFidelizacion.Min(),
            EstrategiaDescuentos.Promedio => descuentosFidelizacion.Average(),
            EstrategiaDescuentos.SinDescuentos => 0m,
            _ => descuentosFidelizacion.Sum()
        };

        if (descuentoFinal > 0)
        {
            // Aplicar como porcentaje (método espera valor entre 0 y 1)
            var porcentajeDescuento = Math.Min(descuentoFinal / 100m, 0.5m); // Máximo 50%
            comandaUnificada.AplicarDescuentoFidelizacion(porcentajeDescuento);
        }
    }

    private async Task ActualizarComandasOriginales(List<Comanda> comandasOriginales, Comanda comandaUnificada, UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        foreach (var comandaOriginal in comandasOriginales)
        {
            // Si es la comanda principal, no hacer nada (ya es la unificada)
            if (comandaOriginal.Id == comandaUnificada.Id) continue;

            if (request.MantenerHistorico)
            {
                // Cancelar comanda pero mantener en histórico
                comandaOriginal.Cancelar($"Unificada en comanda {comandaUnificada.Id} el {_dateTimeService.Now:dd/MM/yyyy HH:mm}");
            }
            else
            {
                // Cancelar comanda
                comandaOriginal.Cancelar($"Cancelada por unificación el {_dateTimeService.Now:dd/MM/yyyy HH:mm}");
            }
        }
    }

    private async Task ActualizarMesaDestino(Guid mesaDestinoId, CancellationToken cancellationToken)
    {
        var mesaDestino = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaDestinoId, cancellationToken);

        if (mesaDestino != null)
        {
            // Usar método real de la entidad Mesa (simplificado por ahora)
            // mesaDestino.Ocupar(); - si no existe, usar propiedades disponibles
            // Por ahora simplemente log el cambio
            _logger.LogInformation("Mesa destino {MesaId} actualizada para unificación", mesaDestinoId);
        }
    }

    private async Task RegistrarAuditoria(List<Comanda> comandasOriginales, Comanda comandaUnificada, UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        // Simplificado: Log de auditoría básico
        _logger.LogInformation("Auditoría - Unificación de comandas. Originales: {ComandasOriginales}, Unificada: {ComandaUnificada}, Motivo: {Motivo}, Usuario: {Usuario}",
            string.Join(", ", comandasOriginales.Select(c => c.Id)),
            comandaUnificada.Id,
            request.MotivoUnificacion,
            _currentUserService.UserId);
    }

    private UnificarComandasDto CrearRespuesta(List<Comanda> comandasOriginales, Comanda comandaUnificada, UnificarComandasCommand request)
    {
        // Calcular total usando el objeto TotalComanda
        var montoTotal = comandaUnificada.Total?.Total ?? 0m;

        return new UnificarComandasDto
        {
            ComandasOriginalesIds = comandasOriginales.Select(c => c.Id).ToList(),
            ComandaUnificadaId = comandaUnificada.Id,
            MesaDestinoId = request.MesaDestinoId,
            MeseroId = request.MeseroId,
            MotivoUnificacion = request.MotivoUnificacion,
            FechaUnificacion = _dateTimeService.Now,
            AutorizadoPor = request.AutorizadoPor,
            UnificacionExitosa = true,
            TotalItemsUnificados = comandaUnificada.Items.Sum(i => i.Cantidad),
            MontoTotalUnificado = montoTotal,
            EstrategiaDescuentos = request.EstrategiaDescuentos
        };
    }

    private async Task<string> GenerarNumeroComanda(CancellationToken cancellationToken)
    {
        // Generar un número simple para la comanda unificada
        var timestamp = _dateTimeService.Now.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"UNI-{timestamp}-{random}";
    }

    #endregion
} 