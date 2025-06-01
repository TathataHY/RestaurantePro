namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;

/// <summary>
/// Handler para ajustar inventario de ingredientes
/// </summary>
public class AjustarInventarioHandler : IRequestHandler<AjustarInventarioCommand, RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;
    private readonly IValidacionInventarioService _validacionService;
    private readonly ILogger<AjustarInventarioHandler> _logger;

    public AjustarInventarioHandler(
        IIngredienteRepository ingredienteRepository,
        IMovimientoInventarioRepository movimientoRepository,
        IValidacionInventarioService validacionService,
        ILogger<AjustarInventarioHandler> logger)
    {
        _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
        _validacionService = validacionService ?? throw new ArgumentNullException(nameof(validacionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maneja el comando de ajuste de inventario
    /// </summary>
    /// <param name="request">Comando con los datos del ajuste</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del ajuste</returns>
    public async Task<RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>> Handle(AjustarInventarioCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando ajuste de inventario para ingrediente {IngredienteId}", request.IngredienteId);

            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId);
            if (ingrediente == null)
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>.Failure($"Ingrediente con ID {request.IngredienteId} no encontrado");
            }

            // Validar el ajuste
            var resultadoValidacion = await _validacionService.ValidarAjusteInventarioAsync(
                request.IngredienteId,
                request.TipoMovimiento,
                request.Cantidad,
                ingrediente.Stock);

            if (!resultadoValidacion.EsValido)
            {
                var errores = string.Join(", ", resultadoValidacion.Errores);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>.Failure($"Validación falló: {errores}");
            }

            // Aplicar el ajuste usando los métodos de dominio
            MovimientoInventario movimiento;
            if (request.TipoMovimiento == TipoMovimientoInventario.Ingreso || 
               request.TipoMovimiento == TipoMovimientoInventario.Incremento)
            {
                movimiento = ingrediente.IncrementarStock(request.Cantidad, request.Motivo ?? "Ajuste manual");
            }
            else
            {
                movimiento = ingrediente.DecrementarStock(request.Cantidad, request.Motivo ?? "Ajuste manual");
            }

            // Actualizar el ingrediente
            await _ingredienteRepository.ActualizarAsync(ingrediente);

            _logger.LogInformation("Ajuste de inventario completado. Movimiento ID: {MovimientoId}, Nuevo stock: {NuevoStock}", 
                movimiento.Id, ingrediente.Stock);
            
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ajustar inventario del ingrediente {IngredienteId}", request.IngredienteId);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>.Failure($"Error interno: {ex.Message}");
        }
    }
} 