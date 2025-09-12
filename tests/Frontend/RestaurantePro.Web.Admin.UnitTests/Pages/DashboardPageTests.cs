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

public class DashboardPageTests : TestContext
{
    private readonly Mock<IDashboardApiService> _dashboardApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public DashboardPageTests()
    {
        _dashboardApiMock = new Mock<IDashboardApiService>();
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_dashboardApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(new DashboardResumenDto());

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();

        // Assert
        component.Find("h1").TextContent.Should().Be("Dashboard Ejecutivo");
        component.Find("p:contains('Resumen de métricas clave y operaciones.')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonPersonalizarWidgets()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(new DashboardResumenDto());

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();

        // Assert
        var boton = component.Find("button:contains('Personalizar Widgets')");
        boton.Should().NotBeNull();
        boton.ClassList.Should().Contain("bg-[var(--primary-color)]");
    }

    // ===== PRUEBAS DE ESTADO DE CARGA =====

    [Fact]
    public void Renderizar_ConCargandoTrue_DeberiaMostrarSpinner()
    {
        // Arrange
        // Configuramos el mock para que nunca se complete (simulando carga infinita)
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .Returns(new Task<DashboardResumenDto?>(() => null));

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();

        // Assert
        // El spinner se muestra inicialmente antes de que se complete la carga
        component.Find("p:contains('Cargando dashboard...')").Should().NotBeNull();
        component.Find("div.animate-spin").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenNull_DeberiaMostrarError()
    {
        // Arrange
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync((DashboardResumenDto?)null);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();

        // Assert
        component.Find("p:contains('Error al cargar el dashboard')").Should().NotBeNull();
        component.Find("button:contains('Reintentar')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarContenido()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1500,
                CrecimientoVentas = 12.5M,
                MesasOcupadas = 8,
                TotalMesas = 12,
                ComandasActivas = 15
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("h2:contains('Métricas Principales')").Should().NotBeNull();
        component.Find("h2:contains('Gráficos Interactivos')").Should().NotBeNull();
        component.Find("h2:contains('Vista Rápida de Operaciones')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE MÉTRICAS PRINCIPALES =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarMetricasPrincipales()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1500,
                CrecimientoVentas = 12.5M,
                MesasOcupadas = 8,
                TotalMesas = 12,
                ComandasActivas = 15
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("p:contains('Ventas del Día')").Should().NotBeNull();
        component.Find("p:contains('Ocupación de Mesas')").Should().NotBeNull();
        component.Find("p:contains('Producto Más Vendido')").Should().NotBeNull();
        component.Find("p:contains('Ingresos por Período')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarValoresDeMetricas()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1500,
                CrecimientoVentas = 12.5M,
                MesasOcupadas = 8,
                TotalMesas = 12,
                ComandasActivas = 15
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("p:contains('$1,500')").Should().NotBeNull();
        component.Find("span:contains('+12.5%')").Should().NotBeNull();
        component.Find("span:contains('+15%')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE GRÁFICOS =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarGraficos()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("canvas#graficoVentasHora").Should().NotBeNull();
        component.Find("canvas#graficoIngresosCategoria").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarTitulosDeGraficos()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("p:contains('Ventas por Hora')").Should().NotBeNull();
        component.Find("p:contains('Ingresos por Categoría')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE VISTA RÁPIDA =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarVistaRapida()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                ComandasActivas = 15
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("p:contains('Comandas Activas')").Should().NotBeNull();
        component.Find("p:contains('Reservaciones del Día')").Should().NotBeNull();
        component.Find("p:contains('Estado de Mesas')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarValoresDeVistaRapida()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                ComandasActivas = 15
            },
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("p:contains('15 comandas activas')").Should().NotBeNull();
        component.Find("p:contains('15 reservaciones hoy')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE ALERTAS =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarAlertas()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("h2:contains('Alertas y Notificaciones')").Should().NotBeNull();
        component.Find("p:contains('Tomates casi agotados')").Should().NotBeNull();
        component.Find("p:contains('Mesas excediendo capacidad')").Should().NotBeNull();
        component.Find("p:contains('5 comandas atrasadas')").Should().NotBeNull();
        component.Find("p:contains('Reserva para 4 a las 7 PM')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarIconosDeAlertas()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var iconos = component.FindAll("span.material-symbols-outlined");
        iconos.Should().NotBeEmpty();
        iconos.Should().Contain(i => i.TextContent.Contains("inventory_2"));
        iconos.Should().Contain(i => i.TextContent.Contains("table_restaurant"));
        iconos.Should().Contain(i => i.TextContent.Contains("running_with_errors"));
        iconos.Should().Contain(i => i.TextContent.Contains("event_available"));
    }

    // ===== PRUEBAS DE ACCESOS RÁPIDOS =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarAccesosRapidos()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        component.Find("h2:contains('Accesos Rápidos')").Should().NotBeNull();
        component.Find("button:contains('Reportes')").Should().NotBeNull();
        component.Find("button:contains('Reservas')").Should().NotBeNull();
        component.Find("button:contains('Inventario')").Should().NotBeNull();
        component.Find("button:contains('Ajustes')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarIconosDeAccesosRapidos()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var iconos = component.FindAll("span.material-symbols-outlined");
        iconos.Should().Contain(i => i.TextContent.Contains("assessment"));
        iconos.Should().Contain(i => i.TextContent.Contains("book_online"));
        iconos.Should().Contain(i => i.TextContent.Contains("inventory"));
        iconos.Should().Contain(i => i.TextContent.Contains("settings"));
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var grid = component.Find("div.grid");
        grid.Should().NotBeNull();
        
        var gridCols = component.FindAll("div[class*='grid-cols-']");
        gridCols.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaTenerClasesDeEstilo()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var contenedor = component.Find("div.bg-gray-50");
        contenedor.Should().NotBeNull();
        
        var cards = component.FindAll("div[class*='rounded-lg']");
        cards.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosBotones()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        botones.Should().Contain(b => b.TextContent.Contains("Personalizar Widgets"));
        botones.Should().Contain(b => b.TextContent.Contains("Reportes"));
        botones.Should().Contain(b => b.TextContent.Contains("Reservas"));
        botones.Should().Contain(b => b.TextContent.Contains("Inventario"));
        botones.Should().Contain(b => b.TextContent.Contains("Ajustes"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConClasesCorrectas()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var botonPersonalizar = component.Find("button:contains('Personalizar Widgets')");
        botonPersonalizar.ClassList.Should().Contain("bg-[var(--primary-color)]");
        
        var botonesAcceso = component.FindAll("button:contains('Reportes'), button:contains('Reservas'), button:contains('Inventario'), button:contains('Ajustes')");
        botonesAcceso.Should().NotBeEmpty();
        botonesAcceso.Should().AllSatisfy(b => b.ClassList.Should().Contain("bg-gray-100"));
    }

    // ===== PRUEBAS DE ICONOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarIconosMaterial()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var iconos = component.FindAll("span.material-symbols-outlined");
        iconos.Should().NotBeEmpty();
        iconos.Should().HaveCountGreaterThan(10); // Debe haber muchos iconos
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_DeberiaInicializarConCargandoTrue()
    {
        // Arrange
        // Configuramos el mock para que nunca se complete (simulando carga infinita)
        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .Returns(new Task<DashboardResumenDto?>(() => null));

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();

        // Assert
        component.Find("p:contains('Cargando dashboard...')").Should().NotBeNull();
    }

    // ===== PRUEBAS DE CANVAS =====

    [Fact]
    public void Renderizar_ConResumenValido_DeberiaMostrarCanvasParaGraficos()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var canvas = component.FindAll("canvas");
        canvas.Should().HaveCount(2);
        canvas[0].GetAttribute("id").Should().Be("graficoVentasHora");
        canvas[1].GetAttribute("id").Should().Be("graficoIngresosCategoria");
    }

    // ===== PRUEBAS DE RESPONSIVIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerClasesResponsivas()
    {
        // Arrange
        var resumen = new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto(),
            IngresosPorHora = new List<IngresosPorHoraDto>()
        };

        _dashboardApiMock.Setup(x => x.ObtenerDashboardAsync())
            .ReturnsAsync(resumen);

        // Act
        var component = RenderComponent<RestaurantePro.Web.Admin.Pages.Index>();
        
        // Esperar a que se complete la carga asíncrona
        component.WaitForAssertion(() => component.Find("h2:contains('Métricas Principales')").Should().NotBeNull());

        // Assert
        var elementosResponsivos = component.FindAll("[class*='sm:'], [class*='lg:'], [class*='md:']");
        elementosResponsivos.Should().NotBeEmpty();
    }
}
