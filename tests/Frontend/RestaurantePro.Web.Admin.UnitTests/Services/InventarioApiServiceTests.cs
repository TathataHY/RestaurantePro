using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class InventarioApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly InventarioApiService _service;

    public InventarioApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStore = new TokenStore();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7001/")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _tokenStore.Token = "test-token";

        _service = new InventarioApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS - INGREDIENTES =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConRespuestaExitosa_DeberiaRetornarLista()
    {
        // Arrange
        var ingredientes = new List<IngredienteDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Tomate", UnidadMedida = "kg", StockActual = 50, CostoUnitario = 2.50m },
            new() { Id = Guid.NewGuid(), Nombre = "Cebolla", UnidadMedida = "kg", StockActual = 30, CostoUnitario = 1.80m }
        };

        var paginatedList = new PaginatedList<IngredienteDto>
        {
            Items = ingredientes,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ObtenerIngredienteAsync_ConIdValido_DeberiaRetornarIngrediente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingrediente = new IngredienteDto
        {
            Id = id,
            Nombre = "Pimiento",
            UnidadMedida = "kg",
            StockActual = 25,
            CostoUnitario = 3.20m
        };

        var responseContent = JsonSerializer.Serialize(ingrediente);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerIngredienteAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Pimiento");
        resultado.StockActual.Should().Be(25);
    }

    [Fact]
    public async Task CrearIngredienteAsync_ConIngredienteValido_DeberiaRetornarIngredienteCreado()
    {
        // Arrange
        var ingrediente = new IngredienteDto
        {
            Nombre = "Ajo",
            UnidadMedida = "kg",
            StockActual = 10,
            CostoUnitario = 4.50m
        };

        var ingredienteCreado = ingrediente;
        ingredienteCreado.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(ingredienteCreado);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearIngredienteAsync(ingrediente);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Ajo");
    }

    [Fact]
    public async Task ActualizarIngredienteAsync_ConIngredienteValido_DeberiaRetornarIngredienteActualizado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingrediente = new IngredienteDto
        {
            Id = id,
            Nombre = "Zanahoria",
            UnidadMedida = "kg",
            StockActual = 40,
            CostoUnitario = 2.00m
        };

        var responseContent = JsonSerializer.Serialize(ingrediente);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarIngredienteAsync(id, ingrediente);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Zanahoria");
    }

    [Fact]
    public async Task EliminarIngredienteAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.EliminarIngredienteAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConRespuestaExitosa_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new InventarioEstadisticasDto
        {
            TotalIngredientes = 100,
            IngredientesActivos = 95,
            StockBajo = 5,
            ValorTotalInventario = 5000.00m
        };

        var responseContent = JsonSerializer.Serialize(estadisticas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TotalIngredientes.Should().Be(100);
        resultado.ValorTotalInventario.Should().Be(5000.00m);
    }

    [Fact]
    public async Task ObtenerAlertasAsync_ConRespuestaExitosa_DeberiaRetornarAlertas()
    {
        // Arrange
        var alertas = new List<AlertaInventarioDto>
        {
            new() { IngredienteId = Guid.NewGuid(), IngredienteNombre = "Tomate", TipoAlerta = TipoAlertaInventario.StockBajo, Mensaje = "Stock bajo" }
        };

        var responseContent = JsonSerializer.Serialize(alertas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAlertasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().HaveCount(1);
        resultado.First().TipoAlerta.Should().Be(TipoAlertaInventario.StockBajo);
    }

    // ===== PRUEBAS BÁSICAS - MOVIMIENTOS =====

    [Fact]
    public async Task ObtenerMovimientosPaginadosAsync_ConRespuestaExitosa_DeberiaRetornarLista()
    {
        // Arrange
        var movimientos = new List<MovimientoInventarioDto>
        {
            new() { Id = Guid.NewGuid(), IngredienteId = Guid.NewGuid(), TipoMovimiento = TipoMovimientoInventario.Entrada, Cantidad = 10 }
        };

        var paginatedList = new PaginatedList<MovimientoInventarioDto>
        {
            Items = movimientos,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerMovimientosPaginadosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(1);
        resultado.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task CrearMovimientoAsync_ConMovimientoValido_DeberiaRetornarMovimientoCreado()
    {
        // Arrange
        var movimiento = new MovimientoInventarioDto
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = TipoMovimientoInventario.Entrada,
            Cantidad = 5,
            CostoUnitario = 2.50m
        };

        var movimientoCreado = movimiento;
        movimientoCreado.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(movimientoCreado);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearMovimientoAsync(movimiento);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TipoMovimiento.Should().Be(TipoMovimientoInventario.Entrada);
    }

    // ===== PRUEBAS BÁSICAS - ÓRDENES DE COMPRA =====

    [Fact]
    public async Task ObtenerOrdenesCompraPaginadasAsync_ConRespuestaExitosa_DeberiaRetornarLista()
    {
        // Arrange
        var ordenes = new List<OrdenCompraDto>
        {
            new() { Id = Guid.NewGuid(), NumeroOrden = "OC-001", Estado = EstadoOrdenCompra.Pendiente, Total = 1000.00m }
        };

        var paginatedList = new PaginatedList<OrdenCompraDto>
        {
            Items = ordenes,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerOrdenesCompraPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(1);
        resultado.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ObtenerOrdenCompraAsync_ConIdValido_DeberiaRetornarOrden()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orden = new OrdenCompraDto
        {
            Id = id,
            NumeroOrden = "OC-002",
            Estado = EstadoOrdenCompra.Completada,
            Total = 2500.00m
        };

        var responseContent = JsonSerializer.Serialize(orden);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerOrdenCompraAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.NumeroOrden.Should().Be("OC-002");
        resultado.Estado.Should().Be(EstadoOrdenCompra.Completada);
    }

    [Fact]
    public async Task CrearOrdenCompraAsync_ConOrdenValida_DeberiaRetornarOrdenCreada()
    {
        // Arrange
        var orden = new OrdenCompraDto
        {
            ProveedorId = Guid.NewGuid(),
            NumeroOrden = "OC-003",
            Estado = EstadoOrdenCompra.Pendiente,
            Total = 1500.00m
        };

        var ordenCreada = orden;
        ordenCreada.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(ordenCreada);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearOrdenCompraAsync(orden);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.NumeroOrden.Should().Be("OC-003");
    }

    [Fact]
    public async Task ActualizarOrdenCompraAsync_ConOrdenValida_DeberiaRetornarOrdenActualizada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orden = new OrdenCompraDto
        {
            Id = id,
            ProveedorId = Guid.NewGuid(),
            NumeroOrden = "OC-004",
            Estado = EstadoOrdenCompra.EnProceso,
            Total = 3000.00m
        };

        var responseContent = JsonSerializer.Serialize(orden);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarOrdenCompraAsync(id, orden);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Estado.Should().Be(EstadoOrdenCompra.EnProceso);
    }

    [Fact]
    public async Task EliminarOrdenCompraAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.EliminarOrdenCompraAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticasOrdenesCompraAsync_ConRespuestaExitosa_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new OrdenCompraEstadisticasDto
        {
            TotalOrdenes = 50,
            OrdenesPendientes = 10,
            OrdenesCompletadas = 35,
            ValorTotalOrdenes = 100000.00m
        };

        var responseContent = JsonSerializer.Serialize(estadisticas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasOrdenesCompraAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TotalOrdenes.Should().Be(50);
        resultado.ValorTotalOrdenes.Should().Be(100000.00m);
    }

    // ===== PRUEBAS BÁSICAS - UTILIDADES =====

    [Fact]
    public async Task ObtenerCategoriasAsync_ConRespuestaExitosa_DeberiaRetornarCategorias()
    {
        // Arrange
        var categorias = new List<string> { "Vegetales", "Carnes", "Lácteos", "Especias" };

        var responseContent = JsonSerializer.Serialize(categorias);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerCategoriasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().HaveCount(4);
        resultado.Should().Contain("Vegetales");
    }

    [Fact]
    public async Task ObtenerProveedoresAsync_ConRespuestaExitosa_DeberiaRetornarProveedores()
    {
        // Arrange
        var proveedores = new List<ProveedorDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Proveedor A", Email = "proveedor@a.com" },
            new() { Id = Guid.NewGuid(), Nombre = "Proveedor B", Email = "proveedor@b.com" }
        };

        var responseContent = JsonSerializer.Serialize(proveedores);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProveedoresAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().HaveCount(2);
        resultado.First().Nombre.Should().Be("Proveedor A");
    }

    [Fact]
    public async Task ExportarInventarioAsync_ConRespuestaExitosa_DeberiaRetornarBytes()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(bytes)
            });

        // Act
        var resultado = await _service.ExportarInventarioAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().HaveCount(13); // "Excel content" = 13 bytes
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerIngredienteAsync_ConError404_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerIngredienteAsync(Guid.NewGuid());

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearIngredienteAsync_ConError500_DeberiaRetornarNull()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Nombre = "Test" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.CrearIngredienteAsync(ingrediente);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarIngredienteAsync_ConError500_DeberiaRetornarFalse()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.EliminarIngredienteAsync(Guid.NewGuid());

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingredientes = new List<IngredienteDto>();
        for (int i = 0; i < 10000; i++)
        {
            ingredientes.Add(new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Ingrediente {i}",
                UnidadMedida = "kg",
                StockActual = i,
                CostoUnitario = i * 0.1m
            });
        }

        var paginatedList = new PaginatedList<IngredienteDto>
        {
            Items = ingredientes,
            TotalCount = 10000,
            PageNumber = 1,
            PageSize = 10000,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(10000);
        resultado.TotalCount.Should().Be(10000);
    }

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingredientes = new List<IngredienteDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Tomate 🍅", UnidadMedida = "kg", StockActual = 50 },
            new() { Id = Guid.NewGuid(), Nombre = "Cebolla 🧅", UnidadMedida = "kg", StockActual = 30 },
            new() { Id = Guid.NewGuid(), Nombre = "Ajo con ñoño", UnidadMedida = "kg", StockActual = 10 },
            new() { Id = Guid.NewGuid(), Nombre = "Pimiento rojo 🌶️", UnidadMedida = "kg", StockActual = 25 }
        };

        var paginatedList = new PaginatedList<IngredienteDto>
        {
            Items = ingredientes,
            TotalCount = 4,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(4);
        resultado.Items.First().Nombre.Should().Be("Tomate 🍅");
    }

    [Fact]
    public async Task CrearIngredienteAsync_ConValoresNumericosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingrediente = new IngredienteDto
        {
            Nombre = "Ingrediente Extremo",
            UnidadMedida = "kg",
            StockActual = decimal.MaxValue,
            StockMinimo = decimal.MaxValue,
            StockMaximo = decimal.MaxValue,
            CostoUnitario = decimal.MaxValue
        };

        var ingredienteCreado = ingrediente;
        ingredienteCreado.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(ingredienteCreado);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearIngredienteAsync(ingrediente);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.StockActual.Should().Be(decimal.MaxValue);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticas = new InventarioEstadisticasDto
        {
            TotalIngredientes = int.MaxValue,
            IngredientesActivos = int.MaxValue,
            IngredientesInactivos = int.MaxValue,
            StockBajo = int.MaxValue,
            VencimientoProximo = int.MaxValue,
            Vencidos = int.MaxValue,
            ValorTotalInventario = decimal.MaxValue,
            ValorStockBajo = decimal.MaxValue,
            ValorVencidos = decimal.MaxValue,
            MovimientosHoy = int.MaxValue,
            MovimientosMes = int.MaxValue,
            CostoTotalMovimientosHoy = decimal.MaxValue,
            CostoTotalMovimientosMes = decimal.MaxValue
        };

        var responseContent = JsonSerializer.Serialize(estadisticas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TotalIngredientes.Should().Be(int.MaxValue);
        resultado.ValorTotalInventario.Should().Be(decimal.MaxValue);
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new InventarioFiltrosDto
        {
            Nombre = "'; DROP TABLE Ingredientes; --",
            Categoria = "'; DELETE FROM Categorias; --",
            Proveedor = "'; UPDATE Usuarios SET Password = 'hacked'; --"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync(filtros: filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearIngredienteAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingrediente = new IngredienteDto
        {
            Nombre = "<script>alert('xss')</script>",
            Descripcion = "<img src=x onerror=alert('xss')>",
            UnidadMedida = "<svg onload=alert('xss')>",
            StockActual = 10
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearIngredienteAsync(ingrediente);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearMovimientoAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var movimiento = new MovimientoInventarioDto
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = TipoMovimientoInventario.Entrada,
            Cantidad = -999999999, // Valor extremo negativo
            CostoUnitario = -999999999, // Valor extremo negativo
            Observaciones = "'; DROP TABLE Movimientos; --",
            UsuarioResponsable = "<script>alert('xss')</script>"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearMovimientoAsync(movimiento);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearOrdenCompraAsync_ConValidacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var orden = new OrdenCompraDto
        {
            ProveedorId = Guid.NewGuid(),
            NumeroOrden = "'; DROP TABLE OrdenesCompra; --",
            Estado = EstadoOrdenCompra.Pendiente,
            Subtotal = -999999999,
            Impuesto = -999999999,
            Total = -999999999,
            Observaciones = "<script>alert('xss')</script>",
            UsuarioResponsable = "'; UPDATE Usuarios SET Password = 'hacked'; --"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearOrdenCompraAsync(orden);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingredientes = new List<IngredienteDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Tomate", UnidadMedida = "kg", StockActual = 50 }
        };

        var paginatedList = new PaginatedList<IngredienteDto>
        {
            Items = ingredientes,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var responseContent = JsonSerializer.Serialize(paginatedList);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<PaginatedList<IngredienteDto>?>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(_service.ObtenerIngredientesPaginadosAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(50);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Items.Should().HaveCount(1));
    }

    [Fact]
    public async Task CrearIngredienteAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var ingrediente = new IngredienteDto
        {
            Nombre = "Ingrediente Concurrencia",
            UnidadMedida = "kg",
            StockActual = 10,
            CostoUnitario = 2.50m
        };

        var ingredienteCreado = ingrediente;
        ingredienteCreado.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(ingredienteCreado);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<IngredienteDto?>>();
        for (int i = 0; i < 30; i++)
        {
            tasks.Add(_service.CrearIngredienteAsync(ingrediente));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(30);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Nombre.Should().Be("Ingrediente Concurrencia"));
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var estadisticas = new InventarioEstadisticasDto
        {
            TotalIngredientes = 100,
            ValorTotalInventario = 5000.00m
        };

        var responseContent = JsonSerializer.Serialize(estadisticas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<InventarioEstadisticasDto?>>();
        for (int i = 0; i < 40; i++)
        {
            tasks.Add(_service.ObtenerEstadisticasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(40);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.TotalIngredientes.Should().Be(100));
    }

    [Fact]
    public async Task CrearMovimientoAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var movimiento = new MovimientoInventarioDto
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = TipoMovimientoInventario.Entrada,
            Cantidad = 5,
            CostoUnitario = 2.50m
        };

        var movimientoCreado = movimiento;
        movimientoCreado.Id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(movimientoCreado);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<MovimientoInventarioDto?>>();
        for (int i = 0; i < 25; i++)
        {
            tasks.Add(_service.CrearMovimientoAsync(movimiento));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(25);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.TipoMovimiento.Should().Be(TipoMovimientoInventario.Entrada));
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerIngredientesPaginadosAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerIngredientesPaginadosAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExportarInventarioAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("Excel content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(bytes)
            });

        // Act
        var tasks = new List<Task<byte[]?>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_service.ExportarInventarioAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(20);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Should().HaveCount(13)); // "Excel content" = 13 bytes
    }

    [Fact]
    public async Task ObtenerAlertasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var alertas = new List<AlertaInventarioDto>
        {
            new() { IngredienteId = Guid.NewGuid(), IngredienteNombre = "Tomate", TipoAlerta = TipoAlertaInventario.StockBajo, Mensaje = "Stock bajo" }
        };

        var responseContent = JsonSerializer.Serialize(alertas);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<List<AlertaInventarioDto>?>>();
        for (int i = 0; i < 35; i++)
        {
            tasks.Add(_service.ObtenerAlertasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(35);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Should().HaveCount(1));
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categorias = new List<string> { "Vegetales", "Carnes", "Lácteos" };

        var responseContent = JsonSerializer.Serialize(categorias);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tasks = new List<Task<List<string>?>>();
        for (int i = 0; i < 30; i++)
        {
            tasks.Add(_service.ObtenerCategoriasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(30);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Should().HaveCount(3));
    }
}
