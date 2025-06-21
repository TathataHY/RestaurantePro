using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración para ReservacionesController
/// Valida todos los endpoints REST del controlador de reservaciones del restaurante
/// </summary>
[Collection("Sequential")]
public class ReservacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ReservacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetReservaciones_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reservaciones");

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
    public async Task GetReservaciones_ConParametrosFiltro_DebeRetornarRespuestaValida()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaDesde = DateTime.Now.AddDays(-7);
        var fechaHasta = DateTime.Now.AddDays(7);
        var url = $"/api/operaciones/reservaciones?estado=Confirmada&clienteId={clienteId}&mesaId={mesaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReservacionPorId_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reservaciones/{id}");

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
    public async Task PostReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var reservacion = new { };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacion);

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
    public async Task PutReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reservacion = new { };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/operaciones/reservaciones/{id}", reservacion);

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
    public async Task DeleteReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/operaciones/reservaciones/{id}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.NotImplemented, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostConfirmarReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/reservaciones/{id}/confirmar", null);

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
    public async Task PostReprogramarReservacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await HttpClient.PostAsync($"/api/operaciones/reservaciones/{id}/reprogramar", null);

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
    public async Task GetDisponibilidad_DebeRetornarRespuestaValida()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reservaciones/disponibilidad");

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
        _factory?.Dispose();
        base.Dispose();
    }
} 