namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Commands;
using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Moq;
using FluentAssertions;
using Xunit;
using MockQueryable.Moq;
using CrearUsuarioHorarioDto = RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario.HorarioTrabajoDto;

/// <summary>
/// Pruebas unitarias para CrearUsuarioHandler
/// Tests que cubren todos los escenarios de creación de usuarios con roles y permisos
/// </summary>
public class CrearUsuarioHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CrearUsuarioHandler>> _mockLogger;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CrearUsuarioHandler _handler;
    private readonly Usuario _usuarioCreadorAdmin;
    private readonly Usuario _usuarioCreadorGerente;
    private readonly UsuarioDto _usuarioDtoEjemplo;

    public CrearUsuarioHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CrearUsuarioHandler>>();
        _mockEmailService = new Mock<IEmailService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _handler = new CrearUsuarioHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockEmailService.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _usuarioCreadorAdmin = CrearUsuarioAdministrador();
        _usuarioCreadorGerente = CrearUsuarioGerente();
        _usuarioDtoEjemplo = CrearUsuarioDtoEjemplo();

        ConfigurarMockContext();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaCrearUsuarioCorrectamente()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearEmpleado(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@restaurantepro.com",
            "555-123-4567",
            "Cocina",
            _usuarioCreadorAdmin.Id);

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(_usuarioDtoEjemplo);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ConUsuarioCreadorNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearEmpleado(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@restaurantepro.com",
            "555-123-4567",
            "Cocina",
            Guid.NewGuid()); // Usuario creador inexistente

        SetupUsuarioNoExistenteMock();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El usuario creador no existe");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConUsuarioSinPermisos_DeberiaRetornarError()
    {
        // Arrange
        var usuarioSinPermisos = Usuario.Crear("empleado.simple", "Empleado Simple", "empleado@test.com", RolUsuario.Mesero);
        
        var command = CrearUsuarioCommand.CrearEmpleado(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@restaurantepro.com",
            "555-123-4567",
            "Cocina",
            usuarioSinPermisos.Id);

        SetupUsuarioExistenteMock(usuarioSinPermisos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No tiene permisos para crear usuarios");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConRolSuperiorAlCreador_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearAdministrador(
            "admin.nuevo",
            "Nuevo Administrador",
            "admin.nuevo@restaurantepro.com",
            "555-999-8888",
            _usuarioCreadorGerente.Id); // Gerente tratando de crear Admin

        SetupUsuarioExistenteMock(_usuarioCreadorGerente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No puede asignar un rol igual o superior al suyo");
    }

    [Fact]
    public async Task Handle_ConRolInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.test",
            NombreCompleto = "Usuario Test",
            Email = "usuario.test@restaurantepro.com",
            Password = "TempPassword123!",
            ConfirmarPassword = "TempPassword123!",
            Rol = "RolInexistente", // Rol que no existe
            UsuarioCreadorId = _usuarioCreadorAdmin.Id
        };

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Rol no válido");
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearSupervisor_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearSupervisor(
            "supervisor.cocina",
            "Supervisor Cocina",
            "supervisor.cocina@restaurantepro.com",
            "555-777-6666",
            "Cocina",
            _usuarioCreadorAdmin.Id);

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar uso del factory method
        command.Rol.Should().Be("Supervisor");
        command.Departamento.Should().Be("Cocina");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConHorariosDeTrabajo_DeberiaLoggearConfiguracion()
    {
        // Arrange
        var horarios = new List<CrearUsuarioHorarioDto>
        {
            new() { DiaSemana = 1, HoraInicio = "08:00", HoraFin = "16:00" },
            new() { DiaSemana = 2, HoraInicio = "08:00", HoraFin = "16:00" }
        };

        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.horarios",
            NombreCompleto = "Usuario con Horarios",
            Email = "usuario.horarios@restaurantepro.com",
            Password = "TempPassword123!",
            ConfirmarPassword = "TempPassword123!",
            Rol = "Empleado",
            HorariosTrabajo = horarios,
            UsuarioCreadorId = _usuarioCreadorAdmin.Id
        };

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se loggea la configuración de horarios
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("horarios omitida")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConConfiguracionPersonal_DeberiaIncluirConfiguraciones()
    {
        // Arrange
        var configuraciones = new Dictionary<string, string>
        {
            { "Tema", "Oscuro" },
            { "Idioma", "es-ES" },
            { "NotificacionesEmail", "true" }
        };

        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.config",
            NombreCompleto = "Usuario con Configuraciones",
            Email = "usuario.config@restaurantepro.com",
            Password = "TempPassword123!",
            ConfirmarPassword = "TempPassword123!",
            Rol = "Empleado",
            ConfiguracionPersonal = configuraciones,
            UsuarioCreadorId = _usuarioCreadorAdmin.Id
        };

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.ConfiguracionPersonal.Should().ContainKey("Tema");
        command.ConfiguracionPersonal.Should().ContainKey("Idioma");
        command.ConfiguracionPersonal.Should().ContainKey("NotificacionesEmail");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnBaseDatos_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearEmpleado(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@restaurantepro.com",
            "555-123-4567",
            "Cocina",
            _usuarioCreadorAdmin.Id);

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al crear el usuario");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiEnviarNotificacionesBienvenida()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearEmpleado(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@restaurantepro.com",
            "555-123-4567",
            "Cocina",
            _usuarioCreadorAdmin.Id);

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se envían notificaciones
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                It.Is<string>(email => email == "juan.perez@restaurantepro.com"),
                It.Is<string>(subject => subject.Contains("Bienvenido")),
                It.IsAny<string>()),
            Times.Once);

        // Verificar logging de notificaciones
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Notificaciones de creación enviadas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCreacion()
    {
        // Arrange
        var command = CrearUsuarioCommand.CrearEmpleado(
            "test.logging",
            "Test Logging",
            "test.logging@restaurantepro.com",
            "555-888-7777",
            "Administración",
            _usuarioCreadorAdmin.Id);

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando creación de usuario")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("creado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Empleado", 1)]
    [InlineData("Supervisor", 5)]
    [InlineData("Gerente", 7)]
    [InlineData("Administrador", 10)]
    public async Task Handle_ConDiferentesRoles_DeberiaAsignarNivelCorrectoAutomaticamente(string rol, int nivelEsperado)
    {
        // Arrange
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = $"usuario.{rol.ToLower()}",
            NombreCompleto = $"Usuario {rol}",
            Email = $"usuario.{rol.ToLower()}@restaurantepro.com",
            Password = "TempPassword123!",
            ConfirmarPassword = "TempPassword123!",
            Rol = rol,
            UsuarioCreadorId = _usuarioCreadorAdmin.Id
        };

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que el nivel esperado es correcto para el rol
        nivelEsperado.Should().BeGreaterThan(0);
        
        switch (rol)
        {
            case "Empleado":
                nivelEsperado.Should().Be(1);
                break;
            case "Supervisor":
                nivelEsperado.Should().Be(5);
                break;
            case "Gerente":
                nivelEsperado.Should().Be(7);
                break;
            case "Administrador":
                nivelEsperado.Should().Be(10);
                break;
        }
    }

    [Fact]
    public async Task Handle_ConPermisosEspecificos_DeberiaIncluirPermisosAdicionales()
    {
        // Arrange
        var permisosEspeciales = new List<string>
        {
            "AccesoSistemaPuntos",
            "ModificarPrecios",
            "GenerarReportes"
        };

        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.permisos",
            NombreCompleto = "Usuario con Permisos Especiales",
            Email = "usuario.permisos@restaurantepro.com",
            Password = "TempPassword123!",
            ConfirmarPassword = "TempPassword123!",
            Rol = "Empleado",
            PermisosEspecificos = permisosEspeciales,
            UsuarioCreadorId = _usuarioCreadorAdmin.Id
        };

        SetupUsuarioExistenteMock(_usuarioCreadorAdmin);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.PermisosEspecificos.Should().Contain("AccesoSistemaPuntos");
        command.PermisosEspecificos.Should().Contain("ModificarPrecios");
        command.PermisosEspecificos.Should().Contain("GenerarReportes");

        // Verificar que se loggea la configuración de permisos
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("permisos para usuario")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Helper Methods

    private void ConfigurarMockContext()
    {
        // Mock para AddAsync - crear un mock simple que acepta cualquier entidad
        var mockUsuarios = new Mock<DbSet<Usuario>>();
        mockUsuarios.Setup(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                   .Returns(default(ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Usuario>>));

        _mockContext.Setup(c => c.Usuarios).Returns(mockUsuarios.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupUsuarioExistenteMock(Usuario usuario)
    {
        var usuariosList = new List<Usuario> { usuario };
        var mockQueryable = usuariosList.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(c => c.Usuarios).Returns(mockQueryable.Object);
    }

    private void SetupUsuarioNoExistenteMock()
    {
        var usuariosList = new List<Usuario>();
        var mockQueryable = usuariosList.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(c => c.Usuarios).Returns(mockQueryable.Object);
    }

    private Usuario CrearUsuarioAdministrador()
    {
        return Usuario.Crear("admin", "Administrador Sistema", "admin@restaurantepro.com", RolUsuario.Administrador);
    }

    private Usuario CrearUsuarioGerente()
    {
        return Usuario.Crear("gerente", "Gerente General", "gerente@restaurantepro.com", RolUsuario.Gerente);
    }

    private UsuarioDto CrearUsuarioDtoEjemplo()
    {
        return new UsuarioDto
        {
            Id = Guid.NewGuid(),
            NombreUsuario = "usuario.test",
            Nombre = "Usuario",
            Apellido = "Test",
            Email = "usuario.test@restaurantepro.com",
            Rol = "Empleado",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    #endregion
} 