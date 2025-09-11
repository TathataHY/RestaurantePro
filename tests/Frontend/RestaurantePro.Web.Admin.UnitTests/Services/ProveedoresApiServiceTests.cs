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
    public class ProveedoresApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly TokenStore _tokenStore;
        private readonly ProveedoresApiService _service;

        public ProveedoresApiServiceTests()
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

            _service = new ProveedoresApiService(_httpClientFactoryMock.Object, _tokenStore);
        }

        // ===== PRUEBAS BÁSICAS - PROVEEDORES =====

        [Fact]
        public async Task GetProveedoresPaginados_ConDatosValidos_DeberiaRetornarProveedores()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 1", Ruc = "12345678901", Ciudad = "Quito", EstaActivo = true },
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 2", Ruc = "98765432109", Ciudad = "Guayaquil", EstaActivo = true }
            };

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
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
            var resultado = await _service.GetProveedoresPaginados();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(2);
            resultado.Items.First().Nombre.Should().Be("Proveedor 1");
        }

        [Fact]
        public async Task GetProveedoresPaginados_ConFiltros_DeberiaRetornarProveedoresFiltrados()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto
            {
                Nombre = "Proveedor",
                Ciudad = "Quito",
                EstaActivo = true,
                FechaCreacionDesde = DateTime.Today.AddDays(-30),
                FechaCreacionHasta = DateTime.Today
            };

            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor Filtrado", Ciudad = "Quito", EstaActivo = true }
            };

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
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
            var resultado = await _service.GetProveedoresPaginados(1, 20, filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetProveedorById_ConIdValido_DeberiaRetornarProveedor()
        {
            // Arrange
            var id = Guid.NewGuid();
            var proveedor = new ProveedorDto
            {
                Id = id,
                Nombre = "Proveedor Test",
                Ruc = "12345678901",
                Ciudad = "Quito",
                Pais = "Ecuador",
                Telefono = "02-1234567",
                Email = "test@proveedor.com",
                EstaActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(proveedor);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.GetProveedorById(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Proveedor Test");
            resultado.Ciudad.Should().Be("Quito");
        }

        [Fact]
        public async Task CreateProveedor_ConDatosValidos_DeberiaRetornarProveedorCreado()
        {
            // Arrange
            var request = new CrearProveedorRequest
            {
                Nombre = "Nuevo Proveedor",
                Ruc = "12345678901",
                Ciudad = "Quito",
                Pais = "Ecuador",
                Telefono = "02-1234567",
                Email = "nuevo@proveedor.com",
                EstaActivo = true
            };

            var proveedorCreado = new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Ruc = request.Ruc,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                Telefono = request.Telefono,
                Email = request.Email,
                EstaActivo = request.EstaActivo,
                FechaCreacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(proveedorCreado);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.CreateProveedor(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Nuevo Proveedor");
            resultado.Ciudad.Should().Be("Quito");
        }

        [Fact]
        public async Task UpdateProveedor_ConDatosValidos_DeberiaRetornarProveedorActualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new CrearProveedorRequest
            {
                Nombre = "Proveedor Actualizado",
                Ruc = "98765432109",
                Ciudad = "Guayaquil",
                Pais = "Ecuador",
                Telefono = "04-7654321",
                Email = "actualizado@proveedor.com",
                EstaActivo = true
            };

            var proveedorActualizado = new ProveedorDto
            {
                Id = id,
                Nombre = request.Nombre,
                Ruc = request.Ruc,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                Telefono = request.Telefono,
                Email = request.Email,
                EstaActivo = request.EstaActivo,
                FechaCreacion = DateTime.UtcNow.AddDays(-30),
                FechaActualizacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(proveedorActualizado);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.UpdateProveedor(id, request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Proveedor Actualizado");
            resultado.Ciudad.Should().Be("Guayaquil");
        }

        [Fact]
        public async Task DeleteProveedor_ConIdValido_DeberiaRetornarTrue()
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
            var resultado = await _service.DeleteProveedor(id);

            // Assert
            resultado.Should().BeTrue();
        }

        // ===== PRUEBAS BÁSICAS - CONTACTOS =====

        [Fact]
        public async Task GetContactosProveedor_ConIdValido_DeberiaRetornarContactos()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var contactos = new List<ContactoProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Juan", Apellido = "Pérez", Cargo = "Gerente", Telefono = "02-1234567", Email = "juan@proveedor.com", ProveedorId = proveedorId },
                new() { Id = Guid.NewGuid(), Nombre = "María", Apellido = "González", Cargo = "Vendedor", Telefono = "02-7654321", Email = "maria@proveedor.com", ProveedorId = proveedorId }
            };

            var responseContent = JsonSerializer.Serialize(contactos);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.GetContactosProveedor(proveedorId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(2);
            resultado.First().Nombre.Should().Be("Juan");
        }

        [Fact]
        public async Task CreateContactoProveedor_ConDatosValidos_DeberiaRetornarContactoCreado()
        {
            // Arrange
            var request = new CrearContactoProveedorRequest
            {
                Nombre = "Carlos",
                Apellido = "Rodríguez",
                Cargo = "Representante",
                Telefono = "02-9876543",
                Email = "carlos@proveedor.com",
                Celular = "0998765432",
                EsContactoPrincipal = true,
                ProveedorId = Guid.NewGuid()
            };

            var contactoCreado = new ContactoProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Cargo = request.Cargo,
                Telefono = request.Telefono,
                Email = request.Email,
                Celular = request.Celular,
                EsContactoPrincipal = request.EsContactoPrincipal,
                ProveedorId = request.ProveedorId,
                FechaCreacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(contactoCreado);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.CreateContactoProveedor(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Carlos");
            resultado.EsContactoPrincipal.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateContactoProveedor_ConDatosValidos_DeberiaRetornarContactoActualizado()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var contactoId = Guid.NewGuid();
            var request = new CrearContactoProveedorRequest
            {
                Nombre = "Carlos Actualizado",
                Apellido = "Rodríguez",
                Cargo = "Gerente de Ventas",
                Telefono = "02-1111111",
                Email = "carlos.actualizado@proveedor.com",
                Celular = "0991111111",
                EsContactoPrincipal = true,
                ProveedorId = proveedorId
            };

            var contactoActualizado = new ContactoProveedorDto
            {
                Id = contactoId,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Cargo = request.Cargo,
                Telefono = request.Telefono,
                Email = request.Email,
                Celular = request.Celular,
                EsContactoPrincipal = request.EsContactoPrincipal,
                ProveedorId = proveedorId,
                FechaCreacion = DateTime.UtcNow.AddDays(-10),
                FechaActualizacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(contactoActualizado);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.UpdateContactoProveedor(proveedorId, contactoId, request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Carlos Actualizado");
            resultado.Cargo.Should().Be("Gerente de Ventas");
        }

        [Fact]
        public async Task DeleteContactoProveedor_ConIdsValidos_DeberiaRetornarTrue()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var contactoId = Guid.NewGuid();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var resultado = await _service.DeleteContactoProveedor(proveedorId, contactoId);

            // Assert
            resultado.Should().BeTrue();
        }

        // ===== PRUEBAS BÁSICAS - ESTADÍSTICAS Y REPORTES =====

        [Fact]
        public async Task GetProveedorEstadisticas_ConDatosValidos_DeberiaRetornarEstadisticas()
        {
            // Arrange
            var estadisticas = new ProveedorEstadisticasDto
            {
                TotalProveedores = 50,
                ProveedoresActivos = 45,
                ProveedoresInactivos = 5,
                TotalContactos = 120,
                TotalOrdenesCompra = 300,
                MontoTotalCompras = 150000.50m,
                MontoPromedioCompras = 500.00m,
                ProveedoresConContactos = 40,
                ProveedoresSinContactos = 10,
                ProveedoresConOrdenes = 35,
                ProveedoresSinOrdenes = 15,
                TopProveedores = new List<ProveedorTopDto>
                {
                    new() { Id = Guid.NewGuid(), Nombre = "Top Proveedor 1", MontoTotalCompras = 25000.00m, TotalOrdenes = 50 }
                },
                ProveedoresPorCiudad = new List<ProveedorPorCiudadDto>
                {
                    new() { Ciudad = "Quito", Cantidad = 25, MontoTotal = 75000.00m }
                }
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
            var resultado = await _service.GetProveedorEstadisticas();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.TotalProveedores.Should().Be(50);
            resultado.ProveedoresActivos.Should().Be(45);
            resultado.MontoTotalCompras.Should().Be(150000.50m);
        }

        [Fact]
        public async Task ExportarProveedoresExcel_ConDatosValidos_DeberiaRetornarBytes()
        {
            // Arrange
            var request = new ExportarProveedoresRequest
            {
                Filtros = new ProveedorFiltrosDto { EstaActivo = true },
                Columnas = new List<string> { "Nombre", "Ruc", "Ciudad", "Email" },
                Formato = "Excel"
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
            var resultado = await _service.ExportarProveedoresExcel(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Should().HaveCount(13); // "Excel content" = 13 bytes
        }

        // ===== PRUEBAS DE ERROR =====

        [Fact]
        public async Task GetProveedoresPaginados_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.GetProveedoresPaginados();

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetProveedorById_Con404_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var resultado = await _service.GetProveedorById(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task CreateProveedor_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var request = new CrearProveedorRequest
            {
                Nombre = "Test Proveedor"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.CreateProveedor(request);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProveedor_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var request = new CrearProveedorRequest
            {
                Nombre = "Test Proveedor"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.UpdateProveedor(Guid.NewGuid(), request);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task DeleteProveedor_ConErrorDeRed_DeberiaRetornarFalse()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.DeleteProveedor(Guid.NewGuid());

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task GetContactosProveedor_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.GetContactosProveedor(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task CreateContactoProveedor_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var request = new CrearContactoProveedorRequest
            {
                Nombre = "Test Contacto",
                ProveedorId = Guid.NewGuid()
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.CreateContactoProveedor(request);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetProveedorEstadisticas_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.GetProveedorEstadisticas();

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExportarProveedoresExcel_ConErrorDeRed_DeberiaRetornarNull()
        {
            // Arrange
            var request = new ExportarProveedoresRequest();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection lost"));

            // Act
            var resultado = await _service.ExportarProveedoresExcel(request);

            // Assert
            resultado.Should().BeNull();
        }

        // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

        [Fact]
        public async Task GetProveedoresPaginados_ConDatosMasivos_DeberiaManejarCorrectamente()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>();
            for (int i = 0; i < 10000; i++)
            {
                proveedores.Add(new ProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = $"Proveedor {i + 1}",
                    Ruc = $"{i:D13}",
                    Ciudad = $"Ciudad {i % 100}",
                    Pais = $"País {i % 50}",
                    Telefono = $"+593{i:D9}",
                    Email = $"proveedor{i}@email.com",
                    EstaActivo = i % 2 == 0,
                    FechaCreacion = DateTime.UtcNow.AddDays(-i),
                    MontoTotalCompras = (decimal)(i * 100.50),
                    TotalOrdenesCompra = i % 100
                });
            }

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores.Take(20).ToList(),
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
            var resultado = await _service.GetProveedoresPaginados();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.TotalCount.Should().Be(10000);
            resultado.TotalPages.Should().Be(500);
        }

        [Fact]
        public async Task GetProveedoresPaginados_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 🍕 Pizza", Ciudad = "Quito 🇪🇨", Observaciones = "Especialista en ingredientes italianos 🧀" },
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 🍣 Sushi", Ciudad = "Guayaquil 🌊", Observaciones = "Pescado fresco del Pacífico 🐟" },
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 🌮 Tacos", Ciudad = "Cuenca 🏔️", Observaciones = "Especias auténticas mexicanas 🌶️" },
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 🍜 Ramen", Ciudad = "Ambato 🌸", Observaciones = "Fideos artesanales japoneses 🥢" },
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor 🥘 Curry", Ciudad = "Manta 🏖️", Observaciones = "Especias exóticas de la India 🍛" }
            };

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
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
            var resultado = await _service.GetProveedoresPaginados();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(5);
            resultado.Items.First().Nombre.Should().Contain("🍕");
        }

        // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

        [Fact]
        public async Task GetProveedoresPaginados_ConInyeccionSQL_DeberiaManejarCorrectamente()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto
            {
                Nombre = "'; DROP TABLE Proveedores; --",
                Ciudad = "'; DROP TABLE Ciudades; --",
                Pais = "'; DROP TABLE Paises; --"
            };

            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor Seguro", Ciudad = "Quito", Pais = "Ecuador" }
            };

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
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
            var resultado = await _service.GetProveedoresPaginados(1, 20, filtros);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateProveedor_ConXSS_DeberiaManejarCorrectamente()
        {
            // Arrange
            var request = new CrearProveedorRequest
            {
                Nombre = "<script>alert('xss')</script>",
                Observaciones = "javascript:alert('xss'); <img src=x onerror=alert('xss')>",
                Ciudad = "<svg onload=alert('xss')>",
                Pais = "';alert('xss');//"
            };

            var proveedorCreado = new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Observaciones = request.Observaciones,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                FechaCreacion = DateTime.UtcNow
            };

            var responseContent = JsonSerializer.Serialize(proveedorCreado);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var resultado = await _service.CreateProveedor(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Contain("<script>");
        }

        // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

        [Fact]
        public async Task GetProveedoresPaginados_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Proveedor Concurrencia", Ciudad = "Quito", EstaActivo = true }
            };

            var paginatedList = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
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
            var tasks = new List<Task<PaginatedList<ProveedorDto>?>>();
            for (int i = 0; i < 50; i++)
            {
                tasks.Add(_service.GetProveedoresPaginados());
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(50);
            resultados.All(r => r != null).Should().BeTrue();
            resultados.All(r => r!.Items.Count == 1).Should().BeTrue();
        }

        [Fact]
        public async Task CreateProveedor_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Test" }), Encoding.UTF8, "application/json")
                });

            // Act
            var tasks = new List<Task<ProveedorDto?>>();
            for (int i = 0; i < 30; i++)
            {
                var request = new CrearProveedorRequest
                {
                    Nombre = $"Proveedor {i}",
                    Ciudad = "Quito",
                    Pais = "Ecuador"
                };
                tasks.Add(_service.CreateProveedor(request));
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(30);
            resultados.All(r => r != null).Should().BeTrue();
        }

        [Fact]
        public async Task GetProveedorEstadisticas_ConConcurrencia_DeberiaManejarCorrectamente()
        {
            // Arrange
            var estadisticas = new ProveedorEstadisticasDto
            {
                TotalProveedores = 100,
                ProveedoresActivos = 80,
                ProveedoresInactivos = 20,
                TotalContactos = 250,
                TotalOrdenesCompra = 500,
                MontoTotalCompras = 150000.50m,
                MontoPromedioCompras = 300.00m
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
            var tasks = new List<Task<ProveedorEstadisticasDto?>>();
            for (int i = 0; i < 40; i++)
            {
                tasks.Add(_service.GetProveedorEstadisticas());
            }

            var resultados = await Task.WhenAll(tasks);

            // Assert
            resultados.Should().HaveCount(40);
            resultados.All(r => r != null).Should().BeTrue();
            resultados.All(r => r!.TotalProveedores == 100).Should().BeTrue();
        }
    }
}
