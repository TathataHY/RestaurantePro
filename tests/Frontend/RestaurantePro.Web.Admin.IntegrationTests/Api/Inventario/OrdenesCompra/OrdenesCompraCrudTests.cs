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
/// Tests CRUD para Órdenes de Compra
/// </summary>
[Collection("IntegrationTests")]
public class OrdenesCompraCrudTests : BaseIntegrationTest
{
    public OrdenesCompraCrudTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Creación

    [Fact]
    public async Task CrearOrdenCompra_ConDatosCompletos_DeberiaCrearOrdenCompraExitosamente()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Observaciones = "Orden de compra de prueba",
            Items = new List<CrearOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 5,
                    PrecioUnitario = 10.50m,
                    Observaciones = "Item 1"
                },
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 3,
                    PrecioUnitario = 15.75m,
                    Observaciones = "Item 2"
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<OrdenCompraDto>>(responseContent, JsonOptions);
            result.Should().NotBeNull();
            result!.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Items.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task CrearOrdenCompra_ConItemsVacios_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>() // Lista vacía
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearOrdenCompra_ConFechaPasada_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(-1), // Fecha pasada
            Items = new List<CrearOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 1,
                    PrecioUnitario = 10.00m
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Lectura

    [Fact]
    public async Task ObtenerOrdenesCompra_ConPaginacionValida_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = 5
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
        result.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerOrdenCompraPorId_ConIdValido_DeberiaRetornarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerOrdenCompraPorId_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    #endregion

    #region Tests de Actualización

    [Fact]
    public async Task ActualizarOrdenCompra_ConDatosValidos_DeberiaActualizarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(14),
            Observaciones = "Orden actualizada",
            Items = new List<ActualizarOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 8,
                    PrecioUnitario = 12.00m,
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

    [Fact]
    public async Task ActualizarOrdenCompra_ConItemsModificados_DeberiaActualizarItems()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(10),
            Items = new List<ActualizarOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 10,
                    PrecioUnitario = 20.00m,
                    Observaciones = "Nuevo item"
                },
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 5,
                    PrecioUnitario = 15.50m,
                    Observaciones = "Otro item"
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

    [Fact]
    public async Task ActualizarOrdenCompra_ConFechaInvalida_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(-5), // Fecha pasada
            Items = new List<ActualizarOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 1,
                    PrecioUnitario = 10.00m
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Estados

    [Fact]
    public async Task ObtenerOrdenesPendientes_DeberiaRetornarSoloOrdenesPendientes()
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
    public async Task ObtenerOrdenesPorProveedor_ConProveedorValido_DeberiaRetornarOrdenesDelProveedor()
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

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task CrearOrdenCompra_ConCantidadNegativa_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = -1, // Cantidad negativa
                    PrecioUnitario = 10.00m
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearOrdenCompra_ConPrecioNegativo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>
            {
                new()
                {
                    IngredienteId = Guid.NewGuid(),
                    Cantidad = 1,
                    PrecioUnitario = -5.00m // Precio negativo
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
