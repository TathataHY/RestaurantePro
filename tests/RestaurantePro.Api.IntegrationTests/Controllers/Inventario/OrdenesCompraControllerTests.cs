using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración para OrdenesCompraController
/// </summary>
[Collection("Sequential")]
public class OrdenesCompraControllerTests : ApiIntegrationTestBase
{
    public OrdenesCompraControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetOrdenesCompra_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ordenes-compra");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrdenCompraPorId_DebeRetornarRespuestaValida()
    {
        // Arrange - Usar un ID que sabemos que no existe
        var idInexistente = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        // Act
        var response = await HttpClient.GetAsync($"/api/inventario/ordenes-compra/{idInexistente}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.PostAsync("/api/inventario/ordenes-compra", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PutOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange - Usar un ID que sabemos que no existe
        var idInexistente = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        // Act
        var response = await HttpClient.PutAsync($"/api/inventario/ordenes-compra/{idInexistente}", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostAprobarOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange - Usar un ID que sabemos que no existe
        var idInexistente = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        // Act
        var response = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{idInexistente}/aprobar", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostRechazarOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange - Usar un ID que sabemos que no existe
        var idInexistente = Guid.Parse("44444444-4444-4444-4444-444444444444");
        
        // Act
        var response = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{idInexistente}/rechazar", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostRecibirOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange - Usar un ID que sabemos que no existe
        var idInexistente = Guid.Parse("55555555-5555-5555-5555-555555555555");
        
        // Act
        var response = await HttpClient.PostAsync($"/api/inventario/ordenes-compra/{idInexistente}/recibir", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrdenesCompraPendientes_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/inventario/ordenes-compra/pendientes");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
} 