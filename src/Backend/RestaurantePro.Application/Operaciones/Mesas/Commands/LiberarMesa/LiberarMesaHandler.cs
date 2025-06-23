namespace RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

/// <summary>
/// Handler para LiberarMesaCommand
/// </summary>
public class LiberarMesaHandler : IRequestHandler<LiberarMesaCommand, Result<MesaDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<LiberarMesaHandler> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LiberarMesaHandler(
        IMesaRepository mesaRepository,
        ILogger<LiberarMesaHandler> logger,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MesaDto>> Handle(LiberarMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Iniciando liberación de mesa {MesaId} por usuario {UserId}", 
                request.MesaId, _currentUser.UserId);

            // Obtener la mesa
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId);
            if (mesa == null)
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} no encontrada", request.MesaId);
                return Result.Failure<MesaDto>("Mesa no encontrada");
            }

            // Verificar estado actual de la mesa
            _logger.LogDebug("Mesa {MesaId} encontrada. Estado actual: {EstadoActual}", 
                mesa.Id, mesa.Estado);

            // Verificar que la mesa no esté fuera de servicio
            if (mesa.Estado == EstadoMesa.FueraDeServicio)
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} está fuera de servicio y no puede ser liberada. Estado actual: {EstadoActual}", 
                    mesa.Id, mesa.Estado);
                return Result.Failure<MesaDto>($"La mesa está fuera de servicio y no puede ser liberada. Estado actual: {mesa.Estado}");
            }

            // Si la mesa ya está disponible, devolver el DTO actual
            if (mesa.Estado == EstadoMesa.Disponible)
            {
                _logger.LogInformation("ℹ️ Mesa {MesaId} ya está disponible. No se requiere acción.", mesa.Id);
                var mesaDtoActual = _mapper.Map<MesaDto>(mesa);
                return Result.Success(mesaDtoActual);
            }

            // Marcar la mesa como disponible
            mesa.MarcarComoDisponible();
            
            _logger.LogDebug("Mesa {MesaId} marcada como disponible. Estado después del cambio: {NuevoEstado}", 
                mesa.Id, mesa.Estado);

            // Actualizar en el repositorio
            await _mesaRepository.ActualizarAsync(mesa);
            
            // Guardar cambios usando UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Obtener la mesa actualizada sin tracking para devolver el DTO
            var mesaActualizada = await _mesaRepository.ObtenerPorIdSinTrackingAsync(request.MesaId);
            if (mesaActualizada == null)
            {
                _logger.LogError("❌ Error al obtener mesa actualizada después de liberar {MesaId}", request.MesaId);
                return Result.Failure<MesaDto>("Error al obtener la mesa actualizada");
            }
            
            var mesaDtoActualizada = _mapper.Map<MesaDto>(mesaActualizada);
            
            _logger.LogInformation("✅ Mesa {NumeroMesa} (ID: {MesaId}) liberada correctamente. Estado final: {EstadoFinal}", 
                mesaActualizada.Numero, mesa.Id, mesaActualizada.Estado);

            return Result.Success(mesaDtoActualizada);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error de negocio al liberar mesa {MesaId}: {Error}", 
                request.MesaId, ex.Message);
            return Result.Failure<MesaDto>($"Error al liberar mesa: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error interno al liberar mesa {MesaId}", request.MesaId);
            return Result.Failure<MesaDto>("Error interno del servidor al liberar la mesa");
        }
    }
} 