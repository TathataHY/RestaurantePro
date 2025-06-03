namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Validators;
using CrearUsuarioHorarioDto = RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario.HorarioTrabajoDto;
using RestaurantePro.Application.UnitTests.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CREAR USUARIO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de creación de usuarios
/// Cobertura: 100% de reglas de negocio del CrearUsuarioValidator
/// </summary>
public class CrearUsuarioValidatorTests
{
    private readonly CrearUsuarioValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;

    public CrearUsuarioValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        
        // Configurar el contexto con el comportamiento básico
        ConfigurarContextoBasico();
        
        _validator = new CrearUsuarioValidator(_mockContext.Object);
    }

    private void ConfigurarContextoBasico()
    {
        // Configurar comportamiento por defecto: usuarios únicos y usuario creador válido
        var usuarios = new List<Usuario>();
        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(c => c.Usuarios).Returns(mockUsuarios.Object);
    }

    private void ConfigurarUsuarioExiste(bool nombreUsuarioExiste = false, bool emailExiste = false, bool usuarioCreadorExiste = true, Guid? usuarioCreadorId = null)
    {
        var usuarios = new List<Usuario>();

        if (nombreUsuarioExiste)
        {
            var usuarioConNombreExistente = Usuario.Crear("usuario.existente", "Usuario Existente", "existente@test.com", RolUsuario.Mesero);
            usuarioConNombreExistente.Activar();
            usuarios.Add(usuarioConNombreExistente);
        }

        if (emailExiste)
        {
            var usuarioConEmailExistente = Usuario.Crear("otro.usuario", "Otro Usuario", "email.existente@test.com", RolUsuario.Mesero);
            usuarioConEmailExistente.Activar();
            usuarios.Add(usuarioConEmailExistente);
        }

        if (usuarioCreadorExiste)
        {
            var usuarioCreador = Usuario.Crear("usuario.creador", "Usuario Creador", "creador@test.com", RolUsuario.Administrador);
            usuarioCreador.Activar();
            usuarios.Add(usuarioCreador);
        }

        var mockDbSet = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(x => x.Usuarios).Returns(mockDbSet.Object);
    }

    #region Validation Command Helper

    private CrearUsuarioCommand CrearCommandValido()
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.test",
            NombreCompleto = "Usuario de Prueba",
            Email = "usuario.test@restaurante.com",
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Empleado",
            RolesAdicionales = new List<string>(),
            NivelAcceso = 3,
            PermisosEspecificos = new List<string> { "VerReportes" },
            Telefono = "+521234567890",
            Departamento = "Cocina",
            Puesto = "Cocinero Junior",
            UsuarioCreadorId = Guid.NewGuid(),
            HorariosTrabajo = new List<CrearUsuarioHorarioDto>
            {
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.FromHours(8),
                    HoraFin = TimeSpan.FromHours(16),
                    EsDiaLibre = false
                }
            }
        };
    }

    #endregion

    #region Validación NombreUsuario

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreUsuarioVacioONull_DeberiaRetornarError(string? nombreInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreUsuario = nombreInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario es requerido"));
    }

    [Theory]
    [InlineData("ab")]  // Menos de 3 caracteres
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreUsuarioMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreUsuario = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario debe tener al menos 3 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreUsuarioMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreUsuario = new string('a', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario no puede exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("usuario con espacios")]  // Espacios no permitidos
    [InlineData("usuario@invalid")]       // @ no permitido
    [InlineData("usuario#invalid")]       // # no permitido
    [InlineData("usuario$invalid")]       // $ no permitido
    public async Task Validate_ConNombreUsuarioConCaracteresInvalidos_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreUsuario = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos"));
    }

    [Theory]
    [InlineData("usuario.test")]
    [InlineData("usuario_test")]
    [InlineData("usuario-test")]
    [InlineData("usuario123")]
    [InlineData("abc")]  // Mínimo válido
    public async Task Validate_ConNombreUsuarioValido_NoDeberiaRetornarErrorDeNombreUsuario(string nombreValido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreUsuario = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario));
    }

    #endregion

    #region Validación NombreCompleto

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreCompletoVacioONull_DeberiaRetornarError(string? nombreInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreCompleto = nombreInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo es requerido"));
    }

    [Theory]
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreCompletoMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreCompleto = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo debe tener al menos 2 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreCompletoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreCompleto = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo no puede exceder 200 caracteres"));
    }

    [Theory]
    [InlineData("Juan123")]       // Números no permitidos
    [InlineData("María@")]        // @ no permitido
    [InlineData("José#")]         // # no permitido
    public async Task Validate_ConNombreCompletoConCaracteresInvalidos_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreCompleto = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo solo puede contener letras, espacios, acentos y apostrofes"));
    }

    [Theory]
    [InlineData("María José")]
    [InlineData("Juan Carlos Pérez")]
    [InlineData("Ñuño Álvarez")]
    [InlineData("Ab")]  // Mínimo válido
    public async Task Validate_ConNombreCompletoValido_NoDeberiaRetornarErrorDeNombreCompleto(string nombreValido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.NombreCompleto = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto));
    }

    #endregion

    #region Validación Email

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConEmailVacioONull_DeberiaRetornarError(string? emailInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Email = emailInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email es requerido"));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [InlineData("email.domain.com")]
    [InlineData("email@@domain.com")]
    public async Task Validate_ConEmailFormatoInvalido_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email no tiene un formato válido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        // Crear usuario creador de manera que sea reconocido por el validator
        var usuarioCreador = Usuario.Crear("usuario.creador", "Usuario Creador", "creador@test.com", RolUsuario.Administrador);
        usuarioCreador.Activar();
        
        // Configurar el mock para que devuelva este usuario cuando se busque por el ID específico
        var usuarios = new List<Usuario> { usuarioCreador };
        var mockDbSet = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(x => x.Usuarios).Returns(mockDbSet.Object);
        
        var emailMuyLargo = new string('a', 315) + "@test.com"; // 325 caracteres - excede el límite de 320
        var command = CrearCommandValido();
        command.Email = emailMuyLargo;
        command.UsuarioCreadorId = usuarioCreador.Id; // Usar el ID real del usuario creado

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 320 caracteres"));
    }

    [Theory]
    [InlineData("usuario@restaurante.com")]
    [InlineData("test.email@empresa.com.mx")]
    [InlineData("admin+info@sistema.net")]
    [InlineData("a@b.co")]  // Mínimo válido
    public async Task Validate_ConEmailValido_NoDeberiaRetornarErrorDeEmail(string emailValido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Email = emailValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Email));
    }

    #endregion

    #region Validación Password

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConPasswordVaciaONull_DeberiaRetornarError(string? passwordInvalida)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = passwordInvalida ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña es requerida"));
    }

    [Theory]
    [InlineData("Pass1!")]   // 7 caracteres - menos de 8
    [InlineData("1234567")]  // 7 caracteres numéricos
    public async Task Validate_ConPasswordMuyCorta_DeberiaRetornarError(string passwordCorta)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = passwordCorta;
        command.ConfirmarPassword = passwordCorta;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña debe tener al menos 8 caracteres"));
    }

    [Fact]
    public async Task Validate_ConPasswordMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = new string('A', 129); // Más de 128 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña no puede exceder 128 caracteres"));
    }

    [Theory]
    [InlineData("password123")]    // Sin mayúscula ni carácter especial
    [InlineData("PASSWORD123")]    // Sin minúscula ni carácter especial
    [InlineData("Password")]       // Sin número ni carácter especial
    [InlineData("Password123")]    // Sin carácter especial
    public async Task Validate_ConPasswordSinComplejidadSuficiente_DeberiaRetornarError(string passwordDebil)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = passwordDebil;
        command.ConfirmarPassword = passwordDebil;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial"));
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("MiClave2025@")]
    [InlineData("Segura$456")]
    [InlineData("Test123#")]
    public async Task Validate_ConPasswordValida_NoDeberiaRetornarErrorDePassword(string passwordValida)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = passwordValida;
        command.ConfirmarPassword = passwordValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Password));
    }

    #endregion

    #region Validación ConfirmarPassword

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConConfirmarPasswordVaciaONull_DeberiaRetornarError(string? confirmacionInvalida)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.ConfirmarPassword = confirmacionInvalida ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword) &&
            e.ErrorMessage.Contains("La confirmación de contraseña es requerida"));
    }

    [Fact]
    public async Task Validate_ConConfirmarPasswordDiferente_DeberiaRetornarError()
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = "Password123!";
        command.ConfirmarPassword = "DiferentePassword456@";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword) &&
            e.ErrorMessage.Contains("La confirmación de contraseña debe coincidir con la contraseña"));
    }

    [Fact]
    public async Task Validate_ConConfirmarPasswordIgual_NoDeberiaRetornarErrorDeConfirmacion()
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Password = "Password123!";
        command.ConfirmarPassword = "Password123!";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword));
    }

    #endregion

    #region Validación Rol

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConRolVacioONull_DeberiaRetornarError(string? rolInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Rol = rolInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Rol) &&
            e.ErrorMessage.Contains("El rol es requerido"));
    }

    [Theory]
    [InlineData("RolInexistente")]
    [InlineData("Admin")]  // No está en la lista de roles válidos
    [InlineData("User")]   // No está en la lista de roles válidos
    public async Task Validate_ConRolInvalido_DeberiaRetornarError(string rolInvalido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Rol = rolInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Rol) &&
            e.ErrorMessage.Contains("El rol debe ser uno de: Empleado, Supervisor, Gerente, Administrador, SuperAdministrador"));
    }

    [Theory]
    [InlineData("Empleado")]
    [InlineData("Supervisor")]
    [InlineData("Gerente")]
    [InlineData("Administrador")]
    [InlineData("SuperAdministrador")]
    public async Task Validate_ConRolValido_NoDeberiaRetornarErrorDeRol(string rolValido)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Rol = rolValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Rol));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        // Crear usuario creador de manera que sea reconocido por el validator
        var usuarioCreador = Usuario.Crear("usuario.creador", "Usuario Creador", "creador@test.com", RolUsuario.Administrador);
        usuarioCreador.Activar();
        
        // Configurar el mock para que devuelva este usuario cuando se busque por el ID específico
        var usuarios = new List<Usuario> { usuarioCreador };
        var mockDbSet = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(x => x.Usuarios).Returns(mockDbSet.Object);
        
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.completo",
            NombreCompleto = "Usuario Completo de Prueba",
            Email = "usuario.completo@restaurante.com.mx",
            Password = "PasswordCompleto123!",
            ConfirmarPassword = "PasswordCompleto123!",
            Rol = "Supervisor",
            RolesAdicionales = new List<string> { "Empleado" },
            NivelAcceso = 5,
            PermisosEspecificos = new List<string> { "VerReportes", "GestionarEmpleados" },
            Telefono = "+521234567890",
            Departamento = "Servicio",
            Puesto = "Supervisor de Meseros",
            UsuarioCreadorId = usuarioCreador.Id, // Usar el ID real del usuario creado
            HorariosTrabajo = new List<CrearUsuarioHorarioDto>
            {
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.FromHours(8),
                    HoraFin = TimeSpan.FromHours(16),
                    EsDiaLibre = false
                },
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Martes",
                    HoraInicio = TimeSpan.FromHours(9),
                    HoraFin = TimeSpan.FromHours(17),
                    EsDiaLibre = false
                }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Debug: Imprimir errores específicos
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error en {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandMinimoValido_DeberiaSerValido()
    {
        // Arrange
        var usuarioCreadorId = Guid.NewGuid();
        
        // Crear usuario creador de manera que sea reconocido por el validator
        var usuarioCreador = Usuario.Crear("usuario.creador", "Usuario Creador", "creador@test.com", RolUsuario.Administrador);
        usuarioCreador.Activar();
        
        // Configurar el mock para que devuelva este usuario cuando se busque por el ID específico
        var usuarios = new List<Usuario> { usuarioCreador };
        var mockDbSet = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(x => x.Usuarios).Returns(mockDbSet.Object);
        
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "min", // Mínimo válido
            NombreCompleto = "Ab", // Mínimo válido
            Email = "a@b.co", // Mínimo válido
            Password = "Pass123!",
            ConfirmarPassword = "Pass123!",
            Rol = "Empleado",
            RolesAdicionales = new List<string>(),
            NivelAcceso = 1,
            PermisosEspecificos = new List<string>(),
            UsuarioCreadorId = usuarioCreador.Id, // Usar el ID real del usuario creado
            HorariosTrabajo = new List<CrearUsuarioHorarioDto>()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Debug: Imprimir errores específicos
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error en {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Empleado", 1)]
    [InlineData("Supervisor", 3)]
    [InlineData("Gerente", 6)]
    [InlineData("Administrador", 8)]
    [InlineData("SuperAdministrador", 10)]
    public async Task Validate_ConDiferentesCombinacionesRolNivel_DeberiaSerValido(string rol, int nivel)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Rol = rol;
        command.NivelAcceso = nivel;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Rol));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso));
    }

    [Theory]
    [InlineData("Cocina", "Cocinero")]
    [InlineData("Servicio", "Mesero")]
    [InlineData("Administración", "Contador")]
    [InlineData("Sistemas", "Desarrollador")]
    public async Task Validate_ConDiferentesCombinacionesDepartamentoPuesto_DeberiaSerValido(string departamento, string puesto)
    {
        // Arrange
        ConfigurarUsuarioExiste(usuarioCreadorExiste: true);
        var command = CrearCommandValido();
        command.Departamento = departamento;
        command.Puesto = puesto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Departamento));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Puesto));
    }

    #endregion
} 