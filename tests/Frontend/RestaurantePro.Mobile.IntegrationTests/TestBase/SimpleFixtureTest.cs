using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Test simple para verificar que el fixture funciona correctamente
/// </summary>
public class SimpleFixtureTest : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public SimpleFixtureTest(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task Fixture_ShouldWork()
    {
        // 🔧 VERIFICAR SI EL ENDPOINT DE AUTENTICACIÓN FUNCIONA
        var isWorking = await TestAuthenticationEndpointAsync();

        // 🔧 SI NO FUNCIONA, FALLAR EL TEST PARA VER EL ERROR
        Assert.True(isWorking, "El fixture no está funcionando correctamente");
    }

    /// <summary>
    /// Método de prueba para verificar si la autenticación está funcionando
    /// </summary>
    private async Task<bool> TestAuthenticationEndpointAsync()
    {
        try
        {
            // 🔧 HACER UNA LLAMADA DIRECTA AL ENDPOINT DE LOGIN
            var loginRequest = new
            {
                Email = "admin@restaurantepro.com",
                Password = "AdminRestaurante123!"
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"🔍 LOGIN RESPONSE: Status={response.StatusCode}, Content={responseContent}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔍 ERROR EN TEST AUTH: {ex.Message}");
            return false;
        }
    }
} 