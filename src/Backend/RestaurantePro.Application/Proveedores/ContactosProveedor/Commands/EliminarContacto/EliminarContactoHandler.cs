namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;

/// <summary>
/// Handler para eliminar contactos de proveedores
/// Gestiona eliminación lógica/física, reasignación de contactos principales y validaciones de seguridad
/// </summary>
public class EliminarContactoHandler : IRequestHandler<EliminarContactoCommand, Result<bool>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly ILogger<EliminarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public EliminarContactoHandler(
        IContactoProveedorRepository contactoRepository,
        IProveedorRepository proveedorRepository,
        ILogger<EliminarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _contactoRepository = contactoRepository;
        _proveedorRepository = proveedorRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(EliminarContactoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando eliminación de contacto {ContactoId} del proveedor {ProveedorId}. Tipo: {TipoEliminacion}, Motivo: {Motivo}", 
            request.Id, request.ProveedorId, request.TipoEliminacion, request.MotivoEliminacion);

        try
        {
            // 1. Verificar que el contacto existe
            var contacto = await _contactoRepository.ObtenerPorIdAsync(request.Id);
            if (contacto == null)
            {
                _logger.LogWarning("Contacto no encontrado: {ContactoId}", request.Id);
                return Result<bool>.Failure("El contacto especificado no existe");
            }

            // 2. Verificar que pertenece al proveedor especificado
            if (contacto.ProveedorId != request.ProveedorId)
            {
                _logger.LogWarning("Intento de eliminar contacto {ContactoId} con proveedor incorrecto. Actual: {ProveedorActual}, Especificado: {ProveedorEspecificado}", 
                    request.Id, contacto.ProveedorId, request.ProveedorId);
                return Result<bool>.Failure("El contacto no pertenece al proveedor especificado");
            }

            // 3. Verificar que el proveedor existe
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return Result<bool>.Failure("El proveedor especificado no existe");
            }

            // 4. Validar reglas de negocio antes de eliminar
            var validacionEliminacion = await ValidarEliminacion(contacto, proveedor, request);
            if (!validacionEliminacion.Succeeded)
            {
                return Result<bool>.Failure(validacionEliminacion.ErrorMessage);
            }

            // 5. Gestionar reasignación de contacto principal si es necesario
            if (contacto.EsPrincipal)
            {
                var resultadoReasignacion = await GestionarReasignacionContactoPrincipal(contacto, request);
                if (!resultadoReasignacion.Succeeded)
                {
                    return Result<bool>.Failure(resultadoReasignacion.ErrorMessage);
                }
            }

            // 6. Ejecutar la eliminación según el tipo solicitado
            bool eliminacionExitosa = false;
            if (request.TipoEliminacion == TipoEliminacion.Logica)
            {
                eliminacionExitosa = await EjecutarEliminacionLogica(contacto, request);
            }
            else
            {
                eliminacionExitosa = await EjecutarEliminacionFisica(contacto, request);
            }

            if (!eliminacionExitosa)
            {
                return Result<bool>.Failure("Error al ejecutar la eliminación del contacto");
            }

            // 7. Actualizar estadísticas del proveedor
            await ActualizarEstadisticasProveedor(proveedor);

            // 8. Registrar evento de auditoría
            await RegistrarEventoAuditoria(contacto, request);

            var tipoEliminacionTexto = request.TipoEliminacion == TipoEliminacion.Logica ? "lógica" : "física";
            _logger.LogInformation("Eliminación {TipoEliminacion} de contacto {ContactoId} completada exitosamente", 
                tipoEliminacionTexto, request.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar contacto {ContactoId}", request.Id);
            return Result<bool>.Failure("Error interno del servidor al eliminar el contacto");
        }
    }

    private async Task<Result> ValidarEliminacion(ContactoProveedor contacto, Proveedor proveedor, EliminarContactoCommand request)
    {
        try
        {
            // Validar que no es el último contacto activo del proveedor
            var cantidadContactosActivos = await _contactoRepository.ContarContactosActivosAsync(proveedor.Id);
            if (cantidadContactosActivos <= 1 && !request.ForzarEliminacion)
            {
                return Result.Failure("No se puede eliminar el último contacto activo del proveedor. Use ForzarEliminacion si es necesario.");
            }

            // Validar dependencias críticas del contacto
            var tieneDependenciasCriticas = await _contactoRepository.TieneDependenciasCriticasAsync(contacto.Id);
            if (tieneDependenciasCriticas && !request.ForzarEliminacion)
            {
                return Result.Failure("El contacto tiene dependencias críticas (pedidos pendientes, autorizaciones, etc.). Use ForzarEliminacion para continuar.");
            }

            // Validar eliminación física solo para casos especiales
            if (request.TipoEliminacion == TipoEliminacion.Fisica)
            {
                if (!request.ForzarEliminacion)
                {
                    return Result.Failure("La eliminación física requiere confirmación forzada");
                }

                // Verificar que no hay referencias críticas en otras tablas
                var tieneReferenciasCriticas = await _contactoRepository.TieneReferenciasCriticasAsync(contacto.Id);
                if (tieneReferenciasCriticas)
                {
                    return Result.Failure("No se puede realizar eliminación física: el contacto tiene referencias críticas en el sistema");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando eliminación de contacto {ContactoId}", contacto.Id);
            return Result.Failure("Error en validación de eliminación");
        }
    }

    private async Task<Result> GestionarReasignacionContactoPrincipal(ContactoProveedor contactoPrincipal, EliminarContactoCommand request)
    {
        try
        {
            if (!request.ReasignarContactoPrincipal)
            {
                // Solo marcar como no principal, sin reasignar
                _logger.LogInformation("Marcando contacto principal {ContactoId} como no principal sin reasignación", contactoPrincipal.Id);
                return Result.Success();
            }

            ContactoProveedor? nuevoContactoPrincipal = null;

            if (request.NuevoContactoPrincipalId.HasValue)
            {
                // Usar el contacto especificado como nuevo principal
                nuevoContactoPrincipal = await _contactoRepository.ObtenerPorIdAsync(request.NuevoContactoPrincipalId.Value);
                if (nuevoContactoPrincipal == null || nuevoContactoPrincipal.ProveedorId != request.ProveedorId)
                {
                    return Result.Failure("El nuevo contacto principal especificado no es válido");
                }
            }
            else
            {
                // Buscar automáticamente el mejor candidato para contacto principal
                nuevoContactoPrincipal = await _contactoRepository.ObtenerMejorCandidatoContactoPrincipalAsync(request.ProveedorId, contactoPrincipal.Id);
                if (nuevoContactoPrincipal == null)
                {
                    return Result.Failure("No se encontró un contacto válido para reasignar como principal");
                }
            }

            // Marcar el nuevo contacto como principal
            var resultadoMarcar = nuevoContactoPrincipal.MarcarComoPrincipal(_currentUser.UserId ?? "Sistema");
            if (!resultadoMarcar.Succeeded)
            {
                return Result.Failure($"Error al marcar nuevo contacto principal: {resultadoMarcar.ErrorMessage}");
            }

            await _contactoRepository.ActualizarAsync(nuevoContactoPrincipal);
            _logger.LogInformation("Nuevo contacto principal asignado: {NuevoContactoId} para proveedor {ProveedorId}", 
                nuevoContactoPrincipal.Id, request.ProveedorId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reasignando contacto principal para proveedor {ProveedorId}", request.ProveedorId);
            return Result.Failure("Error en reasignación de contacto principal");
        }
    }

    private async Task<bool> EjecutarEliminacionLogica(ContactoProveedor contacto, EliminarContactoCommand request)
    {
        try
        {
            var resultadoEliminacion = contacto.EliminarLogicamente(
                request.MotivoEliminacion,
                _currentUser.UserId ?? "Sistema",
                request.DatosAdicionales
            );

            if (!resultadoEliminacion.Succeeded)
            {
                _logger.LogWarning("Error en eliminación lógica: {Error}", resultadoEliminacion.ErrorMessage);
                return false;
            }

            await _contactoRepository.ActualizarAsync(contacto);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando eliminación lógica de contacto {ContactoId}", contacto.Id);
            return false;
        }
    }

    private async Task<bool> EjecutarEliminacionFisica(ContactoProveedor contacto, EliminarContactoCommand request)
    {
        try
        {
            // Registrar información del contacto antes de eliminarlo físicamente
            _logger.LogWarning("Eliminación física de contacto {ContactoId}: {Nombre} {Apellidos} ({Email}) - Motivo: {Motivo}", 
                contacto.Id, contacto.Nombre, contacto.Apellidos, contacto.Email, request.MotivoEliminacion);

            await _contactoRepository.EliminarAsync(contacto.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando eliminación física de contacto {ContactoId}", contacto.Id);
            return false;
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

    private async Task RegistrarEventoAuditoria(ContactoProveedor contacto, EliminarContactoCommand request)
    {
        try
        {
            // Aquí se registraría el evento de auditoría en el sistema de eventos
            // Por ejemplo: await _eventBus.PublishAsync(new ContactoEliminadoEvent(...));
            
            _logger.LogInformation("Evento de auditoría: Contacto {ContactoId} eliminado. Usuario: {Usuario}, Motivo: {Motivo}", 
                contacto.Id, _currentUser.UserId, request.MotivoEliminacion);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando evento de auditoría para contacto {ContactoId}", contacto.Id);
            // No fallar la operación principal por errores en auditoría
        }
    }
} 