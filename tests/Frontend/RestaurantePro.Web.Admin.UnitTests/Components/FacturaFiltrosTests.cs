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

public class FacturaFiltrosTests : TestContext
{
    private readonly Mock<IFacturasApiService> _facturasApiMock;
    private readonly Mock<IClientesApiService> _clientesApiMock;
    private readonly Mock<IMesasApiService> _mesasApiMock;
    private readonly Mock<IUsuariosApiService> _usuariosApiMock;

    public FacturaFiltrosTests()
    {
        _facturasApiMock = new Mock<IFacturasApiService>();
        _clientesApiMock = new Mock<IClientesApiService>();
        _mesasApiMock = new Mock<IMesasApiService>();
        _usuariosApiMock = new Mock<IUsuariosApiService>();

        Services.AddSingleton(_facturasApiMock.Object);
        Services.AddSingleton(_clientesApiMock.Object);
        Services.AddSingleton(_mesasApiMock.Object);
        Services.AddSingleton(_usuariosApiMock.Object);
                Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/facturas"));

        // Configurar JSInterop para manejar llamadas JavaScript
        JSInterop.SetupVoid("console.log", _ => true);
    }

    [Fact]
    public void FacturaFiltros_ShouldRender()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCorrectTitle()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.Find("h6").TextContent.Trim().Should().Be("Filtros de Búsqueda");
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveSearchInput()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input").Should().Contain(i => i.GetAttribute("placeholder") == "Número, cliente, mesero...");
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveAllFilterSelects()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("select").Should().HaveCount(8); // Estado, TipoPago, Cliente, Mesa, Mesero, MetodoPago, OrdenarPor, DireccionOrden
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveDateInputs()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='date']").Should().HaveCount(2); // FechaInicio, FechaFin
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveNumberInputs()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='number']").Should().HaveCount(2); // MontoMinimo, MontoMaximo
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCheckbox()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input[type='checkbox']").Should().HaveCount(1); // esFacturaElectronica
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveActionButtons()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("button").Should().HaveCount(3); // Limpiar, Últimos 7 días, Buscar
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Limpiar"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Últimos 7 días"));
        component.FindAll("button").Should().Contain(b => b.TextContent.Contains("Buscar"));
    }

    [Fact]
    public void FacturaFiltros_ShouldLoadClientes()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();
        var clientes = new PaginatedList<ClienteDto>
        {
            Items = new List<ClienteDto>
            {
                new ClienteDto { Id = Guid.NewGuid(), Nombre = "Juan", Apellidos = "Pérez" },
                new ClienteDto { Id = Guid.NewGuid(), Nombre = "María", Apellidos = "García" }
            }
        };

        _clientesApiMock.Setup(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()))
            .ReturnsAsync(clientes);

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _clientesApiMock.Verify(x => x.ObtenerClientesAsync(It.IsAny<ClienteFiltrosDto>()), Times.Once);
    }

    [Fact]
    public void FacturaFiltros_ShouldLoadMesas()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "1" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "2" }
        };

        _mesasApiMock.Setup(x => x.ObtenerAsync(null, null, null))
            .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        _mesasApiMock.Verify(x => x.ObtenerAsync(null, null, null), Times.Once);
    }

    [Fact]
    public void FacturaFiltros_ShouldHandleClearFilters()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto
        {
            Busqueda = "test",
            Estado = "Pagada",
            TipoPago = "Efectivo"
        };
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        var clearButton = component.Find("button[type='button']");
        clearButton.Click();

        // Assert
        // Verificar que los filtros se han limpiado
        component.Instance.Filtros.Busqueda.Should().BeNullOrEmpty();
        component.Instance.Filtros.Estado.Should().BeNullOrEmpty();
        component.Instance.Filtros.TipoPago.Should().BeNullOrEmpty();
    }

    [Fact]
    public void FacturaFiltros_ShouldHandleQuickFilters()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();
        var callbackInvoked = false;

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, EventCallback.Factory.Create<FacturaFiltrosDto>(this, _ => callbackInvoked = true)));

        var quickFilterButton = component.FindAll("button[type='button']").FirstOrDefault(b => b.TextContent.Contains("Últimos 7 días"));
        quickFilterButton?.Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void FacturaFiltros_ShouldShowLoadingState()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Aplicando, true));

        // Assert
        component.FindAll(".spinner-border").Should().HaveCountGreaterThan(0);
        component.Find("button[type='submit']").GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCorrectFormStructure()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
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
    public void FacturaFiltros_ShouldHaveCorrectLabels()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Buscar"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Estado"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Tipo de Pago"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Cliente"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Mesa"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Mesero"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Método de Pago"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Fecha Inicio"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Fecha Fin"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Monto Mínimo"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Monto Máximo"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Ordenar Por"));
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Dirección"));
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCorrectPlaceholders()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("input").Should().Contain(i => i.GetAttribute("placeholder") == "0");
        component.FindAll("input").Should().Contain(i => i.GetAttribute("placeholder") == "10000");
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCorrectOrderOptions()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Fecha de Emisión"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Número de Factura"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Cliente"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Total"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Estado"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Tipo de Pago"));
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveCorrectDirectionOptions()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Descendente"));
        component.FindAll("option").Should().Contain(o => o.TextContent.Contains("Ascendente"));
    }

    [Fact]
    public void FacturaFiltros_ShouldHaveElectronicInvoiceCheckbox()
    {
        // Arrange
        var filtros = new FacturaFiltrosDto();
        var onFiltrosAplicados = new EventCallback<FacturaFiltrosDto>();

        // Act
        var component = RenderComponent<FacturaFiltros>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados));

        // Assert
        component.FindAll("label").Should().Contain(l => l.TextContent.Contains("Solo Facturas Electrónicas"));
    }
}
