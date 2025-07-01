using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;

/// <summary>
/// Handler para registrar un lote con fecha de vencimiento
/// </summary>
public class RegistrarLoteCommandHandler : IRequestHandler<RegistrarLoteCommand, Result<IngredienteDto>>
{
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<RegistrarLoteCommandHandler> _logger;

    public RegistrarLoteCommandHandler(
        IIngredienteRepository ingredienteRepository,
        IDateTimeService dateTimeService,
        ILogger<RegistrarLoteCommandHandler> logger)
    {
        _ingredienteRepository = ingredienteRepository;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<IngredienteDto>> Handle(RegistrarLoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registrando lote {NumeroLote} para ingrediente {IngredienteId}", 
                request.NumeroLote, request.IngredienteId);

            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(request.IngredienteId, cancellationToken);
            if (ingrediente == null)
            {
                return Result.Failure<IngredienteDto>($"No se encontró el ingrediente con ID {request.IngredienteId}");
            }

            // Incrementar el stock con el lote
            ingrediente.IncrementarStock(
                request.Cantidad,
                $"Registro de lote {request.NumeroLote}");

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

            _logger.LogInformation("Lote registrado exitosamente. Stock actualizado: {StockActual}", ingrediente.Stock);

            return Result.Success(ingredienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar lote para ingrediente {IngredienteId}", request.IngredienteId);
            return Result.Failure<IngredienteDto>($"Error al registrar lote: {ex.Message}");
        }
    }
} 