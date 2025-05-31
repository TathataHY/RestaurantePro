namespace RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;

/// <summary>
/// Handler para el comando ActualizarCliente
/// Maneja la actualización de la información de un cliente existente
/// </summary>
public class ActualizarClienteHandler : IRequestHandler<ActualizarClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarClienteHandler> _logger;

    public ActualizarClienteHandler(
        IClienteRepository repository,
        IMapper mapper,
        ILogger<ActualizarClienteHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClienteDto>> Handle(
        ActualizarClienteCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de cliente: {ClienteId}", request.Id);

        try
        {
            // 1. Buscar el cliente existente
            var cliente = await _repository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado: {ClienteId}", request.Id);
                return Result.Failure<ClienteDto>($"No se encontró el cliente con ID {request.Id}");
            }

            // 2. Verificar si el email ya existe en otro cliente
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != cliente.Email.Value)
            {
                var clienteConEmail = await _repository.ObtenerPorEmailAsync(request.Email, cancellationToken);
                if (clienteConEmail != null && clienteConEmail.Id != request.Id)
                {
                    _logger.LogWarning("⚠️ Ya existe otro cliente con el email: {Email}", request.Email);
                    return Result.Failure<ClienteDto>($"Ya existe otro cliente registrado con el email {request.Email}");
                }
            }

            // 3. Actualizar información de contacto (email y teléfono)
            var emailActualizado = !string.IsNullOrWhiteSpace(request.Email);
            var telefonoActualizado = !string.IsNullOrWhiteSpace(request.Telefono);

            if (emailActualizado || telefonoActualizado)
            {
                var nuevoEmail = emailActualizado ? Email.Create(request.Email!) : cliente.Email;
                var nuevoTelefono = telefonoActualizado ? PhoneNumber.Create(request.Telefono!) : cliente.Telefono;
                
                cliente.ActualizarInformacionContacto(nuevoEmail, nuevoTelefono);
                
                _logger.LogInformation("📧 Información de contacto actualizada para cliente: {ClienteId}", request.Id);
            }

            // 4. Gestionar estado activo/inactivo
            if (request.EstaActivo.HasValue)
            {
                if (request.EstaActivo.Value && !cliente.EstaActivo)
                {
                    cliente.Reactivar();
                    _logger.LogInformation("✅ Cliente reactivado: {ClienteId}", request.Id);
                }
                else if (!request.EstaActivo.Value && cliente.EstaActivo)
                {
                    cliente.Desactivar();
                    _logger.LogInformation("🚫 Cliente desactivado: {ClienteId}", request.Id);
                }
            }

            // 5. Log para campos que no se pueden actualizar actualmente
            if (!string.IsNullOrWhiteSpace(request.Nombre))
            {
                _logger.LogWarning("⚠️ Actualización de nombre no implementada aún para cliente: {ClienteId}", request.Id);
            }

            if (request.FechaNacimiento.HasValue)
            {
                _logger.LogWarning("⚠️ Actualización de fecha de nacimiento no implementada aún para cliente: {ClienteId}", request.Id);
            }

            // 6. Guardar cambios
            await _repository.GuardarAsync(cliente, cancellationToken);

            // 7. Mapear a DTO y retornar
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            _logger.LogInformation("✅ Cliente actualizado exitosamente: {ClienteId}", request.Id);
            return Result.Success(clienteDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al actualizar cliente: {Message}", ex.Message);
            return Result.Failure<ClienteDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al actualizar cliente: {ClienteId}", request.Id);
            return Result.Failure<ClienteDto>("Error interno del servidor al actualizar el cliente");
        }
    }
} 