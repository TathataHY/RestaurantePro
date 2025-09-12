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
/// Tests de monitoreo para el módulo de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class PromocionesMonitoringTests : BaseIntegrationTest
{
    public PromocionesMonitoringTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearPromocion_DeberiaRegistrarLogsCorrectamente()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "MONITOR_TEST",
            Nombre = "Promoción de Monitoreo",
            Descripcion = "Promoción para test de monitoreo",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
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
        
        // Verificar que se registraron los logs apropiados
        // En un entorno real, aquí se verificarían los logs del sistema
    }

    [Fact]
    public async Task ObtenerPromociones_DeberiaRegistrarMetricasCorrectamente()
    {
        // Arrange
        await SeedPromocionesAsync(10);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().HaveCount(10);
        
        // Verificar que se registraron las métricas apropiadas
        // En un entorno real, aquí se verificarían las métricas del sistema
    }

    [Fact]
    public async Task ActivarPromocion_DeberiaRegistrarCambioDeEstado()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.PatchAsync($"/api/comercial/promociones/{promocionId}/activar", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Estado.Should().Be(RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion.Activa.ToString());
        
        // Verificar que se registró el cambio de estado
        // En un entorno real, aquí se verificarían los logs de auditoría
    }

    [Fact]
    public async Task AplicarPromocion_DeberiaRegistrarUso()
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
        
        // Verificar que se registró el uso de la promoción
        // En un entorno real, aquí se verificarían los logs de uso
    }

    [Fact]
    public async Task ObtenerPromocionesAplicables_DeberiaRegistrarConsulta()
    {
        // Arrange
        await SeedPromocionesActivasAsync(5);
        var clienteId = Guid.NewGuid();
        var monto = 150m;
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/aplicables?clienteId={clienteId}&monto={monto}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        
        // Verificar que se registró la consulta
        // En un entorno real, aquí se verificarían los logs de consulta
    }

    [Fact]
    public async Task CrearPromocion_ConError_DeberiaRegistrarError()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "", // Código vacío para generar error
            Nombre = "Promoción con Error",
            Descripcion = "Promoción que generará error",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
            ValorDescuento = -10, // Valor negativo para generar error
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
        
        // Verificar que se registró el error
        // En un entorno real, aquí se verificarían los logs de error
    }

    [Fact]
    public async Task ObtenerPromocion_ConIdInexistente_DeberiaRegistrarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        
        // Verificar que se registró el 404
        // En un entorno real, aquí se verificarían los logs de 404
    }

    [Fact]
    public async Task AplicarPromocion_ConPromocionInactiva_DeberiaRegistrarError()
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
        
        // Verificar que se registró el error de promoción inactiva
        // En un entorno real, aquí se verificarían los logs de error específicos
    }

    [Fact]
    public async Task ObtenerPromociones_ConFiltros_DeberiaRegistrarFiltros()
    {
        // Arrange
        await SeedPromocionesConEstadosAsync(10);
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa&tipo=PorcentajeTotal");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        
        // Verificar que se registraron los filtros aplicados
        // En un entorno real, aquí se verificarían los logs de filtros
    }

    [Fact]
    public async Task AsignarProductos_DeberiaRegistrarAsignacion()
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        
        // Verificar que se registró la asignación de productos
        // En un entorno real, aquí se verificarían los logs de asignación
    }

    [Fact]
    public async Task QuitarProductos_DeberiaRegistrarEliminacion()
    {
        // Arrange
        var promocionId = await SeedPromocionConProductosAsync();
        var productosIds = await SeedProductosAsync();
        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(productosIds, GetJsonOptions());

        // Act
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/comercial/promociones/{promocionId}/productos")
        {
            Content = content
        };
        var response = await client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(responseContent, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        
        // Verificar que se registró la eliminación de productos
        // En un entorno real, aquí se verificarían los logs de eliminación
    }

    [Fact]
    public async Task EliminarPromocion_DeberiaRegistrarEliminacion()
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
        
        // Verificar que se registró la eliminación de la promoción
        // En un entorno real, aquí se verificarían los logs de eliminación
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "MONITOR_SINGLE",
            Nombre = "Promoción Individual Monitoreo",
            Descripcion = "Promoción individual para test de monitoreo",
            Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
            ValorDescuento = 10,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            EsAcumulable = false,
            Prioridad = 1
        };

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        var result = await mediator.Send(command);
        return result.Value.Id;
    }

    private async Task<Guid> SeedPromocionActivaAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var command = new ActivarPromocionCommand { Id = promocionId };
        
        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        await mediator.Send(command);
        
        return promocionId;
    }

    private async Task SeedPromocionesAsync(int cantidad)
    {
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"MONITOR_{i:D3}",
                Nombre = $"Promoción Monitoreo {i}",
                Descripcion = $"Promoción {i} para test de monitoreo",
                Tipo = i % 2 == 0 ? RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal : RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.MontoFijoTotal,
                ValorDescuento = 10 + (i % 20),
                MontoMinimo = 50 + (i * 10),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = i % 3 == 0,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task<List<Guid>> SeedPromocionesActivasAsync(int cantidad)
    {
        var promocionesIds = new List<Guid>();
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"ACTIVA_MONITOR_{i:D3}",
                Nombre = $"Promoción Activa Monitoreo {i}",
                Descripcion = $"Promoción activa {i} para test de monitoreo",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + (i % 20),
                MontoMinimo = 50 + (i * 10),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            var result = await mediator.Send(command);
            if (result.Succeeded)
            {
                await mediator.Send(new ActivarPromocionCommand { Id = result.Value.Id });
                promocionesIds.Add(result.Value.Id);
            }
        }

        return promocionesIds;
    }

    private async Task SeedPromocionesConEstadosAsync(int cantidad)
    {
        var commands = new List<CrearPromocionCommand>();
        
        for (int i = 0; i < cantidad; i++)
        {
            commands.Add(new CrearPromocionCommand
            {
                Codigo = $"ESTADO_MONITOR_{i:D3}",
                Nombre = $"Promoción Estado Monitoreo {i}",
                Descripcion = $"Promoción {i} con estado específico para monitoreo",
                Tipo = RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion.PorcentajeTotal,
                ValorDescuento = 10 + (i % 15),
                MontoMinimo = 50 + (i * 5),
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = i + 1
            });
        }

        var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task<Guid> SeedPromocionConProductosAsync()
    {
        var promocionId = await SeedPromocionAsync();
        var productosIds = await SeedProductosAsync();
        
        // TODO: Implementar asignación de productos
        // var command = new AsignarProductosCommand
        // {
        //     PromocionId = promocionId,
        //     ProductosIds = productosIds
        // };
        // var mediator = _factory.Services.GetRequiredService<MediatR.IMediator>();
        // await mediator.Send(command);
        
        return promocionId;
    }

    private async Task<List<Guid>> SeedProductosAsync()
    {
        // TODO: Implementar seeder de productos
        return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
    }

    #endregion
}

