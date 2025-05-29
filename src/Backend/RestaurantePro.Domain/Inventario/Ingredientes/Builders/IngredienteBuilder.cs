using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Domain.Inventario.Ingredientes.Builders;

/// <summary>
/// Builder para construir instancias de Ingrediente paso a paso con validaciones fluidas.
/// Permite crear ingredientes complejos de manera segura y legible.
/// </summary>
public class IngredienteBuilder
{
    private string? _nombre;
    private string? _codigo;
    private string? _descripcion;
    private UnidadMedida? _unidadMedida;
    private decimal? _stockMinimo;
    private decimal? _stockActual;
    private Guid? _proveedorPrincipalId;
    private RotacionIngrediente _rotacion = RotacionIngrediente.Media;
    private TemporadaIngrediente _temporada = TemporadaIngrediente.TodoElAño;
    private decimal _costoPromedio = 0;
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<IngredienteBuilder> _logger;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public IngredienteBuilder(INotificationManager notificationManager, ILogger<IngredienteBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece el nombre del ingrediente
    /// </summary>
    /// <param name="nombre">Nombre del ingrediente (máximo 200 caracteres)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            _notificationManager.AddError("El nombre del ingrediente es obligatorio", nameof(nombre));
            return this;
        }

        if (nombre.Length > 200)
        {
            _notificationManager.AddError("El nombre no puede exceder 200 caracteres", nameof(nombre));
            return this;
        }

        _nombre = nombre.Trim();
        _logger.LogDebug("Nombre '{Nombre}' asignado al ingrediente", _nombre);
        return this;
    }

    /// <summary>
    /// Establece el código del ingrediente
    /// </summary>
    /// <param name="codigo">Código único del ingrediente (máximo 50 caracteres)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            _notificationManager.AddError("El código del ingrediente es obligatorio", nameof(codigo));
            return this;
        }

        if (codigo.Length > 50)
        {
            _notificationManager.AddError("El código no puede exceder 50 caracteres", nameof(codigo));
            return this;
        }

        _codigo = codigo.Trim().ToUpperInvariant();
        _logger.LogDebug("Código '{Codigo}' asignado al ingrediente", _codigo);
        return this;
    }

    /// <summary>
    /// Establece la descripción del ingrediente
    /// </summary>
    /// <param name="descripcion">Descripción del ingrediente (máximo 500 caracteres)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            _notificationManager.AddError("La descripción del ingrediente es obligatoria", nameof(descripcion));
            return this;
        }

        if (descripcion.Length > 500)
        {
            _notificationManager.AddError("La descripción no puede exceder 500 caracteres", nameof(descripcion));
            return this;
        }

        _descripcion = descripcion.Trim();
        _logger.LogDebug("Descripción asignada al ingrediente");
        return this;
    }

    /// <summary>
    /// Establece la unidad de medida del ingrediente
    /// </summary>
    /// <param name="unidadMedida">Unidad de medida</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConUnidadMedida(UnidadMedida unidadMedida)
    {
        _unidadMedida = unidadMedida;
        _logger.LogDebug("Unidad de medida '{UnidadMedida}' asignada al ingrediente", unidadMedida);
        return this;
    }

    /// <summary>
    /// Establece el stock mínimo del ingrediente
    /// </summary>
    /// <param name="stockMinimo">Stock mínimo (debe ser >= 0)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConStockMinimo(decimal stockMinimo)
    {
        if (stockMinimo < 0)
        {
            _notificationManager.AddError("El stock mínimo no puede ser negativo", nameof(stockMinimo));
            return this;
        }

        _stockMinimo = stockMinimo;
        _logger.LogDebug("Stock mínimo {StockMinimo} asignado al ingrediente", stockMinimo);
        return this;
    }

    /// <summary>
    /// Establece el stock actual del ingrediente
    /// </summary>
    /// <param name="stockActual">Stock actual (debe ser >= 0)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConStockActual(decimal stockActual)
    {
        if (stockActual < 0)
        {
            _notificationManager.AddError("El stock actual no puede ser negativo", nameof(stockActual));
            return this;
        }

        _stockActual = stockActual;
        _logger.LogDebug("Stock actual {StockActual} asignado al ingrediente", stockActual);
        return this;
    }

    /// <summary>
    /// Asocia un proveedor principal al ingrediente
    /// </summary>
    /// <param name="proveedorId">ID del proveedor principal</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConProveedorPrincipal(Guid proveedorId)
    {
        if (proveedorId == Guid.Empty)
        {
            _notificationManager.AddError("El ID del proveedor principal no puede estar vacío", nameof(proveedorId));
            return this;
        }

        _proveedorPrincipalId = proveedorId;
        _logger.LogDebug("Proveedor principal {ProveedorId} asignado al ingrediente", proveedorId);
        return this;
    }

    /// <summary>
    /// Establece el nivel de rotación del ingrediente
    /// </summary>
    /// <param name="rotacion">Nivel de rotación</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConRotacion(RotacionIngrediente rotacion)
    {
        _rotacion = rotacion;
        _logger.LogDebug("Rotación '{Rotacion}' asignada al ingrediente", rotacion);
        return this;
    }

    /// <summary>
    /// Establece la temporada del ingrediente
    /// </summary>
    /// <param name="temporada">Temporada del ingrediente</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConTemporada(TemporadaIngrediente temporada)
    {
        _temporada = temporada;
        _logger.LogDebug("Temporada '{Temporada}' asignada al ingrediente", temporada);
        return this;
    }

    /// <summary>
    /// Establece el costo promedio del ingrediente
    /// </summary>
    /// <param name="costoPromedio">Costo promedio (debe ser >= 0)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public IngredienteBuilder ConCostoPromedio(decimal costoPromedio)
    {
        if (costoPromedio < 0)
        {
            _notificationManager.AddError("El costo promedio no puede ser negativo", nameof(costoPromedio));
            return this;
        }

        _costoPromedio = costoPromedio;
        _logger.LogDebug("Costo promedio {CostoPromedio} asignado al ingrediente", costoPromedio);
        return this;
    }

    /// <summary>
    /// Construye el ingrediente con todas las validaciones
    /// </summary>
    /// <returns>Resultado con el ingrediente creado o errores de validación</returns>
    public Result<Ingrediente> Construir()
    {
        _logger.LogDebug("Iniciando construcción de ingrediente");

        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(_nombre))
            _notificationManager.AddError("El nombre del ingrediente es obligatorio", nameof(_nombre));

        if (string.IsNullOrWhiteSpace(_codigo))
            _notificationManager.AddError("El código del ingrediente es obligatorio", nameof(_codigo));

        if (string.IsNullOrWhiteSpace(_descripcion))
            _notificationManager.AddError("La descripción del ingrediente es obligatoria", nameof(_descripcion));

        if (!_unidadMedida.HasValue)
            _notificationManager.AddError("La unidad de medida es obligatoria", nameof(_unidadMedida));

        if (!_stockMinimo.HasValue)
            _notificationManager.AddError("El stock mínimo es obligatorio", nameof(_stockMinimo));

        if (!_stockActual.HasValue)
            _notificationManager.AddError("El stock actual es obligatorio", nameof(_stockActual));

        // Si hay errores, retornar fallo
        if (_notificationManager.HasErrors)
        {
            _logger.LogWarning("Error en la construcción del ingrediente: errores de validación encontrados");
            return Result.Failure<Ingrediente>("Errores de validación en la construcción del ingrediente");
        }

        try
        {
            // Crear el ingrediente usando el factory method de la entidad
            var ingrediente = Ingrediente.Crear(
                _nombre!,
                _codigo!,
                _descripcion!,
                _unidadMedida!.Value,
                _stockMinimo!.Value,
                _stockActual!.Value,
                _rotacion,
                _temporada);

            // Asociar proveedor principal si se especificó
            if (_proveedorPrincipalId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(_proveedorPrincipalId.Value);
            }

            // Actualizar costo promedio si se especificó
            if (_costoPromedio > 0)
            {
                ingrediente.ActualizarCostoPromedio(_costoPromedio);
            }

            _logger.LogInformation("Ingrediente '{Nombre}' con código '{Codigo}' construido exitosamente", 
                _nombre, _codigo);

            return Result.Success(ingrediente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al construir ingrediente");
            _notificationManager.AddError($"Error al construir ingrediente: {ex.Message}", "Construccion");
            return Result.Failure<Ingrediente>("Error inesperado en la construcción del ingrediente");
        }
    }

    /// <summary>
    /// Resetea el builder para reutilización
    /// </summary>
    /// <returns>Builder limpio para nueva construcción</returns>
    public IngredienteBuilder Reset()
    {
        _nombre = null;
        _codigo = null;
        _descripcion = null;
        _unidadMedida = null;
        _stockMinimo = null;
        _stockActual = null;
        _proveedorPrincipalId = null;
        _rotacion = RotacionIngrediente.Media;
        _temporada = TemporadaIngrediente.TodoElAño;
        _costoPromedio = 0;

        _notificationManager.ClearErrors();
        _logger.LogDebug("Builder reseteado para nueva construcción");
        return this;
    }

    /// <summary>
    /// Método de conveniencia para crear un builder configurado
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nuevo builder configurado</returns>
    public static IngredienteBuilder Nuevo(INotificationManager notificationManager, ILogger<IngredienteBuilder> logger)
    {
        return new IngredienteBuilder(notificationManager, logger);
    }
} 