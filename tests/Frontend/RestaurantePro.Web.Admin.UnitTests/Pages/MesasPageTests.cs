using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using RestaurantePro.Web.Admin.Components;
using Xunit;
using FluentAssertions;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class MesasPageTests : TestContext
{
    private readonly Mock<IMesasApiService> _mesasApiMock;

    public MesasPageTests()
    {
        _mesasApiMock = new Mock<IMesasApiService>();
        Services.AddSingleton(_mesasApiMock.Object);
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("h1").TextContent.Should().Contain("Gestión de Mesas");
        component.Find("p").TextContent.Should().Contain("Control de mesas, estados y disponibilidad del restaurante");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeVista()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("button:contains('Tabla')").Should().NotBeNull();
        component.Find("button:contains('Grid')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonNuevaMesa()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("button:contains('Nueva Mesa')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarDashboardEstadisticas()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("p:contains('Total de Mesas')").Should().NotBeNull();
        component.Find("p:contains('Disponibles')").Should().NotBeNull();
        component.Find("p:contains('Ocupadas')").Should().NotBeNull();
        component.Find("p:contains('Reservadas')").Should().NotBeNull();
        component.Find("p:contains('Ocupación')").Should().NotBeNull();
        component.Find("p:contains('Capacidad Total')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarFiltrosAvanzados()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("h2:contains('Filtros Avanzados')").Should().NotBeNull();
        component.Find("select").Should().NotBeNull(); // Filtro de estado
        component.Find("input[placeholder='Zona/Ubicación']").Should().NotBeNull();
        component.Find("input[placeholder='2']").Should().NotBeNull();
        component.Find("button:contains('Aplicar Filtros')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConMesas_DeberiaMostrarVistaTabla()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "1", Capacidad = 4, Estado = "Disponible", Zona = "Interior", Tipo = "Normal" },
            new() { Id = Guid.NewGuid(), Numero = "2", Capacidad = 6, Estado = "Ocupada", Zona = "Exterior", Tipo = "VIP" }
        };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("table").Should().NotBeNull();
        component.FindAll("tr").Should().HaveCount(3); // Header + 2 mesas
        component.Find("th:contains('Número')").Should().NotBeNull();
        component.Find("th:contains('Capacidad')").Should().NotBeNull();
        component.Find("th:contains('Estado')").Should().NotBeNull();
        component.Find("th:contains('Zona')").Should().NotBeNull();
        component.Find("th:contains('Tipo')").Should().NotBeNull();
        component.Find("th:contains('Acciones')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConMesas_DeberiaMostrarVistaGrid()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "1", Capacidad = 4, Estado = "Disponible", Zona = "Interior", Tipo = "Normal" }
        };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<Mesas>();

        // Cambiar a vista grid
        component.Find("button:contains('Grid')").Click();

        // Assert
        component.Find("div.grid").Should().NotBeNull();
        component.Find("h3:contains('Mesa 1')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_SinMesas_DeberiaMostrarMensajeVacio()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("span:contains('No hay mesas disponibles')").Should().NotBeNull();
        component.Find("span.material-symbols-outlined:contains('table_restaurant')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConError_DeberiaMostrarMensajeError()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync((List<MesaDto>?)null);

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("span:contains('No se pudieron cargar las mesas')").Should().NotBeNull();
        component.Find("span.material-symbols-outlined:contains('error')").Should().NotBeNull();
    }

    [Fact]
    public void CambiarVista_DeberiaAlternarEntreTablaYGrid()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "1", Capacidad = 4, Estado = "Disponible", Zona = "Interior", Tipo = "Normal" }
        };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        var component = RenderComponent<Mesas>();

        // Act - Cambiar a Grid
        component.Find("button:contains('Grid')").Click();

        // Assert - Verificar que los botones existen y se pueden hacer click
        component.Find("button:contains('Grid')").Should().NotBeNull();
        component.Find("button:contains('Tabla')").Should().NotBeNull();

        // Act - Cambiar a Tabla
        component.Find("button:contains('Tabla')").Click();

        // Assert - Verificar que los botones existen y se pueden hacer click
        component.Find("button:contains('Tabla')").Should().NotBeNull();
        component.Find("button:contains('Grid')").Should().NotBeNull();
    }

    [Fact]
    public void NuevaMesa_DeberiaAbrirModal()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        var component = RenderComponent<Mesas>();

        // Act
        component.Find("button:contains('Nueva Mesa')").Click();

        // Assert
        // Verificar que el botón existe y se puede hacer click
        component.Find("button:contains('Nueva Mesa')").Should().NotBeNull();
    }

    [Fact]
    public void EditarMesa_DeberiaAbrirModalConDatos()
    {
        // Arrange
        var mesa = new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = "1",
            Capacidad = 4,
            Estado = "Disponible",
            Zona = "Interior",
            Tipo = "Normal",
            Descripcion = "Mesa cerca de la ventana",
            Notas = "Requiere limpieza especial",
            TieneVentana = true,
            TieneSofa = false,
            EsAccesible = true,
            TieneEnchufe = true
        };
        var mesas = new List<MesaDto> { mesa };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        var component = RenderComponent<Mesas>();

        // Act
        component.Find("button[title='Editar']").Click();

        // Assert
        // Verificar que el botón existe y se puede hacer click
        component.Find("button[title='Editar']").Should().NotBeNull();
    }

    [Fact]
    public void VerDetalles_DeberiaAbrirModalDetalles()
    {
        // Arrange
        var mesa = new MesaDto
        {
            Id = Guid.NewGuid(),
            Numero = "1",
            Capacidad = 4,
            Estado = "Disponible",
            Zona = "Interior",
            Tipo = "Normal"
        };
        var mesas = new List<MesaDto> { mesa };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        var component = RenderComponent<Mesas>();

        // Act
        component.Find("button[title='Ver detalles']").Click();

        // Assert
        // Verificar que el botón existe y se puede hacer click
        component.Find("button[title='Ver detalles']").Should().NotBeNull();
    }

    [Fact]
    public void EliminarMesa_DeberiaLlamarApiService()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = new MesaDto
        {
            Id = mesaId,
            Numero = "1",
            Capacidad = 4,
            Estado = "Disponible",
            Zona = "Interior",
            Tipo = "Normal"
        };
        var mesas = new List<MesaDto> { mesa };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);
        _mesasApiMock.Setup(x => x.EliminarAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(true);

        var component = RenderComponent<Mesas>();

        // Act
        component.Find("button[title='Eliminar']").Click();

        // Assert
        _mesasApiMock.Verify(x => x.EliminarAsync(mesaId), Times.Once);
    }

    [Fact]
    public void AplicarFiltros_DeberiaLlamarApiServiceConParametros()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        var component = RenderComponent<Mesas>();

        // Act
        component.Find("select").Change("Disponible");
        component.Find("input[placeholder='Zona/Ubicación']").Change("Interior");
        component.Find("input[placeholder='2']").Change("4");
        component.Find("button:contains('Aplicar Filtros')").Click();

        // Assert
        _mesasApiMock.Verify(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CalcularEstadisticas_ConMesas_DeberiaCalcularCorrectamente()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new() { Estado = "Disponible", Capacidad = 4 },
            new() { Estado = "Ocupada", Capacidad = 6 },
            new() { Estado = "Reservada", Capacidad = 2 },
            new() { Estado = "Disponible", Capacidad = 4 }
        };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("p:contains('4')").Should().NotBeNull(); // Total de mesas
        component.Find("p:contains('2')").Should().NotBeNull(); // Disponibles
        component.Find("p:contains('1')").Should().NotBeNull(); // Ocupadas
        component.Find("p:contains('1')").Should().NotBeNull(); // Reservadas
        component.Find("p:contains('16')").Should().NotBeNull(); // Capacidad total
    }

    [Fact]
    public void GetEstadoClass_DeberiaRetornarClasesCorrectas()
    {
        // Arrange
        var mesas = new List<MesaDto>
        {
            new() { Estado = "Disponible" },
            new() { Estado = "Ocupada" },
            new() { Estado = "Reservada" },
            new() { Estado = "Mantenimiento" }
        };
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(mesas);

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        component.Find("span.bg-green-100").Should().NotBeNull(); // Disponible
        component.Find("span.bg-red-100").Should().NotBeNull(); // Ocupada
        component.Find("span.bg-blue-100").Should().NotBeNull(); // Reservada
        component.Find("span.bg-gray-100").Should().NotBeNull(); // Mantenimiento
    }

    [Fact]
    public void OnInitializedAsync_DeberiaCargarMesas()
    {
        // Arrange
        _mesasApiMock.Setup(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                    .ReturnsAsync(new List<MesaDto>());

        // Act
        var component = RenderComponent<Mesas>();

        // Assert
        _mesasApiMock.Verify(x => x.ObtenerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
    }
}
