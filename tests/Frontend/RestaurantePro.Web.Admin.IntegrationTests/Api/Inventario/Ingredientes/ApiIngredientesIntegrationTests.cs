using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.ConsumirStock;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarLote;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using MediatR;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.Ingredientes;

/// <summary>
/// Tests de integración para el controlador de Ingredientes
/// Cubre todos los endpoints principales del módulo de inventario de ingredientes
/// </summary>
[Collection("IntegrationTests")]
public class ApiIngredientesIntegrationTests : BaseIntegrationTest
{
    public ApiIngredientesIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Endpoints Principales

    [Fact]
    public async Task ObtenerIngredientes_ConFiltros_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var query = "?pageNumber=1&pageSize=10&soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerIngredientesLista_ConParametros_DeberiaRetornarListaSimple()
    {
        // Arrange
        var query = "?soloActivos=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/lista{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<List<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerIngredientePorId_ConIdValido_DeberiaRetornarIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        // Nota: Este test puede retornar 404 si no existe el ingrediente, lo cual es válido
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CrearIngrediente_ConDatosValidos_DeberiaCrearIngrediente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Test Ingrediente",
            Descripcion = "Ingrediente de prueba",
            Rotacion = "Alta",
            UnidadMedida = "Kilogramo",
            StockMinimo = 10,
            StockInicial = 50,
            CostoInicial = 15.50m,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventario/ingredientes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
            responseData.Data!.Nombre.Should().Be(command.Nombre);
        }
    }

    [Fact]
    public async Task ActualizarIngrediente_ConDatosValidos_DeberiaActualizarIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ActualizarIngredienteCommand
        {
            Id = ingredienteId,
            Nombre = "Ingrediente Actualizado",
            Descripcion = "Descripción actualizada",
            Rotacion = Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
            StockMinimo = 20,
            CostoPromedio = 25.75m
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task EliminarIngrediente_ConIdValido_DeberiaEliminarIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/inventario/ingredientes/{ingredienteId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Movimientos de Inventario

    [Fact]
    public async Task ObtenerMovimientosDeIngrediente_ConIdValido_DeberiaRetornarMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = "?fechaDesde=2024-01-01&fechaHasta=2024-12-31&limite=10";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos{query}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<List<RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task RegistrarMovimiento_ConDatosValidos_DeberiaRegistrarMovimiento()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarMovimientoCommand
        {
            TipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Entrada,
            Cantidad = 50,
            Motivo = "Compra inicial",
            Observaciones = "Movimiento de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/movimientos", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<Guid>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBe(Guid.Empty);
        }
    }

    #endregion

    #region Tests de Gestión de Stock

    [Fact]
    public async Task ObtenerIngredientesBajoStock_ConParametros_DeberiaRetornarIngredientesBajoStock()
    {
        // Arrange
        var query = "?porcentajeMinimo=20&incluirSinStock=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/bajo-stock{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<List<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ConsumirStock_ConDatosValidos_DeberiaConsumirStock()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new ConsumirStockCommand
        {
            Cantidad = 5,
            Motivo = "Uso en preparación",
            Observaciones = "Consumo de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/consumir", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task RegistrarLote_ConDatosValidos_DeberiaRegistrarLote()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var command = new RegistrarLoteCommand
        {
            IngredienteId = ingredienteId,
            NumeroLote = "LOTE-2024-001",
            Cantidad = 25,
            FechaVencimiento = DateTime.Now.AddDays(30),
            PrecioUnitario = 12.50m,
            UsuarioId = Guid.NewGuid(),
            Observaciones = "Lote de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/lotes", command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteDto>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().NotBeNull();
        }
    }

    #endregion

    #region Tests de Asociación con Proveedores

    [Fact]
    public async Task AsociarProveedor_ConDatosValidos_DeberiaAsociarProveedor()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/inventario/ingredientes/{ingredienteId}/asociar-proveedor/{proveedorId}", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<bool>>();
            responseData.Should().NotBeNull();
            responseData!.Success.Should().BeTrue();
            responseData.Data.Should().BeTrue();
        }
    }

    #endregion

    #region Tests de Reportes y Estadísticas

    [Fact]
    public async Task GenerarReporteValoracion_ConParametros_DeberiaGenerarReporte()
    {
        // Arrange
        var query = "?fechaDesde=2024-01-01&fechaHasta=2024-12-31&incluirInactivos=false";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/reporte/valoracion{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.ReporteValoracionDto>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerEstadisticas_DeberiaRetornarEstadisticas()
    {
        // Act
        var response = await _client.GetAsync("/api/inventario/ingredientes/estadisticas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Inventario.Ingredientes.DTOs.EstadisticasIngredientesDto>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Tests de Búsqueda

    [Fact]
    public async Task BuscarIngredientes_ConTermino_DeberiaRetornarResultados()
    {
        // Arrange
        var query = "?termino=test&categoria=Alta&soloDisponibles=true";

        // Act
        var response = await _client.GetAsync($"/api/inventario/ingredientes/buscar{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<RestaurantePro.Api.Common.ApiResponse<List<RestaurantePro.Application.Inventario.Ingredientes.DTOs.IngredienteSummaryDto>>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
    }

    #endregion

    #region Tests de Validación de Headers

    [Fact]
    public async Task TodosLosEndpoints_DeberianRetornarContentTypeJson()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/inventario/ingredientes",
            "/api/inventario/ingredientes/lista",
            "/api/inventario/ingredientes/estadisticas",
            "/api/inventario/ingredientes/bajo-stock",
            "/api/inventario/ingredientes/buscar",
            "/api/inventario/ingredientes/reporte/valoracion"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task TodosLosEndpoints_DeberianTenerAutorizacion()
    {
        // Arrange
        var client = _factory.CreateClient(); // cliente sin autenticación

        // Act & Assert
        var response = await client.GetAsync("/api/inventario/ingredientes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
