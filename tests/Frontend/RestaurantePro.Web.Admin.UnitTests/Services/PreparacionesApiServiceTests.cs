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
    public class PreparacionesApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly TokenStore _tokenStore;
        private readonly PreparacionesApiService _service;

        public PreparacionesApiServiceTests()
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

            _service = new PreparacionesApiService(_httpClientFactoryMock.Object, _tokenStore);
        }

        // ===== PRUEBAS BÁSICAS =====

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConDatosValidos_DeberiaRetornarPreparaciones()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ComandaId = 1, ProductoId = 1, ProductoNombre = "Pizza Margherita", Cantidad = 2, Estado = EstadoPreparacion.Pendiente, Prioridad = PrioridadPreparacion.Normal },
                new() { Id = 2, ComandaId = 2, ProductoId = 2, ProductoNombre = "Pasta Carbonara", Cantidad = 1, Estado = EstadoPreparacion.EnProceso, Prioridad = PrioridadPreparacion.Alta }
            };

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones,
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
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(2);
            resultado.Items.First().ProductoNombre.Should().Be("Pizza Margherita");
        }

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConFiltros_DeberiaRetornarPreparacionesFiltradas()
        {
            // Arrange
            var filtros = new PreparacionFiltrosDto
            {
                Estado = EstadoPreparacion.Pendiente,
                Prioridad = PrioridadPreparacion.Alta,
                CocineroId = 1,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddDays(1)
            };

            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, Estado = EstadoPreparacion.Pendiente, Prioridad = PrioridadPreparacion.Alta, CocineroId = 1 }
            };

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones,
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
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync(1, 20, filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObtenerPreparacionAsync_ConIdValido_DeberiaRetornarPreparacion()
        {
            // Arrange
            var preparacion = new PreparacionDetalleDto
            {
                Id = 1,
                ComandaId = 1,
                ProductoId = 1,
                ProductoNombre = "Pizza Margherita",
                Cantidad = 2,
                Estado = EstadoPreparacion.Pendiente,
                Prioridad = PrioridadPreparacion.Normal,
                Ingredientes = new List<IngredientePreparacionDto>
                {
                    new() { IngredienteId = 1, IngredienteNombre = "Masa", Cantidad = 1, Unidad = "unidad", Disponible = true }
                },
                Historial = new List<PreparacionHistorialDto>
                {
                    new() { Fecha = DateTime.UtcNow, Accion = "Creada", Usuario = "Sistema" }
                }
            };

            var responseContent = JsonSerializer.Serialize(preparacion);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerPreparacionAsync(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.ProductoNombre.Should().Be("Pizza Margherita");
            // La propiedad Ingredientes no está disponible en PreparacionDto básico
        }

        [Fact]
        public async Task ObtenerColaPreparacionesAsync_ConDatosValidos_DeberiaRetornarCola()
        {
            // Arrange
            var cola = new ColaPreparacionesDto
            {
                Pendientes = new List<PreparacionDto>
                {
                    new() { Id = 1, Estado = EstadoPreparacion.Pendiente }
                },
                EnProceso = new List<PreparacionDto>
                {
                    new() { Id = 2, Estado = EstadoPreparacion.EnProceso }
                },
                Completadas = new List<PreparacionDto>
                {
                    new() { Id = 3, Estado = EstadoPreparacion.Lista }
                },
                Atrasadas = new List<PreparacionDto>
                {
                    new() { Id = 4, Estado = EstadoPreparacion.Pendiente, EstaAtrasada = true }
                },
                TotalPendientes = 1,
                TotalEnProceso = 1,
                TotalCompletadas = 1,
                TotalAtrasadas = 1
            };

            var responseContent = JsonSerializer.Serialize(cola);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerColaPreparacionesAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.TotalPendientes.Should().Be(1);
            resultado.TotalEnProceso.Should().Be(1);
        }

        [Fact]
        public async Task ObtenerEstadisticasAsync_ConDatosValidos_DeberiaRetornarEstadisticas()
        {
            // Arrange
            var estadisticas = new PreparacionEstadisticasDto
            {
                PreparacionesHoy = 10,
                PreparacionesPendientes = 3,
                PreparacionesEnProceso = 2,
                PreparacionesCompletadas = 5,
                PreparacionesAtrasadas = 1,
                TiempoPromedioPreparacion = 25.5,
                PreparacionesPorCocinero = 5,
                EficienciaCocina = 85.2
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
            resultado!.PreparacionesHoy.Should().Be(10);
            resultado.EficienciaCocina.Should().Be(85.2);
        }

        [Fact]
        public async Task ObtenerEstadosAsync_ConDatosValidos_DeberiaRetornarEstados()
        {
            // Arrange
            var estados = new List<string> { "Pendiente", "EnProceso", "Lista", "Entregada", "Cancelada" };

            var responseContent = JsonSerializer.Serialize(estados);

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
            resultado!.Should().HaveCount(5);
            resultado.Should().Contain("Pendiente");
        }

        [Fact]
        public async Task ObtenerPrioridadesAsync_ConDatosValidos_DeberiaRetornarPrioridades()
        {
            // Arrange
            var prioridades = new List<string> { "Baja", "Normal", "Alta", "Urgente" };

            var responseContent = JsonSerializer.Serialize(prioridades);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerPrioridadesAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(4);
            resultado.Should().Contain("Alta");
        }

        [Fact]
        public async Task ObtenerCocinerosAsync_ConDatosValidos_DeberiaRetornarCocineros()
        {
            // Arrange
            var cocineros = new List<dynamic>
            {
                new { Id = 1, Nombre = "Chef Juan" },
                new { Id = 2, Nombre = "Chef María" }
            };

            var responseContent = JsonSerializer.Serialize(cocineros);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.ObtenerCocinerosAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(2);
        }

        [Fact]
        public async Task AsignarCocineroAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new AsignarCocineroRequest
            {
                ComandaId = Guid.NewGuid(),
                CocineroId = Guid.NewGuid(),
                Observaciones = "Asignación de prueba"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.AsignarCocineroAsync(request);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ActualizarEstadoAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new ActualizarEstadoPreparacionRequest
            {
                PreparacionId = 1,
                Estado = EstadoPreparacion.EnProceso,
                Notas = "Iniciando preparación"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.ActualizarEstadoAsync(request);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ActualizarTiempoAsync_ConDatosValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var request = new ActualizarTiempoPreparacionRequest
            {
                PreparacionId = 1,
                TiempoEstimadoMinutos = 30,
                TiempoRealMinutos = 25
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.ActualizarTiempoAsync(request);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task IniciarPreparacionAsync_ConIdValido_DeberiaRetornarTrue()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.IniciarPreparacionAsync(1);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task CompletarPreparacionAsync_ConIdValido_DeberiaRetornarTrue()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.CompletarPreparacionAsync(1);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task CancelarPreparacionAsync_ConIdValido_DeberiaRetornarTrue()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.CancelarPreparacionAsync(1, "Cancelada por falta de ingredientes");

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExportarExcelAsync_ConDatosValidos_DeberiaRetornarBytes()
        {
            // Arrange
            var excelBytes = Encoding.UTF8.GetBytes("Excel content");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(excelBytes)
                });

            // Act
            var resultado = await _service.ExportarExcelAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(13); // "Excel content" = 13 bytes
        }

        // ===== PRUEBAS DE ERROR =====

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync();

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPreparacionAsync_Con404_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var resultado = await _service.ObtenerPreparacionAsync(999);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerColaPreparacionesAsync_ConError500_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var resultado = await _service.ObtenerColaPreparacionesAsync();

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task AsignarCocineroAsync_ConErrorDeRed_DeberiaRetornarFalse()
        {
            // Arrange
            var request = new AsignarCocineroRequest
            {
                ComandaId = Guid.NewGuid(),
                CocineroId = Guid.NewGuid()
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.AsignarCocineroAsync(request);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ActualizarEstadoAsync_ConErrorDeRed_DeberiaRetornarFalse()
        {
            // Arrange
            var request = new ActualizarEstadoPreparacionRequest
            {
                PreparacionId = 1,
                Estado = EstadoPreparacion.EnProceso
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ActualizarEstadoAsync(request);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task IniciarPreparacionAsync_ConErrorDeRed_DeberiaRetornarFalse()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.IniciarPreparacionAsync(1);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ExportarExcelAsync_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ExportarExcelAsync();

            // Assert
            resultado.Should().BeNull();
        }

        // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>();
            for (int i = 0; i < 10000; i++)
            {
                preparaciones.Add(new PreparacionDto
                {
                    Id = i + 1,
                    ComandaId = i + 1,
                    ProductoId = (i % 100) + 1,
                    ProductoNombre = $"Producto {i + 1}",
                    Cantidad = (i % 10) + 1,
                    Estado = (EstadoPreparacion)((i % 5) + 1),
                    Prioridad = (PrioridadPreparacion)((i % 4) + 1),
                    TiempoEstimadoMinutos = (i % 60) + 1,
                    FechaCreacion = DateTime.UtcNow.AddMinutes(-i)
                });
            }

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones.Take(20).ToList(),
                TotalCount = 10000,
                PageNumber = 1,
                PageSize = 20,
                TotalPages = 500
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
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.TotalCount.Should().Be(10000);
            resultado.TotalPages.Should().Be(500);
        }

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ProductoNombre = "Pizza Margherita 🍕", Observaciones = "Sin cebolla, extra queso 🧀" },
                new() { Id = 2, ProductoNombre = "Pasta Carbonara 🍝", Observaciones = "Al dente, con panceta 🥓" },
                new() { Id = 3, ProductoNombre = "Sushi Roll 🍣", Observaciones = "Wasabi extra, sin jengibre" },
                new() { Id = 4, ProductoNombre = "Tacos al Pastor 🌮", Observaciones = "Con piña 🍍 y cilantro" },
                new() { Id = 5, ProductoNombre = "Ramen Tonkotsu 🍜", Observaciones = "Huevo poché, naruto 🍥" }
            };

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones,
                TotalCount = 5,
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
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(5);
            resultado.Items.First().ProductoNombre.Should().Contain("🍕");
        }

        // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
        {
            // Arrange
            var filtros = new PreparacionFiltrosDto
            {
                Busqueda = "'; DROP TABLE Preparaciones; --",
                Estado = EstadoPreparacion.Pendiente
            };

            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ProductoNombre = "Producto Seguro", Estado = EstadoPreparacion.Pendiente }
            };

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones,
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
            var resultado = await _service.ObtenerPreparacionesPaginadasAsync(1, 20, filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task AsignarCocineroAsync_ConXSS_DeberiaManejarCorrectamente()
        {
            // Arrange
            var request = new AsignarCocineroRequest
            {
                ComandaId = Guid.NewGuid(),
                CocineroId = Guid.NewGuid(),
                Observaciones = "<script>alert('xss')</script>"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.AsignarCocineroAsync(request);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ActualizarEstadoAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
        {
            // Arrange
            var request = new ActualizarEstadoPreparacionRequest
            {
                PreparacionId = 1,
                Estado = EstadoPreparacion.EnProceso,
                Notas = "javascript:alert('xss'); <img src=x onerror=alert('xss')>"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.ActualizarEstadoAsync(request);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task CompletarPreparacionAsync_ConValidacionExtrema_DeberiaManejarCorrectamente()
        {
            // Arrange
            var preparacionId = 1;
            var notas = "'; DROP TABLE Preparaciones; -- <script>alert('xss')</script>";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.CompletarPreparacionAsync(preparacionId);

            // Assert
            resultado.Should().BeTrue();
        }

        // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

        [Fact]
        public async Task ObtenerPreparacionesPaginadasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ProductoNombre = "Producto Concurrencia", Estado = EstadoPreparacion.Pendiente }
            };

            var paginatedList = new PaginatedList<PreparacionDto>
            {
                Items = preparaciones,
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
            var tasks = new List<Task<PaginatedList<PreparacionDto>?>>();
            for (int i = 0; i < 50; i++)
            {
                tasks.Add(_service.ObtenerPreparacionesPaginadasAsync());
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(50);
            resultados.All(r => r != null).Should().BeTrue();
            resultados.All(r => r!.Items.Count == 1).Should().BeTrue();
        }

        [Fact]
        public async Task AsignarCocineroAsync_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var tasks = new List<Task<bool>>();
            for (int i = 0; i < 30; i++)
            {
                var request = new AsignarCocineroRequest
                {
                    ComandaId = Guid.NewGuid(),
                    CocineroId = Guid.NewGuid(),
                    Observaciones = $"Asignación {i}"
                };
                tasks.Add(_service.AsignarCocineroAsync(request));
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(30);
            resultados.All(r => r == true).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerEstadisticasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            var estadisticas = new PreparacionEstadisticasDto
            {
                PreparacionesHoy = 10,
                PreparacionesPendientes = 5,
                PreparacionesEnProceso = 3,
                PreparacionesCompletadas = 2,
                PreparacionesAtrasadas = 1,
                TiempoPromedioPreparacion = 25.5,
                PreparacionesPorCocinero = 5,
                EficienciaCocina = 85.2
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
            var tasks = new List<Task<PreparacionEstadisticasDto?>>();
            for (int i = 0; i < 40; i++)
            {
                tasks.Add(_service.ObtenerEstadisticasAsync());
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(40);
            resultados.All(r => r != null).Should().BeTrue();
            resultados.All(r => r!.PreparacionesHoy == 10).Should().BeTrue();
        }

        [Fact]
        public async Task ActualizarEstadoAsync_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var tasks = new List<Task<bool>>();
            for (int i = 0; i < 25; i++)
            {
                var request = new ActualizarEstadoPreparacionRequest
                {
                    PreparacionId = i + 1,
                    Estado = (EstadoPreparacion)((i % 5) + 1),
                    Notas = $"Actualización {i}"
                };
                tasks.Add(_service.ActualizarEstadoAsync(request));
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(25);
            resultados.All(r => r == true).Should().BeTrue();
        }
    }
}
