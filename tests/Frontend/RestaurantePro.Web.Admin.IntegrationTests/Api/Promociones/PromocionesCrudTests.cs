using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;
using RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System.Text.Json;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Promociones;

/// <summary>
/// Tests CRUD para el módulo de Promociones
/// </summary>
[Collection("IntegrationTests")]
public class PromocionesCrudTests : BaseIntegrationTest
{
    public PromocionesCrudTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CrearPromocion_ConDatosCompletos_DeberiaCrearPromocionExitosamente()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "CRUD20",
            Nombre = "Promoción CRUD Test",
            Descripcion = "Promoción para tests CRUD",
            Tipo = TipoPromocion.DescuentoPorcentaje,
            ValorDescuento = 20,
            MontoMinimo = 100,
            PuntosRequeridos = 0,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(30),
            MaximoUsos = 100,
            EsAcumulable = false,
            DiasValidos = 30,
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
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Codigo.Should().Be(command.Codigo);
        responseData.Data.Nombre.Should().Be(command.Nombre);
        responseData.Data.Descripcion.Should().Be(command.Descripcion);
        responseData.Data.Tipo.Should().Be(command.Tipo);
        responseData.Data.ValorDescuento.Should().Be(command.ValorDescuento);
        responseData.Data.MontoMinimo.Should().Be(command.MontoMinimo);
        responseData.Data.Estado.Should().Be(EstadoPromocion.Creada);
        responseData.Data.EsAcumulable.Should().Be(command.EsAcumulable);
        responseData.Data.Prioridad.Should().Be(command.Prioridad);
    }

    [Fact]
    public async Task CrearPromocion_ConDescuentoFijo_DeberiaCrearPromocionConTipoCorrecto()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "FIXED15",
            Nombre = "Descuento Fijo $15",
            Descripcion = "Descuento fijo de $15",
            Tipo = TipoPromocion.DescuentoFijo,
            ValorDescuento = 15,
            MontoMinimo = 50,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(15),
            EsAcumulable = true,
            Prioridad = 2
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
        responseData.Data.Tipo.Should().Be(TipoPromocion.DescuentoFijo);
        responseData.Data.ValorDescuento.Should().Be(15);
        responseData.Data.EsAcumulable.Should().BeTrue();
    }

    [Fact]
    public async Task CrearPromocion_ConPromocionPuntos_DeberiaCrearPromocionConPuntos()
    {
        // Arrange
        var command = new CrearPromocionCommand
        {
            Codigo = "PUNTOS100",
            Nombre = "Promoción por Puntos",
            Descripcion = "Descuento por puntos de fidelización",
            Tipo = TipoPromocion.DescuentoPorPuntos,
            ValorDescuento = 10,
            MontoMinimo = 200,
            PuntosRequeridos = 100,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(60),
            EsAcumulable = false,
            Prioridad = 3
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
        responseData.Data.Tipo.Should().Be(TipoPromocion.DescuentoPorPuntos);
        responseData.Data.PuntosRequeridos.Should().Be(100);
    }

    [Fact]
    public async Task ObtenerPromocion_ConIdExistente_DeberiaRetornarPromocion()
    {
        // Arrange
        var promocionId = await SeedPromocionAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/{promocionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(promocionId);
        responseData.Data.Codigo.Should().NotBeNullOrEmpty();
        responseData.Data.Nombre.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ObtenerPromocion_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/comercial/promociones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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
            MontoMinimo = 150,
            Condiciones = "Nuevas condiciones"
        };

        var client = CreateAuthenticatedClient();
        var json = JsonSerializer.Serialize(command, GetJsonOptions());

        // Act
        var response = await client.PutAsync($"/api/comercial/promociones/{promocionId}", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<PromocionDto>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Nombre.Should().Be(command.Nombre);
        responseData.Data.Descripcion.Should().Be(command.Descripcion);
        responseData.Data.ValorDescuento.Should().Be(command.ValorDescuento);
        responseData.Data.MontoMinimo.Should().Be(command.MontoMinimo);
        responseData.Data.Condiciones.Should().Be(command.Condiciones);
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
    public async Task EliminarPromocion_ConIdExistente_DeberiaEliminarPromocion()
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

    [Fact]
    public async Task ObtenerPromociones_ConFiltroPorEstado_DeberiaRetornarPromocionesFiltradas()
    {
        // Arrange
        await SeedPromocionesConEstadosAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().AllSatisfy(p => p.Estado.Should().Be(EstadoPromocion.Activa));
    }

    [Fact]
    public async Task ObtenerPromociones_ConFiltroPorTipo_DeberiaRetornarPromocionesFiltradas()
    {
        // Arrange
        await SeedPromocionesConTiposAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?tipo=DescuentoPorcentaje");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().AllSatisfy(p => p.Tipo.Should().Be(TipoPromocion.DescuentoPorcentaje));
    }

    [Fact]
    public async Task ObtenerPromociones_ConFiltrosCombinados_DeberiaRetornarPromocionesFiltradas()
    {
        // Arrange
        await SeedPromocionesConEstadosYTiposAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones?estado=Activa&tipo=DescuentoFijo");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().AllSatisfy(p => p.Estado.Should().Be(EstadoPromocion.Activa));
        responseData.Data.Should().AllSatisfy(p => p.Tipo.Should().Be(TipoPromocion.DescuentoFijo));
    }

    [Fact]
    public async Task ObtenerPromociones_SinFiltros_DeberiaRetornarTodasLasPromociones()
    {
        // Arrange
        await SeedPromocionesAsync();
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/comercial/promociones");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<PromocionDto>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeEmpty();
        responseData.Data.Should().HaveCountGreaterThan(0);
    }

    #region Helper Methods

    private async Task<Guid> SeedPromocionAsync()
    {
        var command = new CrearPromocionCommand
        {
            Codigo = "CRUDTEST",
            Nombre = "Promoción CRUD",
            Descripcion = "Promoción para tests CRUD",
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

    private async Task SeedPromocionesAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "TEST1",
                Nombre = "Promoción Test 1",
                Descripcion = "Primera promoción de prueba",
                Tipo = TipoPromocion.DescuentoPorcentaje,
                ValorDescuento = 10,
                MontoMinimo = 50,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "TEST2",
                Nombre = "Promoción Test 2",
                Descripcion = "Segunda promoción de prueba",
                Tipo = TipoPromocion.DescuentoFijo,
                ValorDescuento = 15,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = true,
                Prioridad = 2
            }
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task SeedPromocionesConEstadosAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "ACTIVA1",
                Nombre = "Promoción Activa 1",
                Descripcion = "Promoción activa",
                Tipo = TipoPromocion.DescuentoPorcentaje,
                ValorDescuento = 20,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "INACTIVA1",
                Nombre = "Promoción Inactiva 1",
                Descripcion = "Promoción inactiva",
                Tipo = TipoPromocion.DescuentoFijo,
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

    private async Task SeedPromocionesConTiposAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "PORCENTAJE1",
                Nombre = "Descuento Porcentaje 1",
                Descripcion = "Descuento por porcentaje",
                Tipo = TipoPromocion.DescuentoPorcentaje,
                ValorDescuento = 20,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "FIJO1",
                Nombre = "Descuento Fijo 1",
                Descripcion = "Descuento fijo",
                Tipo = TipoPromocion.DescuentoFijo,
                ValorDescuento = 25,
                MontoMinimo = 150,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = true,
                Prioridad = 2
            }
        };

        var mediator = _factory.Services.GetRequiredService<IMediator>();
        foreach (var command in commands)
        {
            await mediator.Send(command);
        }
    }

    private async Task SeedPromocionesConEstadosYTiposAsync()
    {
        var commands = new[]
        {
            new CrearPromocionCommand
            {
                Codigo = "ACTIVA_FIJO1",
                Nombre = "Activa Fijo 1",
                Descripcion = "Promoción activa con descuento fijo",
                Tipo = TipoPromocion.DescuentoFijo,
                ValorDescuento = 20,
                MontoMinimo = 100,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(30),
                EsAcumulable = false,
                Prioridad = 1
            },
            new CrearPromocionCommand
            {
                Codigo = "INACTIVA_PORCENTAJE1",
                Nombre = "Inactiva Porcentaje 1",
                Descripcion = "Promoción inactiva con descuento porcentaje",
                Tipo = TipoPromocion.DescuentoPorcentaje,
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

    #endregion
}
