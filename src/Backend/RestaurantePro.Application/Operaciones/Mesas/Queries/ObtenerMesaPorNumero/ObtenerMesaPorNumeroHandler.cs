using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorNumero;

/// <summary>
/// Handler para ObtenerMesaPorNumeroQuery
/// </summary>
public class ObtenerMesaPorNumeroHandler : IRequestHandler<ObtenerMesaPorNumeroQuery, Result<MesaDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerMesaPorNumeroHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerMesaPorNumeroHandler(
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ObtenerMesaPorNumeroHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<MesaDto>> Handle(ObtenerMesaPorNumeroQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando mesa número {Numero} en zona {Zona}", 
                request.Numero, request.Zona ?? "Todas las zonas");

            // Buscar la mesa por número
            var mesa = await _mesaRepository.ObtenerPorNumeroAsync(request.Numero);

            if (mesa == null)
            {
                var mensajeError = $"Mesa número {request.Numero} no encontrada";
                _logger.LogWarning("⚠️ {MensajeError}", mensajeError);
                return Result.Failure<MesaDto>(mensajeError);
            }

            // Verificar filtro por zona si se especifica
            if (!string.IsNullOrEmpty(request.Zona) && 
                !mesa.Ubicacion.Equals(request.Zona, StringComparison.OrdinalIgnoreCase))
            {
                var mensajeError = $"Mesa número {request.Numero} no encontrada en la zona {request.Zona}";
                _logger.LogWarning("⚠️ Mesa {Numero} encontrada pero en zona diferente. Esperada: {ZonaEsperada}, Actual: {ZonaActual}", 
                    request.Numero, request.Zona, mesa.Ubicacion);
                return Result.Failure<MesaDto>(mensajeError);
            }

            // Mapear a DTO
            var mesaDto = _mapper.Map<MesaDto>(mesa);

            // Log de información adicional
            _logger.LogDebug("📋 Mesa encontrada - ID: {MesaId}, Estado: {Estado}, Capacidad: {Capacidad}, Zona: {Zona}", 
                mesa.Id, mesa.Estado, mesa.Capacidad, mesa.Ubicacion);

            // Log según el estado actual
            switch (mesa.Estado)
            {
                case RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.Disponible:
                    _logger.LogInformation("✅ Mesa {Numero} está disponible", request.Numero);
                    break;
                case RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.Ocupada:
                    _logger.LogInformation("🍽️ Mesa {Numero} está ocupada", request.Numero);
                    break;
                case RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.Reservada:
                    _logger.LogInformation("📅 Mesa {Numero} está reservada", request.Numero);
                    break;
                case RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.FueraDeServicio:
                    _logger.LogInformation("🚫 Mesa {Numero} está fuera de servicio", request.Numero);
                    break;
            }

            _logger.LogInformation("✅ Mesa {Numero} obtenida exitosamente", request.Numero);

            return Result.Success(mesaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener mesa por número {Numero}", request.Numero);
            return Result.Failure<MesaDto>("Error interno del servidor al obtener la mesa");
        }
    }
} 