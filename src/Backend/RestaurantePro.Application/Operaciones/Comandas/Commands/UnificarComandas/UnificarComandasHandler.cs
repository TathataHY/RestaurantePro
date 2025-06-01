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
    private readonly IGeneradorNumeroComandaService _generadorNumero;

    public UnificarComandasHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<UnificarComandasHandler> logger,
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

    public async Task<Result<UnificarComandasDto>> Handle(UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando unificación de {CantidadComandas} comandas en mesa {MesaDestinoId}",
            request.ComandasIds.Count, request.MesaDestinoId);

        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Obtener comandas originales
            var comandasOriginalesResult = await ObtenerComandasOriginales(request.ComandasIds, cancellationToken);
            if (!comandasOriginalesResult.Succeeded)
            {
                return Result.Failure<UnificarComandasDto>(comandasOriginalesResult.Error!);
            }

            var comandasOriginales = comandasOriginalesResult.Value;

            // 2. Determinar comanda principal o crear nueva
            var comandaUnificadaResult = await ObtenerOCrearComandaUnificada(comandasOriginales, request, cancellationToken);
            if (!comandaUnificadaResult.Succeeded)
            {
                return Result.Failure<UnificarComandasDto>(comandaUnificadaResult.Error!);
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
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 9. Crear respuesta
            var response = CrearRespuesta(comandasOriginales, comandaUnificada, request);

            _logger.LogInformation("✅ Unificación completada exitosamente. Comandas originales: {ComandasOriginalesIds}, Comanda unificada: {ComandaUnificadaId}",
                string.Join(", ", request.ComandasIds), comandaUnificada.Id);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al unificar comandas {ComandasIds}: {ErrorMessage}", 
                string.Join(", ", request.ComandasIds), ex.Message);
            return Result.Failure<UnificarComandasDto>($"Error interno al unificar las comandas: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<List<Comanda>>> ObtenerComandasOriginales(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandas = await _context.Comandas
            .Include(c => c.Mesa)
            .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
            .Include(c => c.Descuentos)
            .Where(c => comandasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (comandas.Count != comandasIds.Count)
        {
            var faltantes = comandasIds.Except(comandas.Select(c => c.Id)).ToList();
            return Result.Failure<List<Comanda>>($"Las siguientes comandas no fueron encontradas: {string.Join(", ", faltantes)}");
        }

        return Result.Success(comandas);
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
                return Result.Failure<Comanda>("La comanda principal especificada no se encuentra en la lista de comandas a unificar.");
            }

            // Actualizar propiedades de la comanda principal
            comandaUnificada.MesaId = request.MesaDestinoId;
            comandaUnificada.MeseroId = request.MeseroId;
            comandaUnificada.Observaciones = request.ObservacionesUnificada ?? comandaUnificada.Observaciones;
        }
        else
        {
            // Crear nueva comanda unificada
            var numeroComanda = await _generadorNumero.GenerarNumeroComandaAsync(cancellationToken);
            
            comandaUnificada = new Comanda
            {
                Id = Guid.NewGuid(),
                NumeroComanda = numeroComanda,
                MesaId = request.MesaDestinoId,
                MeseroId = request.MeseroId,
                ClienteId = comandasOriginales.FirstOrDefault()?.ClienteId, // Tomar cliente de la primera comanda
                Estado = EstadoComanda.Creada,
                TipoComanda = comandasOriginales.First().TipoComanda,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = _currentUserService.UserId,
                Observaciones = request.ObservacionesUnificada ?? $"Unificación de comandas: {string.Join(", ", comandasOriginales.Select(c => c.NumeroComanda))}",
                Items = new List<ItemComanda>(),
                Descuentos = new List<DescuentoComanda>()
            };

            _context.Comandas.Add(comandaUnificada);
        }

        return Result.Success(comandaUnificada);
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
                CantidadTotal = grupo.Sum(i => i.Cantidad),
                DescuentoTotal = grupo.Sum(i => i.Descuento),
                ProductoEjemplo = grupo.First().Producto
            })
            .ToList();

        // Crear nuevos items consolidados
        foreach (var itemConsolidado in itemsConsolidados)
        {
            var nuevoItem = new ItemComanda
            {
                Id = Guid.NewGuid(),
                ComandaId = comandaUnificada.Id,
                ProductoId = itemConsolidado.ProductoId,
                Producto = itemConsolidado.ProductoEjemplo,
                Cantidad = itemConsolidado.CantidadTotal,
                PrecioUnitario = itemConsolidado.PrecioUnitario,
                Descuento = itemConsolidado.DescuentoTotal,
                Estado = EstadoItemComanda.Pendiente,
                Observaciones = itemConsolidado.Observaciones,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = _currentUserService.UserId
            };

            comandaUnificada.Items.Add(nuevoItem);
            _context.ItemsComanda.Add(nuevoItem);
        }

        // Calcular totales iniciales
        comandaUnificada.Subtotal = comandaUnificada.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
        comandaUnificada.Total = comandaUnificada.Subtotal;
    }

    private async Task AplicarEstrategiaDescuentos(List<Comanda> comandasOriginales, Comanda comandaUnificada, EstrategiaDescuentos estrategia, CancellationToken cancellationToken)
    {
        var descuentosOriginales = comandasOriginales.SelectMany(c => c.Descuentos).ToList();
        if (!descuentosOriginales.Any()) return;

        decimal descuentoFinal = estrategia switch
        {
            EstrategiaDescuentos.Sumar => descuentosOriginales.Sum(d => d.Monto),
            EstrategiaDescuentos.TomarMayor => descuentosOriginales.Max(d => d.Monto),
            EstrategiaDescuentos.TomarMenor => descuentosOriginales.Min(d => d.Monto),
            EstrategiaDescuentos.Promedio => descuentosOriginales.Average(d => d.Monto),
            EstrategiaDescuentos.SinDescuentos => 0,
            _ => 0
        };

        if (descuentoFinal > 0)
        {
            var descuentoUnificado = new DescuentoComanda
            {
                Id = Guid.NewGuid(),
                ComandaId = comandaUnificada.Id,
                TipoDescuento = $"Unificación - {estrategia}",
                Monto = descuentoFinal,
                Porcentaje = (descuentoFinal / comandaUnificada.Subtotal) * 100,
                Motivo = $"Descuento aplicado por unificación usando estrategia: {estrategia}",
                FechaAplicacion = DateTime.UtcNow,
                AplicadoPor = _currentUserService.UserId
            };

            comandaUnificada.Descuentos.Add(descuentoUnificado);
            _context.DescuentosComanda.Add(descuentoUnificado);
            comandaUnificada.Total = comandaUnificada.Subtotal - descuentoFinal;
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
                // Marcar como unificada pero mantener en histórico
                comandaOriginal.Estado = EstadoComanda.Unificada;
                comandaOriginal.ComandaDestinoId = comandaUnificada.Id;
                comandaOriginal.Observaciones += $" [Unificada en comanda #{comandaUnificada.NumeroComanda} el {DateTime.UtcNow:dd/MM/yyyy HH:mm}]";
            }
            else
            {
                // Marcar para eliminación lógica
                comandaOriginal.Estado = EstadoComanda.Cancelada;
                comandaOriginal.FechaFinalizacion = DateTime.UtcNow;
                comandaOriginal.Observaciones += $" [Cancelada por unificación el {DateTime.UtcNow:dd/MM/yyyy HH:mm}]";
            }

            comandaOriginal.FechaUltimaActualizacion = DateTime.UtcNow;
            comandaOriginal.ActualizadoPor = _currentUserService.UserId;
            _context.Comandas.Update(comandaOriginal);
        }
    }

    private async Task ActualizarMesaDestino(Guid mesaDestinoId, CancellationToken cancellationToken)
    {
        var mesaDestino = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == mesaDestinoId, cancellationToken);

        if (mesaDestino != null)
        {
            mesaDestino.Estado = EstadoMesa.Ocupada;
            mesaDestino.FechaUltimaActualizacion = DateTime.UtcNow;
            _context.Mesas.Update(mesaDestino);
        }
    }

    private async Task RegistrarAuditoria(List<Comanda> comandasOriginales, Comanda comandaUnificada, UnificarComandasCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditoria = new RegistroAuditoria
            {
                EntidadTipo = nameof(Comanda),
                EntidadId = comandaUnificada.Id.ToString(),
                Accion = "Unificación Comandas",
                ValoresAnteriores = JsonSerializer.Serialize(new { ComandasOriginalesIds = comandasOriginales.Select(c => c.Id) }),
                ValoresNuevos = JsonSerializer.Serialize(new { ComandaUnificadaId = comandaUnificada.Id, EstrategiaDescuentos = request.EstrategiaDescuentos }),
                Motivo = request.MotivoUnificacion,
                UsuarioId = _currentUserService.UserId,
                Fecha = DateTime.UtcNow,
                DatosAdicionales = request.DatosAdicionales != null ? JsonSerializer.Serialize(request.DatosAdicionales) : null
            };

            _context.RegistrosAuditoria.Add(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría para unificación de comandas");
        }
    }

    private UnificarComandasDto CrearRespuesta(List<Comanda> comandasOriginales, Comanda comandaUnificada, UnificarComandasCommand request)
    {
        return new UnificarComandasDto
        {
            ComandasOriginalesIds = comandasOriginales.Select(c => c.Id).ToList(),
            ComandaUnificadaId = comandaUnificada.Id,
            MesaDestinoId = request.MesaDestinoId,
            MeseroId = request.MeseroId,
            MotivoUnificacion = request.MotivoUnificacion,
            FechaUnificacion = DateTime.UtcNow,
            AutorizadoPor = request.AutorizadoPor,
            UnificacionExitosa = true,
            TotalItemsUnificados = comandaUnificada.Items.Sum(i => i.Cantidad),
            MontoTotalUnificado = comandaUnificada.Total,
            EstrategiaDescuentos = request.EstrategiaDescuentos
        };
    }

    #endregion
} 