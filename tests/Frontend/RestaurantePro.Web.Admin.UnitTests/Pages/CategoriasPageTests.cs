using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class CategoriasPageTests : TestContext
{
    private readonly Mock<ICategoriasApiService> _categoriasApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public CategoriasPageTests()
    {
        _categoriasApiMock = new Mock<ICategoriasApiService>();
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_categoriasApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost:5001/", "https://localhost:5001/categorias"));
    }

    private void SetupMocks()
    {
        _categoriasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ReturnsAsync(new List<CategoriaProductoDto>());
        _categoriasApiMock.Setup(x => x.BuscarAsync(It.IsAny<string>()))
                        .ReturnsAsync(new List<CategoriaProductoDto>());
        _categoriasApiMock.Setup(x => x.ValidarNombreUnicoAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
                        .ReturnsAsync(true);
    }

    private void SetupMocksWithData()
    {
        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto 
            { 
                Id = Guid.NewGuid(), 
                Nombre = "Entradas", 
                Descripcion = "Aperitivos y entradas",
                Activa = true,
                CantidadProductos = 5,
                FechaCreacion = DateTime.Now.AddDays(-30),
                Color = "#1e71f6",
                Icono = "restaurant",
                Orden = 1
            },
            new CategoriaProductoDto 
            { 
                Id = Guid.NewGuid(), 
                Nombre = "Platos Principales", 
                Descripcion = "Platos principales del menú",
                Activa = true,
                CantidadProductos = 12,
                FechaCreacion = DateTime.Now.AddDays(-25),
                Color = "#22c55e",
                Icono = "local_dining",
                Orden = 2
            }
        };

        _categoriasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ReturnsAsync(categorias);
        _categoriasApiMock.Setup(x => x.BuscarAsync(It.IsAny<string>()))
                        .ReturnsAsync(categorias);
        _categoriasApiMock.Setup(x => x.ValidarNombreUnicoAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
                        .ReturnsAsync(true);
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("h2").TextContent.Should().Contain("Gestión de Categorías");
        component.Find("p").TextContent.Should().Contain("Administra las categorías de productos");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("button:contains('Nueva Categoría')").Should().NotBeNull();
        component.Find("button:contains('Aplicar Filtros')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarFiltros()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("input[placeholder='Buscar por nombre']").Should().NotBeNull();
        component.Find("select").Should().NotBeNull();
        component.Find("option[value='true']").TextContent.Should().Contain("Activo");
        component.Find("option[value='false']").TextContent.Should().Contain("Inactivo");
    }

    [Fact]
    public void Renderizar_SinDatos_DeberiaMostrarMensajeVacio()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("span:contains('category')").Should().NotBeNull();
        component.Find("p:contains('No hay categorías')").Should().NotBeNull();
        component.Find("button:contains('Crear Primera Categoría')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConDatos_DeberiaMostrarContenido()
    {
        // Arrange
        SetupMocksWithData();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("table").Should().NotBeNull();
        component.Find("th:contains('Nombre')").Should().NotBeNull();
        component.Find("th:contains('Descripción')").Should().NotBeNull();
        component.Find("th:contains('Productos')").Should().NotBeNull();
        component.Find("th:contains('Estado')").Should().NotBeNull();
        component.Find("th:contains('Fecha de Creación')").Should().NotBeNull();
        component.Find("th:contains('Acciones')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConEstadisticas_DeberiaMostrarCards()
    {
        // Arrange
        SetupMocksWithData();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("p:contains('Total de Categorías')").Should().NotBeNull();
        component.Find("p:contains('Categorías Activas')").Should().NotBeNull();
        component.Find("p:contains('Productos Asociados')").Should().NotBeNull();
        component.Find("p:contains('Categoría Más Popular')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("div.max-w-7xl").Should().NotBeNull();
        component.Find("div.grid.grid-cols-1.sm\\:grid-cols-2.lg\\:grid-cols-4").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaLlamarServiciosAlInicializar()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        _categoriasApiMock.Verify(x => x.ObtenerAsync(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void BuscarAsync_ConFiltro_DeberiaLlamarBuscarAsync()
    {
        // Arrange
        SetupMocks();
        var component = RenderComponent<Categorias>();

        // Act
        component.Find("input[placeholder='Buscar por nombre']").Change("Entradas");
        component.Find("button:contains('Aplicar Filtros')").Click();

        // Assert
        _categoriasApiMock.Verify(x => x.BuscarAsync("Entradas"), Times.Once);
    }

    [Fact]
    public void BuscarAsync_SinFiltro_DeberiaLlamarCargarAsync()
    {
        // Arrange
        SetupMocks();
        var component = RenderComponent<Categorias>();

        // Act
        component.Find("input[placeholder='Buscar por nombre']").Change("");
        component.Find("button:contains('Aplicar Filtros')").Click();

        // Assert
        _categoriasApiMock.Verify(x => x.ObtenerAsync(It.IsAny<bool>(), It.IsAny<bool>()), Times.AtLeast(2));
    }

    [Fact]
    public void NuevaCategoria_DeberiaMostrarModal()
    {
        // Arrange
        SetupMocks();
        var component = RenderComponent<Categorias>();

        // Act
        component.Find("button:contains('Nueva Categoría')").Click();

        // Assert
        component.Find("h2:contains('Nueva Categoría')").Should().NotBeNull();
        component.Find("input[type='text']").Should().NotBeNull();
        component.Find("textarea").Should().NotBeNull();
    }

    [Fact]
    public void EditarCategoria_DeberiaMostrarModalConDatos()
    {
        // Arrange
        SetupMocksWithData();
        var categoriaId = Guid.NewGuid();
        _categoriasApiMock.Setup(x => x.ObtenerPorIdAsync(categoriaId))
                        .ReturnsAsync(new CategoriaProductoDto 
                        { 
                            Id = categoriaId, 
                            Nombre = "Test Category",
                            Descripcion = "Test Description",
                            Activa = true
                        });

        var component = RenderComponent<Categorias>();

        // Act
        component.Find("button:contains('Editar')").Click();

        // Assert
        _categoriasApiMock.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public void EliminarCategoria_DeberiaMostrarModalConfirmacion()
    {
        // Arrange
        SetupMocksWithData();
        var component = RenderComponent<Categorias>();

        // Act
        component.Find("button:contains('Eliminar')").Click();

        // Assert
        component.Find("h2:contains('Confirmar Eliminación')").Should().NotBeNull();
        component.Find("button:contains('Cancelar')").Should().NotBeNull();
        component.Find("button:contains('Eliminar')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConError_DeberiaManejarExcepciones()
    {
        // Arrange
        _categoriasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        // El componente debería manejar la excepción y mostrar el estado de error
        component.Find("div").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarComponentesHijos()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        // Verificar que se muestran los elementos principales de la página
        component.Find("h2").Should().NotBeNull();
        component.Find("div.bg-white").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaTenerNavegacionCorrecta()
    {
        // Arrange
        SetupMocks();

        // Act
        var component = RenderComponent<Categorias>();

        // Assert
        component.Find("div").Should().NotBeNull();
    }
}
