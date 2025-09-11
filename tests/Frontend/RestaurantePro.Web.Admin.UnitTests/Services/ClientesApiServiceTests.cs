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

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerClientesAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 999999,
            PageSize = 1000,
            Busqueda = "test@#$%^&*()",
            Segmento = "VIP",
            Ciudad = "Madrid",
            Estado = "Activo",
            NivelFidelizacion = "Oro",
            FechaInicio = DateTime.MinValue,
            FechaFin = DateTime.MaxValue,
            GastoMinimo = 0.01m,
            GastoMaximo = 999999.99m,
            VisitasMinimas = 0,
            AceptaMarketing = true,
            OrdenarPor = "TotalGastado",
            DireccionOrden = "desc"
        };

        var clientesEsperados = new PaginatedList<ClienteDto>
        {
            Items = new List<ClienteDto>(),
            TotalCount = 0,
            PageNumber = 999999,
            PageSize = 1000
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
        resultado!.PageNumber.Should().Be(999999);
        resultado.PageSize.Should().Be(1000);
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerClienteAsync_ConTimeout_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerClienteAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearClienteAsync_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var clienteRequest = new CrearClienteRequest
        {
            Nombre = "A".PadRight(100, 'A'), // Nombre muy largo
            Apellidos = "B".PadRight(100, 'B'), // Apellidos muy largos
            Email = "test@example.com",
            Telefono = "12345678901234567890", // Teléfono muy largo
            Direccion = "C".PadRight(500, 'C'), // Dirección muy larga
            Ciudad = "D".PadRight(100, 'D'), // Ciudad muy larga
            CodigoPostal = "12345678901234567890", // Código postal muy largo
            Pais = "E".PadRight(100, 'E'), // País muy largo
            FechaNacimiento = DateTime.MinValue,
            AceptaMarketing = true,
            AceptaTerminos = true,
            PreferenciasAlimentarias = "F".PadRight(1000, 'F'), // Preferencias muy largas
            Alergias = "G".PadRight(1000, 'G'), // Alergias muy largas
            NotasEspeciales = "H".PadRight(1000, 'H') // Notas muy largas
        };

        var clienteEsperado = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = clienteRequest.Nombre,
            Apellidos = clienteRequest.Apellidos,
            Email = clienteRequest.Email
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
        resultado.Data!.Nombre.Should().Be(clienteRequest.Nombre);
    }

    [Fact]
    public async Task ActualizarClienteAsync_ConConexionPerdida_DeberiaRetornarError()
    {
        // Arrange
        var clienteRequest = new ActualizarClienteRequest
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Actualizado",
            Apellidos = "Apellido Actualizado",
            Email = "cliente@email.com"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ActualizarClienteAsync(clienteRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Error al actualizar cliente");
    }

    [Fact]
    public async Task EliminarClienteAsync_ConErrorDeServidor_DeberiaRetornarError()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno del servidor", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.EliminarClienteAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Message.Should().Contain("Error al eliminar cliente");
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConDatosVacios_DeberiaRetornarEstadisticasVacias()
    {
        // Arrange
        var estadisticasEsperadas = new ClienteEstadisticasDto
        {
            TotalClientes = 0,
            ClientesActivos = 0,
            ClientesInactivos = 0,
            ClientesNuevos = 0,
            ClientesFrecuentes = 0,
            ClientesVIP = 0,
            TotalGastado = 0,
            PromedioGasto = 0,
            TotalCompras = 0,
            TotalVisitas = 0
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
        resultado!.TotalClientes.Should().Be(0);
        resultado.ClientesActivos.Should().Be(0);
        resultado.TotalGastado.Should().Be(0);
    }

    [Fact]
    public async Task ValidarEmailAsync_ConEmailInvalido_DeberiaRetornarError()
    {
        // Arrange
        var email = "email_invalido_sin_arroba";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = false,
            Data = false,
            Message = "Formato de email inválido"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarEmailAsync(email);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeFalse();
        resultado.Data.Should().BeFalse();
        resultado.Message.Should().Contain("Error al validar email");
    }

    [Fact]
    public async Task ExportarClientesAsync_ConFiltrosValidos_DeberiaRetornarArchivo()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 100,
            Busqueda = "test"
        };

        var archivoBytes = Encoding.UTF8.GetBytes("Excel file content");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(archivoBytes)
            });

        // Act
        var resultado = await _service.ExportarClientesAsync(filtros, "Excel");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data.Should().HaveCount(archivoBytes.Length);
        resultado.Message.Should().Be("Clientes exportados correctamente");
    }

    [Fact]
    public async Task BuscarClientesAsync_ConTerminoValido_DeberiaRetornarClientes()
    {
        // Arrange
        var request = new BuscarClienteRequest
        {
            Termino = "Juan",
            IncluirInactivos = false,
            Limite = 10
        };

        var clientesEsperados = new BuscarClienteResponse
        {
            Clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Apellidos = "García", Email = "juan@email.com" }
            },
            TotalEncontrados = 1,
            TieneMasResultados = false
        };

        var responseContent = JsonSerializer.Serialize(clientesEsperados);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.BuscarClientesAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Clientes.Should().HaveCount(1);
        resultado.TotalEncontrados.Should().Be(1);
        resultado.TieneMasResultados.Should().BeFalse();
    }
}
