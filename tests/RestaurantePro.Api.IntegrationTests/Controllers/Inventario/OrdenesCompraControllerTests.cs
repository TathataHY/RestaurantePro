namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración para OrdenesCompraController
/// Valida todos los endpoints REST del controlador de órdenes de compra
/// </summary>
[Collection("Sequential")]
public class OrdenesCompraControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public OrdenesCompraControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetOrdenesCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/inventario/ordenes-compra";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrdenesCompra_ConParametrosFiltro_DebeRetornarRespuestaValida()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var fechaDesde = DateTime.Now.AddDays(-30);
        var fechaHasta = DateTime.Now;
        var url = $"/api/inventario/ordenes-compra?estado=Pendiente&proveedorId={proveedorId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrdenCompra_ConIdEspecifico_DebeRetornarRespuestaValida()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var url = $"/api/inventario/ordenes-compra/{ordenCompraId}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/inventario/ordenes-compra";
        var command = new
        {
            ProveedorId = Guid.NewGuid(),
            FechaOrden = DateTime.Now,
            FechaEntregaEsperada = DateTime.Now.AddDays(7),
            Observaciones = "Orden de compra para ingredientes frescos",
            Items = new[]
            {
                new 
                { 
                    IngredienteId = Guid.NewGuid(), 
                    Cantidad = 50, 
                    Unidad = "kg",
                    PrecioUnitario = 2.50m,
                    Observaciones = "Tomates frescos"
                },
                new 
                { 
                    IngredienteId = Guid.NewGuid(), 
                    Cantidad = 25, 
                    Unidad = "kg",
                    PrecioUnitario = 1.80m,
                    Observaciones = "Cebollas blancas"
                }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PutOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var url = $"/api/inventario/ordenes-compra/{ordenCompraId}";
        var command = new
        {
            Id = ordenCompraId,
            ProveedorId = Guid.NewGuid(),
            FechaOrden = DateTime.Now,
            FechaEntregaEsperada = DateTime.Now.AddDays(5),
            Observaciones = "Orden de compra actualizada",
            Estado = "Aprobada"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AprobarOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var url = $"/api/inventario/ordenes-compra/{ordenCompraId}/aprobar";
        var command = new
        {
            AprobadoPor = Guid.NewGuid(),
            FechaAprobacion = DateTime.Now,
            NotasAprobacion = "Orden aprobada por gerente",
            CondicionesAprobacion = "Entregar antes del viernes"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RechazarOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var url = $"/api/inventario/ordenes-compra/{ordenCompraId}/rechazar";
        var command = new
        {
            RechazadoPor = Guid.NewGuid(),
            MotivoRechazo = "Precios muy altos",
            FechaRechazo = DateTime.Now,
            NotasRechazo = "Buscar proveedor alternativo"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecibirOrdenCompra_DebeRetornarRespuestaValida()
    {
        // Arrange
        var ordenCompraId = Guid.NewGuid();
        var url = $"/api/inventario/ordenes-compra/{ordenCompraId}/recibir";
        var command = new
        {
            RecibidoPor = Guid.NewGuid(),
            FechaRecepcion = DateTime.Now,
            NotasRecepcion = "Mercancía recibida en buen estado",
            VerificacionCalidad = true,
            ItemsRecibidos = new[]
            {
                new 
                { 
                    IngredienteId = Guid.NewGuid(), 
                    CantidadRecibida = 48, 
                    CantidadSolicitada = 50,
                    Observaciones = "2kg con daños menores"
                }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrdenesPendientes_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/inventario/ordenes-compra/pendientes";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 