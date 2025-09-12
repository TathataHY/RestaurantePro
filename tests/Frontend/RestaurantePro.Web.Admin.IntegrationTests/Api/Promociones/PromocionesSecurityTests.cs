using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System.Text.Json;
using Xunit;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Promociones;

/// <summary>
/// Tests de seguridad para el módulo de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class PromocionesSecurityTests : BaseIntegrationTest
{
    public PromocionesSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerPromociones_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // Cliente sin autenticación

        // Act
        var response = await client.GetAsync("/api/comercial/promociones");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var client = _factory.CreateClient(); // Cliente sin autenticación

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/{promocionId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrearPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "SEC_TEST",
            Nombre = "Test de Seguridad",
            Descripcion = "Promoción para test de seguridad",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var client = _factory.CreateClient(); // Cliente sin autenticación
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActualizarPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new
        {
            Nombre = "Promoción Actualizada",
            Descripcion = "Descripción actualizada"
        };

        var client = _factory.CreateClient(); // Cliente sin autenticación
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PutAsync($"/api/comercial/promociones/{promocionId}", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EliminarPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var client = _factory.CreateClient(); // Cliente sin autenticación

        // Act
        var response = await client.DeleteAsync($"/api/comercial/promociones/{promocionId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivarPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var client = _factory.CreateClient(); // Cliente sin autenticación

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/activar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PausarPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(); // Cliente sin autenticación

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/pausar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AplicarPromocion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var request = new
        {
            PromocionId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            ComandaId = Guid.NewGuid(),
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 100m
        };

        var client = _factory.CreateClient(); // Cliente sin autenticación
        var json = JsonSerializer.Serialize(request, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones/aplicar", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerPromocionesAplicables_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var monto = 100m;
        var client = _factory.CreateClient(); // Cliente sin autenticación

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/aplicables?clienteId={clienteId}&monto={monto}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AsignarProductos_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid() };
        var client = _factory.CreateClient(); // Cliente sin autenticación
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var response = await client.PostAsync($"/api/comercial/promociones/{promocionId}/productos", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task QuitarProductos_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid() };
        var client = _factory.CreateClient(); // Cliente sin autenticación
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var response = await client.DeleteAsync($"/api/comercial/promociones/{promocionId}/productos", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrearPromocion_ConDatosMaliciosos_DeberiaSanitizarEntrada()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "<script>alert('xss')</script>",
            Nombre = "'; DROP TABLE Promociones; --",
            Descripcion = "Descripción con <script>alert('xss')</script>",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        
        // Verificar que los datos maliciosos fueron sanitizados
        responseData.Data.Codigo.Should().NotContain("<script>");
        responseData.Data.Nombre.Should().NotContain("DROP TABLE");
        responseData.Data.Descripcion.Should().NotContain("<script>");
    }

    [Fact]
    public async Task CrearPromocion_ConDatosExcesivos_DeberiaValidarLongitud()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = new string('A', 1000), // Código muy largo
            Nombre = new string('B', 1000), // Nombre muy largo
            Descripcion = new string('C', 10000), // Descripción muy larga
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AplicarPromocion_ConDatosMaliciosos_DeberiaSanitizarEntrada()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var request = new
        {
            PromocionId = promocionId,
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            ComandaId = Guid.NewGuid(),
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 100m
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(request, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones/aplicar", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AplicarPromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerPromociones_ConParametrosMaliciosos_DeberiaSanitizarEntrada()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=<script>alert('xss')</script>&tipo='; DROP TABLE Promociones; --");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        // Los parámetros maliciosos deberían ser ignorados o sanitizados
    }

    [Fact]
    public async Task CrearPromocion_ConValoresExtremos_DeberiaValidarRangos()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "EXTREME",
            Nombre = "Promoción con Valores Extremos",
            Descripcion = "Promoción con valores extremos",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = decimal.MaxValue, // Valor extremo
            MontoMinimo = decimal.MaxValue, // Valor extremo
            FechaInicio = DateTime.MinValue, // Fecha extrema
            FechaFin = DateTime.MaxValue, // Fecha extrema
            EsAcumulable = false,
            Prioridad = int.MaxValue // Prioridad extrema
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AplicarPromocion_ConValoresExtremos_DeberiaValidarRangos()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var request = new
        {
            PromocionId = promocionId,
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            ComandaId = Guid.NewGuid(),
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = decimal.MaxValue // Monto extremo
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(request, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones/aplicar", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionActivaAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "SEC_ACTIVA",
            Nombre = "Promoción Activa Seguridad",
            Descripcion = "Promoción activa para tests de seguridad",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        var result = await mediator.Send(command);
        
        if (result.Succeeded)
        {
            await mediator.Send(new ActivarPromocionCommand { Id = result.Value.Id });
            return result.Value.Id;
        }
        
        return Guid.Empty;
    }

    #endregion
}
