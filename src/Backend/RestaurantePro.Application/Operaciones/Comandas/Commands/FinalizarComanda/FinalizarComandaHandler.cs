namespace RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

/// <summary>
/// 🍽️ Handler para finalizar comandas
/// </summary>
public class FinalizarComandaHandler : IRequestHandler<FinalizarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FinalizarComandaHandler> _logger;

    public FinalizarComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<FinalizarComandaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(
        FinalizarComandaCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍽️ Iniciando finalización de comanda {ComandaId} por usuario {UsuarioId}", 
            request.ComandaId, request.UsuarioId);

        try
        {
            // 1. Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId);
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda {ComandaId} no encontrada", request.ComandaId);
                return Result<ComandaDto>.Failure("Comanda no encontrada");
            }

            // 2. Validar estado actual
            if (comanda.Estado == EstadoComanda.Finalizada)
            {
                _logger.LogWarning("⚠️ Comanda {ComandaId} ya está finalizada", request.ComandaId);
                return Result<ComandaDto>.Failure("La comanda ya está finalizada");
            }

            if (comanda.Estado == EstadoComanda.Cancelada)
            {
                _logger.LogWarning("⚠️ Comanda {ComandaId} está cancelada", request.ComandaId);
                return Result<ComandaDto>.Failure("No se puede finalizar una comanda cancelada");
            }

            // 3. Validar items si se requiere
            if (request.ValidarTodosItemsListos)
            {
                var itemsPendientes = comanda.Items.Where(i => 
                    i.Estado != EstadoItemComanda.Listo && 
                    i.Estado != EstadoItemComanda.Entregado).ToList();

                if (itemsPendientes.Any())
                {
                    _logger.LogWarning("⚠️ Comanda {ComandaId} tiene {Count} items pendientes", 
                        request.ComandaId, itemsPendientes.Count);
                    return Result<ComandaDto>.Failure(
                        $"La comanda tiene {itemsPendientes.Count} items pendientes de preparación");
                }
            }

            // 4. Finalizar la comanda
            var fechaFinalizacion = request.FechaFinalizacion ?? DateTime.UtcNow;
            var resultadoFinalizacion = comanda.Finalizar(fechaFinalizacion);
            
            if (resultadoFinalizacion.IsFailure)
            {
                _logger.LogError("❌ Error al finalizar comanda {ComandaId}: {Error}", 
                    request.ComandaId, resultadoFinalizacion.Error);
                return Result<ComandaDto>.Failure(resultadoFinalizacion.Error);
            }

            // 5. Agregar observaciones si las hay
            if (!string.IsNullOrWhiteSpace(request.ObservacionesFinalizacion))
            {
                comanda.AgregarObservacion($"[FINALIZACIÓN] {request.ObservacionesFinalizacion}");
            }

            // 6. Guardar cambios
            await _comandaRepository.ActualizarAsync(comanda);

            // 7. Mapear a DTO
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Comanda {ComandaId} finalizada exitosamente. Items: {TotalItems}, Total: {Total:C}", 
                request.ComandaId, comanda.Items.Count, comanda.CalcularTotal());

            // 8. Log adicional si hay notificación de mesero
            if (request.NotificarMesero)
            {
                _logger.LogInformation("📱 Se enviará notificación al mesero para comanda {ComandaId}", 
                    request.ComandaId);
            }

            return Result<ComandaDto>.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al finalizar comanda {ComandaId}", request.ComandaId);
            return Result<ComandaDto>.Failure($"Error interno al finalizar comanda: {ex.Message}");
        }
    }
} 