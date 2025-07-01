using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;

/// <summary>
/// Handler para consumir stock de un ingrediente
/// </summary>
public class ConsumirStockCommandHandler : IRequestHandler<ConsumirStockCommand, Result<IngredienteDto>>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ConsumirStockCommandHandler> _logger;

    public ConsumirStockCommandHandler(
        IIngredienteRepository ingredienteRepository,
        IDateTimeService dateTimeService,
        ILogger<ConsumirStockCommandHandler> logger)
    {
        _ingredienteRepository = ingredienteRepository;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<IngredienteDto>> Handle(ConsumirStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Consumiendo stock del ingrediente: {IngredienteId}, cantidad: {Cantidad}", 
                request.IngredienteId, request.Cantidad);

            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId, cancellationToken);
            if (ingrediente == null)
            {
                return Result.Failure<IngredienteDto>($"No se encontró el ingrediente con ID {request.IngredienteId}");
            }

            // Verificar que hay suficiente stock
            if (ingrediente.Stock < request.Cantidad)
            {
                return Result.Failure<IngredienteDto>($"Stock insuficiente. Disponible: {ingrediente.Stock}, Requerido: {request.Cantidad}");
            }

            // Decrementar el stock
            ingrediente.DecrementarStock(
                request.Cantidad,
                request.Motivo);

            // Actualizar el ingrediente
            await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
            await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);

            // Mapear a DTO
            var ingredienteDto = new IngredienteDto
            {
                Id = ingrediente.Id,
                Nombre = ingrediente.Nombre,
                Descripcion = ingrediente.Descripcion,
                Rotacion = ingrediente.Rotacion,
                UnidadMedida = ingrediente.UnidadMedida,
                UnidadMedidaTexto = ingrediente.UnidadMedida.ToString(),
                StockActual = ingrediente.Stock,
                StockMinimo = ingrediente.StockMinimo,
                StockMaximo = ingrediente.StockMinimo * 3, // Calculado como 3x el mínimo
                CostoUnitario = ingrediente.CostoPromedio,
                CostoPromedio = ingrediente.CostoPromedio,
                ProveedorPrincipalId = ingrediente.ProveedorPrincipalId,
                Activo = ingrediente.EstaActivo,
                RequiereRefrigeracion = false, // Por defecto
                DiasVencimiento = 30, // Por defecto
                EstadoStock = ingrediente.Stock <= ingrediente.StockMinimo * 0.5m ? "Crítico" : 
                              ingrediente.Stock <= ingrediente.StockMinimo ? "Bajo" : "Normal",
                ColorEstado = ingrediente.Stock <= ingrediente.StockMinimo * 0.5m ? "red" : 
                             ingrediente.Stock <= ingrediente.StockMinimo ? "orange" : "green",
                EstaBajoMinimo = ingrediente.Stock <= ingrediente.StockMinimo,
                FechaCreacion = ingrediente.FechaCreacion,
                FechaModificacion = ingrediente.FechaActualizacion
            };

            _logger.LogInformation("Stock consumido exitosamente. Stock restante: {StockActual}", ingrediente.Stock);

            return Result.Success(ingredienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consumir stock del ingrediente {IngredienteId}", request.IngredienteId);
            return Result.Failure<IngredienteDto>($"Error al consumir stock: {ex.Message}");
        }
    }
} 