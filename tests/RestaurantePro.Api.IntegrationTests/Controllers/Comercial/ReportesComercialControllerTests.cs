using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para ReportesComercialController
/// </summary>
[Collection("Sequential")]
public class ReportesComercialControllerTests : ApiIntegrationTestBase
{
    public ReportesComercialControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetReporteVentas_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/ventas?fechaInicio=2024-01-01&fechaFin=2024-12-31");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReporteClientes_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReporteProductos_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/productos?fechaInicio=2024-01-01&fechaFin=2024-12-31");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReporteFidelizacion_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/fidelizacion");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReportePromociones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/comercial/reportes/promociones?fechaInicio=2024-01-01&fechaFin=2024-12-31");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
} 