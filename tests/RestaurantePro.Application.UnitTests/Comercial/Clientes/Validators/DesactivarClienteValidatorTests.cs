namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA DESACTIVAR CLIENTE VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de desactivación de clientes
/// Cobertura: 100% de reglas de negocio del DesactivarClienteValidator
/// </summary>
public class DesactivarClienteValidatorTests
{
    private readonly DesactivarClienteValidator _validator;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<DbSet<Cliente>> _clientesDbSetMock;

    public DesactivarClienteValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _clientesDbSetMock = new Mock<DbSet<Cliente>>();
        
        _contextMock.Setup(x => x.Clientes).Returns(_clientesDbSetMock.Object);
        
        _validator = new DesactivarClienteValidator(_contextMock.Object);
    }

    #region Validation Command Helper

    private DesactivarClienteCommand CrearCommandValido()
    {
        return new DesactivarClienteCommand
        {
            ClienteId = Guid.NewGuid(),
            RazonDesactivacion = "Cliente solicita baja",
            ObservacionesDesactivacion = "Cliente cambió de ciudad",
            UsuarioId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación ClienteId

    [Fact]
    public async Task Validate_ConClienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ClienteId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ClienteId) &&
            e.ErrorMessage.Contains("El ID del cliente es requerido") &&
            e.ErrorCode == "CLIENTE_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConClienteIdValido_NoDeberiaRetornarErrorDeClienteId()
    {
        // Arrange
        var command = CrearCommandValido();
        var clienteId = Guid.NewGuid();
        command.ClienteId = clienteId;

        // Mock cliente existente y activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarClienteCommand.ClienteId));
    }

    #endregion

    #region Validación RazonDesactivacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConRazonDesactivacionVacia_DeberiaRetornarError(string razonVacia)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = razonVacia;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("La razón de desactivación es requerida") &&
            e.ErrorCode == "RAZON_DESACTIVACION_REQUERIDA");
    }

    [Fact]
    public async Task Validate_ConRazonDesactivacionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = new string('A', 251); // Más de 250 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("La razón de desactivación no puede exceder 250 caracteres") &&
            e.ErrorCode == "RAZON_DESACTIVACION_LONGITUD");
    }

    [Theory]
    [InlineData("Cliente solicita baja")]
    [InlineData("Cambio de residencia")]
    [InlineData("Problemas de pago recurrentes")]
    [InlineData("Incumplimiento de políticas")]
    [InlineData("Cierre de empresa")]
    public async Task Validate_ConRazonDesactivacionValida_NoDeberiaRetornarErrorDeRazon(string razonValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = razonValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.RazonDesactivacion));
    }

    #endregion

    #region Validación ObservacionesDesactivacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarObservaciones(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesDesactivacion = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ObservacionesDesactivacion));
    }

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesDesactivacion = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ObservacionesDesactivacion) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres") &&
            e.ErrorCode == "OBSERVACIONES_LONGITUD");
    }

    [Fact]
    public async Task Validate_ConObservacionesValidas_NoDeberiaRetornarErrorDeObservaciones()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesDesactivacion = "Cliente se mudó a otra ciudad y no podrá continuar con el servicio";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ObservacionesDesactivacion));
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario es requerido") &&
            e.ErrorCode == "USUARIO_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorDeUsuarioId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.UsuarioId));
    }

    #endregion

    #region Validación de Estado Cliente

    [Fact]
    public async Task Validate_ConClienteYaDesactivado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var clienteId = Guid.NewGuid();
        command.ClienteId = clienteId;

        // Mock cliente ya desactivado
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = false }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ClienteId) &&
            e.ErrorMessage.Contains("El cliente ya se encuentra desactivado") &&
            e.ErrorCode == "CLIENTE_YA_DESACTIVADO");
    }

    [Fact]
    public async Task Validate_ConClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ClienteId = Guid.NewGuid();

        // Mock lista vacía (cliente no existe)
        var clientes = new List<Cliente>().AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.ClienteId) &&
            e.ErrorMessage.Contains("El cliente no existe") &&
            e.ErrorCode == "CLIENTE_NO_EXISTE");
    }

    #endregion

    #region Validaciones de Patrones de Texto

    [Theory]
    [InlineData("Razón con números 123")]
    [InlineData("Razón con símbolos @#$")]
    [InlineData("Razón válida con tildes áéíóú")]
    public async Task Validate_ConRazonConCaracteresValidos_DeberiaSerValido(string razonValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = razonValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.RazonDesactivacion));
    }

    [Fact]
    public async Task Validate_ConRazonConCaracteresEspeciales_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = "Cliente solicita baja - Motivo: cambio de residencia (temporal)";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarClienteCommand.RazonDesactivacion));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompleto_DeberiaSerValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            RazonDesactivacion = "Cliente solicita baja voluntaria",
            ObservacionesDesactivacion = "El cliente se mudó a otra ciudad y ya no requiere nuestros servicios. Dejó todo al día.",
            UsuarioId = Guid.NewGuid()
        };

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandMinimo_DeberiaSerValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            RazonDesactivacion = "Baja", // Mínimo
            UsuarioId = Guid.NewGuid()
            // ObservacionesDesactivacion opcional
        };

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new DesactivarClienteCommand
        {
            ClienteId = Guid.Empty, // Error
            RazonDesactivacion = "", // Error
            ObservacionesDesactivacion = new string('X', 1001), // Error
            UsuarioId = Guid.Empty // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Cliente solicita baja", "Cambio de residencia")]
    [InlineData("Incumplimiento de políticas", "Múltiples incidencias reportadas")]
    [InlineData("Problemas de pago", "Facturas vencidas recurrentes")]
    [InlineData("Fusión empresarial", "La empresa fue adquirida por otro grupo")]
    public async Task Validate_ConDiferentesEscenariosDesactivacion_DeberiaSerValido(string razon, string observaciones)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.ClienteId = clienteId;
        command.RazonDesactivacion = razon;
        command.ObservacionesDesactivacion = observaciones;

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConDesactivacionAdministrativa_DeberiaSerValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.ClienteId = clienteId;
        command.RazonDesactivacion = "Desactivación administrativa";
        command.ObservacionesDesactivacion = "Cliente no cumple con nuevos requisitos regulatorios implementados";

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Corporativo", Email = "corp@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites

    [Fact]
    public async Task Validate_ConRazonEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.ClienteId = clienteId;
        command.RazonDesactivacion = new string('R', 250); // Exactamente 250 caracteres

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.ClienteId = clienteId;
        command.ObservacionesDesactivacion = new string('O', 1000); // Exactamente 1000 caracteres

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var commands = Enumerable.Range(1, 5)
            .Select(i => 
            {
                var clienteId = Guid.NewGuid();
                var cmd = CrearCommandValido();
                cmd.ClienteId = clienteId;
                cmd.RazonDesactivacion = $"Razón {i}";
                return cmd;
            })
            .ToList();

        // Mock múltiples clientes activos
        var clientes = commands.Select(cmd => 
            new Cliente { Id = cmd.ClienteId, Nombre = $"Cliente {cmd.ClienteId}", Email = "test@email.com", Activo = true }
        ).AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        // Act
        var tasks = commands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.ClienteId = clienteId;

        // Mock cliente activo
        var clientes = new List<Cliente>
        {
            new Cliente { Id = clienteId, Nombre = "Cliente Test", Email = "test@email.com", Activo = true }
        }.AsQueryable();

        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(clientes.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(clientes.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(clientes.GetEnumerator());

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(command);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    #endregion

    #region Tests de Factory Methods (si existen)

    [Fact]
    public void Command_DeberiaCrearseConFactoryMethod()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var command = DesactivarClienteCommand.Crear(clienteId, "Baja voluntaria", usuarioId);

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.RazonDesactivacion.Should().Be("Baja voluntaria");
        command.UsuarioId.Should().Be(usuarioId);
        command.ObservacionesDesactivacion.Should().BeNull();
    }

    [Fact]
    public void Command_FactoryMethodConObservaciones_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var command = DesactivarClienteCommand.Crear(clienteId, "Baja por mudanza", usuarioId, "Cliente se mudó fuera del país");

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.RazonDesactivacion.Should().Be("Baja por mudanza");
        command.UsuarioId.Should().Be(usuarioId);
        command.ObservacionesDesactivacion.Should().Be("Cliente se mudó fuera del país");
    }

    #endregion
} 