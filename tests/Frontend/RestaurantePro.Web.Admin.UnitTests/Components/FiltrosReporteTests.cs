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
    public void FiltrosReporte_ShouldRenderCorrectly()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Filtros de Reporte", component.Markup);
        Assert.Contains("Tipo de Reporte", component.Markup);
        Assert.Contains("Fecha Inicio", component.Markup);
        Assert.Contains("Fecha Fin", component.Markup);
        Assert.Contains("Límite de Resultados", component.Markup);
        Assert.Contains("Mesero", component.Markup);
        Assert.Contains("Mesa", component.Markup);
        Assert.Contains("Categoría", component.Markup);
        Assert.Contains("Ordenar Por", component.Markup);
        Assert.Contains("Dirección", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectTipoReporteOptions()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Ventas por Período", component.Markup);
        Assert.Contains("Productos Más Vendidos", component.Markup);
        Assert.Contains("Rendimiento de Mesas", component.Markup);
        Assert.Contains("Resumen de Comandas", component.Markup);
        Assert.Contains("Ventas por Mesero", component.Markup);
        Assert.Contains("Ventas por Mesa", component.Markup);
        Assert.Contains("Ventas por Hora", component.Markup);
        Assert.Contains("Ventas por Día", component.Markup);
        Assert.Contains("Productos por Categoría", component.Markup);
        Assert.Contains("Comandas por Estado", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectOrdenarPorOptions()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Fecha", component.Markup);
        Assert.Contains("Total de Ventas", component.Markup);
        Assert.Contains("Cantidad", component.Markup);
        Assert.Contains("Nombre", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectDireccionOrdenOptions()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Descendente", component.Markup);
        Assert.Contains("Ascendente", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectButtons()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Limpiar", component.Markup);
        Assert.Contains("Últimos 7 días", component.Markup);
        Assert.Contains("Generar Reporte", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldShowSpinnerWhenGenerating()
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
        Assert.Contains("Generar Reporte", component.Markup);
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
        var submitButton = component.Find("button[type='submit']");
        Assert.True(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public void FiltrosReporte_ShouldLoadMeserosOnInitialization()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;
        var meseros = new List<UsuarioDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Roles = new List<string> { "Mesero" } },
            new() { Id = Guid.NewGuid(), NombreCompleto = "María García", Roles = new List<string> { "Mesero" } }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(1, 1000))
            .ReturnsAsync(meseros);

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        _usuariosApiMock.Verify(x => x.ObtenerUsuariosAsync(1, 1000), Times.Once);
        Assert.Contains("Juan Pérez", component.Markup);
        Assert.Contains("María García", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldLoadMesasOnInitialization()
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
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales" }
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
        Assert.Contains("Platos Principales", component.Markup);
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

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(1, 1000))
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

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>()))
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

        // Should not throw and should render with empty lists
        Assert.Contains("Todos los meseros", component.Markup);
        Assert.Contains("Todas las mesas", component.Markup);
        Assert.Contains("Todas las categorías", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectFormStructure()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("EditForm", component.Markup);
        Assert.Contains("DataAnnotationsValidator", component.Markup);
        Assert.Contains("ValidationMessage", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectCardStructure()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("card", component.Markup);
        Assert.Contains("card-header", component.Markup);
        Assert.Contains("card-body", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectIcons()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("oi-magnifying-glass", component.Markup);
        Assert.Contains("oi-reload", component.Markup);
        Assert.Contains("oi-clock", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectPlaceholders()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("Seleccionar tipo", component.Markup);
        Assert.Contains("Todos los meseros", component.Markup);
        Assert.Contains("Todas las mesas", component.Markup);
        Assert.Contains("Todas las categorías", component.Markup);
        Assert.Contains("100", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectValidationMessages()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("text-danger", component.Markup);
        Assert.Contains("form-text text-muted", component.Markup);
    }

    [Fact]
    public void FiltrosReporte_ShouldHaveCorrectBootstrapClasses()
    {
        // Arrange
        var filtros = new ReporteFiltrosDto();
        var onFiltrosAplicados = EventCallback<ReporteFiltrosDto>.Empty;

        // Act
        var component = RenderComponent<FiltrosReporte>(parameters => parameters
            .Add(p => p.Filtros, filtros)
            .Add(p => p.OnFiltrosAplicados, onFiltrosAplicados)
            .Add(p => p.Generando, false));

        // Assert
        Assert.Contains("form-control", component.Markup);
        Assert.Contains("form-select", component.Markup);
        Assert.Contains("form-label", component.Markup);
        Assert.Contains("btn btn-primary", component.Markup);
        Assert.Contains("btn btn-outline-secondary", component.Markup);
        Assert.Contains("btn btn-outline-info", component.Markup);
        Assert.Contains("row", component.Markup);
        Assert.Contains("col-md-3", component.Markup);
        Assert.Contains("col-md-4", component.Markup);
        Assert.Contains("col-md-6", component.Markup);
    }
}
