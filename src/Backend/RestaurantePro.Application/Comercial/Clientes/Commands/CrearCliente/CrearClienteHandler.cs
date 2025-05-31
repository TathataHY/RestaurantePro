namespace RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;

/// <summary>
/// Handler para el comando CrearCliente
/// Implementa la lógica de negocio para crear clientes
/// </summary>
public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearClienteHandler> _logger;

    public CrearClienteHandler(
        IClienteRepository repository,
        IMapper mapper,
        ILogger<CrearClienteHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClienteDto>> Handle(
        CrearClienteCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🎯 Iniciando creación de cliente: {Email}", request.Email);

        try
        {
            // 1. Verificar que no existe un cliente con el mismo email
            var existeCliente = await _repository.ObtenerPorEmailAsync(request.Email, cancellationToken);
            if (existeCliente != null)
            {
                _logger.LogWarning("⚠️ Ya existe un cliente con el email: {Email}", request.Email);
                return Result.Failure<ClienteDto>($"Ya existe un cliente registrado con el email {request.Email}");
            }

            // 2. Crear ClienteNombre a partir del nombre completo
            var partesNombre = request.Nombre.Trim().Split(' ', 2);
            var nombre = partesNombre[0];
            var apellidos = partesNombre.Length > 1 ? partesNombre[1] : string.Empty;
            var clienteNombre = ClienteNombre.Crear(nombre, apellidos);

            // 3. Crear la entidad usando el factory method del dominio
            var cliente = Cliente.Crear(
                clienteNombre,
                request.Email,
                request.Telefono,
                request.FechaNacimiento);

            // Si no debe estar activo, desactivarlo
            if (!request.EstaActivo)
            {
                cliente.Desactivar();
            }

            // 4. Persistir en el repositorio
            await _repository.GuardarAsync(cliente, cancellationToken);

            // 5. Mapear a DTO y retornar
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            _logger.LogInformation("✅ Cliente creado exitosamente: {Id} - {Email}", cliente.Id, request.Email);
            return Result.Success(clienteDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al crear cliente: {Message}", ex.Message);
            return Result.Failure<ClienteDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al crear cliente: {Email}", request.Email);
            return Result.Failure<ClienteDto>("Error interno del servidor al crear el cliente");
        }
    }
} 