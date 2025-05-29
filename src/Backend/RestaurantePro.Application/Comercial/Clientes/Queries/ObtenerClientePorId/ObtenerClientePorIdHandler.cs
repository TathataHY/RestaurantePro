using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;

/// <summary>
/// Handler para obtener un cliente por su ID
/// Implementa búsqueda directa con manejo de casos de no encontrado
/// </summary>
public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerClientePorIdHandler> _logger;

    public ObtenerClientePorIdHandler(
        IClienteRepository repository,
        IMapper mapper,
        ILogger<ObtenerClientePorIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClienteDto>> Handle(
        ObtenerClientePorIdQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Buscando cliente por ID: {ClienteId}", request.ClienteId);

        try
        {
            // 1. Buscar el cliente en el repositorio
            var cliente = await _repository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado: {ClienteId}", request.ClienteId);
                return Result.Failure<ClienteDto>($"No se encontró un cliente con el ID {request.ClienteId}");
            }

            // 2. Verificar si el cliente está eliminado lógicamente
            if (cliente.EstaEliminado)
            {
                _logger.LogWarning("🗑️ Cliente eliminado: {ClienteId}", request.ClienteId);
                return Result.Failure<ClienteDto>($"El cliente con ID {request.ClienteId} ha sido eliminado");
            }

            // 3. Mapear a DTO y retornar
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            _logger.LogInformation("✅ Cliente encontrado exitosamente: {ClienteId} - {Email}", 
                cliente.Id, 
                cliente.Email.Value);

            return Result.Success(clienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al buscar cliente por ID: {ClienteId}", request.ClienteId);
            return Result.Failure<ClienteDto>("Error interno del servidor al buscar el cliente");
        }
    }
} 