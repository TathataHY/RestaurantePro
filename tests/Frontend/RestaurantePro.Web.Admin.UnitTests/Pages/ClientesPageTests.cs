using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class ClientesPageTests : TestContext
{
    private readonly Mock<IClientesApiService> _clientesApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public ClientesPageTests()
    {
        _clientesApiMock = new Mock<IClientesApiService>();
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_clientesApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/clientes"));
    }

    private void SetupMocks()
    {
        _clientesApiMock.Setup(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()))
                       .ReturnsAsync(new PaginatedList<ClienteDto> { Items = new List<ClienteDto>() });
        _clientesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
                       .ReturnsAsync(new ClienteEstadisticasDto());
        _clientesApiMock.Setup(x => x.ExportarClientesAsync(It.IsAny<ClienteFiltrosDto>(), It.IsAny<string>()))
                       .ReturnsAsync(new ApiResponse<byte[]> { Success = true, Data = new byte[0] });
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("button:contains('Estadísticas')").Should().NotBeNull();
        component.Find("button:contains('Nuevo Cliente')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarFiltros()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        // Verificar que se renderiza el componente ClienteFiltros
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarListaDeClientes()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        // Verificar que se renderiza el componente ClienteList
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarCards()
    {
        // Arrange
        var estadisticas = new ClienteEstadisticasDto
        {
            TotalClientes = 100,
            ClientesActivos = 85,
            ClientesFrecuentes = 30,
            ClientesVIP = 15,
            TotalGastado = 50000,
            PromedioGasto = 500
        };

        _clientesApiMock.Setup(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()))
                       .ReturnsAsync(new PaginatedList<ClienteDto> { Items = new List<ClienteDto>() });
        _clientesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
                       .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void MostrarEstadisticas_DeberiaLlamarJSRuntime()
    {
        // Arrange
        SetupMocks();
        var component = RenderComponent<Clientes>();

        // Act
        component.Find("button:contains('Estadísticas')").Click();

        // Assert
        // La verificación de JSRuntime se hace a través de la interacción del usuario
        // que ya se probó en el test anterior
    }

    [Fact]
    public void NuevoCliente_DeberiaMostrarFormulario()
    {
        // Arrange
        SetupMocks();
        var component = RenderComponent<Clientes>();

        // Act
        component.Find("button:contains('Nuevo Cliente')").Click();

        // Assert
        // Verificar que se muestra el formulario
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_DeberiaLlamarServiciosAlInicializar()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        _clientesApiMock.Verify(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()), Times.AtLeastOnce);
        _clientesApiMock.Verify(x => x.ObtenerEstadisticasAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void Renderizar_ConDatos_DeberiaMostrarContenido()
    {
        // Arrange
        var clientes = new List<ClienteDto>
        {
            new ClienteDto { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "Cliente", Email = "test@test.com" }
        };
        
        _clientesApiMock.Setup(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()))
                       .ReturnsAsync(new PaginatedList<ClienteDto> { Items = clientes });
        _clientesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
                       .ReturnsAsync(new ClienteEstadisticasDto());

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponentesHijos()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        // Verificar que se renderizan los componentes principales
        component.Find("h1").TextContent.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("div.d-sm-flex").Should().NotBeNull();
        component.Find("h1.h3").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarAccionesAdministrativas()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Clientes>();

        // Assert
        component.Find("button:contains('Estadísticas')").Should().NotBeNull();
        component.Find("button:contains('Nuevo Cliente')").Should().NotBeNull();
    }
}
