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

public class ConfiguracionPageTests : TestContext
{
    private readonly Mock<ConfiguracionApiService> _configuracionApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public ConfiguracionPageTests()
    {
        _configuracionApiMock = new Mock<ConfiguracionApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_configuracionApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("h3").TextContent.Should().Be("⚙️ Configuración del Sistema");
        component.Find("p.text-muted").TextContent.Should().Be("Parámetros de sistema, notificaciones y preferencias.");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarDashboardDeEstadisticas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("h4:contains('0')").Should().NotBeNull(); // totalParametros
        component.Find("p:contains('Parámetros')").Should().NotBeNull();
        component.Find("p:contains('Editables')").Should().NotBeNull();
        component.Find("p:contains('Categorías')").Should().NotBeNull();
        component.Find("p:contains('Última Actualización')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCardsConIconos()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var iconos = component.FindAll(".row.mb-4 .card i.oi");
        iconos.Should().HaveCount(4);
        iconos.Should().Contain(i => i.ClassList.Contains("oi-cog"));
        iconos.Should().Contain(i => i.ClassList.Contains("oi-pencil"));
        iconos.Should().Contain(i => i.ClassList.Contains("oi-tags"));
        iconos.Should().Contain(i => i.ClassList.Contains("oi-clock"));
    }

    // ===== PRUEBAS DE PESTAÑAS =====

    [Fact]
    public void Renderizar_DeberiaMostrarPestanas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("button:contains('Parámetros del Sistema')").Should().NotBeNull();
        component.Find("button:contains('Notificaciones')").Should().NotBeNull();
        component.Find("button:contains('Fidelización')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarPestanasConIconos()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var pestanas = component.FindAll("button[role='tab']");
        pestanas.Should().HaveCount(3);
        
        component.Find("button[role='tab'] i.oi-cog").Should().NotBeNull();
        component.Find("button[role='tab'] i.oi-bell").Should().NotBeNull();
        component.Find("button[role='tab'] i.oi-badge").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarPestanaActiva()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var pestanaActiva = component.Find("button.nav-link.active");
        pestanaActiva.Should().NotBeNull();
        pestanaActiva.TextContent.Should().Contain("Parámetros del Sistema");
    }

    // ===== PRUEBAS DE CONTENIDO DE PESTAÑAS =====

    [Fact]
    public void Renderizar_DeberiaMostrarContenidoDePestanas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("div#parametros").Should().NotBeNull();
        component.Find("div#notificaciones").Should().NotBeNull();
        component.Find("div#fidelizacion").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarPestanaParametrosActiva()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var pestanaParametros = component.Find("div#parametros");
        pestanaParametros.ClassList.Should().Contain("show");
        pestanaParametros.ClassList.Should().Contain("active");
    }

    // ===== PRUEBAS DE FILTROS =====

    [Fact]
    public void Renderizar_DeberiaMostrarFiltroDeCategorias()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var select = component.Find("select.form-select");
        select.Should().NotBeNull();
        
        var opciones = component.FindAll("option");
        opciones.Should().HaveCount(6); // 5 categorías + "Todas las categorías"
        opciones.Should().Contain(o => o.TextContent.Contains("Todas las categorías"));
        opciones.Should().Contain(o => o.TextContent.Contains("Sistema"));
        opciones.Should().Contain(o => o.TextContent.Contains("Notificaciones"));
        opciones.Should().Contain(o => o.TextContent.Contains("Fidelización"));
        opciones.Should().Contain(o => o.TextContent.Contains("Seguridad"));
        opciones.Should().Contain(o => o.TextContent.Contains("Comercial"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonActualizar()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var boton = component.Find("button:contains('Actualizar')");
        boton.Should().NotBeNull();
        boton.ClassList.Should().Contain("btn-outline-primary");
        
        var icono = component.Find("button:contains('Actualizar') i.oi-reload");
        icono.Should().NotBeNull();
    }

    // ===== PRUEBAS DE COMPONENTES =====

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteParametrosSistema()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("#parametros").Should().NotBeNull();
        component.Find("h5:contains('Parámetros del Sistema')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteConfiguracionNotificaciones()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("#notificaciones").Should().NotBeNull();
        component.Find("button:contains('Notificaciones')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponenteTarjetasFidelizacion()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("#fidelizacion").Should().NotBeNull();
        component.Find("button:contains('Fidelización')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var row = component.Find(".row.mb-4");
        row.Should().NotBeNull();
        
        var cols = component.FindAll(".col-md-3");
        cols.Should().HaveCount(4);
    }

    [Fact]
    public void Renderizar_DeberiaTenerCardsConColores()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var cards = component.FindAll(".row.mb-4 .card");
        cards.Should().HaveCount(4);
        
        cards[0].ClassList.Should().Contain("bg-primary");
        cards[1].ClassList.Should().Contain("bg-success");
        cards[2].ClassList.Should().Contain("bg-info");
        cards[3].ClassList.Should().Contain("bg-warning");
    }

    [Fact]
    public void Renderizar_DeberiaTenerCardsConTextoBlanco()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var cards = component.FindAll(".card.text-white");
        cards.Should().HaveCount(4);
    }

    // ===== PRUEBAS DE NAVEGACIÓN =====

    [Fact]
    public void Renderizar_DeberiaTenerNavegacionPorPestanas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var nav = component.Find("ul.nav.nav-tabs");
        nav.Should().NotBeNull();
        nav.GetAttribute("role").Should().Be("tablist");
    }

    [Fact]
    public void Renderizar_DeberiaTenerContenidoDePestanas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var contenido = component.Find("div.tab-content");
        contenido.Should().NotBeNull();
        contenido.GetAttribute("id").Should().Be("configTabsContent");
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosBotones()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        botones.Should().Contain(b => b.TextContent.Contains("Parámetros del Sistema"));
        botones.Should().Contain(b => b.TextContent.Contains("Notificaciones"));
        botones.Should().Contain(b => b.TextContent.Contains("Fidelización"));
        botones.Should().Contain(b => b.TextContent.Contains("Actualizar"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConClasesCorrectas()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var botonActualizar = component.Find("button:contains('Actualizar')");
        botonActualizar.ClassList.Should().Contain("btn-outline-primary");
        
        var botonesPestanas = component.FindAll("button[role='tab']");
        botonesPestanas.Should().AllSatisfy(b => b.ClassList.Should().Contain("nav-link"));
    }

    // ===== PRUEBAS DE ICONOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarIconosOpenIconic()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var iconos = component.FindAll("i.oi");
        iconos.Should().NotBeEmpty();
        iconos.Should().HaveCountGreaterThan(3);
    }

    [Fact]
    public void Renderizar_DeberiaMostrarIconosConTamañoCorrecto()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var iconosGrandes = component.FindAll("i[style*='font-size: 2rem']");
        iconosGrandes.Should().HaveCount(4);
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_DeberiaInicializarConValoresPorDefecto()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var select = component.Find("select.form-select");
        select.GetAttribute("value").Should().BeEmpty();
        
        var pestanaActiva = component.Find("button.nav-link.active");
        pestanaActiva.TextContent.Should().Contain("Parámetros del Sistema");
    }

    // ===== PRUEBAS DE ACCESIBILIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerAtributosDeAccesibilidad()
    {
        // Arrange
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

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
        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync((ConfiguracionDto?)null);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var elementosResponsivos = component.FindAll("[class*='col-md-']");
        elementosResponsivos.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE CONTENIDO DINÁMICO =====

    [Fact]
    public void Renderizar_ConParametros_DeberiaMostrarEstadisticas()
    {
        // Arrange
        var parametro = new ConfiguracionDto { Categoria = "Sistema", EsEditable = true, FechaActualizacion = DateTime.Now };

        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync(parametro);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        component.Find("h4:contains('1')").Should().NotBeNull(); // totalParametros
        component.Find("h4:contains('1')").Should().NotBeNull(); // parametrosEditables
        component.Find("h4:contains('1')").Should().NotBeNull(); // categoriasActivas
    }

    // ===== PRUEBAS DE FILTROS DINÁMICOS =====

    [Fact]
    public void Renderizar_ConParametros_DeberiaMostrarOpcionesDeFiltro()
    {
        // Arrange
        var parametro = new ConfiguracionDto { Categoria = "Sistema" };

        _configuracionApiMock.Setup(x => x.ObtenerConfiguracionAsync())
            .ReturnsAsync(parametro);

        // Act
        var component = RenderComponent<Configuracion>();

        // Assert
        var opciones = component.FindAll("option");
        opciones.Should().HaveCount(6); // 5 categorías + "Todas las categorías"
        opciones.Should().Contain(o => o.TextContent.Contains("Sistema"));
        opciones.Should().Contain(o => o.TextContent.Contains("Notificaciones"));
        opciones.Should().Contain(o => o.TextContent.Contains("Fidelización"));
    }
}
