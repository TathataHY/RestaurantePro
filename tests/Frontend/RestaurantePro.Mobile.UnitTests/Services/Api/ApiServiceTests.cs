using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Mobile.UnitTests.Services.Api;

public class ApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;

    public ApiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };
        _apiService = new ApiService(_httpClient);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithValidResponse_ShouldReturnSuccessResponse()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedData, "Success"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint", "test-token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Success", result.Message);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAsync_WithHttpError_ShouldReturnErrorResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error de conexión", result.Message);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task GetAsync_WithEmptyResponse_ShouldReturnErrorResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error inesperado", result.Message);
    }

    [Fact]
    public async Task GetAsync_WithCancellation_ShouldReturnCancelledResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint", cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("error inesperado", result.Message.ToLower());
    }

    #endregion

    #region PostAsync Tests

    [Fact]
    public async Task PostAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestData = new { Name = "Test", Value = 123 };
        var expectedResponse = new { Id = 1, Name = "Test", Value = 123 };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedResponse, "Created"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.PostAsync<object>("test-endpoint", requestData, "test-token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Created", result.Message);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task PostAsync_WithHttpError_ShouldReturnErrorResponse()
    {
        // Arrange
        var requestData = new { Name = "Test" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.PostAsync<object>("test-endpoint", requestData);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error de conexión", result.Message);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region PutAsync Tests

    [Fact]
    public async Task PutAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestData = new { Id = 1, Name = "Updated" };
        var expectedResponse = new { Id = 1, Name = "Updated" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedResponse, "Updated"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.PutAsync<object>("test-endpoint", requestData, "test-token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Updated", result.Message);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region PatchAsync Tests

    [Fact]
    public async Task PatchAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestData = new { Name = "Patched" };
        var expectedResponse = new { Id = 1, Name = "Patched" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedResponse, "Patched"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.PatchAsync<object>("test-endpoint", requestData, "test-token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Patched", result.Message);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.DeleteAsync("test-endpoint", "test-token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Eliminado exitosamente", result.Message);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithHttpError_ShouldReturnErrorResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiService.DeleteAsync("test-endpoint");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error de conexión", result.Message);
        Assert.Equal(404, result.StatusCode);
    }

    #endregion

    #region Retry Logic Tests

    [Fact]
    public async Task GetAsync_WithIOException_ShouldRetryOnce()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedData, "Success"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var callCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new IOException("Connection lost");
                }
                return httpResponse;
            });

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, callCount); // Should have retried once
    }

    [Fact]
    public async Task GetAsync_WithHttpRequestException_ShouldRetryOnce()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedData, "Success"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var callCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Network error");
                }
                return httpResponse;
            });

        // Act
        var result = await _apiService.GetAsync<object>("test-endpoint");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, callCount); // Should have retried once
    }

    #endregion

    #region Auth Header Tests

    [Fact]
    public async Task GetAsync_WithToken_ShouldAddAuthHeader()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedData, "Success"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _apiService.GetAsync<object>("test-endpoint", "test-token");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("X-Bearer-Token"));
        Assert.Equal("test-token", capturedRequest.Headers.GetValues("X-Bearer-Token").First());
    }

    [Fact]
    public async Task GetAsync_WithoutToken_ShouldNotAddAuthHeader()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(ApiResponse<object>.SuccessResponse(expectedData, "Success"));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _apiService.GetAsync<object>("test-endpoint");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.False(capturedRequest.Headers.Contains("X-Bearer-Token"));
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
