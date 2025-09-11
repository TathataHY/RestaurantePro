using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Net.Http.Json;

namespace RestaurantePro.Web.Admin.IntegrationTests;

/// <summary>
/// Clase base para todas las pruebas de integración de la web administrativa
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory>
{
    protected readonly WebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly RestauranteProDbContext Context;

    protected BaseIntegrationTest(WebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
    }

    /// <summary>
    /// Realiza una petición GET y deserializa la respuesta
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await Client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// Realiza una petición POST y deserializa la respuesta
    /// </summary>
    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var response = await Client.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// Realiza una petición PUT y deserializa la respuesta
    /// </summary>
    protected async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var response = await Client.PutAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// Realiza una petición DELETE
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await Client.DeleteAsync(endpoint);
    }

    /// <summary>
    /// Limpia la base de datos de prueba
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        Context.RemoveRange(Context.Set<object>());
        await Context.SaveChangesAsync();
    }
}
