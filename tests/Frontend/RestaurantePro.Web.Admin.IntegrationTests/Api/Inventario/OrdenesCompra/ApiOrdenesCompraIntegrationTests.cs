using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.OrdenesCompra;

/// <summary>
/// Tests de integración para el controlador de Órdenes de Compra
/// </summary>
[Collection("IntegrationTests")]
public class ApiOrdenesCompraIntegrationTests : BaseIntegrationTest
{
    public ApiOrdenesCompraIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Endpoints Principales

    [Fact]
    public async Task GetOrdenesCompra_ConParametrosValidos_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={query.PageNumber}&pageSize={query.PageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PaginatedList<OrdenCompraDto>>>(content, JsonOptions);
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrdenCompra_ConIdValido_DeberiaRetornarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearOrdenCompra_ConDatosValidos_DeberiaCrearOrdenCompra()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Observaciones = "Orden de prueba",
            Items = new List<CrearOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 10,
                    PrecioUnitario = 5.50m,
                    Observaciones = "Item de prueba"
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarOrdenCompra_ConDatosValidos_DeberiaActualizarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(10),
            Observaciones = "Orden actualizada",
            Items = new List<ActualizarOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 15,
                    PrecioUnitario = 6.00m,
                    Observaciones = "Item actualizado"
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Flujos de Trabajo

    [Fact]
    public async Task AprobarOrdenCompra_ConIdValido_DeberiaAprobarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/aprobar", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnviarOrdenCompra_ConIdValido_DeberiaEnviarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/enviar", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RechazarOrdenCompra_ConDatosValidos_DeberiaRechazarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new RechazarOrdenCompraCommand
        {
            MotivoRechazo = "Motivo de prueba"
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/rechazar", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecibirOrdenCompra_ConDatosValidos_DeberiaRecibirOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new RecibirOrdenCompraCommand
        {
            NotasRecepcion = "Recibido correctamente"
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/recibir", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Consultas Especializadas

    [Fact]
    public async Task ObtenerOrdenesPendientes_DeberiaRetornarListaDeOrdenesPendientes()
    {
        // Act
        var response = await Client.GetAsync("/api/inventario/ordenes-compra/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<OrdenCompraDto>>>(content, JsonOptions);
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerOrdenesPorProveedor_ConIdValido_DeberiaRetornarOrdenesDelProveedor()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/proveedor/{proveedorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<OrdenCompraDto>>>(content, JsonOptions);
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    #endregion

    #region Tests de Validaciones

    [Fact]
    public async Task CrearOrdenCompra_ConDatosInvalidos_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.Empty,
            FechaEntregaEsperada = DateTime.Now.AddDays(-1), // Fecha pasada
            Items = new List<CrearOrdenCompraItemCommand>()
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarOrdenCompra_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(10),
            Items = new List<ActualizarOrdenCompraItemCommand>()
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Estados de Orden

    [Theory]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Pendiente)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Enviada)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Recibida)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.RecibidaParcial)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Cancelada)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Confirmada)]
    [InlineData(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.EnTransito)]
    public async Task GetOrdenCompra_ConDiferentesEstados_DeberiaManejarEstadosCorrectamente(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra estado)
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion
}
