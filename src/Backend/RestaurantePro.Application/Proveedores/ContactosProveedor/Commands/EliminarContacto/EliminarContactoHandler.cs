using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;

/// <summary>
/// Handler para eliminar contactos de proveedores
/// Gestiona validaciones de integridad y restricciones de negocio
/// </summary>
public class EliminarContactoHandler : IRequestHandler<EliminarContactoCommand, Result<bool>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly ILogger<EliminarContactoHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public EliminarContactoHandler(
        IProveedorRepository proveedorRepository,
        ILogger<EliminarContactoHandler> logger,
        ICurrentUserService currentUser)
    {
        _proveedorRepository = proveedorRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(EliminarContactoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando eliminación de contacto {ContactoId} para proveedor {ProveedorId}", 
            request.Id, request.ProveedorId);

        try
        {
            // 1. Verificar que el proveedor existe y obtener con contactos
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId, incluirContactos: true, cancellationToken: cancellationToken);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>("El proveedor especificado no existe");
            }

            // 2. Buscar el contacto específico
            var contactoExistente = proveedor.Contactos.FirstOrDefault(c => c.Id == request.Id);
            if (contactoExistente == null)
            {
                _logger.LogWarning("Contacto no encontrado: {ContactoId}", request.Id);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>("El contacto especificado no existe");
            }

            // 3. Verificar que pertenece al proveedor especificado
            if (contactoExistente.ProveedorId != request.ProveedorId)
            {
                _logger.LogWarning("Intento de eliminar contacto {ContactoId} con proveedor incorrecto. Actual: {ProveedorActual}, Especificado: {ProveedorEspecificado}", 
                    request.Id, contactoExistente.ProveedorId, request.ProveedorId);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>("El contacto no pertenece al proveedor especificado");
            }

            // 4. Validaciones de negocio
            var validacionNegocio = ValidarEliminacion(proveedor, contactoExistente);
            if (!validacionNegocio.Succeeded)
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>(validacionNegocio.Error ?? "Error en validación de eliminación");
            }

            // 5. Eliminar el contacto usando el método del dominio
            proveedor.EliminarContacto(request.Id);

            // 6. Guardar cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Contacto eliminado exitosamente: {ContactoId} para proveedor {ProveedorId}", 
                request.Id, request.ProveedorId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar contacto {ContactoId}", request.Id);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>("Error interno del servidor al eliminar el contacto");
        }
    }

    private Result ValidarEliminacion(Proveedor proveedor, ContactoProveedor contacto)
    {
        try
        {
            // Validar que no sea el único contacto del proveedor
            if (proveedor.Contactos.Count <= 1)
            {
                return Result.Failure("No se puede eliminar el único contacto del proveedor");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando eliminación de contacto");
            return Result.Failure("Error en validación de eliminación");
        }
    }
} 