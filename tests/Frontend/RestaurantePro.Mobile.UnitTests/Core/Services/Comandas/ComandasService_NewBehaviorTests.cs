using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Comandas;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Core.Services.Comandas;

public class ComandasService_NewBehaviorTests
{
    private readonly Mock<IApiService> _apiServiceMock = new();
    private readonly Mock<IAuthService> _authMock = new();
    private readonly ComandasService _service;

    public ComandasService_NewBehaviorTests()
    {
        _service = new ComandasService(_apiServiceMock.Object, _authMock.Object);
        _authMock.Setup(a => a.GetTokenAsync()).ReturnsAsync("token");
        _authMock.Setup(a => a.GetUserIdAsync()).ReturnsAsync(Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task BuscarComandasAsync_WhenApiReturnsEmptyItems_ShouldReturnSuccessWithEmptyList()
    {
        var paged = new PaginatedList<ComandaDto>
        {
            Items = new List<ComandaDto>(),
            PageNumber = 1,
            PageSize = 12,
            TotalCount = 0
        };

        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.SuccessResponse(paged));

        var result = await _service.BuscarComandasAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarComandasAsync_WhenApiReturnsNoContent_ShouldReturnErrorWithMessage()
    {
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.ErrorResponse(new List<string>{"No content"}, "No content", 204));

        var result = await _service.BuscarComandasAsync();

        result.Success.Should().BeFalse();
        result.Message.Should().Be("No content");
    }

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task FinalizarComandaAsync_WhenApiReturnsAuthOrRateErrors_ShouldPropagateStatus(int status, string message)
    {
        var comandaId = Guid.NewGuid();
        _apiServiceMock
            .Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.ErrorResponse(new List<string>{message}, message, status));

        var result = await _service.FinalizarComandaAsync(comandaId, "Efectivo");

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    [Theory]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(429, "Too Many Requests")]
    public async Task CambiarEstadoComandaAsync_WhenApiReturnsAuthOrRateErrors_ShouldPropagateStatus(int status, string message)
    {
        var comandaId = Guid.NewGuid();
        _apiServiceMock
            .Setup(x => x.PatchAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ComandaDto>.ErrorResponse(new List<string>{message}, message, status));

        var result = await _service.CambiarEstadoComandaAsync(comandaId, "En Preparación");

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(status);
        result.Message.Should().Be(message);
    }

    [Fact]
    public async Task ObtenerComandasActivasAsync_WhenApiReturnsNullData_ShouldReturnError()
    {
        _apiServiceMock
            .Setup(x => x.GetAsync<PaginatedList<ComandaDto>>(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PaginatedList<ComandaDto>>.ErrorResponse("No content", "No content", 204));

        var result = await _service.ObtenerComandasActivasAsync();

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Error en la operación");
    }
}


