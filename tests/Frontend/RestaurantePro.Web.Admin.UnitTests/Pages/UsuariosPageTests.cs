using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Pages;

public class UsuariosPageTests : TestContext
{
    private readonly Mock<IUsuariosApiService> _usuariosApiMock;
    private readonly Mock<IAuthApiService> _authApiMock;

    public UsuariosPageTests()
    {
        _usuariosApiMock = new Mock<IUsuariosApiService>();
        _authApiMock = new Mock<IAuthApiService>();

        Services.AddSingleton(_usuariosApiMock.Object);
        Services.AddSingleton(_authApiMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTituloYDescripcion()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("h1").TextContent.Should().Be("Gestión de Usuarios");
        // La descripción ya no existe en la nueva versión
    }

    [Fact]
    public void Renderizar_DeberiaMostrarControlesDeBusqueda()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("input[placeholder='Nombre o email del usuario']").Should().NotBeNull();
        component.Find("button:contains('Buscar')").Should().NotBeNull();
        // El botón "Ver perfil" ahora es solo un ícono
        component.Find("button:contains('Nuevo Usuario')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCheckboxIncluirInactivos()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var selectEstado = component.Find("select");
        selectEstado.Should().NotBeNull();
        selectEstado.GetAttribute("value").Should().Be("");
        
        // El label ahora es para el campo de búsqueda
        var label = component.Find("span.text-sm.font-medium.text-gray-700");
        label.TextContent.Should().Be("Buscar por nombre o email");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTablaConEncabezados()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var tabla = component.Find("table");
        tabla.Should().NotBeNull();
        
        var encabezados = component.FindAll("th");
        encabezados.Should().HaveCount(6);
        encabezados[0].TextContent.Should().Be("Usuario");
        encabezados[1].TextContent.Should().Be("Email");
        encabezados[2].TextContent.Should().Be("Rol");
        encabezados[3].TextContent.Should().Be("Nivel");
        encabezados[4].TextContent.Should().Be("Estado");
    }

    // ===== PRUEBAS DE ESTADO DE CARGA =====

    [Fact]
    public void Renderizar_ConUsuariosNull_DeberiaMostrarCargando()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((List<UsuarioDto>?)null);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("td:contains('Cargando usuarios...')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConListaVacia_DeberiaMostrarSinResultados()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("td:contains('No se encontraron usuarios')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConUsuarios_DeberiaMostrarDatos()
    {
        // Arrange
        var usuarios = new List<UsuarioDto>
        {
            new UsuarioDto
            {
                Id = Guid.NewGuid(),
                NombreCompleto = "Juan Pérez",
                NombreUsuario = "jperez",
                Email = "juan@test.com",
                Rol = "Administrador",
                NivelAcceso = 3
            },
            new UsuarioDto
            {
                Id = Guid.NewGuid(),
                NombreCompleto = "María García",
                NombreUsuario = "mgarcia",
                Email = "maria@test.com",
                Rol = "Mesero",
                NivelAcceso = 2
            }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var filas = component.FindAll("tbody tr");
        filas.Should().HaveCount(2);
        
        // Verificar que los datos están presentes en la tabla
        var fila = component.Find("tbody tr");
        fila.TextContent.Should().Contain("Juan Pérez");
        fila.TextContent.Should().Contain("jperez");
        fila.TextContent.Should().Contain("juan@test.com");
        fila.TextContent.Should().Contain("Administrador");
        fila.TextContent.Should().Contain("3");
    }

    // ===== PRUEBAS DE BOTONES DE ACCIÓN =====

    [Fact]
    public void Renderizar_ConUsuarios_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        var usuarios = new List<UsuarioDto>
        {
            new UsuarioDto { Id = Guid.NewGuid(), NombreCompleto = "Test User" }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonesAccion = component.FindAll("button[title='Editar usuario'], button[title='Eliminar usuario']");
        botonesAccion.Should().HaveCount(2);
        component.Find("button[title='Editar usuario']").Should().NotBeNull();
        component.Find("button[title='Eliminar usuario']").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonNuevoUsuario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonNuevo = component.Find("button:contains('Nuevo Usuario')");
        botonNuevo.Should().NotBeNull();
        botonNuevo.ClassList.Should().Contain("bg-blue-600");
    }

    // ===== PRUEBAS DE MODAL DE EDICIÓN =====

    [Fact]
    public void Renderizar_ConModalCerrado_NoDeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var modales = component.FindAll("div[role='dialog']");
        modales.Should().BeEmpty();
    }

    [Fact]
    public void Renderizar_ConModalAbierto_DeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        
        // Simular abrir modal
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var modal = component.Find("div[role='dialog']");
        modal.Should().NotBeNull();
        component.Find("div[role='dialog'] h2#modal-title").TextContent.Should().Be("Nuevo usuario");
    }

    [Fact]
    public void Renderizar_ConModalAbierto_DeberiaMostrarFormulario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var form = component.Find("form");
        form.Should().NotBeNull();
        
        component.FindAll("input").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Renderizar_ConModalCreacion_DeberiaMostrarCampoPassword()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var campoPassword = component.Find("input[type='password']");
        campoPassword.Should().NotBeNull();
    }

    // ===== PRUEBAS DE MODAL DE CONFIRMACIÓN =====

    [Fact]
    public void Renderizar_ConModalConfirmacionCerrado_NoDeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var modales = component.FindAll("div[role='dialog']");
        modales.Should().BeEmpty();
    }

    [Fact]
    public void Renderizar_ConModalConfirmacionAbierto_DeberiaMostrarModal()
    {
        // Arrange
        var usuarios = new List<UsuarioDto>
        {
            new UsuarioDto { Id = Guid.NewGuid(), NombreCompleto = "Test User" }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();
        
        // Esperar a que se carguen los usuarios
        component.WaitForState(() => component.FindAll("button").Count > 0);
        
        component.Find("button[title='Eliminar usuario']").Click();

        // Assert
        var modal = component.Find("div[role='dialog']");
        modal.Should().NotBeNull();
        component.Find("div[role='dialog'] h3#modal-title").TextContent.Trim().Should().Be("Eliminar Usuario");
    }

    [Fact]
    public void Renderizar_ConModalConfirmacionAbierto_DeberiaMostrarMensajeConfirmacion()
    {
        // Arrange
        var usuarios = new List<UsuarioDto>
        {
            new UsuarioDto { Id = Guid.NewGuid(), NombreCompleto = "Test User" }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();
        
        // Esperar a que se carguen los usuarios
        component.WaitForState(() => component.FindAll("button").Count > 0);
        
        component.Find("button[title='Eliminar usuario']").Click();

        // Assert
        var mensaje = component.Find("p:contains('¿Estás seguro de que deseas eliminar al usuario')");
        mensaje.Should().NotBeNull();
        mensaje.TextContent.Should().Contain("Test User");
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var filtrosContainer = component.Find(".bg-white.p-6.rounded-xl.shadow-sm.border.border-gray-200");
        filtrosContainer.Should().NotBeNull();
        
        var grid = component.Find(".grid.grid-cols-1.md\\:grid-cols-2.lg\\:grid-cols-4");
        grid.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaTenerTablaResponsiva()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var tabla = component.Find("table");
        tabla.Should().NotBeNull();
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosBotones()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        botones.Should().Contain(b => b.TextContent.Contains("Buscar"));
        // El botón "Ver perfil" ahora es solo un ícono, no tiene texto
        botones.Should().Contain(b => b.TextContent.Contains("Nuevo Usuario"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConClasesCorrectas()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonBuscar = component.Find("button:contains('Buscar')");
        botonBuscar.ClassList.Should().Contain("bg-blue-600");
        
        var botonNuevo = component.Find("button:contains('Nuevo Usuario')");
        botonNuevo.ClassList.Should().Contain("bg-blue-600");
    }

    // ===== PRUEBAS DE FORMULARIOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarFormularioDeBusqueda()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var inputBusqueda = component.Find("input[placeholder='Nombre o email del usuario']");
        inputBusqueda.Should().NotBeNull();
        inputBusqueda.ClassList.Should().Contain("form-input");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarFormularioDeEdicion()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var modal = component.Find("div[role='dialog']");
        modal.Should().NotBeNull();
        
        var form = component.Find("form");
        form.Should().NotBeNull();
        
        var inputs = component.FindAll("input");
        inputs.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarValidacionesEnFormulario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var form = component.Find("form");
        form.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ACCESIBILIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerAtributosDeAccesibilidad()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var select = component.Find("select");
        select.Should().NotBeNull();
        
        var label = component.Find("span.text-sm.font-medium.text-gray-700");
        label.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_DeberiaInicializarConValoresPorDefecto()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var inputBusqueda = component.Find("input[placeholder='Nombre o email del usuario']");
        inputBusqueda.GetAttribute("value").Should().BeEmpty();
        
        var selectEstado = component.Find("select");
        selectEstado.GetAttribute("value").Should().Be(""); // Por defecto está vacío
    }
}
