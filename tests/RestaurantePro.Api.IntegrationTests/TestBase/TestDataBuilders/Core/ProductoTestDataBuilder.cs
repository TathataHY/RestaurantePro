using Bogus;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Core;

/// <summary>
/// Builder para crear datos de prueba de productos para tests de integración
/// </summary>
public class ProductoTestDataBuilder
{
    private readonly Faker _faker = new Faker("es");
    
    private string? _nombre;
    private string? _descripcion;
    private decimal? _precio;
    private ProductoCategoria _categoria = ProductoCategoria.PlatoPrincipal;
    private bool _disponible = true;
    private string? _imagenUrl;
    private int? _tiempoPreparacion;
    private bool _esVegetariano = false;
    private bool _esVegano = false;
    private bool _contieneGluten = false;
    private string? _alergenos;

    /// <summary>
    /// Establece el nombre del producto
    /// </summary>
    public ProductoTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    /// <summary>
    /// Establece la descripción del producto
    /// </summary>
    public ProductoTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    /// <summary>
    /// Establece el precio del producto
    /// </summary>
    public ProductoTestDataBuilder ConPrecio(decimal precio)
    {
        _precio = precio;
        return this;
    }

    /// <summary>
    /// Establece la categoría del producto
    /// </summary>
    public ProductoTestDataBuilder ConCategoria(ProductoCategoria categoria)
    {
        _categoria = categoria;
        return this;
    }

    /// <summary>
    /// Establece si el producto está disponible
    /// </summary>
    public ProductoTestDataBuilder Disponible(bool disponible)
    {
        _disponible = disponible;
        return this;
    }

    /// <summary>
    /// Establece la URL de la imagen del producto
    /// </summary>
    public ProductoTestDataBuilder ConImagenUrl(string imagenUrl)
    {
        _imagenUrl = imagenUrl;
        return this;
    }

    /// <summary>
    /// Establece el tiempo de preparación del producto
    /// </summary>
    public ProductoTestDataBuilder ConTiempoPreparacion(int tiempoPreparacion)
    {
        _tiempoPreparacion = tiempoPreparacion;
        return this;
    }

    /// <summary>
    /// Establece si el producto es vegetariano
    /// </summary>
    public ProductoTestDataBuilder EsVegetariano(bool esVegetariano)
    {
        _esVegetariano = esVegetariano;
        return this;
    }

    /// <summary>
    /// Establece si el producto es vegano
    /// </summary>
    public ProductoTestDataBuilder EsVegano(bool esVegano)
    {
        _esVegano = esVegano;
        return this;
    }

    /// <summary>
    /// Establece si el producto contiene gluten
    /// </summary>
    public ProductoTestDataBuilder ContieneGluten(bool contieneGluten)
    {
        _contieneGluten = contieneGluten;
        return this;
    }

    /// <summary>
    /// Establece los alérgenos del producto
    /// </summary>
    public ProductoTestDataBuilder ConAlergenos(string alergenos)
    {
        _alergenos = alergenos;
        return this;
    }

    /// <summary>
    /// Construye el objeto de request para crear un producto
    /// </summary>
    public object BuildCrearProductoRequest()
    {
        return new
        {
            Nombre = _nombre ?? _faker.Commerce.ProductName(),
            Descripcion = _descripcion ?? _faker.Lorem.Sentence(5, 10),
            Precio = _precio ?? _faker.Random.Decimal(5, 50),
            Categoria = _categoria,
            Disponible = _disponible,
            ImagenUrl = _imagenUrl ?? _faker.Image.PicsumUrl(),
            TiempoPreparacion = _tiempoPreparacion ?? _faker.Random.Int(5, 30),
            EsVegetariano = _esVegetariano,
            EsVegano = _esVegano,
            ContieneGluten = _contieneGluten,
            Alergenos = _alergenos ?? _faker.PickRandom("", "Lácteos", "Frutos secos", "Mariscos", "Huevos")
        };
    }

    /// <summary>
    /// Construye el objeto de request para actualizar un producto
    /// </summary>
    public object BuildActualizarProductoRequest()
    {
        return new
        {
            Nombre = _nombre ?? _faker.Commerce.ProductName(),
            Descripcion = _descripcion ?? _faker.Lorem.Sentence(5, 10),
            Precio = _precio ?? _faker.Random.Decimal(5, 50),
            Categoria = _categoria,
            Disponible = _disponible,
            ImagenUrl = _imagenUrl ?? _faker.Image.PicsumUrl(),
            TiempoPreparacion = _tiempoPreparacion ?? _faker.Random.Int(5, 30),
            EsVegetariano = _esVegetariano,
            EsVegano = _esVegano,
            ContieneGluten = _contieneGluten,
            Alergenos = _alergenos ?? _faker.PickRandom("", "Lácteos", "Frutos secos", "Mariscos", "Huevos")
        };
    }

    /// <summary>
    /// Construye el objeto de request para buscar productos
    /// </summary>
    public object BuildBuscarProductosRequest(string? termino = null, string? categoria = null, bool? disponible = null)
    {
        return new
        {
            Termino = termino ?? _faker.Commerce.ProductName(),
            Categoria = categoria ?? _categoria.ToString(),
            Disponible = disponible ?? _disponible,
            PageNumber = 1,
            PageSize = 20
        };
    }

    /// <summary>
    /// Construye el objeto de request para cambiar disponibilidad
    /// </summary>
    public object BuildCambiarDisponibilidadRequest(bool? disponible = null)
    {
        return new
        {
            Disponible = disponible ?? _disponible,
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para actualizar precio
    /// </summary>
    public object BuildActualizarPrecioRequest(decimal? precio = null)
    {
        return new
        {
            Precio = precio ?? _precio ?? _faker.Random.Decimal(5, 50),
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para obtener productos por categoría
    /// </summary>
    public object BuildObtenerPorCategoriaRequest(string? categoria = null)
    {
        return new
        {
            Categoria = categoria ?? _categoria.ToString(),
            PageNumber = 1,
            PageSize = 20
        };
    }

    /// <summary>
    /// Construye el objeto de request para obtener productos populares
    /// </summary>
    public object BuildObtenerPopularesRequest(int? cantidad = null)
    {
        return new
        {
            Cantidad = cantidad ?? _faker.Random.Int(5, 10),
            Periodo = _faker.PickRandom("Dia", "Semana", "Mes")
        };
    }

    /// <summary>
    /// Resetea el builder a su estado inicial
    /// </summary>
    public ProductoTestDataBuilder Reset()
    {
        _nombre = null;
        _descripcion = null;
        _precio = null;
        _categoria = ProductoCategoria.PlatoPrincipal;
        _disponible = true;
        _imagenUrl = null;
        _tiempoPreparacion = null;
        _esVegetariano = false;
        _esVegano = false;
        _contieneGluten = false;
        _alergenos = null;
        return this;
    }

    /// <summary>
    /// Crea una nueva instancia del builder
    /// </summary>
    public static ProductoTestDataBuilder Nuevo()
    {
        return new ProductoTestDataBuilder();
    }
}

/// <summary>
/// Categorías de producto para los tests
/// </summary>
public enum ProductoCategoria
{
    Entrada,
    PlatoPrincipal,
    Postre,
    Bebida,
    Acompañamiento,
    Especialidad
} 