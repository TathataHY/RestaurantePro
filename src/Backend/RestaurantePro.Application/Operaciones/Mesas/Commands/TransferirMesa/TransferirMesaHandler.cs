namespace RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;

/// <summary>
/// Handler para transferir comandas entre mesas
/// Gestiona el proceso completo de transferencia con validaciones de negocio
/// </summary>
public class TransferirMesaHandler : IRequestHandler<TransferirMesaCommand, Result<TransferirMesaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TransferirMesaHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificacionService _notificacionService;
    private readonly IUnitOfWork _unitOfWork;

    public TransferirMesaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<TransferirMesaHandler> logger,
        ICurrentUserService currentUserService,
        INotificacionService notificacionService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransferirMesaDto>> Handle(TransferirMesaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando transferencia de comanda {ComandaId} de mesa {MesaOrigenId} a mesa {MesaDestinoId}",
            request.ComandaId, request.MesaOrigenId, request.MesaDestinoId);

        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Obtener entidades necesarias
            var entidadesResult = await ObtenerEntidades(request, cancellationToken);
            if (!entidadesResult.Succeeded)
            {
                return Result.Failure<TransferirMesaDto>(entidadesResult.Error!);
            }

            var (comanda, mesaOrigen, mesaDestino) = entidadesResult.Value;

            // 2. Validar reglas de negocio adicionales
            var validacionResult = await ValidarReglasNegocio(comanda, mesaOrigen, mesaDestino, cancellationToken);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure<TransferirMesaDto>(validacionResult.Error!);
            }

            // 3. Actualizar estado de mesas
            await ActualizarEstadoMesas(mesaOrigen, mesaDestino, cancellationToken);

            // 4. Transferir la comanda
            var transferenciaResult = await EjecutarTransferencia(comanda, mesaDestino, request, cancellationToken);
            if (!transferenciaResult.Succeeded)
            {
                return Result.Failure<TransferirMesaDto>(transferenciaResult.Error!);
            }

            // 5. Registrar auditoría
            await RegistrarAuditoria(request, cancellationToken);

            // 6. Notificar si es necesario
            if (request.NotificarMesero)
            {
                await NotificarTransferencia(comanda, mesaOrigen, mesaDestino, request, cancellationToken);
            }

            // 7. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 8. Crear DTO de respuesta
            var response = CrearRespuesta(comanda, mesaOrigen, mesaDestino, request);

            _logger.LogInformation("✅ Transferencia completada exitosamente para comanda {ComandaId}", request.ComandaId);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al transferir comanda {ComandaId}: {ErrorMessage}", 
                request.ComandaId, ex.Message);
            return Result.Failure<TransferirMesaDto>($"Error interno al transferir la comanda: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<(Comanda comanda, Mesa mesaOrigen, Mesa mesaDestino)>> ObtenerEntidades(
        TransferirMesaCommand request, CancellationToken cancellationToken)
    {
        // Obtener comanda
        var comanda = await _context.Comandas
            .Include(c => c.Mesa)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.ComandaId, cancellationToken);

        if (comanda == null)
        {
            return Result.Failure<(Comanda, Mesa, Mesa)>("La comanda especificada no existe.");
        }

        // Obtener mesa origen
        var mesaOrigen = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == request.MesaOrigenId, cancellationToken);

        if (mesaOrigen == null)
        {
            return Result.Failure<(Comanda, Mesa, Mesa)>("La mesa de origen especificada no existe.");
        }

        // Obtener mesa destino
        var mesaDestino = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == request.MesaDestinoId, cancellationToken);

        if (mesaDestino == null)
        {
            return Result.Failure<(Comanda, Mesa, Mesa)>("La mesa de destino especificada no existe.");
        }

        return Result.Success((comanda, mesaOrigen, mesaDestino));
    }

    private async Task<Result> ValidarReglasNegocio(Comanda comanda, Mesa mesaOrigen, Mesa mesaDestino, CancellationToken cancellationToken)
    {
        // Verificar que la comanda pertenece a la mesa origen
        if (comanda.MesaId != mesaOrigen.Id)
        {
            return Result.Failure("La comanda no pertenece a la mesa de origen especificada.");
        }

        // Verificar que la mesa destino no tenga comandas activas
        var comandasActivasDestino = await _context.Comandas
            .AnyAsync(c => c.MesaId == mesaDestino.Id && 
                          c.Estado != EstadoComanda.Finalizada && 
                          c.Estado != EstadoComanda.Cancelada, 
                     cancellationToken);

        if (comandasActivasDestino)
        {
            return Result.Failure("La mesa de destino ya tiene comandas activas.");
        }

        // Verificar capacidad de la mesa destino
        var numeroPersonas = comanda.NumeroPersonas ?? 1;
        if (mesaDestino.Capacidad < numeroPersonas)
        {
            return Result.Failure($"La mesa de destino no tiene capacidad suficiente ({mesaDestino.Capacidad} vs {numeroPersonas} personas).");
        }

        return Result.Success();
    }

    private async Task ActualizarEstadoMesas(Mesa mesaOrigen, Mesa mesaDestino, CancellationToken cancellationToken)
    {
        // Verificar si la mesa origen queda sin comandas activas
        var comandasActivasOrigen = await _context.Comandas
            .AnyAsync(c => c.MesaId == mesaOrigen.Id && 
                          c.Estado != EstadoComanda.Finalizada && 
                          c.Estado != EstadoComanda.Cancelada, 
                     cancellationToken);

        if (!comandasActivasOrigen)
        {
            mesaOrigen.Estado = EstadoMesa.Disponible;
            mesaOrigen.FechaUltimaActualizacion = DateTime.UtcNow;
        }

        // La mesa destino se marca como ocupada
        mesaDestino.Estado = EstadoMesa.Ocupada;
        mesaDestino.FechaUltimaActualizacion = DateTime.UtcNow;

        _context.Mesas.Update(mesaOrigen);
        _context.Mesas.Update(mesaDestino);
    }

    private async Task<Result> EjecutarTransferencia(Comanda comanda, Mesa mesaDestino, TransferirMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Actualizar la comanda
            comanda.MesaId = mesaDestino.Id;
            comanda.Mesa = mesaDestino;
            
            if (!request.MantenerEstado)
            {
                // Si no se mantiene el estado, se puede resetear a un estado apropiado
                comanda.Estado = EstadoComanda.EnProceso;
            }

            comanda.FechaUltimaActualizacion = DateTime.UtcNow;
            comanda.ActualizadoPor = _currentUserService.UserId;

            _context.Comandas.Update(comanda);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar transferencia de comanda {ComandaId}", comanda.Id);
            return Result.Failure($"Error al ejecutar la transferencia: {ex.Message}");
        }
    }

    private async Task RegistrarAuditoria(TransferirMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditoria = new RegistroAuditoria
            {
                EntidadTipo = nameof(Comanda),
                EntidadId = request.ComandaId.ToString(),
                Accion = "Transferencia Mesa",
                ValoresAnteriores = JsonSerializer.Serialize(new { MesaOrigenId = request.MesaOrigenId }),
                ValoresNuevos = JsonSerializer.Serialize(new { MesaDestinoId = request.MesaDestinoId }),
                Motivo = request.MotivoTransferencia,
                UsuarioId = _currentUserService.UserId,
                Fecha = DateTime.UtcNow,
                DatosAdicionales = request.DatosAdicionales != null ? JsonSerializer.Serialize(request.DatosAdicionales) : null
            };

            _context.RegistrosAuditoria.Add(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría para transferencia de comanda {ComandaId}", request.ComandaId);
            // No fallar la operación por un error de auditoría
        }
    }

    private async Task NotificarTransferencia(Comanda comanda, Mesa mesaOrigen, Mesa mesaDestino, TransferirMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mensaje = $"La comanda #{comanda.NumeroComanda} ha sido transferida de la Mesa {mesaOrigen.Numero} a la Mesa {mesaDestino.Numero}. Motivo: {request.MotivoTransferencia}";
            
            if (request.NotasTransferencia != null)
            {
                mensaje += $" Notas: {request.NotasTransferencia}";
            }

            await _notificacionService.EnviarNotificacionAsync(
                destinatarios: new[] { comanda.MeseroId.ToString() },
                titulo: "Transferencia de Mesa",
                mensaje: mensaje,
                tipo: TipoNotificacion.TransferenciaMesa,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificación de transferencia para comanda {ComandaId}", comanda.Id);
            // No fallar la operación por un error de notificación
        }
    }

    private TransferirMesaDto CrearRespuesta(Comanda comanda, Mesa mesaOrigen, Mesa mesaDestino, TransferirMesaCommand request)
    {
        return new TransferirMesaDto
        {
            ComandaId = comanda.Id,
            MesaAnteriorId = mesaOrigen.Id,
            MesaNuevaId = mesaDestino.Id,
            MotivoTransferencia = request.MotivoTransferencia,
            FechaTransferencia = DateTime.UtcNow,
            AutorizadoPor = request.AutorizadoPor,
            NotasTransferencia = request.NotasTransferencia,
            TransferenciaExitosa = true
        };
    }

    #endregion
} 