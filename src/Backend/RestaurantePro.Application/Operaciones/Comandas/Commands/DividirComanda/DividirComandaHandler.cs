namespace RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;

/// <summary>
/// Handler para dividir comandas en múltiples comandas separadas
/// Gestiona el proceso completo de división con distribución de items y descuentos
/// </summary>
public class DividirComandaHandler : IRequestHandler<DividirComandaCommand, Result<DividirComandaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DividirComandaHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunicationService _notificacionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public DividirComandaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<DividirComandaHandler> logger,
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

    public async Task<Result<DividirComandaDto>> Handle(DividirComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando división de comanda {ComandaOriginalId} en {CantidadDivisiones} partes",
            request.ComandaOriginalId, request.DivisionItems.Count);

        try
        {
            return await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                // 1. Obtener comanda original completa
                var comandaOriginalResult = await ObtenerComandaOriginal(request.ComandaOriginalId, cancellationToken);
                if (!comandaOriginalResult.IsSuccess())
                {
                    return Result<DividirComandaDto>.Failure(comandaOriginalResult.ErrorMessage());
                }

                var comandaOriginal = comandaOriginalResult.Value;

                // 2. Validar distribución de items
                var validacionResult = await ValidarDistribucionItems(comandaOriginal, request, cancellationToken);
                if (!validacionResult.IsSuccess())
                {
                    return Result<DividirComandaDto>.Failure(validacionResult.ErrorMessage());
                }

                // 3. Crear nuevas comandas
                var nuevasComandasResult = await CrearNuevasComandas(comandaOriginal, request, cancellationToken);
                if (!nuevasComandasResult.IsSuccess())
                {
                    return Result<DividirComandaDto>.Failure(nuevasComandasResult.ErrorMessage());
                }

                var nuevasComandas = nuevasComandasResult.Value;

                // 4. Distribuir items entre las nuevas comandas
                await DistribuirItems(comandaOriginal, nuevasComandas, request, cancellationToken);

                // 5. Distribuir descuentos si es necesario
                if (request.DistribuirDescuentos)
                {
                    await DistribuirDescuentos(comandaOriginal, nuevasComandas, cancellationToken);
                }

                // 6. Actualizar comanda original
                await ActualizarComandaOriginal(comandaOriginal, request, cancellationToken);

                // 7. Registrar auditoría
                await RegistrarAuditoria(comandaOriginal, nuevasComandas, request, cancellationToken);

                // 8. Guardar cambios
                await _unitOfWork.GuardarCambiosAsync(cancellationToken);

                // 9. Crear respuesta
                var response = CrearRespuesta(comandaOriginal, nuevasComandas, request);

                _logger.LogInformation("✅ División completada exitosamente. Comanda original: {ComandaOriginalId}, Nuevas comandas: {NuevasComandasIds}",
                    request.ComandaOriginalId, string.Join(", ", nuevasComandas.Select(c => c.Id)));

                return Result<DividirComandaDto>.Success(response);

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al dividir comanda {ComandaOriginalId}: {ErrorMessage}", 
                request.ComandaOriginalId, ex.Message);
            return Result<DividirComandaDto>.Failure($"Error interno al dividir la comanda: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<Comanda>> ObtenerComandaOriginal(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _context.Comandas
            .Include(c => c.Mesa)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);

        if (comanda == null)
        {
            return Result<Comanda>.Failure("La comanda original especificada no existe.");
        }

        return Result<Comanda>.Success(comanda);
    }

    private async Task<Result<string>> ValidarDistribucionItems(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        var itemsOriginales = comandaOriginal.Items.ToDictionary(i => i.Id, i => i.Cantidad);
        var itemsDistribuidos = request.DivisionItems
            .SelectMany(d => d.Items)
            .GroupBy(i => i.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        // Validar que todos los items están distribuidos correctamente
        foreach (var itemOriginal in itemsOriginales)
        {
            if (!itemsDistribuidos.ContainsKey(itemOriginal.Key))
            {
                if (!request.MantenerComandaOriginal)
                {
                    return Result<string>.Failure($"El item {itemOriginal.Key} no está distribuido en ninguna nueva comanda.");
                }
                continue;
            }

            var cantidadDistribuida = itemsDistribuidos[itemOriginal.Key];
            var cantidadOriginal = itemOriginal.Value;

            if (cantidadDistribuida > cantidadOriginal)
            {
                return Result<string>.Failure($"La cantidad distribuida del item {itemOriginal.Key} ({cantidadDistribuida}) excede la cantidad original ({cantidadOriginal}).");
            }

            if (!request.MantenerComandaOriginal && cantidadDistribuida < cantidadOriginal)
            {
                return Result<string>.Failure($"La cantidad distribuida del item {itemOriginal.Key} ({cantidadDistribuida}) es menor que la cantidad original ({cantidadOriginal}).");
            }
        }

        return Result<string>.Success("Validación exitosa");
    }

    private async Task<Result<List<Comanda>>> CrearNuevasComandas(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        var nuevasComandas = new List<Comanda>();

        foreach (var division in request.DivisionItems)
        {
            // Crear nueva comanda usando el factory method correcto
            var nuevaComanda = Comanda.Crear(
                meseroId: division.MeseroId ?? comandaOriginal.MeseroId,
                clienteId: comandaOriginal.ClienteId,
                mesaId: division.MesaDestinoId ?? comandaOriginal.MesaId,
                observaciones: division.Observaciones ?? $"División de comanda #{comandaOriginal.Id}"
            );

            nuevasComandas.Add(nuevaComanda);
            await _context.Comandas.AddAsync(nuevaComanda, cancellationToken);
        }

        return Result<List<Comanda>>.Success(nuevasComandas);
    }

    private async Task DistribuirItems(Comanda comandaOriginal, List<Comanda> nuevasComandas, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.DivisionItems.Count; i++)
        {
            var division = request.DivisionItems[i];
            var nuevaComanda = nuevasComandas[i];

            foreach (var itemDivision in division.Items)
            {
                var itemOriginal = comandaOriginal.Items.FirstOrDefault(io => io.Id == itemDivision.ItemId);
                if (itemOriginal == null) continue;

                // Agregar item usando el método real disponible
                nuevaComanda.AgregarItem(
                    itemOriginal.ProductoId,
                    itemOriginal.NombreProducto,
                    itemDivision.Cantidad,
                    itemOriginal.PrecioUnitario,
                    itemDivision.ObservacionesItem ?? itemOriginal.Observaciones
                );
            }
        }
    }

    private async Task DistribuirDescuentos(Comanda comandaOriginal, List<Comanda> nuevasComandas, CancellationToken cancellationToken)
    {
        // Si la comanda original tiene descuento de fidelización, distribuirlo proporcionalmente
        if (comandaOriginal.DescuentoFidelizacion.HasValue && comandaOriginal.DescuentoFidelizacion.Value > 0)
        {
            var descuentoPorComanda = comandaOriginal.DescuentoFidelizacion.Value / nuevasComandas.Count;
            
            foreach (var nuevaComanda in nuevasComandas)
            {
                nuevaComanda.AplicarDescuentoFidelizacion(descuentoPorComanda);
            }
        }
    }

    private async Task ActualizarComandaOriginal(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        if (!request.MantenerComandaOriginal)
        {
            // Cancelar la comanda original
            comandaOriginal.Cancelar($"Dividida en {request.DivisionItems.Count} comandas el {_dateTimeService.Now:dd/MM/yyyy HH:mm}");
        }
        else
        {
            // Agregar observación sobre la división
            comandaOriginal.AgregarObservacion($"Comanda dividida el {_dateTimeService.Now:dd/MM/yyyy HH:mm}. Motivo: {request.MotivoDivision}");
        }
    }

    private async Task RegistrarAuditoria(Comanda comandaOriginal, List<Comanda> nuevasComandas, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        // Simplificado: Log de auditoría básico
        _logger.LogInformation("Auditoría - División de comanda. Original: {ComandaOriginal}, Nuevas: {ComandasNuevas}, Motivo: {Motivo}, Usuario: {Usuario}",
            comandaOriginal.Id,
            string.Join(", ", nuevasComandas.Select(c => c.Id)),
            request.MotivoDivision,
            _currentUserService.UserId);
    }

    private DividirComandaDto CrearRespuesta(Comanda comandaOriginal, List<Comanda> nuevasComandas, DividirComandaCommand request)
    {
        return new DividirComandaDto
        {
            ComandaOriginalId = comandaOriginal.Id,
            ComandasNuevasIds = nuevasComandas.Select(c => c.Id).ToList(),
            TipoDivision = request.TipoDivision,
            MotivoDivision = request.MotivoDivision,
            FechaDivision = _dateTimeService.Now,
            AutorizadoPor = request.AutorizadoPor,
            DivisionExitosa = true,
            TotalComandasCreadas = nuevasComandas.Count
        };
    }

    #endregion
} 