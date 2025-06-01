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
    private readonly IGeneradorNumeroComandaService _generadorNumero;

    public DividirComandaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<DividirComandaHandler> logger,
        ICurrentUserService currentUserService,
        ICommunicationService notificacionService,
        IUnitOfWork unitOfWork,
        IGeneradorNumeroComandaService generadorNumero)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        _unitOfWork = unitOfWork;
        _generadorNumero = generadorNumero;
    }

    public async Task<Result<DividirComandaDto>> Handle(DividirComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando división de comanda {ComandaOriginalId} en {CantidadDivisiones} partes",
            request.ComandaOriginalId, request.DivisionItems.Count);

        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Obtener comanda original completa
            var comandaOriginalResult = await ObtenerComandaOriginal(request.ComandaOriginalId, cancellationToken);
            if (!comandaOriginalResult.Succeeded)
            {
                return Result.Failure<DividirComandaDto>(comandaOriginalResult.Error!);
            }

            var comandaOriginal = comandaOriginalResult.Value;

            // 2. Validar distribución de items
            var validacionResult = await ValidarDistribucionItems(comandaOriginal, request, cancellationToken);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure<DividirComandaDto>(validacionResult.Error!);
            }

            // 3. Crear nuevas comandas
            var nuevasComandasResult = await CrearNuevasComandas(comandaOriginal, request, cancellationToken);
            if (!nuevasComandasResult.Succeeded)
            {
                return Result.Failure<DividirComandaDto>(nuevasComandasResult.Error!);
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
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 9. Crear respuesta
            var response = CrearRespuesta(comandaOriginal, nuevasComandas, request);

            _logger.LogInformation("✅ División completada exitosamente. Comanda original: {ComandaOriginalId}, Nuevas comandas: {NuevasComandasIds}",
                request.ComandaOriginalId, string.Join(", ", nuevasComandas.Select(c => c.Id)));

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al dividir comanda {ComandaOriginalId}: {ErrorMessage}", 
                request.ComandaOriginalId, ex.Message);
            return Result.Failure<DividirComandaDto>($"Error interno al dividir la comanda: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<Comanda>> ObtenerComandaOriginal(Guid comandaId, CancellationToken cancellationToken)
    {
        var comanda = await _context.Comandas
            .Include(c => c.Mesa)
            .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
            .Include(c => c.Descuentos)
            .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);

        if (comanda == null)
        {
            return Result.Failure<Comanda>("La comanda original especificada no existe.");
        }

        return Result.Success(comanda);
    }

    private async Task<Result> ValidarDistribucionItems(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
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
                    return Result.Failure($"El item {itemOriginal.Key} no está distribuido en ninguna nueva comanda.");
                }
                continue;
            }

            var cantidadDistribuida = itemsDistribuidos[itemOriginal.Key];
            var cantidadOriginal = itemOriginal.Value;

            if (cantidadDistribuida > cantidadOriginal)
            {
                return Result.Failure($"La cantidad distribuida del item {itemOriginal.Key} ({cantidadDistribuida}) excede la cantidad original ({cantidadOriginal}).");
            }

            if (!request.MantenerComandaOriginal && cantidadDistribuida < cantidadOriginal)
            {
                return Result.Failure($"La cantidad distribuida del item {itemOriginal.Key} ({cantidadDistribuida}) es menor que la cantidad original ({cantidadOriginal}).");
            }
        }

        return Result.Success();
    }

    private async Task<Result<List<Comanda>>> CrearNuevasComandas(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        var nuevasComandas = new List<Comanda>();

        foreach (var division in request.DivisionItems)
        {
            var numeroComanda = await _generadorNumero.GenerarNumeroComandaAsync(cancellationToken);
            
            var nuevaComanda = new Comanda
            {
                Id = Guid.NewGuid(),
                NumeroComanda = numeroComanda,
                MesaId = division.MesaDestinoId ?? comandaOriginal.MesaId,
                MeseroId = division.MeseroId ?? comandaOriginal.MeseroId,
                ClienteId = comandaOriginal.ClienteId,
                Estado = EstadoComanda.Creada,
                TipoComanda = comandaOriginal.TipoComanda,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = _currentUserService.UserId,
                ComandaOrigenId = comandaOriginal.Id, // Referencia a la comanda original
                Observaciones = division.Observaciones ?? $"División de comanda #{comandaOriginal.NumeroComanda}",
                Items = new List<ItemComanda>()
            };

            nuevasComandas.Add(nuevaComanda);
            _context.Comandas.Add(nuevaComanda);
        }

        return Result.Success(nuevasComandas);
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

                var nuevoItem = new ItemComanda
                {
                    Id = Guid.NewGuid(),
                    ComandaId = nuevaComanda.Id,
                    ProductoId = itemOriginal.ProductoId,
                    Producto = itemOriginal.Producto,
                    Cantidad = itemDivision.Cantidad,
                    PrecioUnitario = itemOriginal.PrecioUnitario,
                    Descuento = 0, // Los descuentos se distribuyen después
                    Estado = EstadoItemComanda.Pendiente,
                    Observaciones = itemDivision.ObservacionesItem ?? itemOriginal.Observaciones,
                    FechaCreacion = DateTime.UtcNow,
                    CreadoPor = _currentUserService.UserId
                };

                nuevaComanda.Items.Add(nuevoItem);
                _context.ItemsComanda.Add(nuevoItem);

                // Reducir cantidad en el item original si no se mantiene la comanda original
                if (!request.MantenerComandaOriginal)
                {
                    itemOriginal.Cantidad -= itemDivision.Cantidad;
                    if (itemOriginal.Cantidad <= 0)
                    {
                        _context.ItemsComanda.Remove(itemOriginal);
                    }
                    else
                    {
                        _context.ItemsComanda.Update(itemOriginal);
                    }
                }
            }

            // Calcular totales de la nueva comanda
            nuevaComanda.Subtotal = nuevaComanda.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
            nuevaComanda.Total = nuevaComanda.Subtotal; // Se ajustará con descuentos
            _context.Comandas.Update(nuevaComanda);
        }
    }

    private async Task DistribuirDescuentos(Comanda comandaOriginal, List<Comanda> nuevasComandas, CancellationToken cancellationToken)
    {
        if (!comandaOriginal.Descuentos.Any()) return;

        var totalOriginal = comandaOriginal.Subtotal;
        var descuentoTotalOriginal = comandaOriginal.Descuentos.Sum(d => d.Monto);

        foreach (var nuevaComanda in nuevasComandas)
        {
            var proporcion = nuevaComanda.Subtotal / totalOriginal;
            var descuentoProporcional = descuentoTotalOriginal * proporcion;

            if (descuentoProporcional > 0)
            {
                var descuentoComanda = new DescuentoComanda
                {
                    Id = Guid.NewGuid(),
                    ComandaId = nuevaComanda.Id,
                    TipoDescuento = "Proporcional División",
                    Monto = descuentoProporcional,
                    Porcentaje = (descuentoProporcional / nuevaComanda.Subtotal) * 100,
                    Motivo = $"Descuento proporcional por división de comanda #{comandaOriginal.NumeroComanda}",
                    FechaAplicacion = DateTime.UtcNow,
                    AplicadoPor = _currentUserService.UserId
                };

                _context.DescuentosComanda.Add(descuentoComanda);
                nuevaComanda.Total = nuevaComanda.Subtotal - descuentoProporcional;
                _context.Comandas.Update(nuevaComanda);
            }
        }
    }

    private async Task ActualizarComandaOriginal(Comanda comandaOriginal, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        if (request.MantenerComandaOriginal)
        {
            // Mantener la comanda original como está, solo marcar que fue dividida
            comandaOriginal.Observaciones += $" [Dividida el {DateTime.UtcNow:dd/MM/yyyy HH:mm}]";
        }
        else
        {
            // Si no se mantiene, verificar si quedan items
            if (!comandaOriginal.Items.Any())
            {
                comandaOriginal.Estado = EstadoComanda.Dividida;
                comandaOriginal.FechaFinalizacion = DateTime.UtcNow;
            }
            else
            {
                // Recalcular totales
                comandaOriginal.Subtotal = comandaOriginal.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
                comandaOriginal.Total = comandaOriginal.Subtotal - comandaOriginal.Descuentos.Sum(d => d.Monto);
            }
        }

        comandaOriginal.FechaUltimaActualizacion = DateTime.UtcNow;
        comandaOriginal.ActualizadoPor = _currentUserService.UserId;
        _context.Comandas.Update(comandaOriginal);
    }

    private async Task RegistrarAuditoria(Comanda comandaOriginal, List<Comanda> nuevasComandas, DividirComandaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditoria = new RegistroAuditoria
            {
                EntidadTipo = nameof(Comanda),
                EntidadId = comandaOriginal.Id.ToString(),
                Accion = "División Comanda",
                ValoresAnteriores = JsonSerializer.Serialize(new { Estado = comandaOriginal.Estado, Items = comandaOriginal.Items.Count }),
                ValoresNuevos = JsonSerializer.Serialize(new { NuevasComandasIds = nuevasComandas.Select(c => c.Id), TipoDivision = request.TipoDivision }),
                Motivo = request.MotivoDivision,
                UsuarioId = _currentUserService.UserId,
                Fecha = DateTime.UtcNow,
                DatosAdicionales = request.DatosAdicionales != null ? JsonSerializer.Serialize(request.DatosAdicionales) : null
            };

            _context.RegistrosAuditoria.Add(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría para división de comanda {ComandaId}", comandaOriginal.Id);
        }
    }

    private DividirComandaDto CrearRespuesta(Comanda comandaOriginal, List<Comanda> nuevasComandas, DividirComandaCommand request)
    {
        return new DividirComandaDto
        {
            ComandaOriginalId = comandaOriginal.Id,
            ComandasNuevasIds = nuevasComandas.Select(c => c.Id).ToList(),
            TipoDivision = request.TipoDivision,
            MotivoDivision = request.MotivoDivision,
            FechaDivision = DateTime.UtcNow,
            AutorizadoPor = request.AutorizadoPor,
            DivisionExitosa = true,
            TotalComandasCreadas = nuevasComandas.Count
        };
    }

    #endregion
} 