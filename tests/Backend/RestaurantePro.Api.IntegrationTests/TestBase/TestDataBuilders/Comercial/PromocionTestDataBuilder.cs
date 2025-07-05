using RestaurantePro.Domain.Comercial.Promociones;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Core.Productos;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear promociones de prueba con datos válidos y únicos
/// </summary>
public class PromocionTestDataBuilder
{
    private readonly RestauranteProDbContext _dbContext;
    private readonly IDateTimeService _dateTimeService;
    
    // Propiedades de la promoción
    private string _codigo = string.Empty;
    private string _nombre = string.Empty;
    private string _descripcion = string.Empty;
    private TipoPromocion _tipo = TipoPromocion.PorcentajeTotal;
    private decimal _valor = 10.0m;
    private DateTime _fechaInicio = DateTime.Today;
    private DateTime _fechaFin = DateTime.Today.AddDays(30);
    private EstadoPromocion _estado = EstadoPromocion.Activa;
    private List<Guid> _productosAplicablesIds = new();
    private List<Guid> _categoriasAplicablesIds = new();
    private List<Guid> _clientesQueUsaronIds = new();
    private int _usoMaximo = 100;
    private int _usoActual = 0;
    private bool _requiereCodigo = false;
    private string _codigoDescuento = string.Empty;

    public PromocionTestDataBuilder(RestauranteProDbContext dbContext, IDateTimeService dateTimeService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        
        // Inicializar con valores por defecto válidos
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        // Solo mayúsculas, números, guiones y guiones bajos, máximo 20 caracteres
        _codigo = ("PROMO" + guid.Substring(0, 16)).Replace("-", "_");
        _nombre = $"PromocionTest_{guid.Substring(0, 8)}";
        _descripcion = $"Descripcion de promocion de prueba {guid.Substring(0, 8)}";
        _tipo = TipoPromocion.PorcentajeTotal;
        _valor = 15.0m;
        _fechaInicio = DateTime.Today;
        _fechaFin = DateTime.Today.AddDays(30);
        _estado = EstadoPromocion.Creada;
        _usoMaximo = 100;
        _usoActual = 0;
        _productosAplicablesIds = new List<Guid>();
        _categoriasAplicablesIds = new List<Guid>();
    }

    /// <summary>
    /// Establece el código de la promoción
    /// </summary>
    public PromocionTestDataBuilder ConCodigo(string codigo)
    {
        // Validar que el código cumple el formato requerido
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código no puede estar vacío", nameof(codigo));
        }
        
        if (codigo.Length > 20)
        {
            throw new ArgumentException("El código no puede exceder 20 caracteres", nameof(codigo));
        }
        
        // Solo mayúsculas, números, guiones y guiones bajos
        if (!System.Text.RegularExpressions.Regex.IsMatch(codigo, @"^[A-Z0-9_-]+$"))
        {
            throw new ArgumentException("El código solo puede contener letras mayúsculas, números, guiones y guiones bajos", nameof(codigo));
        }
        
        _codigo = codigo;
        return this;
    }

    /// <summary>
    /// Establece el nombre de la promoción
    /// </summary>
    public PromocionTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    /// <summary>
    /// Establece la descripción de la promoción
    /// </summary>
    public PromocionTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    /// <summary>
    /// Establece el tipo de promoción
    /// </summary>
    public PromocionTestDataBuilder ConTipo(TipoPromocion tipo)
    {
        _tipo = tipo;
        return this;
    }

    /// <summary>
    /// Establece el valor de la promoción
    /// </summary>
    public PromocionTestDataBuilder ConValor(decimal valor)
    {
        _valor = valor;
        return this;
    }

    /// <summary>
    /// Establece las fechas de vigencia
    /// </summary>
    public PromocionTestDataBuilder ConFechasVigencia(DateTime fechaInicio, DateTime fechaFin)
    {
        _fechaInicio = fechaInicio;
        _fechaFin = fechaFin;
        return this;
    }

    /// <summary>
    /// Establece el estado de la promoción
    /// </summary>
    public PromocionTestDataBuilder ConEstado(EstadoPromocion estado)
    {
        _estado = estado;
        return this;
    }

    /// <summary>
    /// Establece los productos aplicables
    /// </summary>
    public PromocionTestDataBuilder ConProductosAplicables(List<Guid> productosIds)
    {
        if (productosIds != null)
        {
            foreach (var id in productosIds)
            {
                if (!_productosAplicablesIds.Contains(id))
                    _productosAplicablesIds.Add(id);
            }
        }
        return this;
    }

    /// <summary>
    /// Establece las categorías aplicables
    /// </summary>
    public PromocionTestDataBuilder ConCategoriasAplicables(List<Guid> categoriasIds)
    {
        _categoriasAplicablesIds = categoriasIds ?? new List<Guid>();
        return this;
    }

    /// <summary>
    /// Establece el uso máximo
    /// </summary>
    public PromocionTestDataBuilder ConUsoMaximo(int usoMaximo)
    {
        _usoMaximo = usoMaximo;
        return this;
    }

    /// <summary>
    /// Establece el uso actual
    /// </summary>
    public PromocionTestDataBuilder ConUsoActual(int usoActual)
    {
        _usoActual = usoActual;
        return this;
    }

    /// <summary>
    /// Establece si requiere código de descuento
    /// </summary>
    public PromocionTestDataBuilder ConRequiereCodigo(bool requiereCodigo)
    {
        _requiereCodigo = requiereCodigo;
        return this;
    }

    /// <summary>
    /// Establece el código de descuento
    /// </summary>
    public PromocionTestDataBuilder ConCodigoDescuento(string codigoDescuento)
    {
        _codigoDescuento = codigoDescuento;
        return this;
    }

    /// <summary>
    /// Construye la promoción con datos por defecto válidos
    /// </summary>
    public PromocionTestDataBuilder ConDatosPorDefecto()
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var randomSuffix = new Random().Next(1000, 9999).ToString();
        
        // Generar código único: PROMO + timestamp + random + guid (máximo 20 caracteres)
        var codigoBase = $"PROMO{timestamp.Substring(timestamp.Length - 8)}{randomSuffix}";
        _codigo = codigoBase.Length > 20 ? codigoBase.Substring(0, 20) : codigoBase;
        
        _nombre = $"Promocion Test {guid.Substring(0, 8)}";
        _descripcion = $"Descripcion de la promocion de prueba {guid.Substring(0, 8)}";
        _tipo = TipoPromocion.PorcentajeTotal;
        _valor = 15.0m;
        _fechaInicio = _dateTimeService.Now;
        _fechaFin = _dateTimeService.Now.AddDays(30);
        _estado = EstadoPromocion.Creada;
        _usoMaximo = 100;
        _usoActual = 0;
        _productosAplicablesIds = new List<Guid>();
        _categoriasAplicablesIds = new List<Guid>();
        _clientesQueUsaronIds = new List<Guid>();
        
        return this;
    }

    /// <summary>
    /// Construye y guarda la promoción en la base de datos
    /// </summary>
    public async Task<Promocion> BuildAsync()
    {
        // Generar datos por defecto solo si no se han establecido
        if (string.IsNullOrEmpty(_codigo))
        {
            ConDatosPorDefecto();
        }

        // Crear la promoción usando el factory method
        var promocion = Promocion.Crear(
            _codigo,
            _nombre,
            _descripcion,
            _tipo,
            _valor,
            _fechaInicio,
            _fechaFin,
            0, // montoMinimo
            0, // puntosRequeridos
            _usoMaximo > 0 ? _usoMaximo : null, // maximoUsos
            false // esAcumulable
        );

        // Agregar productos aplicables si existen
        foreach (var productoId in _productosAplicablesIds)
        {
            promocion.AgregarProductoAplicable(productoId);
        }

        // Agregar categorías aplicables si existen
        foreach (var categoriaId in _categoriasAplicablesIds)
        {
            promocion.AgregarCategoriaAplicable(categoriaId);
        }

        // Cambiar estado si es diferente a Creada
        if (_estado != EstadoPromocion.Creada)
        {
            switch (_estado)
            {
                case EstadoPromocion.Activa:
                    promocion.Activar();
                    break;
                case EstadoPromocion.Pausada:
                    promocion.Activar();
                    promocion.Pausar();
                    break;
                case EstadoPromocion.Finalizada:
                    promocion.Activar();
                    promocion.Finalizar();
                    break;
                case EstadoPromocion.Cancelada:
                    promocion.Activar();
                    promocion.Cancelar("Cancelada por test");
                    break;
            }
        }

        // Agregar a la base de datos
        _dbContext.Promociones.Add(promocion);
        await _dbContext.SaveChangesAsync();

        return promocion;
    }

    /// <summary>
    /// Crea una promoción de descuento por porcentaje
    /// </summary>
    public async Task<Promocion> CrearPromocionPorcentajeAsync(string? codigo = null, decimal porcentaje = 15.0m)
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return await ConDatosPorDefecto()
            .ConCodigo(codigo ?? $"PCT{guid.Substring(0, 17)}")
            .ConTipo(TipoPromocion.PorcentajeTotal)
            .ConValor(porcentaje)
            .ConNombre($"Descuento {porcentaje}%")
            .ConDescripcion($"Promoción de descuento del {porcentaje}%")
            .BuildAsync();
    }

    /// <summary>
    /// Crea una promoción de descuento por monto fijo
    /// </summary>
    public async Task<Promocion> CrearPromocionMontoFijoAsync(string? codigo = null, decimal monto = 5000.0m)
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return await ConDatosPorDefecto()
            .ConCodigo(codigo ?? $"MF{guid.Substring(0, 18)}")
            .ConTipo(TipoPromocion.MontoFijoTotal)
            .ConValor(monto)
            .ConNombre($"Descuento ${monto:N0}")
            .ConDescripcion($"Promoción de descuento fijo de ${monto:N0}")
            .BuildAsync();
    }

    /// <summary>
    /// Crea una promoción activa para productos específicos
    /// </summary>
    public async Task<Promocion> CrearPromocionParaProductosAsync(List<Guid> productosIds, string? codigo = null)
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return await ConDatosPorDefecto()
            .ConCodigo(codigo ?? $"PROD{guid.Substring(0, 16)}")
            .ConProductosAplicables(productosIds)
            .ConNombre("Promoción Específica")
            .ConDescripcion("Promoción aplicable a productos específicos")
            .BuildAsync();
    }

    /// <summary>
    /// Crea una promoción pausada
    /// </summary>
    public async Task<Promocion> CrearPromocionPausadaAsync(string? codigo = null)
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return await ConDatosPorDefecto()
            .ConCodigo(codigo ?? $"PAUS{guid.Substring(0, 16)}")
            .ConEstado(EstadoPromocion.Pausada)
            .ConNombre("Promoción Pausada")
            .ConDescripcion("Promoción temporalmente pausada")
            .BuildAsync();
    }

    /// <summary>
    /// Crea una promoción con código de descuento requerido
    /// </summary>
    public async Task<Promocion> CrearPromocionConCodigoAsync(string codigoDescuento = "DESCUENTO2024")
    {
        var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return await ConDatosPorDefecto()
            .ConCodigo($"COD{guid.Substring(0, 17)}")
            .ConRequiereCodigo(true)
            .ConCodigoDescuento(codigoDescuento)
            .ConNombre("Promoción con Código")
            .ConDescripcion("Promoción que requiere código de descuento")
            .BuildAsync();
    }
} 