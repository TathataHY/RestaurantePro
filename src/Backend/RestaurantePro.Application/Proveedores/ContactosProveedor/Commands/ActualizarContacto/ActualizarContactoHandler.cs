using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;

/// <summary>
/// Handler para actualizar contactos de proveedores
/// Gestiona validación de cambios, integridad de datos y auditoría
/// </summary>
public class ActualizarContactoHandler : IRequestHandler<ActualizarContactoCommand, Result<ContactoProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ActualizarContactoHandler(
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<ActualizarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<ContactoProveedorDto>> Handle(ActualizarContactoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando actualización de contacto {ContactoId} para proveedor {ProveedorId}: {Nombre} {Apellidos}", 
            request.Id, request.ProveedorId, request.Nombre, request.Apellidos);

        try
        {
            // 1. Verificar que el proveedor existe y obtener con contactos
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId, incluirContactos: true, cancellationToken: cancellationToken);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("El proveedor especificado no existe");
            }

            // 2. Buscar el contacto específico
            var contactoExistente = proveedor.Contactos.FirstOrDefault(c => c.Id == request.Id);
            if (contactoExistente == null)
            {
                _logger.LogWarning("Contacto no encontrado: {ContactoId}", request.Id);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("El contacto especificado no existe");
            }

            // 3. Verificar que pertenece al proveedor especificado
            if (contactoExistente.ProveedorId != request.ProveedorId)
            {
                _logger.LogWarning("Intento de actualizar contacto {ContactoId} con proveedor incorrecto. Actual: {ProveedorActual}, Especificado: {ProveedorEspecificado}", 
                    request.Id, contactoExistente.ProveedorId, request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("El contacto no pertenece al proveedor especificado");
            }

            // 4. Validar unicidad del email (si cambió)
            if (!contactoExistente.Email.Value.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var contactoConEmail = proveedor.Contactos.FirstOrDefault(c => 
                    c.Id != request.Id && 
                    c.Email.Value.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
                
                if (contactoConEmail != null)
                {
                    _logger.LogWarning("Conflicto de email: {Email} ya está en uso por el contacto {ContactoId}", 
                        request.Email, contactoConEmail.Id);
                    return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("Ya existe otro contacto con este email para el proveedor");
                }
            }

            // 5. Validaciones adicionales de negocio
            var validacionNegocio = ValidarReglasDeNegocio(request);
            if (!validacionNegocio.Succeeded)
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>(validacionNegocio.Error ?? "Error en validación de reglas de negocio");
            }

            // 6. Crear el nombre completo
            var nombreCompleto = !string.IsNullOrWhiteSpace(request.Apellidos) 
                ? $"{request.Nombre} {request.Apellidos}" 
                : request.Nombre;

            // 7. Actualizar el contacto usando el método del dominio
            contactoExistente.ActualizarInformacion(
                nombreCompleto,
                request.Cargo ?? "No especificado",
                request.Telefono ?? string.Empty,
                request.Email);

            // 8. Guardar cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Contacto actualizado exitosamente: {ContactoId} para proveedor {ProveedorId}", 
                contactoExistente.Id, request.ProveedorId);

            // 9. Mapear y retornar
            var contactoDto = _mapper.Map<ContactoProveedorDto>(contactoExistente);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success(contactoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar contacto {ContactoId}", request.Id);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("Error interno del servidor al actualizar el contacto");
        }
    }

    private Result ValidarReglasDeNegocio(ActualizarContactoCommand request)
    {
        try
        {
            // Validar datos mínimos requeridos
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return Result.Failure("El nombre del contacto es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Result.Failure("El email del contacto es obligatorio");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando reglas de negocio para contacto");
            return Result.Failure("Error en validación de reglas de negocio");
        }
    }
} 