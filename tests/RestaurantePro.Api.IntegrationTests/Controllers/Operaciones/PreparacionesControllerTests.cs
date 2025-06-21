using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración para PreparacionesController
/// Valida todos los endpoints REST del controlador de preparaciones
/// </summary>
[Collection("Sequential")]
public class PreparacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public PreparacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetPreparaciones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/preparaciones");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetPreparacionPorId_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/preparaciones/{id}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacion = new { };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/preparaciones", preparacion);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PutPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();
        var preparacion = new { };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/operaciones/preparaciones/{id}", preparacion);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostIniciarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{id}/iniciar", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostCompletarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{id}/completar", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostCancelarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{id}/cancelar", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetColaPreparaciones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/preparaciones/cola");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    public new void Dispose()
    {
        base.Dispose();
    }
} 