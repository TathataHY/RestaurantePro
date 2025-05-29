using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Services;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;

/// <summary>
/// Handler para el comando CrearIngrediente
/// Utiliza el servicio de dominio para crear ingredientes con stock inicial
/// </summary>
public class CrearIngredienteHandler : IRequestHandler<CrearIngredienteCommand, Result<IngredienteDto>>
{
    private readonly IIngredienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearIngredienteHandler> _logger;
    private readonly IInventarioServiceFacade _inventarioService;

    public CrearIngredienteHandler(
        IIngredienteRepository repository,
        IMapper mapper,
        ILogger<CrearIngredienteHandler> logger,
        IInventarioServiceFacade inventarioService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _inventarioService = inventarioService;
    }

    public async Task<Result<IngredienteDto>> Handle(
        CrearIngredienteCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🧪 Iniciando creación de ingrediente: {Nombre} - Código: {Codigo}", 
            request.Nombre, request.Codigo);

        try
        {
            // 1. Verificar que no existe un ingrediente con el mismo nombre
            var ingredientesExistentes = await _repository.ObtenerPorNombreAsync(request.Nombre, cancellationToken);
            if (ingredientesExistentes.Any(i => i.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("⚠️ Ya existe un ingrediente con el nombre: {Nombre}", request.Nombre);
                return Result.Failure<IngredienteDto>($"Ya existe un ingrediente registrado con el nombre {request.Nombre}");
            }

            // 2. Convertir enums desde strings
            if (!Enum.TryParse<RotacionIngrediente>(request.Rotacion, true, out var rotacion))
            {
                return Result.Failure<IngredienteDto>($"Rotación no válida: {request.Rotacion}");
            }

            if (!Enum.TryParse<TemporadaIngrediente>(request.Temporada, true, out var temporada))
            {
                return Result.Failure<IngredienteDto>($"Temporada no válida: {request.Temporada}");
            }

            // 3. Usar el servicio de dominio para crear el ingrediente
            var resultadoCreacion = await _inventarioService.RegistrarIngredienteAsync(
                request.Nombre,
                request.Descripcion,
                request.UnidadMedida,
                request.StockMinimo,
                request.StockInicial,
                rotacion,
                temporada,
                request.CostoInicial,
                cancellationToken);

            if (!resultadoCreacion.Succeeded)
            {
                _logger.LogWarning("❌ Error al crear ingrediente: {Error}", resultadoCreacion.Error);
                return Result.Failure<IngredienteDto>(resultadoCreacion.Error);
            }

            var ingrediente = resultadoCreacion.Value;

            _logger.LogInformation("📦 Stock inicial configurado: {Cantidad} {UnidadMedida}", 
                request.StockInicial, request.UnidadMedida);

            // 4. Mapear a DTO y retornar
            var ingredienteDto = _mapper.Map<IngredienteDto>(ingrediente);

            _logger.LogInformation("✅ Ingrediente creado exitosamente: {Id} - {Nombre}", 
                ingrediente.Id, request.Nombre);

            return Result.Success(ingredienteDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al crear ingrediente: {Message}", ex.Message);
            return Result.Failure<IngredienteDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al crear ingrediente: {Nombre}", request.Nombre);
            return Result.Failure<IngredienteDto>("Error interno del servidor al crear el ingrediente");
        }
    }
}