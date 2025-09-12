using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System.Text.Json;
using Xunit;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using MediatR;
using AppPromociones = RestaurantePro.Application.Comercial.Promociones.DTOs;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Promociones;

/// <summary>
/// Tests de manejo de errores para el módulo de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class PromocionesErrorHandlingTests : BaseIntegrationTest
{
    public PromocionesErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearPromocion_ConCodigoDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var codigoExistente = "DUPLICADO123";
        await SeedPromocionConCodigoAsync(codigoExistente);

        var command = new CrearPromocionCommand
        {
            Codigo = codigoExistente, // Mismo código
            Nombre = "Promoción Duplicada",
            Descripcion = "Promoción con código duplicado",
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
        responseData.Errors.Should().Contain(e => e.Contains("código") || e.Contains("duplicado"));
    }

    [Fact]
    public async Task CrearPromocion_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "", // Código vacío
            Nombre = "", // Nombre vacío
            Descripcion = "Promoción con datos inválidos",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = -10, // Valor negativo
            MontoMinimo = -50, // Monto negativo
            FechaInicio = DateTime.UtcNow.AddDays(10), // Fecha inicio futura
            FechaFin = DateTime.UtcNow, // Fecha fin anterior a inicio
            EsAcumulable = false,
            Prioridad = -1 // Prioridad negativa
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
    public async Task CrearPromocion_ConFechasInvalidas_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "FECHAS_INVALIDAS",
            Nombre = "Promoción con Fechas Inválidas",
            Descripcion = "Promoción con fechas incorrectas",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow.AddDays(10), // Fecha inicio futura
            FechaFin = DateTime.UtcNow, // Fecha fin anterior a inicio
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
        responseData.Errors.Should().Contain(e => e.Contains("fecha") || e.Contains("inicio") || e.Contains("fin"));
    }

    [Fact]
    public async Task CrearPromocion_ConValoresNegativos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "VALORES_NEGATIVOS",
            Nombre = "Promoción con Valores Negativos",
            Descripcion = "Promoción con valores negativos",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = -20, // Valor negativo
            MontoMinimo = -100, // Monto negativo
            PuntosRequeridos = -50, // Puntos negativos
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = -5 // Prioridad negativa
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
    public async Task ActualizarPromocion_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var command = new ActualizarPromocionCommand
        {
            Id = idInexistente,
            Nombre = "Promoción Inexistente",
            Descripcion = "Esta promoción no existe"
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PutAsync($"/api/comercial/promociones/{idInexistente}", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarPromocion_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var command = new ActualizarPromocionCommand
        {
            Id = promocionId,
            Nombre = "", // Nombre vacío
            Descripcion = "Descripción actualizada",
            ValorDescuento = -15, // Valor negativo
            MontoMinimo = -75 // Monto negativo
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PutAsync($"/api/comercial/promociones/{promocionId}", 
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
    public async Task ActivarPromocion_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{idInexistente}/activar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActivarPromocion_ConPromocionYaActiva_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/activar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PausarPromocion_ConIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{idInexistente}/pausar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PausarPromocion_ConPromocionNoActiva_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync(); // Promoción creada pero no activa
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/pausar", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AplicarPromocion_ConPromocionInactiva_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync(); // Promoción creada pero no activa
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 100m
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

    [Fact]
    public async Task AplicarPromocion_ConPromocionExpirada_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionExpiradaAsync();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 100m
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

    [Fact]
    public async Task AplicarPromocion_ConMontoInsuficiente_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 10m // Monto muy bajo
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

    [Fact]
    public async Task AplicarPromocion_ConPromocionAgotada_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = await SeedPromocionAgotadaAsync();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = new List<Guid> { Guid.NewGuid() },
            MontoOriginal = 200m
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

    [Fact]
    public async Task AsignarProductos_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid() };
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var response = await client.PostAsync($"/api/comercial/promociones/{idInexistente}/productos", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task QuitarProductos_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid() };
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/comercial/promociones/{idInexistente}/productos")
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EliminarPromocion_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.DeleteAsync($"/api/comercial/promociones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "ERROR_TEST",
            Nombre = "Promoción Error Test",
            Descripcion = "Promoción para tests de error",
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
        return result.Value.Id;
    }

    private async Task<Guid> SeedPromocionConCodigoAsync(string codigo)
    {
        var command = new CrearPromocionCommand
        {
            Codigo = codigo,
            Nombre = "Promoción Existente",
            Descripcion = "Promoción con código existente",
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
        return result.Value.Id;
    }

    private async Task<Guid> SeedPromocionActivaAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var command = new ActivarPromocionCommand { Id = promocionId };
        
        var mediator = _factory.Services.GetRequiredService<IMediator>();
        await mediator.Send(command);
        
        return promocionId;
    }

    private async Task<Guid> SeedPromocionExpiradaAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "EXPIRADA",
            Nombre = "Promoción Expirada",
            Descripcion = "Promoción expirada",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow.AddDays(-30),
            FechaFin = DateTime.UtcNow.AddDays(-1), // Expirada
            EsAcumulable = false,
            Prioridad = 1
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        var result = await mediator.Send(command);
        return result.Value.Id;
    }

    private async Task<Guid> SeedPromocionAgotadaAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "AGOTADA",
            Nombre = "Promoción Agotada",
            Descripcion = "Promoción agotada",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            MaximoUsos = 0, // Sin usos disponibles
            EsAcumulable = false,
            Prioridad = 1
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        var result = await mediator.Send(command);
        return result.Value.Id;
    }

    #endregion
}
