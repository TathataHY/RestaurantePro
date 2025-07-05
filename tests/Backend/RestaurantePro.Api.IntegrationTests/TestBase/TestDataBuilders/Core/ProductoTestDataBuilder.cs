using Bogus;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
using RestaurantePro.Domain.Core.Productos;

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
    private Guid? _categoriaId;
    private bool? _activo;

    /// <summary>
    /// Genera un nombre de producto único
    /// </summary>
    private string GenerarNombreUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Producto_{guid.Substring(0, 8)}";
    }

    /// <summary>
    /// Genera una descripción única
    /// </summary>
    private string GenerarDescripcionUnica()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Descripción del producto {guid.Substring(0, 4)}";
    }

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
    /// Establece el ID de la categoría del producto
    /// </summary>
    public ProductoTestDataBuilder ConCategoriaId(Guid categoriaId)
    {
        _categoriaId = categoriaId;
        return this;
    }

    /// <summary>
    /// Establece si el producto está activo
    /// </summary>
    public ProductoTestDataBuilder Activo(bool activo)
    {
        _activo = activo;
        return this;
    }

    /// <summary>
    /// Construye el comando para crear un producto
    /// </summary>
    public CrearProductoCommand BuildCrearProductoCommand()
    {
        return new CrearProductoCommand
        {
            Nombre = _nombre ?? GenerarNombreUnico(),
            Descripcion = _descripcion ?? GenerarDescripcionUnica(),
            Precio = _precio ?? 10.50m,
            CategoriaId = _categoriaId ?? Guid.NewGuid(),
            Activo = _activo ?? true
        };
    }

    /// <summary>
    /// Construye el comando para actualizar un producto
    /// </summary>
    public ActualizarProductoCommand BuildActualizarProductoCommand(Guid productoId)
    {
        return new ActualizarProductoCommand
        {
            Id = productoId,
            Nombre = _nombre ?? GenerarNombreUnico(),
            Descripcion = _descripcion ?? GenerarDescripcionUnica(),
            Precio = _precio ?? 10.50m,
            CategoriaId = _categoriaId ?? Guid.NewGuid(),
            Activo = _activo ?? true
        };
    }

    /// <summary>
    /// Construye un producto válido por defecto
    /// </summary>
    public CrearProductoCommand BuildProductoValido()
    {
        return new CrearProductoCommand
        {
            Nombre = GenerarNombreUnico(),
            Descripcion = GenerarDescripcionUnica(),
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid(),
            Activo = true
        };
    }

    /// <summary>
    /// Construye un producto inválido para testing
    /// </summary>
    public CrearProductoCommand BuildProductoInvalido()
    {
        return new CrearProductoCommand
        {
            Nombre = "", // Nombre vacío
            Descripcion = "", // Descripción vacía
            Precio = -10.00m, // Precio negativo
            CategoriaId = Guid.Empty, // Categoría inválida
            Activo = true
        };
    }

    /// <summary>
    /// Resetea el builder a sus valores por defecto
    /// </summary>
    public ProductoTestDataBuilder Reset()
    {
        _nombre = null;
        _descripcion = null;
        _precio = null;
        _categoriaId = null;
        _activo = null;
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