using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using RestaurantePro.Web.Admin.UnitTests.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Web.Admin.UnitTests.Components;

public class ComandaFiltrosTests : TestContext
{
    private readonly Mock<IComandasApiService> _comandasApiMock;
    private readonly Mock<IClientesApiService> _clientesApiMock;
    private readonly Mock<IMesasApiService> _mesasApiMock;

    public ComandaFiltrosTests()
    {
        _comandasApiMock = new Mock<IComandasApiService>();
        _clientesApiMock = new Mock<IClientesApiService>();
        _mesasApiMock = new Mock<IMesasApiService>();

        Services.AddSingleton(_comandasApiMock.Object);
        Services.AddSingleton(_clientesApiMock.Object);
        Services.AddSingleton(_mesasApiMock.Object);
                Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/comandas"));

        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.log", _ => true);
    }

    [Fact]
    public void ComandaFiltros_ShouldRender()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveCorrectTitle()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.Find("h6").TextContent.Trim().Should().Be("Filtros de Búsqueda");
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveSearchInput()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input").Should().Contain(i => i.GetAttribute("placeholder") == "Número, cliente, mesero...");
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveAllFilterSelects()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("select").Should().HaveCount(8); // Estado, Prioridad, TipoComanda, Mesa, Cliente, Mesero, OrdenarPor, DireccionOrden
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveDateInputs()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='date']").Should().HaveCount(2); // FechaInicio, FechaFin
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveNumberInputs()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='number']").Should().HaveCount(2); // TiempoMinimo, TiempoMaximo
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveCheckboxes()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='checkbox']").Should().HaveCount(3); // esUrgente, esDomicilio, esLenta
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveActionButtons()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("button").Should().HaveCount(4); // Limpiar, Últimas 24 horas, Solo Urgentes, Buscar
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Limpiar"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Últimas 24 horas"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Solo Urgentes"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Buscar"));
    }

    [Fact]
    public void ComandaFiltros_ShouldLoadEstados()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var estados = new List<string> { "Pendiente", "En Preparación", "Lista", "Completada" };

        _comandasApiMock.Setup(x => x.ObtenerEstadosAsync())
            .ReturnsAsync(estados);

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _comandasApiMock.Verify(x => x.ObtenerEstadosAsync(), Times.Once);
    }

    [Fact]
    public void ComandaFiltros_ShouldLoadPrioridades()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var prioridades = new List<string> { "Baja", "Media", "Alta", "Urgente" };

        _comandasApiMock.Setup(x => x.ObtenerPrioridadesAsync())
            .ReturnsAsync(prioridades);

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _comandasApiMock.Verify(x => x.ObtenerPrioridadesAsync(), Times.Once);
    }

    [Fact]
    public void ComandaFiltros_ShouldLoadTiposComanda()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var tiposComanda = new List<string> { "Presencial", "Domicilio", "Para Llevar" };

        _comandasApiMock.Setup(x => x.ObtenerTiposComandaAsync())
            .ReturnsAsync(tiposComanda);

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _comandasApiMock.Verify(x => x.ObtenerTiposComandaAsync(), Times.Once);
    }

    [Fact]
    public void ComandaFiltros_ShouldLoadClientes()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var clientes = new List<ClienteDto>
        {
            new ClienteDto { Id = Guid.NewGuid(), Nombre = "Juan", Apellidos = "Pérez" },
            new ClienteDto { Id = Guid.NewGuid(), Nombre = "María", Apellidos = "García" }
        };

        _clientesApiMock.Setup(x => x.ObtenerClientesAsync())
            .ReturnsAsync(clientes);

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _clientesApiMock.Verify(x => x.ObtenerClientesAsync(), Times.Once);
    }

    [Fact]
    public void ComandaFiltros_ShouldLoadMesas()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "1" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "2" }
        };

        _mesasApiMock.Setup(x => x.ObtenerAsync(null, null, null))
            .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _mesasApiMock.Verify(x => x.ObtenerAsync(null, null, null), Times.Once);
    }

    [Fact]
    public void ComandaFiltros_ShouldHandleClearFilters()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto
        {
            Busqueda = "test",
            Estado = "Pendiente",
            Prioridad = "Alta"
        };
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        var clearButton = component.Find("button[type='button']");
        clearButton.Click();

        // Assert
        // Verificar que los filtros se han limpiado
        component.Instance.Filtros.Busqueda.Should().BeNullOrEmpty();
        component.Instance.Filtros.Estado.Should().BeNullOrEmpty();
        component.Instance.Filtros.Prioridad.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ComandaFiltros_ShouldHandleQuickFilters()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var callbackInvoked = false;

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, EventCallback.Factory.Create<ComandaFiltrosDto>(this, _ => callbackInvoked = true)));

        var quickFilterButton = component.FindAll("button[type='button']").FirstOrDefault(b => b.TextContent.Contains("Últimas 24 horas"));
        quickFilterButton?.Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void ComandaFiltros_ShouldHandleUrgentFilters()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();
        var callbackInvoked = false;

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, EventCallback.Factory.Create<ComandaFiltrosDto>(this, _ => callbackInvoked = true)));

        var urgentFilterButton = component.FindAll("button[type='button']").FirstOrDefault(b => b.TextContent.Contains("Solo Urgentes"));
        urgentFilterButton?.Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void ComandaFiltros_ShouldShowLoadingState()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Aplicando, true));

        // Assert
        component.FindAll(".spinner-border").Should().HaveCountGreaterThan(0);
        component.Find("button[type='submit']").GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveCorrectFormStructure()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.Find("form").Should().NotBeNull();
        component.FindAll(".row").Should().HaveCountGreaterThan(0);
        component.FindAll(".col-md-4").Should().HaveCountGreaterThan(0);
        component.FindAll(".col-md-3").Should().HaveCountGreaterThan(0);
        component.FindAll(".col-md-6").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ComandaFiltros_ShouldHaveCorrectLabels()
    {
        // Arrange
        var filtros = new ComandaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<ComandaFiltrosDto>();

        // Act
        var component = RenderComponent<ComandaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Buscar"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Estado"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Prioridad"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Tipo de Comanda"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Mesa"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Cliente"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Mesero"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Fecha Inicio"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Fecha Fin"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Tiempo Mínimo"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Tiempo Máximo"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Ordenar Por"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Dirección"));
    }
}
