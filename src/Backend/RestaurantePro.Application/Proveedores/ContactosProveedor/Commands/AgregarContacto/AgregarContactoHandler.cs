namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;

/// <summary>
/// Handler para agregar contactos a proveedores
/// Gestiona validación de unicidad, contactos principales y relaciones
/// </summary>
public class AgregarContactoHandler : IRequestHandler<AgregarContactoCommand, Result<ContactoProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public AgregarContactoHandler(
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<AgregarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<ContactoProveedorDto>> Handle(AgregarContactoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de contacto para proveedor {ProveedorId}: {Nombre} {Apellidos}", 
            request.ProveedorId, request.Nombre, request.Apellidos);

        try
        {
            // 1. Verificar que el proveedor existe y obtener con contactos
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId, incluirContactos: true, cancellationToken: cancellationToken);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("El proveedor especificado no existe");
            }

            // 2. Validar unicidad del email
            var contactoExistente = proveedor.Contactos.FirstOrDefault(c => c.Email.Value.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (contactoExistente != null)
            {
                _logger.LogWarning("Ya existe un contacto con email {Email} para el proveedor {ProveedorId}", 
                    request.Email, request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("Ya existe un contacto con este email para el proveedor");
            }

            // 3. Validaciones adicionales de negocio
            var validacionNegocio = ValidarReglasDeNegocio(proveedor, request);
            if (!validacionNegocio.Succeeded)
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>(validacionNegocio.Error ?? "Error en validación de reglas de negocio");
            }

            // 4. Crear el nombre completo
            var nombreCompleto = !string.IsNullOrWhiteSpace(request.Apellidos) 
                ? $"{request.Nombre} {request.Apellidos}" 
                : request.Nombre;

            // 5. Usar el método del dominio para agregar el contacto
            var contacto = proveedor.AgregarContacto(
                nombreCompleto,
                request.Cargo ?? "No especificado",
                request.Telefono ?? string.Empty,
                request.Email,
                request.EsPrincipal,
                request.Notas);

            // 6. Guardar cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Contacto creado exitosamente: {ContactoId} para proveedor {ProveedorId}", 
                contacto.Id, request.ProveedorId);

            // 7. Mapear y retornar
            var contactoDto = _mapper.Map<ContactoProveedorDto>(contacto);
            return Result<ContactoProveedorDto>.Success(contactoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear contacto para proveedor {ProveedorId}", request.ProveedorId);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ContactoProveedorDto>("Error interno del servidor al crear el contacto");
        }
    }

    private Result ValidarReglasDeNegocio(Proveedor proveedor, AgregarContactoCommand request)
    {
        try
        {
            // Validar límites de contactos por proveedor (máximo 10)
            if (proveedor.Contactos.Count >= 10)
            {
                return Result.Failure("El proveedor ha alcanzado el límite máximo de contactos (10)");
            }

            // Validar que solo haya un contacto principal
            if (request.EsPrincipal && proveedor.Contactos.Any())
            {
                _logger.LogInformation("Se establecerá nuevo contacto principal para proveedor {ProveedorId}", proveedor.Id);
            }

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