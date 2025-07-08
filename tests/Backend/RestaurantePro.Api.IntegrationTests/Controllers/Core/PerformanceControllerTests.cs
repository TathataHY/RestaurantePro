using System.Diagnostics;
using System.Net;
using System.Text.Json;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.Models.Requests;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de rendimiento para endpoints críticos de la API
/// </summary>
[Collection("ApiTestCollection")]
public class PerformanceControllerTests : ApiIntegrationTestBase
{
    public PerformanceControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact(DisplayName = "Performance_Login_DeberiaCompletarEnMenosDe500ms")]
    public async Task Performance_Login_DeberiaCompletarEnMenosDe500ms()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "El login debe completarse en menos de 500ms");
        
        Logger.LogInformation("✅ Login completado en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    [Fact(DisplayName = "Performance_ConsultaUsuarios_DeberiaCompletarEnMenosDe250ms")]
    public async Task Performance_ConsultaUsuarios_DeberiaCompletarEnMenosDe250ms()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await HttpClient.GetAsync("/api/core/usuarios");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(250, "La consulta de usuarios debe completarse en menos de 250ms");
        
        Logger.LogInformation("✅ Consulta de usuarios completada en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    [Fact(DisplayName = "Performance_ConsultaPerfil_DeberiaCompletarEnMenosDe200ms")]
    public async Task Performance_ConsultaPerfil_DeberiaCompletarEnMenosDe200ms()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await HttpClient.GetAsync("/api/auth/profile");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "La consulta de perfil debe completarse en menos de 200ms");
        
        Logger.LogInformation("✅ Consulta de perfil completada en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    [Fact(DisplayName = "Performance_ConcurrentRequests_DeberiaMantenerRendimiento")]
    public async Task Performance_ConcurrentRequests_DeberiaMantenerRendimiento()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tasks = new List<Task<(HttpStatusCode StatusCode, long ElapsedMs)>>();
        var stopwatch = Stopwatch.StartNew();

        // Act: Ejecutar 5 requests concurrentes
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(ExecuteConcurrentRequest());
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(5);
        results.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue("Todos los requests deben ser exitosos");
        results.All(r => r.ElapsedMs < 500).Should().BeTrue("Todos los requests deben completarse en menos de 500ms");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Los 5 requests concurrentes deben completarse en menos de 2 segundos");
        
        Logger.LogInformation("✅ 5 requests concurrentes completados en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        Logger.LogInformation("📊 Tiempo promedio por request: {AvgMs}ms", results.Average(r => r.ElapsedMs));
    }

    private async Task<(HttpStatusCode StatusCode, long ElapsedMs)> ExecuteConcurrentRequest()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.GetAsync("/api/core/usuarios");
        stopwatch.Stop();
        
        return (response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
} 