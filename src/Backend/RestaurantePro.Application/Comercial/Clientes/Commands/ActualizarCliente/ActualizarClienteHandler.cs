using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;

/// <summary>
/// Handler para actualizar datos de un cliente existente
/// Implementa actualización parcial y validación de duplicados
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
        _logger.LogInformation("🔄 Iniciando actualización de cliente: {ClienteId}", request.ClienteId);

        try
        {
            // 1. Buscar el cliente existente
            var cliente = await _repository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado para actualizar: {ClienteId}", request.ClienteId);
                return Result.Failure<ClienteDto>($"No se encontró un cliente con el ID {request.ClienteId}");
            }

            if (cliente.EstaEliminado)
            {
                _logger.LogWarning("🗑️ Intento de actualizar cliente eliminado: {ClienteId}", request.ClienteId);
                return Result.Failure<ClienteDto>("No se puede actualizar un cliente eliminado");
            }

            // 2. Verificar duplicado de email si se va a actualizar
            if (!string.IsNullOrWhiteSpace(request.Email) && 
                !string.Equals(cliente.Email.Value, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var clienteConEmail = await _repository.ObtenerPorEmailAsync(request.Email, cancellationToken);
                if (clienteConEmail != null && clienteConEmail.Id != request.ClienteId)
                {
                    _logger.LogWarning("⚠️ Email duplicado al actualizar cliente: {Email}", request.Email);
                    return Result.Failure<ClienteDto>($"Ya existe otro cliente con el email {request.Email}");
                }
            }

            // 3. Aplicar actualizaciones usando métodos del dominio
            var actualizado = false;

            // Actualizar información de contacto si se proporciona email o teléfono
            var emailActualizado = !string.IsNullOrWhiteSpace(request.Email);
            var telefonoActualizado = !string.IsNullOrWhiteSpace(request.Telefono);
            
            if (emailActualizado || telefonoActualizado)
            {
                var nuevoEmail = emailActualizado ? Email.Create(request.Email!) : cliente.Email;
                var nuevoTelefono = telefonoActualizado ? PhoneNumber.Create(request.Telefono!) : cliente.Telefono;
                
                cliente.ActualizarInformacionContacto(nuevoEmail, nuevoTelefono);
                actualizado = true;
                
                if (emailActualizado)
                    _logger.LogInformation("📧 Email actualizado: {NuevoEmail}", request.Email);
                if (telefonoActualizado)
                    _logger.LogInformation("📱 Teléfono actualizado: {NuevoTelefono}", request.Telefono);
            }

            // Actualizar nombre y fecha de nacimiento requieren recrear el cliente
            // Por ahora mostramos mensaje informativo ya que la entidad no tiene métodos para esto
            if (!string.IsNullOrWhiteSpace(request.Nombre))
            {
                _logger.LogWarning("⚠️ Actualización de nombre requiere métodos adicionales en la entidad Cliente");
                // TODO: Implementar método ActualizarNombre en la entidad Cliente
            }

            if (request.FechaNacimiento.HasValue)
            {
                _logger.LogWarning("⚠️ Actualización de fecha de nacimiento requiere métodos adicionales en la entidad Cliente");
                // TODO: Implementar método ActualizarFechaNacimiento en la entidad Cliente
            }

            if (!actualizado)
            {
                _logger.LogWarning("⚠️ No se proporcionó ningún campo para actualizar: {ClienteId}", request.ClienteId);
                return Result.Failure<ClienteDto>("No se proporcionó ningún campo para actualizar");
            }

            // 4. Persistir los cambios
            await _repository.GuardarAsync(cliente, cancellationToken);

            // 5. Mapear a DTO y retornar
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            _logger.LogInformation("✅ Cliente actualizado exitosamente: {ClienteId}", request.ClienteId);
            return Result.Success(clienteDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al actualizar cliente: {Message}", ex.Message);
            return Result.Failure<ClienteDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al actualizar cliente: {ClienteId}", request.ClienteId);
            return Result.Failure<ClienteDto>("Error interno del servidor al actualizar el cliente");
        }
    }
} 