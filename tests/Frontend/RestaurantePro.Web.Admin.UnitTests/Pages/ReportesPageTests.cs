using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class ReportesPageTests : TestContext
{
    private readonly Mock<ReportesApiService> _reportesApiMock;
    private readonly Mock<ReportesComercialesApiService> _reportesComercialesApiMock;
    private readonly Mock<ReportesInventarioApiService> _reportesInventarioApiMock;
    private readonly Mock<UsuariosApiService> _usuariosApiMock;
    private readonly Mock<MesasApiService> _mesasApiMock;
    private readonly Mock<CategoriasApiService> _categoriasApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public ReportesPageTests()
    {
        _reportesApiMock = new Mock<ReportesApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _reportesComercialesApiMock = new Mock<ReportesComercialesApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _reportesInventarioApiMock = new Mock<ReportesInventarioApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _usuariosApiMock = new Mock<UsuariosApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _mesasApiMock = new Mock<MesasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _categoriasApiMock = new Mock<CategoriasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_reportesApiMock.Object);
        Services.AddSingleton(_reportesComercialesApiMock.Object);
        Services.AddSingleton(_reportesInventarioApiMock.Object);
        Services.AddSingleton(_usuariosApiMock.Object);
        Services.AddSingleton(_mesasApiMock.Object);
        Services.AddSingleton(_categoriasApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);

        // Configurar mocks con datos por defecto
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(1, 1000, null, true, "NombreCompleto", "asc"))
            .ReturnsAsync(new List<UsuarioDto>());
        _mesasApiMock.Setup(x => x.ObtenerAsync(null, null, null))
            .ReturnsAsync(new List<MesaDto>());
        _categoriasApiMock.Setup(x => x.ObtenerAsync(false, false))
            .ReturnsAsync(new List<CategoriaProductoDto>());
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYBotones()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("h1").TextContent.Should().Be("Reportes y Análisis");
        component.Find("button:contains('Historial')").Should().NotBeNull();
        component.Find("button:contains('Nuevo Reporte')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConIconos()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botonHistorial = component.Find("button:contains('Historial')");
        component.Find("button:contains('Historial') i.oi-clock").Should().NotBeNull();
        
        var botonNuevo = component.Find("button:contains('Nuevo Reporte')");
        component.Find("button:contains('Nuevo Reporte') i.oi-plus").Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTADÍSTICAS =====

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarCards()
    {
        // Arrange
        var estadisticas = new ReporteEstadisticasDto
        {
            TotalReportesGenerados = 150,
            ReportesHoy = 5,
            ReportesEstaSemana = 25,
            ReportesEsteMes = 100,
            TipoMasSolicitado = TipoReporte.VentasPorPeriodo,
            UltimoReporteGenerado = DateTime.Now
        };

        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("div:contains('Total Reportes')").Should().NotBeNull();
        component.Find("div:contains('Hoy')").Should().NotBeNull();
        component.Find("div:contains('Esta Semana')").Should().NotBeNull();
        component.Find("div:contains('Este Mes')").Should().NotBeNull();
        component.Find("div:contains('Tipo Más Usado')").Should().NotBeNull();
        component.Find("div:contains('Último Reporte')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarValores()
    {
        // Arrange
        var estadisticas = new ReporteEstadisticasDto
        {
            TotalReportesGenerados = 150,
            ReportesHoy = 5,
            ReportesEstaSemana = 25,
            ReportesEsteMes = 100,
            TipoMasSolicitado = TipoReporte.VentasPorPeriodo,
            UltimoReporteGenerado = DateTime.Now
        };

        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("div:contains('150')").Should().NotBeNull();
        component.Find("div:contains('5')").Should().NotBeNull();
        component.Find("div:contains('25')").Should().NotBeNull();
        component.Find("div:contains('100')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarCardsConColores()
    {
        // Arrange
        var estadisticas = new ReporteEstadisticasDto();
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var cards = component.FindAll(".card");
        cards.Should().HaveCount(6);
        
        cards[0].ClassList.Should().Contain("border-left-primary");
        cards[1].ClassList.Should().Contain("border-left-success");
        cards[2].ClassList.Should().Contain("border-left-info");
        cards[3].ClassList.Should().Contain("border-left-warning");
        cards[4].ClassList.Should().Contain("border-left-danger");
        cards[5].ClassList.Should().Contain("border-left-primary");
    }

    // ===== PRUEBAS DE PESTAÑAS =====

    [Fact]
    public void Renderizar_DeberiaMostrarPestanas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("button:contains('Reportes Operativos')").Should().NotBeNull();
        component.Find("button:contains('Reportes Comerciales')").Should().NotBeNull();
        component.Find("button:contains('Reportes de Inventario')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarPestanasConIconos()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var pestanas = component.FindAll("button[role='tab']");
        pestanas.Should().HaveCount(3);
        
        component.Find("button[role='tab'] i.oi-bar-chart").Should().NotBeNull();
        component.Find("button[role='tab'] i.oi-graph").Should().NotBeNull();
        component.Find("button[role='tab'] i.oi-box").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarPestanaOperativosActiva()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var pestanaActiva = component.Find("button.nav-link.active");
        pestanaActiva.Should().NotBeNull();
        pestanaActiva.TextContent.Should().Contain("Reportes Operativos");
    }

    // ===== PRUEBAS DE CONTENIDO DE PESTAÑAS =====

    [Fact]
    public void Renderizar_DeberiaMostrarContenidoDePestanas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("div#operativos").Should().NotBeNull();
        component.Find("div#comerciales").Should().NotBeNull();
        component.Find("div#inventario").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarContenidoPestanaOperativosActiva()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var pestanaOperativos = component.Find("div#operativos");
        pestanaOperativos.ClassList.Should().Contain("show");
        pestanaOperativos.ClassList.Should().Contain("active");
    }

    // ===== PRUEBAS DE REPORTES COMERCIALES =====

    [Fact]
    public void Renderizar_DeberiaMostrarCardsDeReportesComerciales()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("h6:contains('Análisis de Clientes')").Should().NotBeNull();
        component.Find("h6:contains('Segmentación de Clientes')").Should().NotBeNull();
        component.Find("h6:contains('Análisis de Productos')").Should().NotBeNull();
        component.Find("h6:contains('Rentabilidad por Producto')").Should().NotBeNull();
        component.Find("h6:contains('Análisis de Promociones')").Should().NotBeNull();
        component.Find("h6:contains('Tendencias de Ventas')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeGenerarComerciales()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botones = component.FindAll("button:contains('Generar')");
        botones.Should().HaveCountGreaterThan(6); // Al menos 6 botones de generar
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesComercialesConIconos()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var iconos = component.FindAll("i.oi");
        iconos.Should().Contain(i => i.TextContent.Contains("people"));
        iconos.Should().Contain(i => i.TextContent.Contains("tags"));
        iconos.Should().Contain(i => i.TextContent.Contains("fork"));
        iconos.Should().Contain(i => i.TextContent.Contains("dollar"));
        iconos.Should().Contain(i => i.TextContent.Contains("badge"));
        iconos.Should().Contain(i => i.TextContent.Contains("graph"));
    }

    // ===== PRUEBAS DE REPORTES DE INVENTARIO =====

    [Fact]
    public void Renderizar_DeberiaMostrarCardsDeReportesInventario()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("h6:contains('Análisis de Stock')").Should().NotBeNull();
        component.Find("h6:contains('Movimientos de Inventario')").Should().NotBeNull();
        component.Find("h6:contains('Productos Próximos a Vencer')").Should().NotBeNull();
        component.Find("h6:contains('Rotación de Inventario')").Should().NotBeNull();
        component.Find("h6:contains('Análisis de Costos')").Should().NotBeNull();
        component.Find("h6:contains('Predicción de Demanda')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeGenerarInventario()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botones = component.FindAll("button:contains('Generar')");
        botones.Should().HaveCountGreaterThan(6); // Al menos 6 botones de generar
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesInventarioConColores()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botones = component.FindAll("button:contains('Generar')");
        botones.Should().Contain(b => b.ClassList.Contains("btn-primary"));
        botones.Should().Contain(b => b.ClassList.Contains("btn-warning"));
        botones.Should().Contain(b => b.ClassList.Contains("btn-info"));
    }

    // ===== PRUEBAS DE COMPONENTES =====

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteFiltrosReporte()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var componente = component.Find("FiltrosReporte");
        componente.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteReporteVentas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var componente = component.Find("ReporteVentas");
        componente.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteReporteProductos()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var componente = component.Find("ReporteProductos");
        componente.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var row = component.Find(".row.mb-4");
        row.Should().NotBeNull();
        
        var cols = component.FindAll(".col-md-");
        cols.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaTenerCardsConEstructuraCorrecta()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var cards = component.FindAll(".card");
        cards.Should().NotBeEmpty();
        
        var cardHeaders = component.FindAll(".card-header");
        cardHeaders.Should().NotBeEmpty();
        
        var cardBodies = component.FindAll(".card-body");
        cardBodies.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE NAVEGACIÓN =====

    [Fact]
    public void Renderizar_DeberiaTenerNavegacionPorPestanas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var nav = component.Find("ul.nav.nav-tabs");
        nav.Should().NotBeNull();
        nav.GetAttribute("role").Should().Be("tablist");
    }

    [Fact]
    public void Renderizar_DeberiaTenerContenidoDePestanas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var contenido = component.Find("div.tab-content");
        contenido.Should().NotBeNull();
        contenido.GetAttribute("id").Should().Be("reportTabsContent");
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosBotones()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        botones.Should().Contain(b => b.TextContent.Contains("Historial"));
        botones.Should().Contain(b => b.TextContent.Contains("Nuevo Reporte"));
        botones.Should().Contain(b => b.TextContent.Contains("Reportes Operativos"));
        botones.Should().Contain(b => b.TextContent.Contains("Reportes Comerciales"));
        botones.Should().Contain(b => b.TextContent.Contains("Reportes de Inventario"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConClasesCorrectas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var botonHistorial = component.Find("button:contains('Historial')");
        botonHistorial.ClassList.Should().Contain("btn-outline-info");
        
        var botonNuevo = component.Find("button:contains('Nuevo Reporte')");
        botonNuevo.ClassList.Should().Contain("btn-primary");
        
        var botonesPestanas = component.FindAll("button[role='tab']");
        botonesPestanas.Should().AllSatisfy(b => b.ClassList.Should().Contain("nav-link"));
    }

    // ===== PRUEBAS DE ICONOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarIconosOpenIconic()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var iconos = component.FindAll("i.oi");
        iconos.Should().NotBeEmpty();
        iconos.Should().HaveCountGreaterThan(10); // Debe haber muchos iconos
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_DeberiaInicializarConValoresPorDefecto()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var pestanaActiva = component.Find("button.nav-link.active");
        pestanaActiva.TextContent.Should().Contain("Reportes Operativos");
    }

    // ===== PRUEBAS DE ACCESIBILIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerAtributosDeAccesibilidad()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var nav = component.Find("ul[role='tablist']");
        nav.Should().NotBeNull();
        
        var botonesPestanas = component.FindAll("button[role='tab']");
        botonesPestanas.Should().HaveCount(3);
        
        var contenidoPestanas = component.FindAll("div[role='tabpanel']");
        contenidoPestanas.Should().HaveCount(3);
    }

    // ===== PRUEBAS DE RESPONSIVIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerClasesResponsivas()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        var elementosResponsivos = component.FindAll("[class*='col-md-']");
        elementosResponsivos.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE CONTENIDO DINÁMICO =====

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarValoresCorrectos()
    {
        // Arrange
        var estadisticas = new ReporteEstadisticasDto
        {
            TotalReportesGenerados = 200,
            ReportesHoy = 10,
            ReportesEstaSemana = 50,
            ReportesEsteMes = 150,
            TipoMasSolicitado = TipoReporte.ProductosMasVendidos,
            UltimoReporteGenerado = new DateTime(2024, 1, 15)
        };

        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(estadisticas);

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("div:contains('200')").Should().NotBeNull();
        component.Find("div:contains('10')").Should().NotBeNull();
        component.Find("div:contains('50')").Should().NotBeNull();
        component.Find("div:contains('150')").Should().NotBeNull();
        component.Find("div:contains('15/01')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE ALERTAS =====

    [Fact]
    public void Renderizar_DeberiaMostrarAlertaDeSeleccion()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("div.alert.alert-info").Should().NotBeNull();
        component.Find("div:contains('Selecciona un tipo de reporte')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE DESCRIPCIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarDescripcionesDeReportes()
    {
        // Arrange
        _reportesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
            .ReturnsAsync(new ReporteEstadisticasDto());

        // Act
        var component = RenderComponent<Reportes>();

        // Assert
        component.Find("p:contains('Análisis detallado del comportamiento')").Should().NotBeNull();
        component.Find("p:contains('Clasificación de clientes por segmentos')").Should().NotBeNull();
        component.Find("p:contains('Análisis de rendimiento y popularidad')").Should().NotBeNull();
        component.Find("p:contains('Análisis completo del estado actual')").Should().NotBeNull();
        component.Find("p:contains('Historial detallado de movimientos')").Should().NotBeNull();
        component.Find("p:contains('Lista de productos con vencimiento')").Should().NotBeNull();
    }
}
