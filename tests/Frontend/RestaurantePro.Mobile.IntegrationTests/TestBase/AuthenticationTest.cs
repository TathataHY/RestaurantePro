using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Test simple para verificar si la autenticación está funcionando
/// </summary>
public class AuthenticationTest : MobileIntegrationTestBase
{
    public AuthenticationTest(MobileIntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestAuthenticationEndpoint_ShouldReturnOk()
    {
        // 🔧 HACER UNA LLAMADA DIRECTA AL ENDPOINT DE LOGIN
        var loginRequest = new
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!" // Corrected password
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"🔧 STATUS CODE: {response.StatusCode}");
        Console.WriteLine($"🔧 RESPONSE CONTENT: {responseContent}");

        response.EnsureSuccessStatusCode(); // Assert that the status code is 2xx
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // 🔧 PARSEAR LA RESPUESTA JSON
        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        // 🔧 VERIFICAR QUE LA RESPUESTA ES EXITOSA
        Assert.True(root.TryGetProperty("success", out var successElement) && successElement.GetBoolean(), "La respuesta no indica éxito.");
        
        // 🔧 VERIFICAR QUE HAY DATOS EN LA RESPUESTA
        Assert.True(root.TryGetProperty("data", out var dataElement), "Propiedad 'data' no encontrada en la respuesta.");
        
        // 🔧 VERIFICAR QUE EL TOKEN ESTÁ PRESENTE EN LOS DATOS
        Assert.True(dataElement.TryGetProperty("token", out var tokenElement) && !string.IsNullOrEmpty(tokenElement.GetString()), "Token no encontrado o vacío.");
        
        // 🔧 VERIFICAR DATOS ESPECÍFICOS DEL USUARIO
        Assert.True(dataElement.TryGetProperty("userName", out var userNameElement) && userNameElement.GetString() == "admin@restaurantepro.com", "Email del usuario incorrecto.");
        Assert.True(dataElement.TryGetProperty("roles", out var rolesElement) && rolesElement.EnumerateArray().Any(r => r.GetString() == "Administrador"), "Rol 'Administrador' no encontrado para el usuario.");

        Console.WriteLine("✅ TestAuthenticationEndpoint_ShouldReturnOk PASSED");
    }

    [Fact]
    public async Task TestAuthenticationEndpoint_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // 🔧 HACER UNA LLAMADA CON CREDENCIALES INVÁLIDAS
        var loginRequest = new
        {
            Email = "invalid@example.com",
            Password = "wrongpassword"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"🔧 STATUS CODE: {response.StatusCode}");
        Console.WriteLine($"🔧 RESPONSE CONTENT: {responseContent}");
        
        // 🔧 VERIFICAR QUE LA RESPUESTA SEA 401 UNAUTHORIZED
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestClientesEndpoint_ShouldReturnClientes()
    {
        // 🔧 PRIMERO AUTENTICARSE PARA OBTENER EL TOKEN
        var loginRequest = new
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        
        Console.WriteLine($"🔧 LOGIN STATUS CODE: {loginResponse.StatusCode}");
        Console.WriteLine($"🔧 LOGIN RESPONSE CONTENT: {loginResponseContent}");

        loginResponse.EnsureSuccessStatusCode();

        // 🔧 EXTRAER EL TOKEN DE LA RESPUESTA (ESTRUCTURA ANIDADA)
        using var loginDoc = JsonDocument.Parse(loginResponseContent);
        var loginRoot = loginDoc.RootElement;
        var loginDataElement = loginRoot.GetProperty("data");
        var token = loginDataElement.GetProperty("token").GetString();
        
        Console.WriteLine($"🔧 TOKEN OBTENIDO: {token?.Substring(0, Math.Min(50, token.Length))}...");

        // 🔧 HACER UNA LLAMADA AUTENTICADA AL ENDPOINT DE CLIENTES
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.GetAsync("/api/comercial/clientes");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"🔧 CLIENTES STATUS CODE: {response.StatusCode}");
        Console.WriteLine($"🔧 CLIENTES RESPONSE CONTENT: {responseContent}");

        // 🔧 VERIFICAR QUE LA RESPUESTA ES 200 OK
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // 🔧 PARSEAR LA RESPUESTA JSON
        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        // 🔧 VERIFICAR QUE LA RESPUESTA TIENE LA ESTRUCTURA CORRECTA
        Assert.True(root.TryGetProperty("data", out var dataElement), "Propiedad 'data' no encontrada en la respuesta.");
        Assert.True(dataElement.TryGetProperty("items", out var itemsElement), "Propiedad 'items' no encontrada en la respuesta.");

        // 🔧 VERIFICAR QUE HAY CLIENTES EN LA RESPUESTA
        var clientes = itemsElement.EnumerateArray().ToList();
        Assert.NotEmpty(clientes);

        Console.WriteLine($"✅ TestClientesEndpoint_ShouldReturnClientes PASSED - {clientes.Count} clientes encontrados");
    }
} 