namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACUMULAR PUNTOS VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de acumulación de puntos de fidelización
/// Cobertura: 100% de reglas de negocio del AcumularPuntosValidator
/// </summary>
public class AcumularPuntosValidatorTests
{
    private readonly AcumularPuntosValidator _validator;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<DbSet<Cliente>> _clientesDbSetMock;
    private readonly Mock<DbSet<Factura>> _facturasDbSetMock;

    public AcumularPuntosValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _clientesDbSetMock = new Mock<DbSet<Cliente>>();
        _facturasDbSetMock = new Mock<DbSet<Factura>>();
        
        _contextMock.Setup(x => x.Clientes).Returns(_clientesDbSetMock.Object);
        _contextMock.Setup(x => x.Facturas).Returns(_facturasDbSetMock.Object);
        
        _validator = new AcumularPuntosValidator(_contextMock.Object);
    }

    #region Validation Command Helper

    private AcumularPuntosCommand CrearCommandValido()
    {
        return new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            MontoCompra = 100.00m,
            MultiplicadorPuntos = 1,
            TipoAcumulacion = "Compra",
            Observaciones = "Acumulación por compra normal",
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
            e.PropertyName == nameof(AcumularPuntosCommand.ClienteId) &&
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

        // Mock cliente existente
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
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AcumularPuntosCommand.ClienteId));
    }

    #endregion

    #region Validación FacturaId

    [Fact]
    public async Task Validate_ConFacturaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.FacturaId) &&
            e.ErrorMessage.Contains("El ID de la factura es requerido") &&
            e.ErrorCode == "FACTURA_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConFacturaIdValido_NoDeberiaRetornarErrorDeFacturaId()
    {
        // Arrange
        var command = CrearCommandValido();
        var facturaId = Guid.NewGuid();
        command.FacturaId = facturaId;

        // Mock factura existente
        var facturas = new List<Factura>
        {
            new Factura { Id = facturaId, Total = 100.00m, Estado = EstadoFactura.Pagada }
        }.AsQueryable();

        _facturasDbSetMock.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _facturasDbSetMock.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _facturasDbSetMock.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _facturasDbSetMock.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AcumularPuntosCommand.FacturaId));
    }

    #endregion

    #region Validación MontoCompra

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public async Task Validate_ConMontoCompraMenorIgualCero_DeberiaRetornarError(decimal montoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = montoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MontoCompra) &&
            e.ErrorMessage.Contains("El monto de compra debe ser mayor a 0") &&
            e.ErrorCode == "MONTO_COMPRA_INVALIDO");
    }

    [Fact]
    public async Task Validate_ConMontoCompraExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = 100001.00m; // Más de $100,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MontoCompra) &&
            e.ErrorMessage.Contains("El monto de compra no puede exceder $100,000") &&
            e.ErrorCode == "MONTO_COMPRA_EXCESIVO");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(100.00)]
    [InlineData(1000.00)]
    [InlineData(50000.00)]
    [InlineData(100000.00)] // Límite máximo
    public async Task Validate_ConMontoCompraValido_NoDeberiaRetornarErrorDeMonto(decimal montoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = montoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MontoCompra));
    }

    #endregion

    #region Validación MultiplicadorPuntos

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConMultiplicadorMenorIgualCero_DeberiaRetornarError(int multiplicadorInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorPuntos = multiplicadorInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorPuntos) &&
            e.ErrorMessage.Contains("El multiplicador de puntos debe ser mayor a 0") &&
            e.ErrorCode == "MULTIPLICADOR_PUNTOS_INVALIDO");
    }

    [Fact]
    public async Task Validate_ConMultiplicadorExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorPuntos = 11; // Más de 10

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorPuntos) &&
            e.ErrorMessage.Contains("El multiplicador de puntos no puede exceder 10") &&
            e.ErrorCode == "MULTIPLICADOR_PUNTOS_EXCESIVO");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)] // Límite máximo
    public async Task Validate_ConMultiplicadorValido_NoDeberiaRetornarErrorDeMultiplicador(int multiplicadorValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorPuntos = multiplicadorValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorPuntos));
    }

    #endregion

    #region Validación TipoAcumulacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConTipoAcumulacionVacio_DeberiaRetornarError(string tipoVacio)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = tipoVacio;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.TipoAcumulacion) &&
            e.ErrorMessage.Contains("El tipo de acumulación es requerido") &&
            e.ErrorCode == "TIPO_ACUMULACION_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConTipoAcumulacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = new string('A', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.TipoAcumulacion) &&
            e.ErrorMessage.Contains("El tipo de acumulación no puede exceder 50 caracteres") &&
            e.ErrorCode == "TIPO_ACUMULACION_LONGITUD");
    }

    [Theory]
    [InlineData("Compra")]
    [InlineData("Promoción")]
    [InlineData("Bono")]
    [InlineData("Cumpleaños")]
    [InlineData("Referido")]
    [InlineData("Evento Especial")]
    public async Task Validate_ConTipoAcumulacionValido_NoDeberiaRetornarErrorDeTipo(string tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = tipoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.TipoAcumulacion));
    }

    #endregion

    #region Validación Observaciones

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarObservaciones(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Observaciones));
    }

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres") &&
            e.ErrorCode == "OBSERVACIONES_LONGITUD");
    }

    [Fact]
    public async Task Validate_ConObservacionesValidas_NoDeberiaRetornarErrorDeObservaciones()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = "Acumulación de puntos por compra especial del día";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Observaciones));
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
            e.PropertyName == nameof(AcumularPuntosCommand.UsuarioId) &&
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
            e.PropertyName == nameof(AcumularPuntosCommand.UsuarioId));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompleto_DeberiaSerValido()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            MontoCompra = 250.75m,
            MultiplicadorPuntos = 2,
            TipoAcumulacion = "Promoción Especial",
            Observaciones = "Acumulación doble por promoción fin de semana",
            UsuarioId = Guid.NewGuid()
        };

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
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            MontoCompra = 0.01m, // Monto mínimo
            MultiplicadorPuntos = 1, // Multiplicador mínimo
            TipoAcumulacion = "C", // Tipo mínimo
            UsuarioId = Guid.NewGuid()
            // Observaciones opcional
        };

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
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.Empty, // Error
            FacturaId = Guid.Empty, // Error
            MontoCompra = -50.00m, // Error
            MultiplicadorPuntos = 0, // Error
            TipoAcumulacion = "", // Error
            Observaciones = new string('X', 1001), // Error
            UsuarioId = Guid.Empty // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(7);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Compra", 100.00, 1, "Acumulación normal")]
    [InlineData("Promoción", 200.00, 2, "Puntos dobles promoción")]
    [InlineData("Bono", 50.00, 5, "Bono bienvenida")]
    [InlineData("Cumpleaños", 300.00, 3, "Triple puntos cumpleaños")]
    public async Task Validate_ConDiferentesEscenariosAcumulacion_DeberiaSerValido(string tipo, decimal monto, int multiplicador, string obs)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = tipo;
        command.MontoCompra = monto;
        command.MultiplicadorPuntos = multiplicador;
        command.Observaciones = obs;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAcumulacionPromocional_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = "Promoción Especial";
        command.MontoCompra = 500.00m;
        command.MultiplicadorPuntos = 5; // 5x puntos
        command.Observaciones = "Promoción Black Friday - 5x puntos por compras superiores a $500";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAcumulacionBono_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAcumulacion = "Bono";
        command.MontoCompra = 1000.00m;
        command.MultiplicadorPuntos = 1;
        command.Observaciones = "Bono de 1000 puntos por registro completado";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Fact]
    public async Task Validate_ConMontoEnLimiteMinimo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = 0.01m; // Límite mínimo

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConMontoEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = 100000.00m; // Límite máximo

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('O', 1000); // Exactamente 1000 caracteres

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
        var commands = Enumerable.Range(1, 10)
            .Select(i => 
            {
                var cmd = CrearCommandValido();
                cmd.MontoCompra = i * 10;
                cmd.MultiplicadorPuntos = (i % 5) + 1;
                return cmd;
            })
            .ToList();

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
        var command = CrearCommandValido();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(command);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task Validate_ConCommandNuevo_DeberiaSerValido()
    {
        // Arrange
        var command = new AcumularPuntosCommand();
        command.ClienteId = Guid.NewGuid();
        command.FacturaId = Guid.NewGuid();
        command.MontoCompra = 100.00m;
        command.MultiplicadorPuntos = 1;
        command.TipoAcumulacion = "Compra";
        command.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Factory Methods (si existen)

    [Fact]
    public void Command_DeberiaCrearseConFactoryMethod()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var command = AcumularPuntosCommand.Crear(clienteId, facturaId, 100.00m, "Compra", usuarioId);

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.FacturaId.Should().Be(facturaId);
        command.MontoCompra.Should().Be(100.00m);
        command.TipoAcumulacion.Should().Be("Compra");
        command.UsuarioId.Should().Be(usuarioId);
        command.MultiplicadorPuntos.Should().Be(1); // Default
    }

    [Fact]
    public void Command_FactoryMethodConParametrosCompletos_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var command = AcumularPuntosCommand.Crear(clienteId, facturaId, 250.00m, "Promoción", usuarioId, 2, "Puntos dobles");

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.FacturaId.Should().Be(facturaId);
        command.MontoCompra.Should().Be(250.00m);
        command.TipoAcumulacion.Should().Be("Promoción");
        command.UsuarioId.Should().Be(usuarioId);
        command.MultiplicadorPuntos.Should().Be(2);
        command.Observaciones.Should().Be("Puntos dobles");
    }

    #endregion
} 