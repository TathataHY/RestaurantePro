using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;

/// <summary>
/// Handler para agregar un producto a una comanda existente
/// Gestiona las reglas de negocio para modificación de comandas
/// </summary>
public class AgregarItemComandaHandler : IRequestHandler<AgregarItemComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregarItemComandaHandler> _logger;

    public AgregarItemComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<AgregarItemComandaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(AgregarItemComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛒 Iniciando proceso de agregar item a comanda {ComandaId} - Producto: {ProductoId} x{Cantidad}", 
            request.ComandaId, request.ProductoId, request.Cantidad);

        try
        {
            // 1. Buscar la comanda existente
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId);
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda no encontrada: {ComandaId}", request.ComandaId);
                return Result.Failure<ComandaDto>("La comanda especificada no existe");
            }

            // 2. Verificar que la comanda se puede modificar
            if (!PuedeModificarComanda(comanda))
            {
                _logger.LogWarning("🚫 Intento de modificar comanda en estado no permitido. ComandaId: {ComandaId}, Estado: {Estado}", 
                    request.ComandaId, comanda.Estado);
                return Result.Failure<ComandaDto>($"No se puede agregar items a una comanda en estado '{comanda.Estado}'");
            }

            // 3. Agregar el producto usando el método del dominio
            var itemResult = await AgregarProductoAComanda(comanda, request);
            if (!itemResult.Succeeded)
            {
                return Result.Failure<ComandaDto>(itemResult.Error);
            }

            // 4. Guardar los cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync();

            // 5. Mapear y retornar el resultado
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Item agregado exitosamente a comanda {ComandaId}. Nuevo total: ${Total:F2}", 
                request.ComandaId, comandaDto.Total);

            return Result.Success(comandaDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("💼 Violación de regla de negocio al agregar item: {Error}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("📝 Error de argumentos al agregar item: {Error}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al agregar item a comanda {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>("Ocurrió un error interno al procesar la solicitud");
        }
    }

    /// <summary>
    /// Verifica si una comanda se puede modificar según su estado actual
    /// </summary>
    private static bool PuedeModificarComanda(Comanda comanda)
    {
        var estadosModificables = new[] { "Creada", "EnProceso" };
        return estadosModificables.Contains(comanda.Estado.ToString());
    }

    /// <summary>
    /// Agrega un producto a la comanda usando los métodos del dominio
    /// </summary>
    private async Task<Result> AgregarProductoAComanda(Comanda comanda, AgregarItemComandaCommand request)
    {
        try
        {
            // Usar el método del dominio que retorna ItemComanda
            var itemComanda = comanda.AgregarItem(
                request.ProductoId,
                request.NombreProducto,
                request.Cantidad,
                request.PrecioUnitario,
                request.Observaciones);

            // Agregar personalizaciones si las hay
            if (request.Personalizaciones.Any())
            {
                foreach (var personalizacionDto in request.Personalizaciones)
                {
                    var personalizacion = await CrearPersonalizacion(personalizacionDto);
                    if (personalizacion.Succeeded)
                    {
                        // TODO: Implementar método para agregar personalizaciones al item
                        // itemComanda.AgregarPersonalizacion(personalizacion.Value);
                        _logger.LogDebug("✨ Personalización pendiente de implementación: {Tipo}", personalizacionDto.Tipo);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Error al crear personalización: {Error}", personalizacion.Error);
                        return Result.Failure(personalizacion.Error);
                    }
                }
            }

            _logger.LogInformation("📦 Item agregado: {NombreProducto} x{Cantidad} = ${Subtotal:F2}", 
                request.NombreProducto, request.Cantidad, itemComanda.Subtotal);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al agregar producto al dominio");
            return Result.Failure($"Error al agregar el producto: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una personalización desde el DTO
    /// TODO: Este método debería usar un factory del dominio cuando esté disponible
    /// </summary>
    private async Task<Result<object>> CrearPersonalizacion(PersonalizacionCreateDto dto)
    {
        try
        {
            // TODO: Implementar factory de Personalización en el dominio
            // Por ahora, retornamos éxito simulado
            await Task.CompletedTask;
            
            _logger.LogDebug("✨ Personalización creada: {Tipo} - {IngredienteId}", dto.Tipo, dto.IngredienteId);
            return Result.Success<object>(new object()); // Placeholder
        }
        catch (Exception ex)
        {
            return Result.Failure<object>($"Error al crear personalización: {ex.Message}");
        }
    }
} 