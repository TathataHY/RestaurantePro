namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;

/// <summary>
/// Handler para actualizar contactos de proveedores
/// Gestiona validación de cambios, integridad de datos y auditoría
/// </summary>
public class ActualizarContactoHandler : IRequestHandler<ActualizarContactoCommand, Result<ContactoProveedorDto>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ActualizarContactoHandler(
        IContactoProveedorRepository contactoRepository,
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<ActualizarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _contactoRepository = contactoRepository;
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
            // 1. Verificar que el contacto existe
            var contactoExistente = await _contactoRepository.ObtenerPorIdAsync(request.Id);
            if (contactoExistente == null)
            {
                _logger.LogWarning("Contacto no encontrado: {ContactoId}", request.Id);
                return Result<ContactoProveedorDto>.Failure("El contacto especificado no existe");
            }

            // 2. Verificar que pertenece al proveedor especificado
            if (contactoExistente.ProveedorId != request.ProveedorId)
            {
                _logger.LogWarning("Intento de actualizar contacto {ContactoId} con proveedor incorrecto. Actual: {ProveedorActual}, Especificado: {ProveedorEspecificado}", 
                    request.Id, contactoExistente.ProveedorId, request.ProveedorId);
                return Result<ContactoProveedorDto>.Failure("El contacto no pertenece al proveedor especificado");
            }

            // 3. Verificar que el proveedor existe
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return Result<ContactoProveedorDto>.Failure("El proveedor especificado no existe");
            }

            // 4. Detectar conflictos de email
            var conflictoEmail = await DetectarConflictoEmail(request);
            if (!conflictoEmail.Succeeded)
            {
                return Result<ContactoProveedorDto>.Failure(conflictoEmail.ErrorMessage);
            }

            // 5. Gestionar cambios en contacto principal
            var resultadoPrincipal = await GestionarCambioContactoPrincipal(request, contactoExistente);
            if (!resultadoPrincipal.Succeeded)
            {
                return Result<ContactoProveedorDto>.Failure(resultadoPrincipal.ErrorMessage);
            }

            // 6. Detectar cambios significativos para auditoría
            var cambiosDetectados = DetectarCambiosSignificativos(contactoExistente, request);

            // 7. Actualizar el contacto con los nuevos datos
            var resultadoActualizacion = contactoExistente.Actualizar(
                request.Nombre,
                request.Apellidos,
                request.Cargo,
                request.Departamento,
                request.Email,
                request.EmailSecundario,
                request.Telefono,
                request.TelefonoMovil,
                request.Extension,
                request.HorarioContacto,
                request.Notas,
                request.EsPrincipal,
                request.PuedeAutorizarPedidos,
                request.LimiteAutorizacion,
                request.RecibeNotificaciones,
                request.TiposNotificaciones,
                request.Activo,
                request.DatosAdicionales,
                _currentUser.UserId ?? "Sistema",
                request.MotivoActualizacion,
                cambiosDetectados
            );

            if (!resultadoActualizacion.Succeeded)
            {
                _logger.LogWarning("Error al actualizar contacto: {Error}", resultadoActualizacion.ErrorMessage);
                return Result<ContactoProveedorDto>.Failure(resultadoActualizacion.ErrorMessage);
            }

            // 8. Validaciones adicionales de negocio
            var validacionNegocio = await ValidarReglasDeNegocio(contactoExistente, proveedor);
            if (!validacionNegocio.Succeeded)
            {
                return Result<ContactoProveedorDto>.Failure(validacionNegocio.ErrorMessage);
            }

            // 9. Guardar cambios
            await _contactoRepository.ActualizarAsync(contactoExistente);

            // 10. Actualizar estadísticas del proveedor si es necesario
            if (cambiosDetectados.Any(c => c.Campo == "Activo" || c.Campo == "EsPrincipal"))
            {
                await ActualizarEstadisticasProveedor(proveedor);
            }

            _logger.LogInformation("Contacto actualizado exitosamente: {ContactoId} para proveedor {ProveedorId}. Cambios: {CantidadCambios}", 
                contactoExistente.Id, request.ProveedorId, cambiosDetectados.Count);

            // 11. Mapear y retornar
            var contactoDto = _mapper.Map<ContactoProveedorDto>(contactoExistente);
            return Result<ContactoProveedorDto>.Success(contactoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar contacto {ContactoId}", request.Id);
            return Result<ContactoProveedorDto>.Failure("Error interno del servidor al actualizar el contacto");
        }
    }

    private async Task<Result> DetectarConflictoEmail(ActualizarContactoCommand request)
    {
        var contactoConEmail = await _contactoRepository.BuscarPorEmailAsync(request.Email, request.ProveedorId);
        
        if (contactoConEmail != null && contactoConEmail.Id != request.Id)
        {
            _logger.LogWarning("Conflicto de email: {Email} ya está en uso por el contacto {ContactoId}", 
                request.Email, contactoConEmail.Id);
            return Result.Failure("Ya existe otro contacto con este email para el proveedor");
        }

        return Result.Success();
    }

    private async Task<Result> GestionarCambioContactoPrincipal(ActualizarContactoCommand request, ContactoProveedor contactoActual)
    {
        // Si se está marcando como principal y no lo era antes
        if (request.EsPrincipal && !contactoActual.EsPrincipal)
        {
            var contactoPrincipalActual = await _contactoRepository.ObtenerContactoPrincipalAsync(request.ProveedorId);
            if (contactoPrincipalActual != null)
            {
                var resultadoDesmarcar = contactoPrincipalActual.MarcarComoNoPrincipal(_currentUser.UserId ?? "Sistema");
                if (!resultadoDesmarcar.Succeeded)
                {
                    return Result.Failure($"Error al desmarcar contacto principal anterior: {resultadoDesmarcar.ErrorMessage}");
                }
                
                await _contactoRepository.ActualizarAsync(contactoPrincipalActual);
                _logger.LogInformation("Contacto principal anterior desmarcado: {ContactoId}", contactoPrincipalActual.Id);
            }
        }
        // Si se está desmarcando como principal, validar que haya al menos otro contacto activo
        else if (!request.EsPrincipal && contactoActual.EsPrincipal)
        {
            var cantidadContactosActivos = await _contactoRepository.ContarContactosActivosAsync(request.ProveedorId);
            if (cantidadContactosActivos <= 1)
            {
                return Result.Failure("No se puede desmarcar el contacto principal si es el único contacto activo del proveedor");
            }
        }

        return Result.Success();
    }

    private static List<CambioAuditoria> DetectarCambiosSignificativos(ContactoProveedor contactoActual, ActualizarContactoCommand request)
    {
        var cambios = new List<CambioAuditoria>();

        if (contactoActual.Nombre != request.Nombre)
            cambios.Add(new CambioAuditoria("Nombre", contactoActual.Nombre, request.Nombre));
            
        if (contactoActual.Apellidos != request.Apellidos)
            cambios.Add(new CambioAuditoria("Apellidos", contactoActual.Apellidos, request.Apellidos));
            
        if (contactoActual.Email != request.Email)
            cambios.Add(new CambioAuditoria("Email", contactoActual.Email, request.Email));
            
        if (contactoActual.Telefono != request.Telefono)
            cambios.Add(new CambioAuditoria("Telefono", contactoActual.Telefono, request.Telefono));
            
        if (contactoActual.EsPrincipal != request.EsPrincipal)
            cambios.Add(new CambioAuditoria("EsPrincipal", contactoActual.EsPrincipal.ToString(), request.EsPrincipal.ToString()));
            
        if (contactoActual.PuedeAutorizarPedidos != request.PuedeAutorizarPedidos)
            cambios.Add(new CambioAuditoria("PuedeAutorizarPedidos", contactoActual.PuedeAutorizarPedidos.ToString(), request.PuedeAutorizarPedidos.ToString()));
            
        if (contactoActual.LimiteAutorizacion != request.LimiteAutorizacion)
            cambios.Add(new CambioAuditoria("LimiteAutorizacion", contactoActual.LimiteAutorizacion?.ToString() ?? "null", request.LimiteAutorizacion?.ToString() ?? "null"));
            
        if (contactoActual.Activo != request.Activo)
            cambios.Add(new CambioAuditoria("Activo", contactoActual.Activo.ToString(), request.Activo.ToString()));

        return cambios;
    }

    private async Task<Result> ValidarReglasDeNegocio(ContactoProveedor contacto, Proveedor proveedor)
    {
        try
        {
            // Validar límite de autorización según el tipo de proveedor
            if (contacto.PuedeAutorizarPedidos && contacto.LimiteAutorizacion.HasValue)
            {
                var limiteMaximoProveedor = proveedor.CalcularLimiteMaximoAutorizacion();
                if (contacto.LimiteAutorizacion > limiteMaximoProveedor)
                {
                    return Result.Failure($"El límite de autorización excede el máximo permitido para este proveedor: ${limiteMaximoProveedor:N2}");
                }
            }

            // Validar tipos de notificaciones permitidas
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
            _logger.LogError(ex, "Error validando reglas de negocio para contacto {ContactoId}", contacto.Id);
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