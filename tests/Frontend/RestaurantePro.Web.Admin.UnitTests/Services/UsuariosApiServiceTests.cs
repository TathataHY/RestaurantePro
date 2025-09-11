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
    public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaRetornarNull()
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

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
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
            Nombre = "Usuario Actualizado",
            Apellido = "Apellido Actualizado",
            Rol = "Cocinero",
            Activo = true
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
}
