using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Validation;

namespace RestaurantePro.Domain.Core.Productos.Services;

/// <summary>
/// Implementación del servicio para calculos de recetas.
/// </summary>
public class CalculoRecetaService : ICalculoRecetaService
{
    private readonly IProductoRepository _productoRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly INotificationManager _notificationManager;

    public CalculoRecetaService(
        IProductoRepository productoRepository,
        IIngredienteRepository ingredienteRepository,
        INotificationManager notificationManager)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
    }

    public async Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
        if (producto == null)
        {
            _notificationManager.AddError("El producto no existe.");
            return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
        }

        var receta = producto.Recetas.FirstOrDefault();
        if (receta == null)
        {
            _notificationManager.AddError("El producto no tiene una receta asignada.");
            return _notificationManager.ToResult<Dictionary<Guid, decimal>>(null);
        }

        var ingredientes = receta.Ingredientes.ToDictionary(i => i.IngredienteId, i => i.Cantidad);
        return Result.Success(ingredientes);
    }

    public async Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
    {
        _notificationManager.CreateNewNotification();

        var ingredientesResult = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
        if (ingredientesResult.IsFailure())
        {
            _notificationManager.AddError(string.Join(", ", ingredientesResult.Errors));
            return _notificationManager.ToResult<bool>(false);
        }

        foreach (var (ingredienteId, cantidadRequerida) in ingredientesResult.Value)
        {
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
            if (ingrediente == null || ingrediente.Stock < (cantidadRequerida * cantidad))
            {
                _notificationManager.AddError($"No hay suficiente stock para el ingrediente {ingrediente?.Nombre ?? ingredienteId.ToString()}.");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        return Result.Success(true);
    }

    public async Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken)
    {
        _notificationManager.CreateNewNotification();

        var ingredientesResult = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
        if (ingredientesResult.IsFailure())
        {
            _notificationManager.AddError(string.Join(", ", ingredientesResult.Errors));
            return _notificationManager.ToResult<decimal>(0);
        }

        decimal costoTotal = 0;
        foreach (var (ingredienteId, cantidad) in ingredientesResult.Value)
        {
            var ingredienteInfo = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
            if (ingredienteInfo == null)
            {
                _notificationManager.AddError($"No se encontró información para el ingrediente {ingredienteId}.");
                return _notificationManager.ToResult<decimal>(0);
            }
            costoTotal += ingredienteInfo.CostoPromedio * cantidad;
        }

        return Result.Success(costoTotal);
    }
}