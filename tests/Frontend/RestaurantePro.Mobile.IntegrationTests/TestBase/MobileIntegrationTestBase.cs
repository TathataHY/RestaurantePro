namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Clase base para pruebas de integración móvil con backend real
/// </summary>
public class MobileIntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _httpClient;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IConfiguration _configuration;
    protected readonly Fixture _fixture;

    public MobileIntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        // Configurar HttpClient para pruebas
        _httpClient = _factory.CreateClient();
        
        // Configurar servicios
        _serviceProvider = _factory.Services;
        _configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        
        // Configurar AutoFixture
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Crea un usuario de prueba y obtiene el token JWT
    /// </summary>
    protected async Task<string> GetAuthTokenAsync(string email = "admin@restaurantepro.com", string password = "Admin@123")
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return authResponse?.Token ?? throw new InvalidOperationException("Token no encontrado en la respuesta");
        }

        throw new InvalidOperationException($"Error en login: {response.StatusCode}");
    }

    /// <summary>
    /// Configura el token de autenticación en el HttpClient
    /// </summary>
    protected void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Realiza un login completo y configura el token
    /// </summary>
    protected async Task<string> LoginAndSetTokenAsync(string email = "admin@restaurantepro.com", string password = "Admin@123")
    {
        var token = await GetAuthTokenAsync(email, password);
        SetAuthToken(token);
        return token;
    }

    /// <summary>
    /// Realiza una petición GET autenticada
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        return await _httpClient.GetAsync(endpoint);
    }

    /// <summary>
    /// Realiza una petición POST autenticada
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(endpoint, content);
    }

    /// <summary>
    /// Realiza una petición PUT autenticada
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync(endpoint, content);
    }

    /// <summary>
    /// Realiza una petición DELETE autenticada
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await _httpClient.DeleteAsync(endpoint);
    }

    /// <summary>
    /// Deserializa una respuesta HTTP a un objeto
    /// </summary>
    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
} 