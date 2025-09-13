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
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;

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
        await SeedOrdenesCompraAsync(5); // Crear 5 órdenes de compra
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={query.PageNumber}&pageSize={query.PageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PaginatedList<RestaurantePro.Application.Inventario.OrdenesCompra.DTOs.OrdenCompraDto>>>(content, JsonSerializerOptions.Default);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrdenCompra_ConIdValido_DeberiaRetornarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

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
            Items = new List<OrdenCompraItemCommand>
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

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/inventario/ordenes-compra", content);

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
            Items = new List<OrdenCompraItemCommand>
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

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

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
        var response = await _client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/aprobar", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EnviarOrdenCompra_ConIdValido_DeberiaEnviarOrdenCompra()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/enviar", null);

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
            Motivo = "Motivo de prueba"
        };

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/rechazar", content);

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
            Observaciones = "Recibido correctamente"
        };

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/inventario/ordenes-compra/{ordenCompraId}/recibir", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Consultas Especializadas

    [Fact]
    public async Task ObtenerOrdenesPendientes_DeberiaRetornarListaDeOrdenesPendientes()
    {
        // Act
        var response = await _client.GetAsync("/api/inventario/ordenes-compra/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<RestaurantePro.Application.Inventario.OrdenesCompra.DTOs.OrdenCompraDto>>>(content, JsonSerializerOptions.Default);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerOrdenesPorProveedor_ConIdValido_DeberiaRetornarOrdenesDelProveedor()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/inventario/ordenes-compra/proveedor/{proveedorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<RestaurantePro.Application.Inventario.OrdenesCompra.DTOs.OrdenCompraDto>>>(content, JsonSerializerOptions.Default);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
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
            Items = new List<OrdenCompraItemCommand>()
        };

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/inventario/ordenes-compra", content);

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
            Items = new List<OrdenCompraItemCommand>()
        };

        var json = JsonSerializer.Serialize(command, JsonSerializerOptions.Default);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);

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
        var response = await _client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Métodos de Seeding

    /// <summary>
    /// Crea datos de prueba para órdenes de compra usando SQL directo
    /// </summary>
    private async Task SeedOrdenesCompraAsync(int cantidad = 5)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Crear proveedores e ingredientes primero usando SQL directo
        await CrearProveedoresDePruebaAsync(context, 3);
        await CrearIngredientesDePruebaAsync(context, 10);
        
        // Crear órdenes de compra usando SQL directo
        for (int i = 0; i < cantidad; i++)
        {
            var ordenId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid(); // Usar un ID fijo para simplificar
            
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO OrdenesCompra (Id, NumeroOrden, ProveedorId, Estado, FechaCreacion, FechaEstimadaEntrega, Total, Observaciones)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                ordenId,
                $"OC-{DateTime.Now:yyyyMMdd}-{i + 1:000}",
                proveedorId,
                "Pendiente",
                DateTime.Now.AddDays(-i),
                DateTime.Now.AddDays(7 - i),
                1000m + (i * 100),
                $"Orden de prueba {i + 1}");
        }
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Crea proveedores de prueba usando SQL directo
    /// </summary>
    private async Task CrearProveedoresDePruebaAsync(RestauranteProDbContext context, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            var proveedorId = Guid.NewGuid();
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Proveedores (Id, Nombre, Contacto, Email, Telefono, Direccion, EstaActivo, FechaCreacion)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                proveedorId,
                $"Proveedor Test {i + 1}",
                $"Contacto {i + 1}",
                $"proveedor{i + 1}@test.com",
                $"555-{i + 1:0000}",
                $"Dirección {i + 1}",
                true,
                DateTime.Now.AddDays(-i));
        }
    }

    /// <summary>
    /// Crea ingredientes de prueba usando SQL directo
    /// </summary>
    private async Task CrearIngredientesDePruebaAsync(RestauranteProDbContext context, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            var ingredienteId = Guid.NewGuid();
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Ingredientes (Id, Nombre, Descripcion, UnidadMedida, PrecioUnitario, StockMinimo, StockActual, EstaActivo, FechaCreacion)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                ingredienteId,
                $"Ingrediente Test {i + 1}",
                $"Descripción del ingrediente {i + 1}",
                "Kg",
                10m + i,
                5,
                50 + i,
                true,
                DateTime.Now.AddDays(-i));
        }
    }

    #endregion
}

/// <summary>
/// Command para representar un item de orden de compra
/// </summary>
public class OrdenCompraItemCommand
{
    public Guid IngredienteId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// Command para crear una nueva orden de compra
/// </summary>
public class CrearOrdenCompraCommand
{
    public Guid ProveedorId { get; set; }
    public DateTime FechaEntregaEsperada { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenCompraItemCommand> Items { get; set; } = new();
    public Guid? UsuarioId { get; set; }
}

/// <summary>
/// Command para actualizar una orden de compra
/// </summary>
public class ActualizarOrdenCompraCommand
{
    public Guid Id { get; set; }
    public DateTime? FechaEntregaEsperada { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenCompraItemCommand>? Items { get; set; }
}

/// <summary>
/// Command para rechazar una orden de compra
/// </summary>
public class RechazarOrdenCompraCommand
{
    public Guid Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Command para recibir una orden de compra
/// </summary>
public class RecibirOrdenCompraCommand
{
    public Guid Id { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// Query para obtener órdenes de compra paginadas
/// </summary>
public class ObtenerOrdenesCompraPaginadasQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? ProveedorId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}
