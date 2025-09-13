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
using RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;

namespace RestaurantePro.Web.Admin.IntegrationTests.Core;

/// <summary>
/// Clase base para pruebas de integración con funcionalidades comunes
/// </summary>
[Collection("IntegrationTests")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly WebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected RestauranteProDbContext _context;

    protected BaseIntegrationTest(WebApplicationFactory factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient();
    }

    /// <summary>
    /// Obtiene las opciones de serialización JSON configuradas para las pruebas
    /// </summary>
    protected JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            Converters = { 
                new JsonStringEnumConverter(),
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
        // Crear un contexto fresco para cada prueba
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
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
        try
        {
            // Limpiar ChangeTracker primero
            _context.ChangeTracker.Clear();
            
            // Limpiar todas las tablas principales de manera segura

            await LimpiarTabla(_context.Usuarios);
            await LimpiarTabla(_context.Clientes);
            await LimpiarTabla(_context.Productos);
            await LimpiarTabla(_context.ProductoCategorias);
            await LimpiarTabla(_context.Ingredientes);
            await LimpiarTabla(_context.Recetas);
            await LimpiarTabla(_context.Promociones);
            await LimpiarTabla(_context.MovimientosInventario);
            await LimpiarTabla(_context.OrdenesCompra);
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log del error pero no fallar el test
            Console.WriteLine($"Warning: Error during cleanup: {ex.Message}");
            
            // Intentar limpiar de manera más agresiva
            try
            {
                _context.ChangeTracker.Clear();
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Si falla, continuar sin limpiar
            }
        }
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
    /// Crea un comando CrearRecetaCommand válido con ingredientes
    /// </summary>
    protected async Task<CrearRecetaCommand> CrearComandoRecetaValidoAsync(Guid productoId, int cantidadIngredientes = 3)
    {
        // Asegurar que tenemos ingredientes disponibles
        var ingredientesExistentes = await _context.Ingredientes.CountAsync();
        if (ingredientesExistentes == 0)
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(_context, 10);
        }

        // Obtener ingredientes disponibles
        var ingredientesDisponibles = await _context.Ingredientes.Take(cantidadIngredientes).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            throw new InvalidOperationException("No se pudieron obtener ingredientes para crear la receta");
        }

        // Crear el comando con ingredientes válidos
        var command = new CrearRecetaCommand
        {
            ProductoId = productoId,
            Preparacion = "Receta de prueba con ingredientes válidos",
            TiempoPreparacionMinutos = 30,
            Ingredientes = ingredientesDisponibles.Select(ing => new RestaurantePro.Application.Core.Recetas.DTOs.AgregarIngredienteDto
            {
                IngredienteId = ing.Id,
                Cantidad = 1.0m
            }).ToList()
        };

        return command;
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

    #region Clientes Helper Methods

    /// <summary>
    /// Crea clientes de prueba y retorna sus IDs
    /// </summary>
    protected async Task<List<Guid>> CrearClientesDePruebaAsync(int total, int activos)
    {
        return await ClientesTestSeeder.SeedClientesAsync(_context, total, activos);
    }

    /// <summary>
    /// Crea clientes con un segmento específico
    /// </summary>
    protected async Task<List<Guid>> CrearClientesConSegmentoAsync(int cantidad, string segmento)
    {
        return await ClientesTestSeeder.SeedClientesConSegmentoAsync(_context, cantidad, segmento);
    }

    /// <summary>
    /// Crea clientes con fechas de registro específicas
    /// </summary>
    protected async Task<List<Guid>> CrearClientesConFechasRegistroAsync(int cantidad, DateTime fechaDesde, DateTime fechaHasta)
    {
        return await ClientesTestSeeder.SeedClientesConFechasRegistroAsync(_context, cantidad, fechaDesde, fechaHasta);
    }

    /// <summary>
    /// Crea clientes con filtros combinados
    /// </summary>
    protected async Task<List<Guid>> CrearClientesConFiltrosCombinadosAsync(int cantidad, string segmento, DateTime fechaDesde, DateTime fechaHasta)
    {
        return await ClientesTestSeeder.SeedClientesConFiltrosCombinadosAsync(_context, cantidad, segmento, fechaDesde, fechaHasta, true);
    }

    /// <summary>
    /// Crea solo clientes activos
    /// </summary>
    protected async Task<List<Guid>> CrearClientesActivosAsync(int cantidad)
    {
        return await ClientesTestSeeder.SeedClientesActivosAsync(_context, cantidad);
    }

    #endregion

    private async Task LimpiarTabla<T>(DbSet<T> dbSet) where T : class
    {
        try
        {
            var entities = await dbSet.ToListAsync();
            if (entities.Any())
            {
                dbSet.RemoveRange(entities);
            }
        }
        catch (Exception ex)
        {
            // Ignorar errores de limpieza para evitar fallos en tests
            Console.WriteLine($"Error limpiando tabla {typeof(T).Name}: {ex.Message}");
        }
    }
}
