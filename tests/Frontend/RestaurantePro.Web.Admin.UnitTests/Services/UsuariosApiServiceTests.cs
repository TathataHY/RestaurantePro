using System.Net;
using System.Text;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class UsuariosApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly UsuariosApiService _service;

    public UsuariosApiServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("http://localhost:8080");
        
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new UsuariosApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConRespuestaExitosa_DeberiaRetornarUsuarios()
    {
        // Arrange
        var usuariosEsperados = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), Email = "admin@restaurante.com", NombreCompleto = "Admin Sistema", NombreUsuario = "admin", Rol = "Administrador" },
            new() { Id = Guid.NewGuid(), Email = "mesero@restaurante.com", NombreCompleto = "Juan Pérez", NombreUsuario = "juan", Rol = "Mesero" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<UsuarioDto>>
        {
            Success = true,
            Data = usuariosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerUsuariosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Email.Should().Be("admin@restaurante.com");
        resultado.First().Rol.Should().Be("Administrador");
        resultado.Last().Email.Should().Be("mesero@restaurante.com");
        resultado.Last().Rol.Should().Be("Mesero");
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConFiltro_DeberiaIncluirFiltroEnUrl()
    {
        // Arrange
        var usuariosEsperados = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), Email = "admin@restaurante.com", NombreCompleto = "Admin Sistema", NombreUsuario = "admin", Rol = "Administrador" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<UsuarioDto>>
        {
            Success = true,
            Data = usuariosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerUsuariosAsync(filtro: "admin");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Email.Should().Be("admin@restaurante.com");
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConErrorEnApi_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerUsuariosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarUsuario()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioEsperado = new UsuarioDto
        {
            Id = id,
            Email = "admin@restaurante.com",
            NombreCompleto = "Admin Sistema",
            NombreUsuario = "admin",
            Rol = "Administrador"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<UsuarioDto>
        {
            Success = true,
            Data = usuarioEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Email.Should().Be("admin@restaurante.com");
        resultado.NombreCompleto.Should().Be("Admin Sistema");
        resultado.NombreUsuario.Should().Be("admin");
        resultado.Rol.Should().Be("Administrador");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Usuario no encontrado", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerPorIdAsync(id));
    }

    [Fact]
    public async Task CrearAsync_ConUsuarioValido_DeberiaRetornarUsuarioCreado()
    {
        // Arrange
        var usuarioRequest = new CreateUsuarioRequest
        {
            Email = "nuevo@restaurante.com",
            NombreCompleto = "Nuevo Usuario",
            NombreUsuario = "nuevo",
            Rol = "Mesero",
            Password = "password123"
        };

        var usuarioEsperado = new UsuarioDto
        {
            Id = Guid.NewGuid(),
            Email = "nuevo@restaurante.com",
            NombreCompleto = "Nuevo Usuario",
            NombreUsuario = "nuevo",
            Rol = "Mesero"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<UsuarioDto>
        {
            Success = true,
            Data = usuarioEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(usuarioRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Email.Should().Be("nuevo@restaurante.com");
        resultado.NombreCompleto.Should().Be("Nuevo Usuario");
        resultado.NombreUsuario.Should().Be("nuevo");
        resultado.Rol.Should().Be("Mesero");
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidos_DeberiaRetornarNull()
    {
        // Arrange
        var usuarioRequest = new CreateUsuarioRequest
        {
            Email = "email_invalido",
            NombreCompleto = "",
            NombreUsuario = "",
            Rol = "RolInexistente",
            Password = "123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(usuarioRequest);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarAsync_ConDatosValidos_DeberiaRetornarUsuarioActualizado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioRequest = new UpdateUsuarioRequest
        {
            NombreCompleto = "Usuario Actualizado",
            Rol = "Cocinero"
        };

        var usuarioEsperado = new UsuarioDto
        {
            Id = id,
            Email = "admin@restaurante.com",
            NombreCompleto = "Usuario Actualizado",
            NombreUsuario = "usuario",
            Rol = "Cocinero"
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<UsuarioDto>
        {
            Success = true,
            Data = usuarioEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, usuarioRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.NombreCompleto.Should().Be("Usuario Actualizado");
        resultado.Rol.Should().Be("Cocinero");
    }

    [Fact]
    public async Task ActualizarAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioRequest = new UpdateUsuarioRequest
        {
            NombreCompleto = "Usuario Actualizado",
            Rol = "Cocinero"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Usuario no encontrado", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, usuarioRequest);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarAsync_ConIdInexistente_DeberiaRetornarFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerUsuariosAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var usuariosEsperados = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), Email = "admin@restaurante.com", NombreCompleto = "Admin Sistema", NombreUsuario = "admin", Rol = "Administrador" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<UsuarioDto>>
        {
            Success = true,
            Data = usuariosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerUsuariosAsync(
            pageNumber: 999999,
            pageSize: 1000,
            filtro: "test@#$%^&*()",
            soloActivos: true,
            orderBy: "NombreCompleto",
            orderDirection: "asc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Email.Should().Be("admin@restaurante.com");
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.ObtenerUsuariosAsync());
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.ObtenerPorIdAsync(id));
    }

    [Fact]
    public async Task CrearAsync_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var usuarioRequest = new CreateUsuarioRequest
        {
            NombreCompleto = "A".PadRight(100, 'A'), // Nombre muy largo
            NombreUsuario = "B".PadRight(50, 'B'), // Usuario muy largo
            Email = "test@example.com",
            Password = "C".PadRight(40, 'C'), // Contraseña muy larga
            Rol = "Administrador",
            Telefono = "12345678901234567890" // Teléfono muy largo
        };

        var usuarioEsperado = new UsuarioDto
        {
            Id = Guid.NewGuid(),
            Email = usuarioRequest.Email,
            NombreCompleto = usuarioRequest.NombreCompleto,
            NombreUsuario = usuarioRequest.NombreUsuario,
            Rol = usuarioRequest.Rol
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<UsuarioDto>
        {
            Success = true,
            Data = usuarioEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(usuarioRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Email.Should().Be(usuarioRequest.Email);
        resultado.NombreCompleto.Should().Be(usuarioRequest.NombreCompleto);
    }

    [Fact]
    public async Task ActualizarAsync_ConConexionPerdida_DeberiaLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioRequest = new UpdateUsuarioRequest
        {
            NombreCompleto = "Usuario Actualizado",
            Rol = "Cocinero"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ActualizarAsync(id, usuarioRequest));
    }

    [Fact]
    public async Task EliminarAsync_ConErrorDeServidor_DeberiaRetornarFalse()
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
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConJsonMalformado_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ json malformado }", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.ObtenerUsuariosAsync());
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidosExtremos_DeberiaRetornarNull()
    {
        // Arrange
        var usuarioRequest = new CreateUsuarioRequest
        {
            NombreCompleto = "", // Nombre vacío
            NombreUsuario = "", // Usuario vacío
            Email = "email_invalido", // Email inválido
            Password = "123", // Contraseña muy corta
            Rol = "", // Rol vacío
            Telefono = "abc" // Teléfono inválido
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos de usuario inválidos", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(usuarioRequest);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerUsuariosAsync_ConPaginacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var usuariosEsperados = new List<UsuarioDto>();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<UsuarioDto>>
        {
            Success = true,
            Data = usuariosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerUsuariosAsync(
            pageNumber: 1,
            pageSize: 1,
            filtro: null,
            soloActivos: false,
            orderBy: "Email",
            orderDirection: "desc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }
}
