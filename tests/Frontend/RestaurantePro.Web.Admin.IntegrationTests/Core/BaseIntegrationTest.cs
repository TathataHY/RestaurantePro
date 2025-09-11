using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

namespace RestaurantePro.Web.Admin.IntegrationTests.Core;

/// <summary>
/// Clase base para pruebas de integración con funcionalidades comunes
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory>, IAsyncLifetime
{
    protected readonly WebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly RestauranteProDbContext _context;

    protected BaseIntegrationTest(WebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Obtener un DbContext fresco para cada prueba
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
    }

    /// <summary>
    /// Obtiene las opciones de serialización JSON configuradas para las pruebas
    /// </summary>
    protected JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Inicialización asíncrona para cada prueba
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Limpiar la base de datos antes de cada prueba
        await CleanupDatabaseAsync();
    }

    /// <summary>
    /// Limpieza asíncrona después de cada prueba
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        // Limpiar la base de datos después de cada prueba
        await CleanupDatabaseAsync();
    }

    /// <summary>
    /// Limpia la base de datos en memoria para la siguiente prueba
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Crea un cliente de prueba con datos válidos
    /// </summary>
    protected static CrearClienteRequest CreateValidClienteRequest(string? email = null)
    {
        return new CrearClienteRequest
        {
            Nombre = "Cliente Prueba",
            Email = email ?? $"test{Guid.NewGuid():N}@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };
    }

    /// <summary>
    /// Crea un cliente de prueba con datos inválidos
    /// </summary>
    protected static CrearClienteRequest CreateInvalidClienteRequest()
    {
        return new CrearClienteRequest
        {
            Nombre = "", // Nombre vacío
            Email = "email-invalido", // Email inválido
            Telefono = "", // Teléfono vacío
            FechaNacimiento = DateTime.Today.AddYears(1), // Fecha futura
            AceptaTerminos = false // No acepta términos
        };
    }

    #region Productos Helper Methods

    /// <summary>
    /// Crea categorías de productos de prueba y retorna sus IDs
    /// </summary>
    protected async Task<List<Guid>> CrearCategoriasDePruebaAsync()
    {
        return await ProductosTestSeeder.SeedCategoriasAsync(_context);
    }

    /// <summary>
    /// Crea productos de prueba y retorna sus IDs
    /// </summary>
    protected async Task<List<Guid>> CrearProductosDePruebaAsync(List<Guid> categoriaIds)
    {
        return await ProductosTestSeeder.SeedProductosAsync(_context, categoriaIds);
    }

    /// <summary>
    /// Crea un request válido para crear un producto
    /// </summary>
    protected static CrearProductoCommand CreateValidProductoRequest(Guid categoriaId, string? nombre = null, decimal? precio = null)
    {
        return new CrearProductoCommand
        {
            Nombre = nombre ?? "Producto Prueba",
            Descripcion = "Descripción del producto de prueba",
            Precio = precio ?? 15.99m,
            CategoriaId = categoriaId,
            Activo = true
        };
    }

    /// <summary>
    /// Crea un request inválido para crear un producto
    /// </summary>
    protected static CrearProductoCommand CreateInvalidProductoRequest()
    {
        return new CrearProductoCommand
        {
            Nombre = "", // Nombre vacío
            Descripcion = "", // Descripción vacía
            Precio = -10m, // Precio negativo
            CategoriaId = Guid.Empty, // Categoría inválida
            Activo = true
        };
    }

    /// <summary>
    /// Obtiene una categoría aleatoria de la lista proporcionada
    /// </summary>
    protected static Guid GetRandomCategoriaId(List<Guid> categoriaIds)
    {
        return ProductosTestSeeder.GetRandomCategoriaId(categoriaIds);
    }

    /// <summary>
    /// Obtiene la primera categoría de la lista
    /// </summary>
    protected static Guid GetFirstCategoriaId(List<Guid> categoriaIds)
    {
        return ProductosTestSeeder.GetFirstCategoriaId(categoriaIds);
    }

    #endregion
}
