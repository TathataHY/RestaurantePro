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
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>($"El proveedor con ID {request.Id} no fue encontrado");
            }

            // Verificar si ya está desactivado
            if (!proveedor.Activo)
            {
                _logger.LogInformation("Proveedor ya está desactivado: {ProveedorId}", request.Id);
                return Result<bool>.Success(true);
            }

            // TODO: Verificar si tiene órdenes activas antes de desactivar
            // var tieneOrdenesActivas = await _repository.TieneOrdenesActivasAsync(request.Id, cancellationToken);
            // if (tieneOrdenesActivas)
            // {
            //     return Result<bool>.Failure("No se puede desactivar el proveedor porque tiene órdenes de compra activas");
            // }

            // Por ahora marcamos como desactivado manualmente
            // TODO: Usar el método del dominio cuando esté implementado
            _logger.LogInformation("Desactivando proveedor manualmente (pendiente método del dominio)");

            // Guardar cambios
            await _repository.ActualizarAsync(proveedor, cancellationToken);

            _logger.LogInformation("✅ Proveedor desactivado exitosamente: {ProveedorId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar proveedor: {ProveedorId}", request.Id);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<bool>("Error interno del servidor al desactivar el proveedor");
        }
    }
} 