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
                        // Aplicar personalización usando los métodos del dominio
                        var aplicado = await AplicarPersonalizacion(itemComanda, personalizacionDto);
                        if (!aplicado.Succeeded)
                        {
                            _logger.LogWarning("⚠️ Error al aplicar personalización: {Error}", aplicado.Error);
                            return Result.Failure(aplicado.Error);
                        }
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
    /// Valida los datos de una personalización desde el DTO
    /// </summary>
    private async Task<Result<object>> CrearPersonalizacion(PersonalizacionCreateDto dto)
    {
        try
        {
            await Task.CompletedTask; // No necesitamos operaciones async

            // Validaciones básicas
            if (dto.IngredienteId == Guid.Empty)
            {
                return Result.Failure<object>("El ID del ingrediente es requerido para la personalización");
            }

            if (string.IsNullOrWhiteSpace(dto.Tipo))
            {
                return Result.Failure<object>("El tipo de personalización es requerido");
            }

            var tipoUpper = dto.Tipo.ToUpperInvariant();
            if (tipoUpper != "EXTRA" && tipoUpper != "QUITAR" && tipoUpper != "SUSTITUIR")
            {
                return Result.Failure<object>($"Tipo de personalización no válido: {dto.Tipo}");
            }

            // Validaciones específicas por tipo
            if (tipoUpper == "EXTRA" && dto.Cantidad <= 0)
            {
                return Result.Failure<object>("La cantidad debe ser mayor que cero para personalizaciones de tipo Extra");
            }

            if (tipoUpper == "SUSTITUIR" && !dto.IngredienteSustitucionId.HasValue)
            {
                return Result.Failure<object>("Se requiere especificar el ingrediente de sustitución para personalizaciones de tipo Sustituir");
            }

            if (dto.PrecioAdicional < 0)
            {
                return Result.Failure<object>("El precio adicional no puede ser negativo");
            }
            
            _logger.LogDebug("✅ Personalización validada: {Tipo} - {IngredienteId}", dto.Tipo, dto.IngredienteId);
            return Result.Success<object>(new object()); // Solo retornamos éxito, no necesitamos el objeto
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al validar personalización");
            return Result.Failure<object>($"Error al validar personalización: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplica una personalización a un itemComanda usando los métodos del dominio
    /// </summary>
    private async Task<Result> AplicarPersonalizacion(ItemComanda itemComanda, PersonalizacionCreateDto dto)
    {
        try
        {
            await Task.CompletedTask; // No necesitamos operaciones async aquí

            switch (dto.Tipo.ToUpperInvariant())
            {
                case "EXTRA":
                    // Usar método del dominio para agregar extra
                    itemComanda.AgregarPersonalizacionExtra(
                        dto.IngredienteId,
                        $"Extra ingrediente {dto.IngredienteId}", // TODO: Obtener nombre real del ingrediente
                        dto.Cantidad,
                        dto.PrecioAdicional);
                    
                    _logger.LogDebug("✨ Extra agregado: Ingrediente {IngredienteId} x{Cantidad} (+${Precio:F2})", 
                        dto.IngredienteId, dto.Cantidad, dto.PrecioAdicional);
                    break;

                case "QUITAR":
                    // Usar método del dominio para quitar ingrediente
                    itemComanda.AgregarPersonalizacionQuitar(
                        dto.IngredienteId,
                        $"Quitar ingrediente {dto.IngredienteId}"); // TODO: Obtener nombre real del ingrediente
                    
                    _logger.LogDebug("✨ Ingrediente removido: {IngredienteId}", dto.IngredienteId);
                    break;

                case "SUSTITUIR":
                    // Validar que existe ingrediente de sustitución
                    if (!dto.IngredienteSustitucionId.HasValue)
                    {
                        return Result.Failure("Para sustituir un ingrediente se requiere especificar el ingrediente de sustitución");
                    }

                    // Usar método del dominio para sustituir
                    itemComanda.AgregarPersonalizacionSustituir(
                        dto.IngredienteId,
                        $"Ingrediente {dto.IngredienteId}", // TODO: Obtener nombre real del ingrediente
                        dto.IngredienteSustitucionId.Value,
                        $"Ingrediente {dto.IngredienteSustitucionId.Value}", // TODO: Obtener nombre real del ingrediente
                        dto.Cantidad,
                        dto.PrecioAdicional);
                    
                    _logger.LogDebug("✨ Sustitución aplicada: {IngredienteOriginal} → {IngredienteSustituto} (+${Precio:F2})", 
                        dto.IngredienteId, dto.IngredienteSustitucionId.Value, dto.PrecioAdicional);
                    break;

                default:
                    return Result.Failure($"Tipo de personalización no válido: {dto.Tipo}. Valores válidos: Extra, Quitar, Sustituir");
            }

            _logger.LogInformation("✅ Personalización {Tipo} aplicada exitosamente al item {ItemId}", 
                dto.Tipo, itemComanda.Id);
            
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("💼 Error de lógica de negocio al aplicar personalización: {Error}", ex.Message);
            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("📝 Error de argumentos al aplicar personalización: {Error}", ex.Message);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al aplicar personalización {Tipo}", dto.Tipo);
            return Result.Failure($"Error interno al aplicar personalización: {ex.Message}");
        }
    }
} 