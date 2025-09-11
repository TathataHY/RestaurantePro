using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Inventory;

namespace RestaurantePro.Mobile.UnitTests.Services.Preparaciones;

/// <summary>
/// Tests unitarios para PreparacionesService
/// </summary>
public class PreparacionesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly PreparacionesService _service;

    public PreparacionesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        
        // Configurar el mock de autenticación para devolver un token válido
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("test-token");
        
        _service = new PreparacionesService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_ConDatosValidos_DebeRetornarPreparaciones()
    {
        // Arrange
        var preparaciones = new List<PreparacionDto>
        {
            new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true,
                FechaCreacion = DateTime.Now
            }
        };

        var preparacionesPaginadas = new PreparacionesPaginadasDto
        {
            Items = preparaciones,
            TotalCount = preparaciones.Count,
            PageNumber = 1,
            PageSize = 10
        };

        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.SuccessResponse(preparacionesPaginadas);

        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.ObtenerPreparacionesAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Nombre.Should().Be("Pizza Margherita");
    }

    [Fact]
    public async Task ObtenerPreparacionAsync_ConIdValido_DebeRetornarPreparacion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var preparacion = new PreparacionDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = true
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacion);

        _mockApiService.Setup(x => x.GetAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.ObtenerPreparacionAsync(id);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(id);
        result.Data.Nombre.Should().Be("Pizza Margherita");
    }

    [Fact]
    public async Task CrearPreparacionAsync_ConDatosValidos_DebeRetornarPreparacionCreada()
    {
        // Arrange
        var preparacion = new PreparacionDto
        {
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = true
        };

        var preparacionCreada = new PreparacionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = true
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacionCreada);

        _mockApiService.Setup(x => x.PostAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<PreparacionDto>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.CrearPreparacionAsync(preparacion);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Nombre.Should().Be("Pizza Margherita");
    }

    [Fact]
    public async Task IniciarPreparacionAsync_ConIdValido_DebeRetornarPreparacionIniciada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new IniciarPreparacionDto
        {
            PreparacionId = id,
            Observaciones = "Iniciado desde móvil"
        };

        var preparacionIniciada = new PreparacionDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = true
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacionIniciada);

        _mockApiService.Setup(x => x.PostAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.IniciarPreparacionAsync(id, dto);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(id);
    }

    [Fact]
    public async Task CompletarPreparacionAsync_ConIdValido_DebeRetornarPreparacionCompletada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var preparacionCompletada = new PreparacionDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = true
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacionCompletada);

        _mockApiService.Setup(x => x.PostAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.CompletarPreparacionAsync(id);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(id);
    }

    [Fact]
    public async Task CancelarPreparacionAsync_ConIdValido_DebeRetornarPreparacionCancelada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CancelarPreparacionDto
        {
            PreparacionId = id,
            MotivoCancelacion = "Sin ingredientes",
            Observaciones = "Cancelado desde móvil"
        };

        var preparacionCancelada = new PreparacionDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            NombreProducto = "Pizza Margherita",
            Descripcion = "Pizza tradicional italiana",
            Categoria = "Pizzas",
            Precio = 15.99m,
            TiempoPreparacionMinutos = 15,
            Disponible = false
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacionCancelada);

        _mockApiService.Setup(x => x.PostAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.CancelarPreparacionAsync(id, dto);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(id);
    }

    [Fact]
    public async Task ObtenerColaPreparacionesAsync_ConDatosValidos_DebeRetornarCola()
    {
        // Arrange
        var colaPreparaciones = new List<PreparacionDto>
        {
            new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                NombreProducto = "Pizza Margherita",
                Descripcion = "Pizza tradicional italiana",
                Categoria = "Pizzas",
                Precio = 15.99m,
                TiempoPreparacionMinutos = 15,
                Disponible = true
            },
            new PreparacionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pasta Carbonara",
                NombreProducto = "Pasta Carbonara",
                Descripcion = "Pasta italiana con salsa carbonara",
                Categoria = "Pastas",
                Precio = 12.99m,
                TiempoPreparacionMinutos = 10,
                Disponible = true
            }
        };

        var apiResponse = ApiResponse<List<PreparacionDto>>.SuccessResponse(colaPreparaciones);

        _mockApiService.Setup(x => x.GetAsync<List<PreparacionDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.ObtenerColaPreparacionesAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_ConErrorDeApi_DebeRetornarError()
    {
        // Arrange
        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.ErrorResponse(new List<string> { "Error de conexión" });

        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _service.ObtenerPreparacionesAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error de conexión");
    }

    [Fact]
    public async Task ObtenerPreparacionAsync_ConExcepcion_DebeRetornarError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockApiService.Setup(x => x.GetAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new HttpRequestException("Error de red"));

        // Act
        var result = await _service.ObtenerPreparacionAsync(id);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error de red");
    }
} 