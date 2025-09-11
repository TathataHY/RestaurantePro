using System.Net;
using System.Text;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class ClientesApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly ClientesApiService _service;

    public ClientesApiServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("http://localhost:8080");
        
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new ClientesApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ObtenerClientesAsync_ConFiltrosValidos_DeberiaRetornarClientesPaginados()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Busqueda = "Juan",
            OrdenarPor = "NombreCompleto",
            DireccionOrden = "asc"
        };

        var clientesEsperados = new PaginatedList<ClienteDto>
        {
            Items = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Juan", Apellidos = "Pérez", Email = "juan@email.com", Telefono = "123456789" },
                new() { Id = Guid.NewGuid(), Nombre = "Juan Carlos", Apellidos = "García", Email = "juancarlos@email.com", Telefono = "987654321" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ClienteDto>>
        {
            Success = true,
            Data = clientesEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerClientesAsync(filtros);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
        resultado.PageNumber.Should().Be(1);
        resultado.PageSize.Should().Be(10);
        resultado.Items.First().Nombre.Should().Be("Juan");
    }

    [Fact]
    public async Task ObtenerClientesAsync_ConErrorEnApi_DeberiaRetornarNull()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerClientesAsync(filtros);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerClienteAsync_ConIdValido_DeberiaRetornarCliente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clienteEsperado = new ClienteDto
        {
            Id = id,
            Nombre = "María",
            Apellidos = "González",
            Email = "maria@email.com",
            Telefono = "555555555",
            FechaNacimiento = new DateTime(1990, 5, 15),
            Direccion = "Calle Principal 123",
            Ciudad = "Madrid",
            CodigoPostal = "28001"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ClienteDto>
        {
            Success = true,
            Data = clienteEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerClienteAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Nombre.Should().Be("María");
        resultado.Apellidos.Should().Be("González");
        resultado.Email.Should().Be("maria@email.com");
    }

    [Fact]
    public async Task ObtenerClienteAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Cliente no encontrado", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerClienteAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearClienteAsync_ConDatosValidos_DeberiaRetornarClienteCreado()
    {
        // Arrange
        var clienteRequest = new CrearClienteRequest
        {
            Nombre = "Carlos",
            Apellidos = "López",
            Email = "carlos@email.com",
            Telefono = "666666666",
            FechaNacimiento = new DateTime(1985, 8, 20),
            Direccion = "Avenida Central 456",
            Ciudad = "Barcelona",
            CodigoPostal = "08001"
        };

        var clienteEsperado = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Carlos",
            Apellidos = "López",
            Email = "carlos@email.com",
            Telefono = "666666666"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ClienteDto>
        {
            Success = true,
            Data = clienteEsperado,
            Message = "Cliente creado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearClienteAsync(clienteRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Carlos");
        resultado.Data.Apellidos.Should().Be("López");
        resultado.Data.Email.Should().Be("carlos@email.com");
        resultado.Message.Should().Be("Cliente creado correctamente");
    }

    [Fact]
    public async Task CrearClienteAsync_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var clienteRequest = new CrearClienteRequest
        {
            Nombre = "",
            Apellidos = "",
            Email = "email_invalido",
            Telefono = "123"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ClienteDto>
        {
            Success = false,
            Message = "Datos de cliente inválidos"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearClienteAsync(clienteRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Be("Datos de cliente inválidos");
    }

    [Fact]
    public async Task ActualizarClienteAsync_ConDatosValidos_DeberiaRetornarClienteActualizado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clienteRequest = new ActualizarClienteRequest
        {
            Id = id,
            Nombre = "Carlos Actualizado",
            Apellidos = "López Actualizado",
            Email = "carlos.actualizado@email.com",
            Telefono = "777777777"
        };

        var clienteEsperado = new ClienteDto
        {
            Id = id,
            Nombre = "Carlos Actualizado",
            Apellidos = "López Actualizado",
            Email = "carlos.actualizado@email.com",
            Telefono = "777777777"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ClienteDto>
        {
            Success = true,
            Data = clienteEsperado,
            Message = "Cliente actualizado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarClienteAsync(clienteRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Id.Should().Be(id);
        resultado.Data.Nombre.Should().Be("Carlos Actualizado");
        resultado.Data.Apellidos.Should().Be("López Actualizado");
        resultado.Message.Should().Be("Cliente actualizado correctamente");
    }

    [Fact]
    public async Task EliminarClienteAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Cliente eliminado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.EliminarClienteAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
        resultado.Message.Should().Be("Cliente eliminado correctamente");
    }

    [Fact]
    public async Task ToggleActivarClienteAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Estado del cliente cambiado correctamente"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ToggleActivarClienteAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
        resultado.Message.Should().Be("Estado del cliente cambiado correctamente");
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConRespuestaExitosa_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticasEsperadas = new ClienteEstadisticasDto
        {
            TotalClientes = 150,
            ClientesActivos = 120,
            ClientesInactivos = 30,
            ClientesNuevos = 25,
            ClientesFrecuentes = 45,
            PromedioGasto = 85.50m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ClienteEstadisticasDto>
        {
            Success = true,
            Data = estadisticasEsperadas
        });

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
        resultado!.TotalClientes.Should().Be(150);
        resultado.ClientesActivos.Should().Be(120);
        resultado.ClientesInactivos.Should().Be(30);
        resultado.ClientesNuevos.Should().Be(25);
        resultado.PromedioGasto.Should().Be(85.50m);
    }

    [Fact]
    public async Task ValidarEmailAsync_ConEmailDisponible_DeberiaRetornarTrue()
    {
        // Arrange
        var email = "nuevo@email.com";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Email disponible"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarEmailAsync(email);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
        resultado.Message.Should().Be("Email disponible");
    }

    [Fact]
    public async Task ValidarEmailAsync_ConEmailExistente_DeberiaRetornarFalse()
    {
        // Arrange
        var email = "existente@email.com";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = false,
            Message = "Email ya existe"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarEmailAsync(email);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeFalse();
        resultado.Message.Should().Be("Email ya existe");
    }
}
