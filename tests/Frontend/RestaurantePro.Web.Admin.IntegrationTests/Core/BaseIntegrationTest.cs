using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Web.Admin.IntegrationTests.Converters;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

namespace RestaurantePro.Web.Admin.IntegrationTests.Core;

/// <summary>
/// Clase base para pruebas de integración con funcionalidades comunes
/// </summary>
[Collection("IntegrationTests")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly WebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly RestauranteProDbContext _context;

    protected BaseIntegrationTest(WebApplicationFactory factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient();

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
            Converters = { 
                new EstadoUsuarioConverter(),
                new TipoUsuarioConverter(),
                new RolUsuarioConverter()
            },
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Crea un cliente HTTP autenticado para pruebas que requieren autorización
    /// </summary>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        return client;
    }

    /// <summary>
    /// Crea un cliente HTTP sin autenticación para pruebas de seguridad
    /// </summary>
    protected HttpClient CreateUnauthenticatedClient()
    {
        return _factory.CreateClient();
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

    #region Usuarios Helper Methods

    /// <summary>
    /// Crea un usuario administrador de prueba y retorna su ID
    /// </summary>
    protected async Task<Guid> CrearUsuarioAdministradorDePruebaAsync()
    {
        return await UsuariosTestSeeder.SeedUsuarioAdministradorAsync(_context);
    }

    #endregion

    #region Recetas Helper Methods

    /// <summary>
    /// Crea productos de prueba y retorna sus IDs
    /// </summary>
    protected async Task<List<Guid>> CrearProductosDePruebaAsync(int cantidad = 5)
    {
        var categoriaIds = await CrearCategoriasDePruebaAsync();
        return await ProductosTestSeeder.SeedProductosAsync(_context, categoriaIds, cantidad);
    }

    /// <summary>
    /// Crea ingredientes de prueba y retorna sus IDs
    /// </summary>
    protected async Task<List<Guid>> CrearIngredientesDePruebaAsync(int cantidad = 10)
    {
        Console.WriteLine($"=== CrearIngredientesDePruebaAsync: Iniciando creación de {cantidad} ingredientes ===");
        try
        {
            var resultado = await IngredientesTestSeeder.SeedIngredientesAsync(_context, cantidad);
            Console.WriteLine($"=== CrearIngredientesDePruebaAsync: {resultado.Count} ingredientes creados exitosamente ===");
            return resultado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== CrearIngredientesDePruebaAsync: ERROR - {ex.Message} ===");
            throw;
        }
    }

    /// <summary>
    /// Obtiene ingredientes existentes de la base de datos
    /// </summary>
    protected async Task<List<Guid>> ObtenerIngredientesExistentesAsync(int cantidad = 5)
    {
        // Siempre crear ingredientes nuevos para asegurar que existan
        var ingredientes = await CrearIngredientesDePruebaAsync(cantidad);
        
        // Verificar que se crearon correctamente
        var ingredientesVerificados = await _context.Ingredientes
            .Where(i => i.EstaActivo)
            .Take(cantidad)
            .Select(i => i.Id)
            .ToListAsync();
        
        Console.WriteLine($"Ingredientes creados: {ingredientes.Count}");
        Console.WriteLine($"Ingredientes verificados: {ingredientesVerificados.Count}");
        
        // Debug: Listar todos los ingredientes en la base de datos
        var todosLosIngredientes = await _context.Ingredientes
            .Select(i => new { i.Id, i.Nombre, i.EstaActivo })
            .ToListAsync();
        
        Console.WriteLine($"Total ingredientes en BD: {todosLosIngredientes.Count}");
        foreach (var ing in todosLosIngredientes)
        {
            Console.WriteLine($"  - {ing.Id}: {ing.Nombre} (Activo: {ing.EstaActivo})");
        }
        
        return ingredientes;
    }

    /// <summary>
    /// Crea una receta de prueba y retorna su ID
    /// </summary>
    protected async Task<Guid> CrearRecetaDePruebaAsync(Guid productoId)
    {
        return await ProductosTestSeeder.SeedRecetaAsync(_context, productoId);
    }

    /// <summary>
    /// Crea una receta compleja de prueba y retorna su ID
    /// </summary>
    protected async Task<Guid> CrearRecetaComplejaAsync(Guid productoId)
    {
        return await ProductosTestSeeder.SeedRecetaComplejaAsync(_context, productoId);
    }

    /// <summary>
    /// Crea una receta con muchos ingredientes de prueba y retorna su ID
    /// </summary>
    protected async Task<Guid> CrearRecetaConMuchosIngredientesAsync(Guid productoId)
    {
        return await ProductosTestSeeder.SeedRecetaConMuchosIngredientesAsync(_context, productoId);
    }

    /// <summary>
    /// Crea un cliente HTTP autenticado con un rol específico
    /// </summary>
    protected HttpClient CreateAuthenticatedClientWithRole(string rol)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Test-Role", rol);
        return client;
    }

    #endregion
}
