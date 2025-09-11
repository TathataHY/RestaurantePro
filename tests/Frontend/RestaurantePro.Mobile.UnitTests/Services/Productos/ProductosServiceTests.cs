using AutoFixture;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Services.Productos;

public class ProductosServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly ProductosService _productosService;
    private readonly IFixture _fixture;

    public ProductosServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _productosService = new ProductosService(_mockApiService.Object, _mockAuthService.Object);
        _fixture = new Fixture();
    }

    #region ObtenerProductosPaginadosAsync

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConParametrosValidos_DeberiaRetornarListaProductos()
    {
        // Arrange
        var productos = _fixture.CreateMany<ProductoDto>(5).ToList();
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = productos.Count,
            PageNumber = 1,
            PageSize = 20
        };
        var expectedResponse = ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList);
        
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 20, null, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(5);
        result.Data.Should().BeEquivalentTo(productos);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConFiltro_DeberiaIncluirFiltroEnQuery()
    {
        // Arrange
        var productos = _fixture.CreateMany<ProductoDto>(3).ToList();
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = productos.Count,
            PageNumber = 1,
            PageSize = 20
        };
        var expectedResponse = ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList);
        
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.Is<string>(s => s.StartsWith("api/core/productos?") && s.Contains("pageNumber=1") && s.Contains("pageSize=20") && s.Contains("filtro=hamburguesa")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 20, "hamburguesa", true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ProductoDto>>(It.Is<string>(s => s.Contains("pageNumber=1") && s.Contains("pageSize=20") && s.Contains("filtro=hamburguesa")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_SinFiltro_DeberiaIncluirPageNumberYPageSize()
    {
        var productos = _fixture.CreateMany<ProductoDto>(2).ToList();
        var paginatedList = new PaginatedList<ProductoDto>
        {
            Items = productos,
            TotalCount = productos.Count,
            PageNumber = 2,
            PageSize = 50
        };
        var expectedResponse = ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(paginatedList);

        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.Is<string>(s => s.StartsWith("api/core/productos?") && s.Contains("pageNumber=2") && s.Contains("pageSize=50") && s.Contains("soloActivos=True")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _productosService.ObtenerProductosPaginadosAsync(2, 50, null, true);

        result.Success.Should().BeTrue();
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ProductoDto>>(It.Is<string>(s => s.Contains("pageNumber=2") && s.Contains("pageSize=50")), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConExcepcion_DeberiaRetornarError()
    {
        // Arrange
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 20, null, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error al obtener productos paginados");
    }

    #endregion

    #region Auth/RateLimit/NoContent Errors

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task ObtenerProductosPaginadosAsync_ShouldPropagateStatusAndMessage_OnApiError(int status, string message)
    {
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(new List<string> { message }, message, status));

        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 20, null, true);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ShouldReturnError_OnNoContent()
    {
        _mockApiService
            .Setup(x => x.GetAsync<PaginatedList<ProductoDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(new List<string> { "No Content" }, "No Content", 204));

        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 20, null, true);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Contain("No Content");
    }

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task ObtenerProductoPorIdAsync_ShouldPropagateStatusAndMessage_OnApiError(int status, string message)
    {
        _mockApiService
            .Setup(x => x.GetAsync<ProductoDto>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.ErrorResponse(new List<string> { message }, message, status));

        var result = await _productosService.ObtenerProductoPorIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    #endregion

    #region ObtenerProductoPorIdAsync

    [Fact]
    public async Task ObtenerProductoPorIdAsync_ConIdValido_DeberiaRetornarProducto()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var producto = _fixture.Create<ProductoDto>();
        producto.Id = productoId;
        
        var expectedResponse = ApiResponse<ProductoDto>.SuccessResponse(producto);
        
        _mockApiService
            .Setup(x => x.GetAsync<ProductoDto>($"api/core/productos/{productoId}", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductoPorIdAsync(productoId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(productoId);
    }

    [Fact]
    public async Task ObtenerProductoPorIdAsync_ConIdVacio_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.ObtenerProductoPorIdAsync(Guid.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El ID del producto es requerido");
    }

    #endregion

    #region ObtenerProductosPorCategoriaAsync

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_ConCategoriaValida_DeberiaRetornarProductos()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var productos = _fixture.CreateMany<ProductoDto>(4).ToList();
        var expectedResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/categoria/{categoriaId}?soloActivos=True", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(categoriaId, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(4);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_ConCategoriaIdVacio_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(Guid.Empty, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El ID de la categoría es requerido");
    }

    #endregion

    #region VerificarDisponibilidadProductoAsync

    [Fact]
    public async Task VerificarDisponibilidadProductoAsync_ConIdValido_DeberiaRetornarDisponibilidad()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var disponibilidad = _fixture.Create<DisponibilidadProductoDto>();
        disponibilidad.ProductoId = productoId;
        
        var expectedResponse = ApiResponse<DisponibilidadProductoDto>.SuccessResponse(disponibilidad);
        
        _mockApiService
            .Setup(x => x.GetAsync<DisponibilidadProductoDto>($"api/core/productos/{productoId}/disponibilidad", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.VerificarDisponibilidadProductoAsync(productoId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ProductoId.Should().Be(productoId);
    }

    [Fact]
    public async Task VerificarDisponibilidadProductoAsync_ConIdVacio_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.VerificarDisponibilidadProductoAsync(Guid.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El ID del producto es requerido");
    }

    #endregion

    #region BuscarProductosAsync

    [Fact]
    public async Task BuscarProductosAsync_ConTextoValido_DeberiaRetornarResultados()
    {
        // Arrange
        var textoBusqueda = "hamburguesa";
        var productos = _fixture.CreateMany<ProductoDto>(3).ToList();
        var expectedResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<ProductoDto>>(It.Is<string>(s => s.Contains("texto=hamburguesa")), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.BuscarProductosAsync(textoBusqueda, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task BuscarProductosAsync_ConTextoVacio_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.BuscarProductosAsync("", true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El texto de búsqueda es requerido");
    }

    [Fact]
    public async Task BuscarProductosAsync_ConTextoNull_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.BuscarProductosAsync(null!, true);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El texto de búsqueda es requerido");
    }

    #endregion

    #region ObtenerProductosPopularesAsync

    [Fact]
    public async Task ObtenerProductosPopularesAsync_ConLimiteValido_DeberiaRetornarProductos()
    {
        // Arrange
        var limite = 5;
        var productos = _fixture.CreateMany<ProductoDto>(limite).ToList();
        var expectedResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/populares?limite={limite}", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosPopularesAsync(limite);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(limite);
    }

    [Fact]
    public async Task ObtenerProductosPopularesAsync_ConLimiteCero_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.ObtenerProductosPopularesAsync(0);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El límite debe ser mayor a 0");
    }

    [Fact]
    public async Task ObtenerProductosPopularesAsync_ConLimiteNegativo_DeberiaRetornarError()
    {
        // Act
        var result = await _productosService.ObtenerProductosPopularesAsync(-5);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("El límite debe ser mayor a 0");
    }

    #endregion

    #region ObtenerProductosDisponiblesParaComandasAsync

    [Fact]
    public async Task ObtenerProductosDisponiblesParaComandasAsync_SinCategoria_DeberiaRetornarProductos()
    {
        // Arrange
        var productos = _fixture.CreateMany<ProductoDto>(6).ToList();
        var expectedResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<ProductoDto>>("api/core/productos/disponibles-comandas", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosDisponiblesParaComandasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(6);
    }

    [Fact]
    public async Task ObtenerProductosDisponiblesParaComandasAsync_ConCategoria_DeberiaIncluirCategoriaEnQuery()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var productos = _fixture.CreateMany<ProductoDto>(4).ToList();
        var expectedResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/disponibles-comandas?categoriaId={categoriaId}", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerProductosDisponiblesParaComandasAsync(categoriaId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(4);
    }

    #endregion

    #region ObtenerCategoriasAsync

    [Fact]
    public async Task ObtenerCategoriasAsync_Exitoso_DeberiaRetornarCategorias()
    {
        // Arrange
        var categorias = _fixture.CreateMany<CategoriaProductoDto>(3).ToList();
        var expectedResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        
        _mockApiService
            .Setup(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _productosService.ObtenerCategoriasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConExcepcion_DeberiaRetornarError()
    {
        // Arrange
        _mockApiService
            .Setup(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _productosService.ObtenerCategoriasAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error al obtener categorías");
    }

    #endregion
} 