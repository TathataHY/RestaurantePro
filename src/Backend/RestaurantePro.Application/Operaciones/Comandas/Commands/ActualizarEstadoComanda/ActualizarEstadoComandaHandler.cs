namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarEstadoComanda;

/// <summary>
/// Handler para actualizar el estado de una comanda
/// Gestiona el flujo operativo del restaurante con validaciones de transición
/// </summary>
public class ActualizarEstadoComandaHandler : IRequestHandler<ActualizarEstadoComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarEstadoComandaHandler> _logger;

    public ActualizarEstadoComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ActualizarEstadoComandaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(
        ActualizarEstadoComandaCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de estado - Comanda: {ComandaId}, Nuevo estado: {NuevoEstado}", 
            request.ComandaId, 
            request.NuevoEstado);

        try
        {
            // 1. Buscar la comanda existente
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
            if (comanda == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada: {ComandaId}", request.ComandaId);
                return Result.Failure<ComandaDto>($"No se encontró una comanda con el ID {request.ComandaId}");
            }

            var estadoAnterior = comanda.Estado;
            _logger.LogInformation("📊 Estado actual de la comanda: {EstadoActual}", estadoAnterior);

            // 2. Convertir string a enum
            if (!Enum.TryParse<EstadoComanda>(request.NuevoEstado, out var nuevoEstadoEnum))
            {
                _logger.LogWarning("⚠️ Estado inválido: {NuevoEstado}", request.NuevoEstado);
                return Result.Failure<ComandaDto>($"El estado '{request.NuevoEstado}' no es válido");
            }

            // 3. Validar que el estado sea diferente
            if (estadoAnterior == nuevoEstadoEnum)
            {
                _logger.LogInformation("ℹ️ La comanda ya está en el estado solicitado: {Estado}", request.NuevoEstado);
                var comandaDto = _mapper.Map<ComandaDto>(comanda);
                return Result.Success(comandaDto);
            }

            // 4. Aplicar el cambio de estado usando los métodos específicos del dominio
            var resultado = await AplicarCambioEstado(comanda, nuevoEstadoEnum, request);
            if (!resultado.Succeeded)
            {
                return Result.Failure<ComandaDto>(resultado.Error);
            }

            // 5. Persistir los cambios
            await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);

            // 6. Mapear a DTO y retornar
            var comandaActualizada = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Estado actualizado exitosamente - Comanda: {ComandaId}, {EstadoAnterior} → {NuevoEstado}", 
                request.ComandaId, 
                estadoAnterior, 
                nuevoEstadoEnum);

            return Result.Success(comandaActualizada);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al actualizar estado: {Message}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Operación inválida al actualizar estado: {Message}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al actualizar estado de comanda: {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>("Error interno del servidor al actualizar el estado de la comanda");
        }
    }

    /// <summary>
    /// Aplica el cambio de estado usando los métodos específicos del dominio
    /// </summary>
    private async Task<Result> AplicarCambioEstado(
        Comanda comanda, 
        EstadoComanda nuevoEstado, 
        ActualizarEstadoComandaCommand request)
    {
        try
        {
            switch (nuevoEstado)
            {
                case EstadoComanda.EnProceso:
                    var enProcesoExitoso = comanda.MarcarEnPreparacion();
                    if (!enProcesoExitoso)
                    {
                        return Result.Failure("No se puede marcar la comanda en preparación desde su estado actual");
                    }
                    _logger.LogInformation("🍳 Comanda marcada en preparación");
                    break;

                case EstadoComanda.Lista:
                    var listaExitoso = comanda.MarcarLista();
                    if (!listaExitoso)
                    {
                        return Result.Failure("No se puede marcar la comanda como lista desde su estado actual");
                    }
                    _logger.LogInformation("✅ Comanda marcada como lista");
                    break;

                case EstadoComanda.Entregada:
                    var entregadaExitoso = comanda.MarcarEntregada();
                    if (!entregadaExitoso)
                    {
                        return Result.Failure("No se puede marcar la comanda como entregada desde su estado actual");
                    }
                    _logger.LogInformation("🚚 Comanda marcada como entregada");
                    break;

                case EstadoComanda.Finalizada:
                    _logger.LogInformation("💰 [ESTADO HANDLER] Intentando finalizar comanda {ComandaId}", comanda.Id);
                    var finalizadaExitoso = comanda.MarcarPagada();
                    if (!finalizadaExitoso)
                    {
                        _logger.LogWarning("❌ [ESTADO HANDLER] No se puede finalizar la comanda {ComandaId} desde su estado actual {EstadoActual}", comanda.Id, comanda.Estado);
                        return Result.Failure("No se puede finalizar la comanda desde su estado actual");
                    }
                    _logger.LogInformation("✅ [ESTADO HANDLER] Comanda {ComandaId} finalizada (pagada) exitosamente", comanda.Id);
                    break;

                case EstadoComanda.Cancelada:
                    var motivo = request.Observaciones ?? "Cancelación solicitada por usuario";
                    comanda.Cancelar(motivo);
                    _logger.LogInformation("❌ Comanda cancelada - Motivo: {Motivo}", motivo);
                    break;

                default:
                    _logger.LogWarning("⚠️ Transición de estado no implementada: {NuevoEstado}", nuevoEstado);
                    return Result.Failure($"La transición al estado '{nuevoEstado}' no está implementada");
            }

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Transición de estado inválida: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
} 