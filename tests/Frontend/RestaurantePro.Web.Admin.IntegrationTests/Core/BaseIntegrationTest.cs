using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

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
}
