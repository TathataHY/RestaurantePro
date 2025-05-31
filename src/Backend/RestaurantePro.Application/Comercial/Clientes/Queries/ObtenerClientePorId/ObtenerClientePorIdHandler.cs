using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Handler para obtener un cliente por su ID
/// Implementa búsqueda directa con manejo de casos de no encontrado
/// </summary>
public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, Result<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerClientePorIdHandler> _logger;

    public ObtenerClientePorIdHandler(
        IClienteRepository clienteRepository,
        IMapper mapper,
        ILogger<ObtenerClientePorIdHandler> logger)
    {
        _clienteRepository = clienteRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClienteDto>> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Consultando cliente por ID: {ClienteId}", request.ClienteId);

            // 1. Obtener el cliente del repositorio
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<ClienteDto>("El cliente especificado no existe.");
            }

            // 2. Usar el mapper para crear el DTO
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            _logger.LogInformation("Cliente {ClienteId} consultado exitosamente", request.ClienteId);

            return Result.Success(clienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar cliente {ClienteId}", request.ClienteId);
            return Result.Failure<ClienteDto>("Error interno al obtener el cliente.");
        }
    }
} 