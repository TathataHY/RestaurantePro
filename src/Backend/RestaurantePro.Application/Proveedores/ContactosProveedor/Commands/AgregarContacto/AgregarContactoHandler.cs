namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;

/// <summary>
/// Handler para agregar contactos a proveedores
/// Gestiona validación de unicidad, contactos principales y relaciones
/// </summary>
public class AgregarContactoHandler : IRequestHandler<AgregarContactoCommand, Result<ContactoProveedorDto>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly ContactoProveedorBuilder _contactoBuilder;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public AgregarContactoHandler(
        IContactoProveedorRepository contactoRepository,
        IProveedorRepository proveedorRepository,
        ContactoProveedorBuilder contactoBuilder,
        IMapper mapper,
        ILogger<AgregarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _contactoRepository = contactoRepository;
        _proveedorRepository = proveedorRepository;
        _contactoBuilder = contactoBuilder;
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
            // 1. Verificar que el proveedor existe
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return Result<ContactoProveedorDto>.Failure("El proveedor especificado no existe");
            }

            // 2. Validar unicidad del email
            var contactoExistente = await _contactoRepository.BuscarPorEmailAsync(request.Email, request.ProveedorId);
            if (contactoExistente != null)
            {
                _logger.LogWarning("Ya existe un contacto con email {Email} para el proveedor {ProveedorId}", 
                    request.Email, request.ProveedorId);
                return Result<ContactoProveedorDto>.Failure("Ya existe un contacto con este email para el proveedor");
            }

            // 3. Validar contacto principal único
            if (request.EsPrincipal)
            {
                var contactoPrincipalExistente = await _contactoRepository.ObtenerContactoPrincipalAsync(request.ProveedorId);
                if (contactoPrincipalExistente != null)
                {
                    // Marcar el anterior como no principal
                    var resultadoDesmarcar = contactoPrincipalExistente.MarcarComoNoPrincipal(_currentUser.UserId ?? "Sistema");
                    if (!resultadoDesmarcar.Succeeded)
                    {
                        return Result<ContactoProveedorDto>.Failure($"Error al actualizar contacto principal anterior: {resultadoDesmarcar.ErrorMessage}");
                    }
                    
                    await _contactoRepository.ActualizarAsync(contactoPrincipalExistente);
                    _logger.LogInformation("Contacto principal anterior desmarcado: {ContactoId}", contactoPrincipalExistente.Id);
                }
            }

            // 4. Usar el builder del dominio para crear el contacto
            var resultadoBuilder = _contactoBuilder
                .DeProveedor(request.ProveedorId)
                .ConNombre(request.Nombre, request.Apellidos)
                .ConCargo(request.Cargo)
                .ConEmail(request.Email)
                .ConTelefono(request.Telefono)
                .ConDepartamento(request.Departamento)
                .ConEmailSecundario(request.EmailSecundario)
                .ConTelefonoMovil(request.TelefonoMovil)
                .ConExtension(request.Extension)
                .ConHorarioContacto(request.HorarioContacto)
                .ConNotas(request.Notas)
                .ComoPrincipal(request.EsPrincipal)
                .ConPermisosAutorizacion(request.PuedeAutorizarPedidos, request.LimiteAutorizacion)
                .ConNotificaciones(request.RecibeNotificaciones, request.TiposNotificaciones)
                .ConDatosAdicionales(request.DatosAdicionales)
                .CreadoPor(_currentUser.UserId ?? "Sistema")
                .Construir();

            if (!resultadoBuilder.Succeeded)
            {
                _logger.LogWarning("Error al construir contacto: {Error}", resultadoBuilder.ErrorMessage);
                return Result<ContactoProveedorDto>.Failure(resultadoBuilder.ErrorMessage);
            }

            var contacto = resultadoBuilder.Value;

            // 5. Validaciones adicionales de negocio
            var validacionNegocio = await ValidarReglasDeNegocio(contacto, proveedor);
            if (!validacionNegocio.Succeeded)
            {
                return Result<ContactoProveedorDto>.Failure(validacionNegocio.ErrorMessage);
            }

            // 6. Agregar el contacto al repositorio
            await _contactoRepository.AgregarAsync(contacto);

            // 7. Actualizar estadísticas del proveedor
            await ActualizarEstadisticasProveedor(proveedor);

            _logger.LogInformation("Contacto creado exitosamente: {ContactoId} para proveedor {ProveedorId}", 
                contacto.Id, request.ProveedorId);

            // 8. Mapear y retornar
            var contactoDto = _mapper.Map<ContactoProveedorDto>(contacto);
            return Result<ContactoProveedorDto>.Success(contactoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear contacto para proveedor {ProveedorId}", request.ProveedorId);
            return Result<ContactoProveedorDto>.Failure("Error interno del servidor al crear el contacto");
        }
    }

    private async Task<Result> ValidarReglasDeNegocio(ContactoProveedor contacto, Proveedor proveedor)
    {
        try
        {
            // Validar límites de contactos por proveedor
            var cantidadContactos = await _contactoRepository.ContarContactosActivosAsync(proveedor.Id);
            if (cantidadContactos >= 10) // Máximo 10 contactos por proveedor
            {
                return Result.Failure("El proveedor ha alcanzado el límite máximo de contactos (10)");
            }

            // Validar que el límite de autorización sea coherente con el tipo de proveedor
            if (contacto.PuedeAutorizarPedidos && contacto.LimiteAutorizacion.HasValue)
            {
                var limiteMaximoProveedor = proveedor.CalcularLimiteMaximoAutorizacion();
                if (contacto.LimiteAutorizacion > limiteMaximoProveedor)
                {
                    return Result.Failure($"El límite de autorización excede el máximo permitido para este proveedor: ${limiteMaximoProveedor:N2}");
                }
            }

            // Validar tipos de notificaciones permitidas según el perfil del proveedor
            if (contacto.TiposNotificaciones.Any())
            {
                var tiposPermitidos = proveedor.ObtenerTiposNotificacionesPermitidas();
                var tiposNoPermitidos = contacto.TiposNotificaciones.Except(tiposPermitidos, StringComparer.OrdinalIgnoreCase);
                
                if (tiposNoPermitidos.Any())
                {
                    return Result.Failure($"Tipos de notificaciones no permitidos para este proveedor: {string.Join(", ", tiposNoPermitidos)}");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando reglas de negocio para contacto");
            return Result.Failure("Error en validación de reglas de negocio");
        }
    }

    private async Task ActualizarEstadisticasProveedor(Proveedor proveedor)
    {
        try
        {
            var resultadoActualizacion = proveedor.ActualizarCantidadContactos(_currentUser.UserId ?? "Sistema");
            if (resultadoActualizacion.Succeeded)
            {
                await _proveedorRepository.ActualizarAsync(proveedor);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error actualizando estadísticas del proveedor {ProveedorId}", proveedor.Id);
            // No fallar la operación principal por errores en estadísticas
        }
    }
} 