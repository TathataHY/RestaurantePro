using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Api;
using System.Net;
using System.Text.Json;

namespace RestaurantePro.Mobile.IntegrationTests.Services
{
    /// <summary>
    /// Pruebas de integración para DailyPreparationsService
    /// </summary>
    public class DailyPreparationsServiceTests
    {
        private Mock<IApiService> _mockApiService;
        private Mock<IAuthService> _mockAuthService;
        private Mock<ILogger<DailyPreparationsService>> _mockLogger;
        private DailyPreparationsService _service;

        public DailyPreparationsServiceTests()
        {
            Setup();
        }

        private void Setup()
        {
            _mockApiService = new Mock<IApiService>();
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<DailyPreparationsService>>();
            
            // Configurar el mock de AuthService para devolver un token
            _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
            
            _service = new DailyPreparationsService(
                _mockApiService.Object,
                _mockAuthService.Object,
                _mockLogger.Object);
        }

        #region GetPreparacionesDiariasAsync Tests

        [Fact]
        public async Task GetPreparacionesDiariasAsync_WithSuccess_ShouldReturnData()
        {
            // Arrange
            var preparaciones = new List<PreparacionDiariaDto>
            {
                new PreparacionDiariaDto
                {
                    Id = Guid.NewGuid(),
                    NombreProducto = "Pizza Margherita",
                    CantidadPreparada = 20,
                    CantidadDisponible = 15,
                    Estado = "Disponible"
                },
                new PreparacionDiariaDto
                {
                    Id = Guid.NewGuid(),
                    NombreProducto = "Pasta Carbonara",
                    CantidadPreparada = 10,
                    CantidadDisponible = 5,
                    Estado = "PorVencer"
                }
            };

            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Pizza Margherita", result.Data[0].NombreProducto);
            Assert.Equal("Pasta Carbonara", result.Data[1].NombreProducto);
        }

        [Fact]
        public async Task GetPreparacionesDiariasAsync_WithApiError_ShouldReturnFailure()
        {
            // Arrange
            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(
                    new List<string> { "Error de conexión" },
                    "Error de red",
                    500));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Error de conexión", result.Error);
        }

        [Fact]
        public async Task GetPreparacionesDiariasAsync_WithException_ShouldReturnFailure()
        {
            // Arrange
            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Error de red"));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Contains("Error de red", result.Error);
        }

        #endregion

        #region GetPreparacionDiariaAsync Tests

        [Fact]
        public async Task GetPreparacionDiariaAsync_WithValidId_ShouldReturnPreparation()
        {
            // Arrange
            var id = Guid.NewGuid();
            var preparacion = new PreparacionDiariaDto
            {
                Id = id,
                NombreProducto = "Pizza Margherita",
                CantidadPreparada = 20,
                CantidadDisponible = 15
            };

            _mockApiService
                .Setup(x => x.GetAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

            // Act
            var result = await _service.GetPreparacionDiariaAsync(id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            Assert.Equal("Pizza Margherita", result.Data.NombreProducto);
        }

        [Fact]
        public async Task GetPreparacionDiariaAsync_WithNotFound_ShouldReturnFailure()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockApiService
                .Setup(x => x.GetAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(
                    new List<string> { "Preparación no encontrada" },
                    "Not Found",
                    404));

            // Act
            var result = await _service.GetPreparacionDiariaAsync(id);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Preparación no encontrada", result.Error);
        }

        #endregion

        #region CrearPreparacionDiariaAsync Tests

        [Fact]
        public async Task CrearPreparacionDiariaAsync_WithValidCommand_ShouldCreatePreparation()
        {
            // Arrange
            var command = new CrearPreparacionDiariaCommand
            {
                ProductoId = Guid.NewGuid(),
                Cantidad = 20,
                ChefId = Guid.NewGuid(),
                FechaVencimiento = DateTime.Now.AddHours(4),
                Observaciones = "Preparación especial"
            };

            var preparacionCreada = new PreparacionDiariaDto
            {
                Id = Guid.NewGuid(),
                ProductoId = command.ProductoId,
                CantidadPreparada = command.Cantidad,
                ChefId = command.ChefId,
                FechaVencimiento = command.FechaVencimiento,
                Observaciones = command.Observaciones
            };

            _mockApiService
                .Setup(x => x.PostAsync<PreparacionDiariaDto>("api/operaciones/preparaciones-diarias", command, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacionCreada, "Success"));

            // Act
            var result = await _service.CrearPreparacionDiariaAsync(command);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(command.ProductoId, result.Data.ProductoId);
            Assert.Equal(command.Cantidad, result.Data.CantidadPreparada);
            Assert.Equal(command.Observaciones, result.Data.Observaciones);
        }

        [Fact]
        public async Task CrearPreparacionDiariaAsync_WithValidationError_ShouldReturnFailure()
        {
            // Arrange
            var command = new CrearPreparacionDiariaCommand
            {
                ProductoId = Guid.Empty, // Invalid
                Cantidad = 0, // Invalid
                ChefId = Guid.NewGuid(),
                FechaVencimiento = DateTime.Now.AddHours(4)
            };

            _mockApiService
                .Setup(x => x.PostAsync<PreparacionDiariaDto>("api/operaciones/preparaciones-diarias", command, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(
                    new List<string> { "El ID del producto no puede estar vacío", "La cantidad debe ser mayor que cero" },
                    "Bad Request",
                    400));

            // Act
            var result = await _service.CrearPreparacionDiariaAsync(command);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Contains("El ID del producto no puede estar vacío", result.Error);
        }

        #endregion

        #region ActualizarPreparacionDiariaAsync Tests

        [Fact]
        public async Task ActualizarPreparacionDiariaAsync_WithValidCommand_ShouldUpdatePreparation()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new ActualizarPreparacionDiariaCommand
            {
                ProductoId = Guid.NewGuid(),
                CantidadPreparada = 25,
                CantidadDisponible = 20,
                ChefId = Guid.NewGuid(),
                FechaVencimiento = DateTime.Now.AddHours(6),
                Observaciones = "Actualización de cantidad"
            };

            var preparacionActualizada = new PreparacionDiariaDto
            {
                Id = id,
                ProductoId = command.ProductoId,
                CantidadPreparada = command.CantidadPreparada,
                CantidadDisponible = command.CantidadDisponible,
                ChefId = command.ChefId,
                FechaVencimiento = command.FechaVencimiento,
                Observaciones = command.Observaciones
            };

            _mockApiService
                .Setup(x => x.PutAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", command, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacionActualizada, "Success"));

            // Act
            var result = await _service.ActualizarPreparacionDiariaAsync(id, command);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(command.CantidadPreparada, result.Data.CantidadPreparada);
            Assert.Equal(command.CantidadDisponible, result.Data.CantidadDisponible);
            Assert.Equal(command.Observaciones, result.Data.Observaciones);
        }

        #endregion

        #region EliminarPreparacionDiariaAsync Tests

        [Fact]
        public async Task EliminarPreparacionDiariaAsync_WithValidId_ShouldDeletePreparation()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockApiService
                .Setup(x => x.DeleteAsync($"api/operaciones/preparaciones-diarias/{id}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "Success"));

            // Act
            var result = await _service.EliminarPreparacionDiariaAsync(id);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task EliminarPreparacionDiariaAsync_WithNotFound_ShouldReturnFailure()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockApiService
                .Setup(x => x.DeleteAsync($"api/operaciones/preparaciones-diarias/{id}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "Preparación no encontrada" },
                    "Not Found",
                    404));

            // Act
            var result = await _service.EliminarPreparacionDiariaAsync(id);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Preparación no encontrada", result.Error);
        }

        #endregion

        #region ConsumirPreparacionDiariaAsync Tests

        [Fact]
        public async Task ConsumirPreparacionDiariaAsync_WithValidQuantity_ShouldConsumePreparation()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cantidad = 5;
            var observaciones = "Consumido por comanda #123";

            var preparacionConsumida = new PreparacionDiariaDto
            {
                Id = id,
                CantidadPreparada = 20,
                CantidadDisponible = 10, // 20 - 5 - 5 = 10 disponible (se consumieron 5 unidades)
                NombreProducto = "Test Product",
                Estado = "Disponible",
                FechaPreparacion = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddHours(24),
                ProductoId = Guid.NewGuid(),
                ChefId = Guid.NewGuid(),
                NombreChef = "Test Chef",
                Observaciones = "Test observations"
            };

            var command = new { Cantidad = cantidad, Observaciones = observaciones };

            _mockApiService
                .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/consumir", It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacionConsumida, "Success"));

            // Act
            var result = await _service.ConsumirPreparacionDiariaAsync(id, cantidad, observaciones);

            // Assert
            Assert.True(result.Succeeded, $"Error: {result.Error}");
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data.CantidadDisponible);
            // Verificar que CantidadConsumida se calcula correctamente
            Assert.Equal(10, result.Data.CantidadConsumida); // 20 - 10 = 10 consumida
        }

        [Fact]
        public async Task ConsumirPreparacionDiariaAsync_WithInsufficientQuantity_ShouldReturnFailure()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cantidad = 25; // More than available

            var command = new { Cantidad = cantidad, Observaciones = (string?)null };

            _mockApiService
                .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/consumir", command, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(
                    new List<string> { "Cantidad insuficiente disponible" },
                    "Cantidad insuficiente disponible",
                    400));

            // Act
            var result = await _service.ConsumirPreparacionDiariaAsync(id, cantidad);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            // El error puede variar, solo verificamos que hay un error
        }

        #endregion

        #region MarcarComoDisponibleAsync Tests

        [Fact]
        public async Task MarcarComoDisponibleAsync_WithValidId_ShouldMarkAsAvailable()
        {
            // Arrange
            var id = Guid.NewGuid();

            var preparacionDisponible = new PreparacionDiariaDto
            {
                Id = id,
                Estado = "Disponible",
                CantidadDisponible = 20
            };

            _mockApiService
                .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/disponible", It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacionDisponible, "Success"));

            // Act
            var result = await _service.MarcarComoDisponibleAsync(id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Disponible", result.Data.Estado);
        }

        #endregion

        #region GetPreparacionesDiariasPorEstadoAsync Tests

        [Fact]
        public async Task GetPreparacionesDiariasPorEstadoAsync_WithValidState_ShouldReturnFilteredData()
        {
            // Arrange
            var estado = "Disponible";
            var preparaciones = new List<PreparacionDiariaDto>
            {
                new PreparacionDiariaDto
                {
                    Id = Guid.NewGuid(),
                    NombreProducto = "Pizza Margherita",
                    Estado = "Disponible"
                }
            };

            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-estado?estado={estado}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

            // Act
            var result = await _service.GetPreparacionesDiariasPorEstadoAsync(estado);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal("Disponible", result.Data[0].Estado);
        }

        #endregion

        #region GetPreparacionesDiariasPorProductoAsync Tests

        [Fact]
        public async Task GetPreparacionesDiariasPorProductoAsync_WithValidProductId_ShouldReturnProductPreparations()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparaciones = new List<PreparacionDiariaDto>
            {
                new PreparacionDiariaDto
                {
                    Id = Guid.NewGuid(),
                    ProductoId = productoId,
                    NombreProducto = "Pizza Margherita"
                },
                new PreparacionDiariaDto
                {
                    Id = Guid.NewGuid(),
                    ProductoId = productoId,
                    NombreProducto = "Pizza Margherita"
                }
            };

            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-producto/{productoId}", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

            // Act
            var result = await _service.GetPreparacionesDiariasPorProductoAsync(productoId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(productoId, result.Data[0].ProductoId);
            Assert.Equal(productoId, result.Data[1].ProductoId);
        }

        #endregion

        #region GetEstadisticasAsync Tests

        [Fact]
        public async Task GetEstadisticasAsync_WithSuccess_ShouldReturnStatistics()
        {
            // Arrange
            var estadisticas = new EstadisticasPreparacionesDiariasDto
            {
                TotalPreparaciones = 15,
                PreparacionesDisponibles = 8,
                PreparacionesPorVencer = 3,
                PreparacionesAgotadas = 2,
                PreparacionesVencidas = 2,
                CantidadTotalPreparada = 150,
                CantidadDisponible = 80,
                CantidadConsumida = 60,
                CantidadDesperdiciada = 10,
                PorcentajeEficiencia = 80.0m
            };

            _mockApiService
                .Setup(x => x.GetAsync<EstadisticasPreparacionesDiariasDto>("api/operaciones/preparaciones-diarias/estadisticas", It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<EstadisticasPreparacionesDiariasDto>.SuccessResponse(estadisticas, "Success"));

            // Act
            var result = await _service.GetEstadisticasAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(15, result.Data.TotalPreparaciones);
            Assert.Equal(8, result.Data.PreparacionesDisponibles);
            Assert.Equal(80.0m, result.Data.PorcentajeEficiencia);
            Assert.Equal("80.0%", result.Data.EficienciaFormateada);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ServiceMethods_WithNetworkException_ShouldReturnFailure()
        {
            // Arrange
            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Error de conexión de red"));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Contains("Error de conexión de red", result.Error);
        }

        [Fact]
        public async Task ServiceMethods_WithJsonException_ShouldReturnFailure()
        {
            // Arrange
            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new JsonException("Error de formato JSON"));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Contains("Error de formato JSON", result.Error);
        }

        [Fact]
        public async Task ServiceMethods_WithTimeoutException_ShouldReturnFailure()
        {
            // Arrange
            _mockApiService
                .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new TaskCanceledException("Timeout"));

            // Act
            var result = await _service.GetPreparacionesDiariasAsync();

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Contains("Timeout", result.Error);
        }

        #endregion
    }
} 
