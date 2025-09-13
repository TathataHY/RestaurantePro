using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
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
/// Tests de integración para el controlador de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class ApiPromocionesIntegrationTests : BaseIntegrationTest
{
    public ApiPromocionesIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerPromociones_ConFiltros_DeberiaRetornarPromocionesFiltradas()
    {
        // Arrange
        await SeedPromocionesAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa&tipo=PorcentajeTotal");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<AppPromociones.PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().AllSatisfy(p => p.Estado.Should().Be(EstadoPromocion.Activa));
        responseData.Data.Should().AllSatisfy(p => p.Tipo.Should().Be(RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal));
    }

    [Fact]
    public async Task ObtenerPromocionPorId_ConIdValido_DeberiaRetornarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/{promocionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(promocionId);
    }

    [Fact]
    public async Task CrearPromocion_ConDatosValidos_DeberiaCrearPromocion()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "TEST20",
            Nombre = "Test Promoción",
            Descripcion = "Promoción de prueba",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
            ValorDescuento = 20,
            MontoMinimo = 100,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            MaximoUsos = 100,
            EsAcumulable = false,
            Prioridad = 1,
            Condiciones = "Válida para todos los productos"
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PostAsync("/api/comercial/promociones", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Codigo.Should().Be(command.Codigo);
        responseData.Data.Nombre.Should().Be(command.Nombre);
    }

    [Fact]
    public async Task ActualizarPromocion_ConDatosValidos_DeberiaActualizarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var command = new ActualizarPromocionCommand
        {
            Id = promocionId,
            Nombre = "Promoción Actualizada",
            Descripcion = "Descripción actualizada",
            ValorDescuento = 25,
            MontoMinimo = 150
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PutAsync($"/api/comercial/promociones/{promocionId}", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Nombre.Should().Be(command.Nombre);
        responseData.Data.ValorDescuento.Should().Be(command.ValorDescuento);
    }

    [Fact]
    public async Task ActivarPromocion_ConIdValido_DeberiaActivarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/activar", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Estado.Should().Be(EstadoPromocion.Activa);
    }

    [Fact]
    public async Task PausarPromocion_ConIdValido_DeberiaPausarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/pausar", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Estado.Should().Be(EstadoPromocion.Pausada);
    }

    [Fact]
    public async Task AplicarPromocion_ConDatosValidos_DeberiaAplicarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionActivaAsync();
        var clienteId = await SeedClienteAsync();
        var facturaId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var productosIds = await SeedProductosAsync();

        var request = new
        {
            PromocionId = promocionId,
            ClienteId = clienteId,
            FacturaId = facturaId,
            ComandaId = comandaId,
            ProductosIds = productosIds,
            MontoOriginal = 200m
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
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerPromocionesAplicables_ConClienteYMonto_DeberiaRetornarPromocionesValidas()
    {
        // Arrange
        await SeedPromocionesActivasAsync();
        var clienteId = await SeedClienteAsync();
        var monto = 150m;
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/aplicables?clienteId={clienteId}&monto={monto}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<AppPromociones.PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().AllSatisfy(p => p.Estado.Should().Be(EstadoPromocion.Activa));
        responseData.Data.Should().AllSatisfy(p => p.MontoMinimo.Should().BeLessOrEqualTo(monto));
    }

    [Fact]
    public async Task AsignarProductos_ConIdValido_DeberiaAsignarProductos()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var productosIds = await SeedProductosAsync();
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var response = await client.PostAsync($"/api/comercial/promociones/{promocionId}/productos", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.ProductosAplicablesIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task QuitarProductos_ConIdValido_DeberiaQuitarProductos()
    {
        // Arrange
        var promocionId = await SeedPromocionConProductosAsync();
        var productosIds = await SeedProductosAsync();
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/comercial/promociones/{promocionId}/productos")
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task EliminarPromocion_ConIdValido_DeberiaEliminarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.DeleteAsync($"/api/comercial/promociones/{promocionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<bool>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().BeTrue();
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionAsync()
    {
        var promocion = new
        {
            Codigo = "TEST10",
            Nombre = "Promoción Test",
            Descripcion = "Promoción de prueba",
            Tipo = "PorcentajeTotal",
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow.AddDays(10), // Fecha futura para pasar validación
            FechaFin = DateTime.UtcNow.AddDays(40),
            MaximoUsos = 50,
            EsAcumulable = false,
            Prioridad = 1,
            Condiciones = "Válida para todos los productos"
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(promocion, GetJsonOptions());
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("/api/comercial/promociones", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error creando promoción: {response.StatusCode} - {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppPromociones.PromocionDto>>(responseContent, GetJsonOptions());
        
        if (responseData?.Data == null)
        {
            throw new Exception($"Respuesta inválida: {responseContent}");
        }
        
        return responseData.Data.Id;
    }

    private async Task<Guid> SeedPromocionActivaAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var command = new ActivarPromocionCommand { Id = promocionId };
        
        var mediator = _factory.Services.GetRequiredService<IMediator>();
        await mediator.Send(command);
        
        return promocionId;
    }

    private async Task SeedPromocionesAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "ACTIVA20",
                Nombre = "Promoción Activa 20%",
                Descripcion = "Descuento del 20%",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 20,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "INACTIVA15",
                Nombre = "Promoción Inactiva 15%",
                Descripcion = "Descuento del 15%",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.MontoFijoTotal,
                ValorDescuento = 15,
                MontoMinimo = 80,
                FechaInicio = DateTime.UtcNow.AddDays(-10),
                FechaFin = DateTime.UtcNow.AddDays(-1),
                EsAcumulable = false,
                Prioridad = 2
            }
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task SeedPromocionesActivasAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "ACTIVA10",
                Nombre = "Promoción 10%",
                Descripcion = "Descuento del 10%",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "ACTIVA25",
                Nombre = "Promoción 25%",
                Descripcion = "Descuento del 25%",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.MontoFijoTotal,
                ValorDescuento = 25,
                MontoMinimo = 200,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 2
            }
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        foreach (var command in commands)
        {
            var result = await mediator.Send(command);
            if (result.Succeeded)
            {
                await mediator.Send(new ActivarPromocionCommand { Id = result.Value.Id });
            }
        }
    }

    private async Task<Guid> SeedPromocionConProductosAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var productosIds = await SeedProductosAsync();
        
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/comercial/promociones/{promocionId}/productos")
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return promocionId;
    }

    private async Task<Guid> SeedClienteAsync()
    {
        // TODO: Implementar seeder de clientes
        return Guid.NewGuid();
    }

    private async Task<List<Guid>> SeedProductosAsync()
    {
        // TODO: Implementar seeder de productos
        return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
    }

    #endregion
}

/// <summary>
/// Command para asignar productos a una promoción
/// </summary>
public class AsignarProductosCommand
{
    public Guid PromocionId { get; set; }
    public List<Guid> ProductosIds { get; set; } = new();
}
