using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Inventario;

/// <summary>
/// Builder para crear datos de prueba de ingredientes para tests de integración
/// </summary>
public class IngredienteTestDataBuilder
{
    private string? _nombre;
    private string? _codigo;
    private string? _descripcion;
    private string? _unidadMedida;
    private decimal? _stockInicial;
    private decimal? _stockMinimo;
    private string? _rotacion;
    private string? _temporada;
    private Guid? _proveedorPrincipalId;
    private decimal? _costoInicial;
    private bool? _estaActivo;
    private Guid? _usuarioId;
    private string? _motivoStockInicial;

    /// <summary>
    /// Genera un nombre de ingrediente único
    /// </summary>
    private string GenerarNombreUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Ingrediente_{guid.Substring(0, 8)}";
    }

    /// <summary>
    /// Genera un código único
    /// </summary>
    private string GenerarCodigoUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"ING_{guid.Substring(0, 6)}";
    }

    /// <summary>
    /// Genera una descripción única
    /// </summary>
    private string GenerarDescripcionUnica()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Descripción del ingrediente {guid.Substring(0, 4)}";
    }

    /// <summary>
    /// Establece el nombre del ingrediente
    /// </summary>
    public IngredienteTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    /// <summary>
    /// Establece el código del ingrediente
    /// </summary>
    public IngredienteTestDataBuilder ConCodigo(string codigo)
    {
        _codigo = codigo;
        return this;
    }

    /// <summary>
    /// Establece la descripción del ingrediente
    /// </summary>
    public IngredienteTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    /// <summary>
    /// Establece la unidad de medida
    /// </summary>
    public IngredienteTestDataBuilder ConUnidadMedida(string unidadMedida)
    {
        _unidadMedida = unidadMedida;
        return this;
    }

    /// <summary>
    /// Establece el stock inicial
    /// </summary>
    public IngredienteTestDataBuilder ConStockInicial(decimal stockInicial)
    {
        _stockInicial = stockInicial;
        return this;
    }

    /// <summary>
    /// Establece el stock mínimo
    /// </summary>
    public IngredienteTestDataBuilder ConStockMinimo(decimal stockMinimo)
    {
        _stockMinimo = stockMinimo;
        return this;
    }

    /// <summary>
    /// Establece la rotación
    /// </summary>
    public IngredienteTestDataBuilder ConRotacion(string rotacion)
    {
        _rotacion = rotacion;
        return this;
    }

    /// <summary>
    /// Establece la temporada
    /// </summary>
    public IngredienteTestDataBuilder ConTemporada(string temporada)
    {
        _temporada = temporada;
        return this;
    }

    /// <summary>
    /// Establece el ID del proveedor principal
    /// </summary>
    public IngredienteTestDataBuilder ConProveedorPrincipalId(Guid? proveedorPrincipalId)
    {
        _proveedorPrincipalId = proveedorPrincipalId;
        return this;
    }

    /// <summary>
    /// Establece el costo inicial
    /// </summary>
    public IngredienteTestDataBuilder ConCostoInicial(decimal costoInicial)
    {
        _costoInicial = costoInicial;
        return this;
    }

    /// <summary>
    /// Establece si está activo
    /// </summary>
    public IngredienteTestDataBuilder EstaActivo(bool estaActivo)
    {
        _estaActivo = estaActivo;
        return this;
    }

    /// <summary>
    /// Establece el ID del usuario
    /// </summary>
    public IngredienteTestDataBuilder ConUsuarioId(Guid usuarioId)
    {
        _usuarioId = usuarioId;
        return this;
    }

    /// <summary>
    /// Establece el motivo del stock inicial
    /// </summary>
    public IngredienteTestDataBuilder ConMotivoStockInicial(string motivoStockInicial)
    {
        _motivoStockInicial = motivoStockInicial;
        return this;
    }

    /// <summary>
    /// Construye el comando para crear un ingrediente
    /// </summary>
    public CrearIngredienteCommand BuildCrearIngredienteCommand()
    {
        // Si no se especificó UsuarioId, asignar uno válido por defecto
        if (!_usuarioId.HasValue || _usuarioId.Value == Guid.Empty)
        {
            _usuarioId = Guid.NewGuid();
        }

        return new CrearIngredienteCommand
        {
            Nombre = _nombre ?? GenerarNombreUnico(),
            Codigo = _codigo ?? GenerarCodigoUnico(),
            Descripcion = _descripcion ?? GenerarDescripcionUnica(),
            UnidadMedida = _unidadMedida ?? "Kilogramo",
            StockInicial = _stockInicial ?? 10.0m,
            StockMinimo = _stockMinimo ?? 5.0m,
            Rotacion = _rotacion ?? "Media",
            Temporada = _temporada ?? "TodoElAño",
            ProveedorPrincipalId = _proveedorPrincipalId,
            CostoInicial = _costoInicial ?? 5.50m,
            EstaActivo = _estaActivo ?? true,
            UsuarioId = _usuarioId.Value,
            MotivoStockInicial = _motivoStockInicial ?? "Stock inicial al crear ingrediente"
        };
    }

    /// <summary>
    /// Construye un ingrediente válido por defecto
    /// </summary>
    public CrearIngredienteCommand BuildIngredienteValido()
    {
        return new CrearIngredienteCommand
        {
            Nombre = GenerarNombreUnico(),
            Codigo = GenerarCodigoUnico(),
            Descripcion = GenerarDescripcionUnica(),
            UnidadMedida = "Kilogramo",
            StockInicial = 25.0m,
            StockMinimo = 5.0m,
            Rotacion = "Media",
            Temporada = "TodoElAño",
            ProveedorPrincipalId = Guid.NewGuid(),
            CostoInicial = 8.99m,
            EstaActivo = true,
            UsuarioId = Guid.NewGuid(),
            MotivoStockInicial = "Stock inicial al crear ingrediente"
        };
    }

    /// <summary>
    /// Alias para BuildCrearIngredienteCommand (compatibilidad con tests)
    /// </summary>
    public CrearIngredienteCommand BuildCrearIngredienteRequest()
    {
        return BuildCrearIngredienteCommand();
    }

    /// <summary>
    /// Construye el comando para actualizar un ingrediente
    /// </summary>
    public ActualizarIngredienteCommand BuildActualizarIngredienteRequest()
    {
        return new ActualizarIngredienteCommand
        {
            Id = Guid.NewGuid(), 
            Nombre = _nombre ?? GenerarNombreUnico(),
            Descripcion = _descripcion ?? GenerarDescripcionUnica(),
            StockMinimo = _stockMinimo ?? 5.0m,
            Rotacion = RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
            Temporada = RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño,
            ProveedorPrincipalId = _proveedorPrincipalId,
            CostoPromedio = _costoInicial ?? 5.50m,
            BloqueadoControlCalidad = false,
            FechaExpiracion = null
        };
    }

    /// <summary>
    /// Construye un ingrediente inválido para testing
    /// </summary>
    public CrearIngredienteCommand BuildIngredienteInvalido()
    {
        return new CrearIngredienteCommand
        {
            Nombre = "", // Nombre vacío
            Codigo = "", // Código vacío
            Descripcion = "", // Descripción vacía
            UnidadMedida = "", // Unidad de medida vacía
            StockInicial = -10.0m, // Stock inicial negativo
            StockMinimo = -5.0m, // Stock mínimo negativo
            Rotacion = "",
            Temporada = "",
            ProveedorPrincipalId = null,
            CostoInicial = -5.00m, // Costo negativo
            EstaActivo = true,
            UsuarioId = Guid.Empty, // Usuario inválido
            MotivoStockInicial = ""
        };
    }

    /// <summary>
    /// Resetea el builder a sus valores por defecto
    /// </summary>
    public IngredienteTestDataBuilder Reset()
    {
        _nombre = null;
        _codigo = null;
        _descripcion = null;
        _unidadMedida = null;
        _stockInicial = null;
        _stockMinimo = null;
        _rotacion = null;
        _temporada = null;
        _proveedorPrincipalId = null;
        _costoInicial = null;
        _estaActivo = null;
        _usuarioId = null;
        _motivoStockInicial = null;
        return this;
    }

    /// <summary>
    /// Crea una nueva instancia del builder
    /// </summary>
    public static IngredienteTestDataBuilder Nuevo()
    {
        return new IngredienteTestDataBuilder();
    }
} 