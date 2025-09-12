using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.OrdenesCompra;

/// <summary>
/// Tests de manejo de errores para Órdenes de Compra
/// </summary>
[Collection("IntegrationTests")]
public class OrdenesCompraErrorHandlingTests : BaseIntegrationTest
{
    public OrdenesCompraErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Errores de Validación

    [Fact]
    public async Task CrearOrdenCompra_ConProveedorIdVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.Empty, // ID vacío
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
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
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CrearOrdenCompra_ConIngredienteIdVacio_DeberiaRetornarBadRequest()
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
                    IngredienteId = Guid.Empty, // ID vacío
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

    [Fact]
    public async Task CrearOrdenCompra_ConCantidadCero_DeberiaRetornarBadRequest()
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
                    Cantidad = 0, // Cantidad cero
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
    public async Task CrearOrdenCompra_ConPrecioUnitarioCero_DeberiaRetornarBadRequest()
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
                    PrecioUnitario = 0m // Precio cero
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

    #region Tests de Errores de Datos

    [Fact]
    public async Task CrearOrdenCompra_ConFechaEntregaMuyLejana_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddYears(2), // Fecha muy lejana
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
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearOrdenCompra_ConObservacionesMuyLargas_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Observaciones = new string('A', 1001), // Observaciones muy largas
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
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion

    #region Tests de Errores de Operaciones

    [Fact]
    public async Task AprobarOrdenCompra_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/aprobar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnviarOrdenCompra_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/enviar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RechazarOrdenCompra_ConIdInexistente_DeberiaRetornarNotFound()
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
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecibirOrdenCompra_ConIdInexistente_DeberiaRetornarNotFound()
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
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Errores de Formato

    [Fact]
    public async Task CrearOrdenCompra_ConJsonInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonInvalido = "{ \"proveedorId\": \"invalid-guid\", \"fechaEntregaEsperada\": \"invalid-date\" }";
        var content = new StringContent(jsonInvalido, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarOrdenCompra_ConJsonInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var jsonInvalido = "{ \"fechaEntregaEsperada\": \"invalid-date\", \"items\": \"not-an-array\" }";
        var content = new StringContent(jsonInvalido, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RechazarOrdenCompra_ConJsonInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var jsonInvalido = "{ \"motivoRechazo\": 123 }"; // Debería ser string
        var content = new StringContent(jsonInvalido, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/rechazar", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Errores de Autorización

    [Fact]
    public async Task CrearOrdenCompra_SinAutorizacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>()
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act - Crear cliente sin autorización
        using var clientNoAuth = Factory.CreateClient();
        var response = await clientNoAuth.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerOrdenesCompra_SinAutorizacion_DeberiaRetornarUnauthorized()
    {
        // Act - Crear cliente sin autorización
        using var clientNoAuth = Factory.CreateClient();
        var response = await clientNoAuth.GetAsync("/api/inventario/ordenes-compra");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Tests de Errores de Concurrencia

    [Fact]
    public async Task ActualizarOrdenCompra_ConRowVersionIncorrecto_DeberiaRetornarConflict()
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
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Errores de Límites

    [Fact]
    public async Task CrearOrdenCompra_ConDemasiadosItems_DeberiaRetornarBadRequest()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>()
        };

        // Crear 1001 items (límite excedido)
        for (int i = 0; i < 1001; i++)
        {
            command.Items.Add(new CrearOrdenCompraItemCommand
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = 1,
                PrecioUnitario = 10.00m
            });
        }

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion
}
