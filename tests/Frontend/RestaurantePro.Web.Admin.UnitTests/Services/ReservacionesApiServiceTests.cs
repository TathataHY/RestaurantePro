using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.UnitTests.Services
{
    public class ReservacionesApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly TokenStore _tokenStore;
        private readonly ReservacionesApiService _service;

        public ReservacionesApiServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _tokenStore = new TokenStore();
            _tokenStore.Token = "test-token";

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7001/")
            };

            _httpClientFactoryMock
                .Setup(x => x.CreateClient("Api"))
                .Returns(httpClient);

            _service = new ReservacionesApiService(_httpClientFactoryMock.Object, _tokenStore);
        }

        // ===== PRUEBAS BÁSICAS =====

        [Fact]
        public async Task ObtenerReservacionesAsync_ConDatosValidos_DeberiaRetornarReservaciones()
        {
            // Arrange
            var filtros = new ReservacionFiltrosDto
            {
                PageNumber = 1,
                PageSize = 20,
                OrdenarPor = "FechaHoraCompleta",
                DireccionOrden = "asc"
            };

            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES001", ClienteNombre = "Juan Pérez", MesaNumero = 1, NumeroPersonas = 4, Estado = "Confirmada" },
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES002", ClienteNombre = "María González", MesaNumero = 2, NumeroPersonas = 2, Estado = "Pendiente" }
            };

            var paginatedList = new PaginatedList<ReservacionDto>
            {
                Items = reservaciones,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 20,
                TotalPages = 1
            };

            var apiResponse = new ApiResponse<PaginatedList<ReservacionDto>>
            {
                Success = true,
                Data = paginatedList
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerReservacionesAsync(filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(2);
            resultado.Items.First().NumeroReservacion.Should().Be("RES001");
        }

        [Fact]
        public async Task ObtenerReservacionAsync_ConIdValido_DeberiaRetornarReservacion()
        {
            // Arrange
            var id = Guid.NewGuid();
            var reservacion = new ReservacionDto
            {
                Id = id,
                NumeroReservacion = "RES001",
                ClienteNombre = "Juan Pérez",
                ClienteEmail = "juan@email.com",
                ClienteTelefono = "0991234567",
                MesaNumero = 1,
                MesaNombre = "Mesa 1",
                NumeroPersonas = 4,
                Estado = "Confirmada",
                FechaReservacion = DateTime.Today.AddDays(1),
                HoraReservacion = TimeSpan.FromHours(19),
                Observaciones = "Reservación de prueba"
            };

            var apiResponse = new ApiResponse<ReservacionDto>
            {
                Success = true,
                Data = reservacion
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerReservacionAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NumeroReservacion.Should().Be("RES001");
            resultado.ClienteNombre.Should().Be("Juan Pérez");
        }

        [Fact]
        public async Task CrearReservacionAsync_ConDatosValidos_DeberiaRetornarReservacionCreada()
        {
            // Arrange
            var request = new CrearReservacionRequest
            {
                ClienteId = Guid.NewGuid(),
                MesaId = Guid.NewGuid(),
                FechaReservacion = DateTime.Today.AddDays(1),
                HoraReservacion = TimeSpan.FromHours(19),
                NumeroPersonas = 4,
                Observaciones = "Reservación de prueba",
                CanalReservacion = "Web",
                EsUrgente = false,
                EsVIP = false
            };

            var reservacionCreada = new ReservacionDto
            {
                Id = Guid.NewGuid(),
                NumeroReservacion = "RES001",
                ClienteId = request.ClienteId,
                MesaId = request.MesaId,
                FechaReservacion = request.FechaReservacion,
                HoraReservacion = request.HoraReservacion,
                NumeroPersonas = request.NumeroPersonas,
                Estado = "Pendiente",
                CanalReservacion = request.CanalReservacion,
                FechaCreacion = DateTime.UtcNow
            };

            var apiResponse = new ApiResponse<ReservacionDto>
            {
                Success = true,
                Data = reservacionCreada,
                Message = "Reservación creada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.CrearReservacionAsync(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NumeroReservacion.Should().Be("RES001");
        }

        [Fact]
        public async Task ActualizarReservacionAsync_ConDatosValidos_DeberiaRetornarReservacionActualizada()
        {
            // Arrange
            var request = new ActualizarReservacionRequest
            {
                Id = Guid.NewGuid(),
                FechaReservacion = DateTime.Today.AddDays(2),
                HoraReservacion = TimeSpan.FromHours(20),
                NumeroPersonas = 6,
                Observaciones = "Reservación actualizada",
                EsUrgente = true
            };

            var reservacionActualizada = new ReservacionDto
            {
                Id = request.Id,
                NumeroReservacion = "RES001",
                FechaReservacion = request.FechaReservacion.Value,
                HoraReservacion = request.HoraReservacion.Value,
                NumeroPersonas = request.NumeroPersonas.Value,
                Observaciones = request.Observaciones,
                EsUrgente = request.EsUrgente.Value,
                Estado = "Confirmada"
            };

            var apiResponse = new ApiResponse<ReservacionDto>
            {
                Success = true,
                Data = reservacionActualizada,
                Message = "Reservación actualizada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ActualizarReservacionAsync(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Data!.EsUrgente.Should().Be(true);
        }

        [Fact]
        public async Task EliminarReservacionAsync_ConIdValido_DeberiaRetornarTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            var apiResponse = new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Reservación eliminada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.EliminarReservacionAsync(id);

            // Assert
            resultado.Should().Be(true);
        }

        [Fact]
        public async Task ConfirmarReservacionAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new ConfirmarReservacionRequest
            {
                ReservacionId = Guid.NewGuid(),
                Observaciones = "Reservación confirmada"
            };

            var apiResponse = new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Reservación confirmada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ConfirmarReservacionAsync(request);

            // Assert
            resultado.Should().Be(true);
        }

        [Fact]
        public async Task CancelarReservacionAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new CancelarReservacionRequest
            {
                ReservacionId = Guid.NewGuid(),
                MotivoCancelacion = "Cambio de planes",
                Observaciones = "Cliente canceló por cambio de planes"
            };

            var apiResponse = new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Reservación cancelada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.CancelarReservacionAsync(request);

            // Assert
            resultado.Should().Be(true);
        }

        [Fact]
        public async Task MarcarLlegadaAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new MarcarLlegadaRequest
            {
                ReservacionId = Guid.NewGuid(),
                Observaciones = "Cliente llegó puntual"
            };

            var apiResponse = new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Llegada marcada correctamente"
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.MarcarLlegadaAsync(request);

            // Assert
            resultado.Should().Be(true);
        }

        [Fact]
        public async Task VerificarDisponibilidadAsync_ConDatosValidos_DeberiaRetornarDisponibilidad()
        {
            // Arrange
            var request = new VerificarDisponibilidadRequest
            {
                Fecha = DateTime.Today.AddDays(1),
                Hora = TimeSpan.FromHours(19),
                NumeroPersonas = 4,
                DuracionEstimada = TimeSpan.FromHours(2)
            };

            var disponibilidad = new DisponibilidadResponse
            {
                Disponible = true,
                MesasDisponibles = new List<MesaDisponibleDto>
                {
                    new() { MesaId = Guid.NewGuid(), MesaNombre = "Mesa 1", MesaNumero = 1, Capacidad = 4, EsIdeal = true }
                },
                Conflictos = new List<ReservacionConflictoDto>(),
                Mensaje = "Mesas disponibles para la fecha y hora solicitada"
            };

            var responseContent = JsonSerializer.Serialize(disponibilidad);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.VerificarDisponibilidadAsync(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Disponible.Should().BeTrue();
            resultado.MesasDisponibles.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObtenerEstadisticasAsync_ConDatosValidos_DeberiaRetornarEstadisticas()
        {
            // Arrange
            var estadisticas = new ReservacionEstadisticasDto
            {
                TotalReservaciones = 100,
                ReservacionesPendientes = 20,
                ReservacionesConfirmadas = 50,
                ReservacionesEnProceso = 15,
                ReservacionesCompletadas = 10,
                ReservacionesCanceladas = 3,
                ReservacionesNoShow = 2,
                ReservacionesHoy = 8,
                ReservacionesUrgentes = 5,
                ReservacionesVIP = 10,
                ReservacionesGrupo = 15,
                TasaConfirmacion = 85.5m,
                TasaNoShow = 5.2m,
                TasaCancelacion = 8.3m,
                IngresosPorReservaciones = 15000.00m,
                OcupacionPromedio = 75.5m
            };

            var apiResponse = new ApiResponse<ReservacionEstadisticasDto>
            {
                Success = true,
                Data = estadisticas
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

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
            resultado!.TotalReservaciones.Should().Be(100);
            resultado.TasaConfirmacion.Should().Be(85.5m);
        }

        [Fact]
        public async Task ObtenerReservacionesHoyAsync_ConDatosValidos_DeberiaRetornarReservaciones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES001", ClienteNombre = "Juan Pérez", MesaNumero = 1, Estado = "Confirmada" },
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES002", ClienteNombre = "María González", MesaNumero = 2, Estado = "Pendiente" }
            };

            var apiResponse = new ApiResponse<List<ReservacionDto>>
            {
                Success = true,
                Data = reservaciones
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerReservacionesHoyAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(2);
            resultado.First().NumeroReservacion.Should().Be("RES001");
        }

        [Fact]
        public async Task ObtenerEstadosAsync_ConDatosValidos_DeberiaRetornarEstados()
        {
            // Arrange
            var estados = new List<string> { "Pendiente", "Confirmada", "EnProceso", "Completada", "Cancelada", "NoShow" };

            var apiResponse = new ApiResponse<List<string>>
            {
                Success = true,
                Data = estados
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerEstadosAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(6);
            resultado.Should().Contain("Confirmada");
        }

        [Fact]
        public async Task ObtenerCanalesAsync_ConDatosValidos_DeberiaRetornarCanales()
        {
            // Arrange
            var canales = new List<string> { "Web", "Telefono", "Presencial", "App" };

            var apiResponse = new ApiResponse<List<string>>
            {
                Success = true,
                Data = canales
            };

            var responseContent = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerCanalesAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(4);
            resultado.Should().Contain("Web");
        }

        [Fact]
        public async Task ExportarReservacionesAsync_ConDatosValidos_DeberiaRetornarBytes()
        {
            // Arrange
            var filtros = new ReservacionFiltrosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today
            };

            var excelBytes = Encoding.UTF8.GetBytes("Excel content");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(excelBytes)
                });

            // Act
            var resultado = await _service.ExportarReservacionesAsync(filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Success.Should().BeTrue();
            resultado.Data!.Should().HaveCount(13); // "Excel content" = 13 bytes
        }

        // ===== PRUEBAS DE ERROR =====

        [Fact]
        public async Task ObtenerReservacionesAsync_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var filtros = new ReservacionFiltrosDto();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ObtenerReservacionesAsync(filtros);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerReservacionAsync_Con404_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var resultado = await _service.ObtenerReservacionAsync(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task CrearReservacionAsync_ConErrorDeRed_DeberiaRetornarError()
        {
            // Arrange
            var request = new CrearReservacionRequest
            {
                ClienteId = Guid.NewGuid(),
                MesaId = Guid.NewGuid(),
                FechaReservacion = DateTime.Today.AddDays(1),
                HoraReservacion = TimeSpan.FromHours(19),
                NumeroPersonas = 4
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.CrearReservacionAsync(request);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ConfirmarReservacionAsync_ConErrorDeRed_DeberiaRetornarError()
        {
            // Arrange
            var request = new ConfirmarReservacionRequest
            {
                ReservacionId = Guid.NewGuid()
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ConfirmarReservacionAsync(request);

            // Assert
            resultado.Should().Be(false);
        }

        [Fact]
        public async Task VerificarDisponibilidadAsync_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var request = new VerificarDisponibilidadRequest
            {
                Fecha = DateTime.Today.AddDays(1),
                Hora = TimeSpan.FromHours(19),
                NumeroPersonas = 4
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.VerificarDisponibilidadAsync(request);

            // Assert
            resultado.Should().BeNull();
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

        [Fact]
        public async Task ExportarReservacionesAsync_ConErrorDeRed_DeberiaRetornarError()
        {
            // Arrange
            var filtros = new ReservacionFiltrosDto();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ExportarReservacionesAsync(filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Success.Should().BeFalse();
            resultado.Message.Should().Contain("Connection lost");
        }
    }
}
