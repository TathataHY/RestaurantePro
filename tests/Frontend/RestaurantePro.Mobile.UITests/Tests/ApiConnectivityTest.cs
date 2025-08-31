using Xunit;
using Xunit.Abstractions;
using System.Net.Http;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Test simple que solo verifica que la API real esté accesible
/// </summary>
public class ApiConnectivityTest
{
    private readonly ITestOutputHelper _testOutput;

    public ApiConnectivityTest(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    [Fact]
    public async Task Api_Real_Should_Be_Accessible()
    {
        try
        {
            _testOutput.WriteLine("🔍 Verificando que la API real esté accesible...");
            
            // Crear un HttpClient simple para probar conectividad
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // Intentar hacer una petición simple a la API (usar endpoint que existe)
            var response = await httpClient.GetAsync("http://192.168.1.85:8080/api/core/productos");
            
            _testOutput.WriteLine($"✅ API real responde con status: {response.StatusCode}");
            
            // Verificar que la API responda (aunque sea con 401, significa que está viva)
            Assert.True(response.IsSuccessStatusCode || 
                       response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                       response.StatusCode == System.Net.HttpStatusCode.NotFound);
            
            _testOutput.WriteLine("✅ API real está accesible y respondiendo");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error verificando API real: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task Api_Health_Endpoint_Should_Respond()
    {
        try
        {
            _testOutput.WriteLine("🔍 Verificando endpoint de productos de la API...");
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // Probar un endpoint que sabemos que existe (productos)
            var response = await httpClient.GetAsync("http://192.168.1.85:8080/api/core/productos");
            
            _testOutput.WriteLine($"✅ Endpoint productos responde con status: {response.StatusCode}");
            
            // Si responde 401, significa que el servidor está vivo pero requiere autenticación
            // Si responde 200, significa que el endpoint existe y está funcionando
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                       response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            
            _testOutput.WriteLine("✅ Endpoint de productos responde correctamente");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error verificando endpoint productos: {ex.Message}");
            throw;
        }
    }
}
