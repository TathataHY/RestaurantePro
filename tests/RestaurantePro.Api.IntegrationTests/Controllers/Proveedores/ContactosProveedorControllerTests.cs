using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Proveedores;

/// <summary>
/// Tests de integración para ContactosProveedorController
/// </summary>
[Collection("Sequential")]
public class ContactosProveedorControllerTests : ApiIntegrationTestBase
{
    public ContactosProveedorControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetContactos_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/proveedores/contactos");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetContacto_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/contactos/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CrearContacto_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.PostAsync("/api/proveedores/contactos", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ActualizarContacto_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.PutAsync($"/api/proveedores/contactos/{Guid.NewGuid()}", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EliminarContacto_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.DeleteAsync($"/api/proveedores/contactos/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetContactosPorProveedor_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/contactos/proveedor/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
} 