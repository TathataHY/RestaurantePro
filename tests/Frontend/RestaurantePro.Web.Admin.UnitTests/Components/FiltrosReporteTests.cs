using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using Xunit;
using Bunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components;

public class FiltrosReporteTests : TestContext
{
    private readonly Mock<IUsuariosApiService> _usuariosApiMock;
    private readonly Mock<IMesasApiService> _mesasApiMock;
    private readonly Mock<ICategoriasApiService> _categoriasApiMock;

    public FiltrosReporteTests()
    {
        _usuariosApiMock = new Mock<IUsuariosApiService>();
        _mesasApiMock = new Mock<IMesasApiService>();
        _categoriasApiMock = new Mock<ICategoriasApiService>();

        Services.AddSingleton(_usuariosApiMock.Object);
        Services.AddSingleton(_mesasApiMock.Object);
        Services.AddSingleton(_categoriasApiMock.Object);
    }

    [Fact]
    public void FiltrosReporte_ShouldLoadMeserosOnInitialized()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;
        var meseros = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Roles = new List<string> { "Mesero" } },
            new() { Id = Guid.NewGuid(), NombreCompleto = "María García", Roles = new List<string> { "Mesero" } }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(meseros);

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        _usuariosApiMock.Verify(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        Assert.Contains("Juan Pérez", component.Markup);
        Assert.Contains("María García", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldLoadMesasOnInitialized()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;
        var mesas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), NombreCliente = "Mesa 1" },
            new() { Id = Guid.NewGuid(), NombreCliente = "Mesa 2" }
        };

        _mesasApiMock.Setup(x => x.ObtenerMesasAsync())
            .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        _mesasApiMock.Verify(x => x.ObtenerMesasAsync(), Times.Once);
        Assert.Contains("Mesa 1", component.Markup);
        Assert.Contains("Mesa 2", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldLoadCategoriasOnInitialization()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Bebidas" },
            new() { Id = Guid.NewGuid(), Nombre = "Comidas" }
        };

        _categoriasApiMock.Setup(x => x.ObtenerCategoriasAsync())
            .ReturnsAsync(categorias);

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        _categoriasApiMock.Verify(x => x.ObtenerCategoriasAsync(), Times.Once);
        Assert.Contains("Bebidas", component.Markup);
        Assert.Contains("Comidas", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldFilterMeserosByRole()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;
        var usuarios = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Roles = new List<string> { "Mesero" } },
            new() { Id = Guid.NewGuid(), NombreCompleto = "María García", Roles = new List<string> { "Admin" } },
            new() { Id = Guid.NewGuid(), NombreCompleto = "Carlos López", Roles = new List<string> { "Mesero" } }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Juan Pérez", component.Markup);
        Assert.Contains("Carlos López", component.Markup);
        Assert.DoesNotContain("María García", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHandleApiErrorsGracefully()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("API Error"));
        _mesasApiMock.Setup(x => x.ObtenerMesasAsync())
            .ThrowsAsync(new Exception("API Error"));
        _categoriasApiMock.Setup(x => x.ObtenerCategoriasAsync())
            .ThrowsAsync(new Exception("API Error"));

        // Act & Assert
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Should not throw exception
        Assert.NotNull(component);
    }

    [Fact]
    public void FiltrosReporte_ShouldApplyFiltersWhenButtonClicked()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicadosCalled = false;
        var onFiltrosAplicados = EventCallback.Factory.Create<ReporteFiltrosDto>(
            this, (ReporteFiltrosDto filtros) => onFiltrosAplicadosCalled = true);

        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Act
        var applyButton = component.Find("button[type='submit']");
        applyButton.Click();

        // Assert
        Assert.True(onFiltrosAplicadosCalled);
    }

    [Fact]
    public void FiltrosReporte_ShouldShowLoadingStateWhenGenerating()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, true));

        // Assert
        Assert.Contains("spinner-border", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldDisableButtonWhenGenerating()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, true));

        // Assert
        var applyButton = component.Find("button[type='submit']");
        Assert.True(applyButton.HasAttribute("disabled"));
    }
}
