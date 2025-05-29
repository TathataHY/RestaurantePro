namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;

public class DesactivarProveedorHandler : IRequestHandler<DesactivarProveedorCommand, Result<bool>>
{
    private readonly IProveedorRepository _repository;
    private readonly ILogger<DesactivarProveedorHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DesactivarProveedorHandler(
        IProveedorRepository repository,
        ILogger<DesactivarProveedorHandler> logger,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        DesactivarProveedorCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando desactivación de proveedor: {ProveedorId}", request.Id);

        try
        {
            // Obtener proveedor existente
            var proveedor = await _repository.ObtenerPorIdAsync(request.Id);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado para desactivar: {ProveedorId}", request.Id);
                return Result<bool>.Failure($"El proveedor con ID {request.Id} no fue encontrado");
            }

            // Verificar si ya está desactivado
            if (!proveedor.Activo)
            {
                _logger.LogInformation("Proveedor ya está desactivado: {ProveedorId}", request.Id);
                return Result<bool>.Success(true);
            }

            // Verificar dependencias críticas (órdenes activas, contratos, etc.)
            var tieneOrdenesActivas = await _repository.TieneOrdenesActivasAsync(request.Id);
            if (tieneOrdenesActivas)
            {
                _logger.LogWarning("No se puede desactivar proveedor con órdenes activas: {ProveedorId}", request.Id);
                return Result<bool>.Failure("No se puede desactivar el proveedor porque tiene órdenes de compra activas");
            }

            // Desactivar usando método del dominio
            var resultadoDesactivacion = proveedor.Desactivar(
                request.RazonDesactivacion,
                _currentUserService.UserId ?? "Sistema"
            );

            if (!resultadoDesactivacion.Succeeded)
            {
                _logger.LogWarning("Error al desactivar proveedor: {Error}", resultadoDesactivacion.ErrorMessage);
                return Result<bool>.Failure(resultadoDesactivacion.ErrorMessage);
            }

            // Guardar cambios
            await _repository.ActualizarAsync(proveedor);

            _logger.LogInformation("Proveedor desactivado exitosamente: {ProveedorId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar proveedor: {ProveedorId}", request.Id);
            return Result<bool>.Failure("Error interno del servidor al desactivar el proveedor");
        }
    }
} 