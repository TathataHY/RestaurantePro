using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Inventory;

namespace RestaurantePro.Mobile.UnitTests.Services.Reservaciones;

public class ReservacionesServiceTests
{
    private Mock<IApiService> _mockApiService;
    private ReservacionesService _reservacionesService;

    public ReservacionesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _reservacionesService = new ReservacionesService(_mockApiService.Object);
    }

    [Fact]
    public async Task ObtenerReservacionesAsync_ConParametrosValidos_DeberiaRetornarListaReservaciones()
    {
        // Arrange
        var reservaciones = new List<ReservacionDto>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                NombreCliente = "Juan Pérez", 
                Telefono = "123456789",
                Email = "juan@email.com",
                FechaReservacion = DateTime.Now.AddDays(1),
                HoraReservacion = TimeSpan.FromHours(19),
                NumeroPersonas = 4,
                Estado = "Confirmada"
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                NombreCliente = "María García", 
                Telefono = "987654321",
                Email = "maria@email.com",
                FechaReservacion = DateTime.Now.AddDays(2),
                HoraReservacion = TimeSpan.FromHours(20),
                NumeroPersonas = 2,
                Estado = "Pendiente"
            }
        };

        var apiResponse = ApiResponse<List<ReservacionDto>>.SuccessResponse(reservaciones);

        _mockApiService.Setup(x => x.GetAsync<List<ReservacionDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _reservacionesService.ObtenerReservacionesAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObtenerReservacionAsync_ConIdValido_DeberiaRetornarReservacion()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var reservacion = new ReservacionDto 
        { 
            Id = reservacionId, 
            NombreCliente = "Juan Pérez",
            Telefono = "123456789",
            Email = "juan@email.com",
            FechaReservacion = DateTime.Now.AddDays(1),
            HoraReservacion = TimeSpan.FromHours(19),
            NumeroPersonas = 4,
            Estado = "Confirmada"
        };
        var apiResponse = ApiResponse<ReservacionDto>.SuccessResponse(reservacion);

        _mockApiService.Setup(x => x.GetAsync<ReservacionDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _reservacionesService.ObtenerReservacionAsync(reservacionId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.NombreCliente.Should().Be("Juan Pérez");
    }

    [Fact]
    public async Task CrearReservacionAsync_ConDatosValidos_DeberiaRetornarReservacionCreada()
    {
        // Arrange
        var reservacion = new ReservacionDto
        {
            NombreCliente = "Juan Pérez",
            Telefono = "123456789",
            Email = "juan@email.com",
            FechaReservacion = DateTime.Now.AddDays(1),
            HoraReservacion = TimeSpan.FromHours(19),
            NumeroPersonas = 4,
            Estado = "Pendiente"
        };

        var reservacionCreada = new ReservacionDto 
        { 
            Id = Guid.NewGuid(), 
            NombreCliente = "Juan Pérez",
            Telefono = "123456789",
            Email = "juan@email.com",
            FechaReservacion = DateTime.Now.AddDays(1),
            HoraReservacion = TimeSpan.FromHours(19),
            NumeroPersonas = 4,
            Estado = "Pendiente"
        };
        var apiResponse = ApiResponse<ReservacionDto>.SuccessResponse(reservacionCreada);

        _mockApiService.Setup(x => x.PostAsync<ReservacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _reservacionesService.CrearReservacionAsync(reservacion);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerReservacionesAsync_ConErrorDeApi_DeberiaRetornarError()
    {
        // Arrange
        var apiResponse = ApiResponse<List<ReservacionDto>>.ErrorResponse(new List<string> { "Error del servidor" });

        _mockApiService.Setup(x => x.GetAsync<List<ReservacionDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _reservacionesService.ObtenerReservacionesAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error del servidor");
    }
} 
