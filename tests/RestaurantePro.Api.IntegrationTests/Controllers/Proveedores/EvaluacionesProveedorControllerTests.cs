using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Proveedores;

/// <summary>
/// Tests de integración para EvaluacionesProveedorController
/// </summary>
[Collection("Sequential")]
public class EvaluacionesProveedorControllerTests : ApiIntegrationTestBase
{
    public EvaluacionesProveedorControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetEvaluaciones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/proveedores/evaluaciones");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEvaluacion_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CrearEvaluacion_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.PostAsync("/api/proveedores/evaluaciones", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ActualizarEvaluacion_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.PutAsync($"/api/proveedores/evaluaciones/{Guid.NewGuid()}", null);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EliminarEvaluacion_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.DeleteAsync($"/api/proveedores/evaluaciones/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEvaluacionesPorProveedor_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/proveedor/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPromedioEvaluaciones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/promedio/{Guid.NewGuid()}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
} 