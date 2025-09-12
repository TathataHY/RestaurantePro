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
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("h3").TextContent.Should().Be("Usuarios");
        component.Find("p.text-muted").TextContent.Should().Be("Gestión de personal y roles.");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarControlesDeBusqueda()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("input[placeholder='Buscar por nombre o email']").Should().NotBeNull();
        component.Find("button:contains('Buscar')").Should().NotBeNull();
        component.Find("button:contains('Ver perfil')").Should().NotBeNull();
        component.Find("button:contains('Nuevo Usuario')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCheckboxIncluirInactivos()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var checkbox = component.Find("input[type='checkbox']");
        checkbox.Should().NotBeNull();
        checkbox.GetAttribute("id").Should().Be("chkIncluirInactivos");
        
        var label = component.Find("label[for='chkIncluirInactivos']");
        label.TextContent.Should().Be("Incluir inactivos");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTablaConEncabezados()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var tabla = component.Find("table.table-striped");
        tabla.Should().NotBeNull();
        
        var encabezados = component.FindAll("th");
        encabezados.Should().HaveCount(6);
        encabezados[0].TextContent.Should().Be("Nombre");
        encabezados[1].TextContent.Should().Be("Usuario");
        encabezados[2].TextContent.Should().Be("Email");
        encabezados[3].TextContent.Should().Be("Rol");
        encabezados[4].TextContent.Should().Be("Nivel");
    }

    // ===== PRUEBAS DE ESTADO DE CARGA =====

    [Fact]
    public void Renderizar_ConUsuariosNull_DeberiaMostrarCargando()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((List<UsuarioDto>?)null);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("td:contains('Cargando...')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConListaVacia_DeberiaMostrarSinResultados()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        component.Find("td:contains('Sin resultados o no autorizado.')").Should().NotBeNull();
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

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var filas = component.FindAll("tbody tr");
        filas.Should().HaveCount(2);
        
        component.Find("tbody tr td:nth-child(1)").TextContent.Should().Be("Juan Pérez");
        component.Find("tbody tr td:nth-child(2)").TextContent.Should().Be("jperez");
        component.Find("tbody tr td:nth-child(3)").TextContent.Should().Be("juan@test.com");
        component.Find("tbody tr td:nth-child(4)").TextContent.Should().Be("Administrador");
        component.Find("tbody tr td:nth-child(5)").TextContent.Should().Be("Alto");
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

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonesAccion = component.FindAll("button.btn-sm");
        botonesAccion.Should().HaveCount(2);
        component.Find("button.btn-sm:contains('Editar')").Should().NotBeNull();
        component.Find("button.btn-sm:contains('Eliminar')").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonNuevoUsuario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonNuevo = component.Find("button:contains('Nuevo Usuario')");
        botonNuevo.Should().NotBeNull();
        botonNuevo.ClassList.Should().Contain("btn-success");
    }

    // ===== PRUEBAS DE MODAL DE EDICIÓN =====

    [Fact]
    public void Renderizar_ConModalCerrado_NoDeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var modales = component.FindAll(".modal.d-block");
        modales.Should().BeEmpty();
    }

    [Fact]
    public void Renderizar_ConModalAbierto_DeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        
        // Simular abrir modal
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var modal = component.Find(".modal.d-block");
        modal.Should().NotBeNull();
        component.Find(".modal.d-block .modal-title").TextContent.Should().Be("Nuevo usuario");
    }

    [Fact]
    public void Renderizar_ConModalAbierto_DeberiaMostrarFormulario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var form = component.Find("form");
        form.Should().NotBeNull();
        
        component.Find("input[placeholder='Nombre completo']").Should().NotBeNull();
        component.Find("input[placeholder='Nombre de usuario']").Should().NotBeNull();
        component.Find("input[placeholder='Email']").Should().NotBeNull();
        component.Find("input[placeholder='Rol']").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_ConModalCreacion_DeberiaMostrarCampoPassword()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var campoPassword = component.Find("input[type='password']");
        campoPassword.Should().NotBeNull();
        campoPassword.GetAttribute("placeholder").Should().Be("Contraseña temporal");
    }

    // ===== PRUEBAS DE MODAL DE CONFIRMACIÓN =====

    [Fact]
    public void Renderizar_ConModalConfirmacionCerrado_NoDeberiaMostrarModal()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var modales = component.FindAll(".modal.d-block");
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

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Eliminar')").Click();

        // Assert
        var modal = component.Find(".modal.d-block");
        modal.Should().NotBeNull();
        component.Find(".modal.d-block .modal-title").TextContent.Should().Be("Eliminar usuario");
    }

    [Fact]
    public void Renderizar_ConModalConfirmacionAbierto_DeberiaMostrarMensajeConfirmacion()
    {
        // Arrange
        var usuarios = new List<UsuarioDto>
        {
            new UsuarioDto { Id = Guid.NewGuid(), NombreCompleto = "Test User" }
        };

        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(usuarios);

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Eliminar')").Click();

        // Assert
        var mensaje = component.Find("p:contains('¿Seguro que deseas eliminar a')");
        mensaje.Should().NotBeNull();
        mensaje.TextContent.Should().Contain("Test User");
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraResponsiva()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var row = component.Find(".row.mb-3");
        row.Should().NotBeNull();
        
        var cols = component.FindAll(".col-md-");
        cols.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaTenerTablaResponsiva()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var tablaResponsiva = component.Find(".table-responsive");
        tablaResponsiva.Should().NotBeNull();
        
        var tabla = component.Find(".table-responsive table.table-striped");
        tabla.Should().NotBeNull();
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosBotones()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        botones.Should().Contain(b => b.TextContent.Contains("Buscar"));
        botones.Should().Contain(b => b.TextContent.Contains("Ver perfil"));
        botones.Should().Contain(b => b.TextContent.Contains("Nuevo Usuario"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesConClasesCorrectas()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var botonBuscar = component.Find("button:contains('Buscar')");
        botonBuscar.ClassList.Should().Contain("btn-primary");
        
        var botonNuevo = component.Find("button:contains('Nuevo Usuario')");
        botonNuevo.ClassList.Should().Contain("btn-success");
    }

    // ===== PRUEBAS DE FORMULARIOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarFormularioDeBusqueda()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var inputBusqueda = component.Find("input[placeholder='Buscar por nombre o email']");
        inputBusqueda.Should().NotBeNull();
        inputBusqueda.ClassList.Should().Contain("form-control");
    }

    [Fact]
    public void Renderizar_DeberiaMostrarFormularioDeEdicion()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var form = component.Find("form");
        form.Should().NotBeNull();
        
        var inputs = component.FindAll("input.form-control");
        inputs.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarValidacionesEnFormulario()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();
        component.Find("button:contains('Nuevo Usuario')").Click();

        // Assert
        var validator = component.Find("DataAnnotationsValidator");
        validator.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ACCESIBILIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerAtributosDeAccesibilidad()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var checkbox = component.Find("input[type='checkbox']");
        checkbox.GetAttribute("id").Should().NotBeNullOrEmpty();
        
        var label = component.Find("label[for='chkIncluirInactivos']");
        label.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_DeberiaInicializarConValoresPorDefecto()
    {
        // Arrange
        _usuariosApiMock.Setup(x => x.ObtenerUsuariosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UsuarioDto>());

        // Act
        var component = RenderComponent<Usuarios>();

        // Assert
        var inputBusqueda = component.Find("input[placeholder='Buscar por nombre o email']");
        inputBusqueda.GetAttribute("value").Should().BeEmpty();
        
        var checkbox = component.Find("input[type='checkbox']");
        checkbox.GetAttribute("checked").Should().BeNull(); // Por defecto no está marcado
    }
}
