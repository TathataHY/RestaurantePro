using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Common;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Clase base para tests de autorización
/// Permite probar que los endpoints protegidos devuelven 401 sin token
/// </summary>
public abstract class AuthorizationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly TestWebApplicationFactory Factory;

    protected AuthorizationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        
        // Crear un cliente HTTP sin autenticación
        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifica que un endpoint requiere autenticación (devuelve 401)
    /// </summary>
    protected async Task Endpoint_DebeRequerirAutenticacion(string endpoint, HttpMethod method = null)
    {
        // Arrange
        method ??= HttpMethod.Get;
        var request = new HttpRequestMessage(method, endpoint);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifica que un endpoint requiere un rol específico
    /// </summary>
    protected async Task Endpoint_DebeRequerirRol(string endpoint, string role, HttpMethod method = null)
    {
        // Arrange
        method ??= HttpMethod.Get;
        var request = new HttpRequestMessage(method, endpoint);
        
        // Agregar header de autenticación con un usuario que NO tiene el rol requerido
        // Usamos "Test User_12345678-1234-1234-1234-123456789012_UsuarioSinRol" 
        // donde "UsuarioSinRol" es un rol que no coincide con el requerido
        request.Headers.Add("Authorization", "Test User_12345678-1234-1234-1234-123456789012_UsuarioSinRol");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifica que un endpoint público funciona sin autenticación
    /// </summary>
    protected async Task Endpoint_Publico_DebeFuncionarSinAutenticacion(string endpoint, HttpMethod method = null)
    {
        // Arrange
        method ??= HttpMethod.Get;
        var request = new HttpRequestMessage(method, endpoint);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
} 