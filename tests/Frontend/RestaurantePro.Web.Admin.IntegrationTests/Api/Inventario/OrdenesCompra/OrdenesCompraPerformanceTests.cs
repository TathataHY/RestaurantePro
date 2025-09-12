using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Inventario.OrdenesCompra.Commands;
using RestaurantePro.Application.Inventario.OrdenesCompra.Queries;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Inventario.OrdenesCompra;

/// <summary>
/// Tests de rendimiento para Órdenes de Compra
/// </summary>
[Collection("IntegrationTests")]
public class OrdenesCompraPerformanceTests : BaseIntegrationTest
{
    public OrdenesCompraPerformanceTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Tiempo de Respuesta

    [Fact]
    public async Task ObtenerOrdenesCompra_ConPaginacion_DeberiaResponderEnMenosDe2Segundos()
    {
        // Arrange
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = 50
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "La consulta debería responder en menos de 2 segundos");
    }

    [Fact]
    public async Task ObtenerOrdenCompraPorId_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/{ordenCompraId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta por ID debería responder en menos de 1 segundo");
    }

    [Fact]
    public async Task ObtenerOrdenesPendientes_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/inventario/ordenes-compra/pendientes");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta de órdenes pendientes debería responder en menos de 1 segundo");
    }

    [Fact]
    public async Task ObtenerOrdenesPorProveedor_DeberiaResponderEnMenosDe1Segundo()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra/proveedor/{proveedorId}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta por proveedor debería responder en menos de 1 segundo");
    }

    #endregion

    #region Tests de Carga

    [Fact]
    public async Task CrearOrdenCompra_ConMultiplesItems_DeberiaResponderEnMenosDe3Segundos()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>()
        };

        // Crear 100 items para probar rendimiento
        for (int i = 0; i < 100; i++)
        {
            command.Items.Add(new CrearOrdenCompraItemCommand
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = i + 1,
                PrecioUnitario = 10.00m + i,
                Observaciones = $"Item {i + 1}"
            });
        }

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "La creación con múltiples items debería completarse en menos de 3 segundos");
    }

    [Fact]
    public async Task ActualizarOrdenCompra_ConMultiplesItems_DeberiaResponderEnMenosDe3Segundos()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var command = new ActualizarOrdenCompraCommand
        {
            FechaEntregaEsperada = DateTime.Now.AddDays(10),
            Items = new List<ActualizarOrdenCompraItemCommand>()
        };

        // Crear 50 items para probar rendimiento
        for (int i = 0; i < 50; i++)
        {
            command.Items.Add(new ActualizarOrdenCompraItemCommand
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = i + 1,
                PrecioUnitario = 15.00m + i,
                Observaciones = $"Item actualizado {i + 1}"
            });
        }

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.PutAsync($"/api/inventario/ordenes-compra/{ordenCompraId}", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "La actualización con múltiples items debería completarse en menos de 3 segundos");
    }

    #endregion

    #region Tests de Concurrencia

    [Fact]
    public async Task CrearOrdenCompra_Concurrentemente_DeberiaManejarMultiplesRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
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
                    PrecioUnitario = 10.00m
                }
            }
        };

        var json = JsonSerializer.Serialize(command, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act - Crear 10 requests concurrentes
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Client.PostAsync("/api/inventario/ordenes-compra", content));
        }

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(10);
        responses.All(r => r.StatusCode == HttpStatusCode.Created || r.StatusCode == HttpStatusCode.BadRequest).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Los 10 requests concurrentes deberían completarse en menos de 5 segundos");
    }

    [Fact]
    public async Task ObtenerOrdenesCompra_Concurrentemente_DeberiaManejarMultiplesRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Crear 20 requests concurrentes
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={i % 5 + 1}&pageSize=10"));
        }

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(20);
        responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Los 20 requests concurrentes deberían completarse en menos de 3 segundos");
    }

    #endregion

    #region Tests de Memoria

    [Fact]
    public async Task ObtenerOrdenesCompra_ConPaginacionGrande_NoDeberiaConsumirMuchaMemoria()
    {
        // Arrange
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = 1000 // Página grande
        };

        var memoryBefore = GC.GetTotalMemory(false);

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={query.PageNumber}&pageSize={query.PageSize}");

        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        memoryUsed.Should().BeLessThan(50 * 1024 * 1024, "No debería usar más de 50MB de memoria adicional");
    }

    #endregion

    #region Tests de Escalabilidad

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ObtenerOrdenesCompra_ConDiferentesTamanosDePagina_DeberiaEscalarCorrectamente(int pageSize)
    {
        // Arrange
        var query = new ObtenerOrdenesCompraPaginadasQuery
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync($"/api/inventario/ordenes-compra?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // El tiempo debería crecer linealmente con el tamaño de página
        var maxTime = pageSize * 20; // 20ms por item máximo
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxTime, 
            $"El tiempo para {pageSize} items debería ser proporcional al tamaño");
    }

    #endregion

    #region Tests de Optimización

    [Fact]
    public async Task ObtenerOrdenesCompra_ConFiltros_DeberiaSerMasRapidoQueSinFiltros()
    {
        // Arrange
        var stopwatchSinFiltros = Stopwatch.StartNew();
        var responseSinFiltros = await Client.GetAsync("/api/inventario/ordenes-compra?pageNumber=1&pageSize=100");
        stopwatchSinFiltros.Stop();

        var stopwatchConFiltros = Stopwatch.StartNew();
        var responseConFiltros = await Client.GetAsync("/api/inventario/ordenes-compra?pageNumber=1&pageSize=100&estado=Pendiente");
        stopwatchConFiltros.Stop();

        // Assert
        responseSinFiltros.StatusCode.Should().Be(HttpStatusCode.OK);
        responseConFiltros.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Los filtros deberían hacer la consulta más rápida (o al menos no significativamente más lenta)
        stopwatchConFiltros.ElapsedMilliseconds.Should().BeLessThan(stopwatchSinFiltros.ElapsedMilliseconds * 1.5,
            "Los filtros no deberían hacer la consulta significativamente más lenta");
    }

    #endregion

    #region Tests de Límites de Rendimiento

    [Fact]
    public async Task CrearOrdenCompra_ConMaximoNumeroDeItems_DeberiaManejarCorrectamente()
    {
        // Arrange
        var command = new CrearOrdenCompraCommand
        {
            ProveedorId = Guid.NewGuid(),
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Items = new List<CrearOrdenCompraItemCommand>()
        };

        // Crear el máximo número de items permitido (1000)
        for (int i = 0; i < 1000; i++)
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

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await Client.PostAsync("/api/inventario/ordenes-compra", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Incluso con 1000 items, debería completarse en menos de 10 segundos");
    }

    #endregion
}
