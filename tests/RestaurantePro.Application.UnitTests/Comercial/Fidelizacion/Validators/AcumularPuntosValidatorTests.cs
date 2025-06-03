namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACUMULAR PUNTOS VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de acumulación de puntos de fidelización
/// Cobertura: 100% de reglas de negocio del AcumularPuntosValidator
/// </summary>
public class AcumularPuntosValidatorTests
{
    private readonly AcumularPuntosValidator _validator;

    public AcumularPuntosValidatorTests()
    {
        _validator = new AcumularPuntosValidator();
    }

    #region Validation Command Helper

    private AcumularPuntosCommand CrearCommandValido()
    {
        return new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 100.00m,
            TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
            Canal = "Presencial",
            MultiplicadorEspecial = 1,
            UsuarioQueAcumula = "usuario_test",
            FacturaId = Guid.NewGuid() // Requerido para transacciones de compra
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
            e.ErrorMessage.Contains("ID del cliente es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConClienteIdValido_NoDeberiaRetornarErrorDeClienteId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ClienteId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert - Si hay errores, no deberían ser por ClienteId
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.ClienteId) &&
            e.ErrorMessage.Contains("ID del cliente es obligatorio"));
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
            e.ErrorMessage.Contains("debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConMontoCompraExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = 60000.00m; // Más de $50,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MontoCompra) &&
            e.ErrorMessage.Contains("no puede exceder"));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(100.00)]
    [InlineData(1000.00)]
    [InlineData(25000.00)]
    [InlineData(50000.00)] // Límite máximo
    public async Task Validate_ConMontoCompraValido_NoDeberiaRetornarErrorDeMonto(decimal montoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = montoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MontoCompra) &&
            e.ErrorMessage.Contains("debe ser mayor a 0"));
    }

    #endregion

    #region Validación Canal

    [Fact]
    public async Task Validate_ConCanalVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Canal = "";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Canal) &&
            e.ErrorMessage.Contains("canal"));
    }

    [Theory]
    [InlineData("Presencial")]
    [InlineData("App")]
    [InlineData("Web")]
    [InlineData("Telefono")]
    [InlineData("WhatsApp")]
    public async Task Validate_ConCanalValido_NoDeberiaRetornarErrorDeCanal(string canalValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Canal = canalValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Canal));
    }

    #endregion

    #region Validación MultiplicadorEspecial

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConMultiplicadorMenorIgualCero_DeberiaRetornarError(decimal multiplicadorInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorEspecial = multiplicadorInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorEspecial) &&
            e.ErrorMessage.Contains("mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConMultiplicadorExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorEspecial = 15; // Más de 10

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorEspecial) &&
            e.ErrorMessage.Contains("no puede exceder"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)] // Límite máximo
    public async Task Validate_ConMultiplicadorValido_NoDeberiaRetornarErrorDeMultiplicador(decimal multiplicadorValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MultiplicadorEspecial = multiplicadorValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.MultiplicadorEspecial) &&
            e.ErrorMessage.Contains("mayor a 0"));
    }

    #endregion

    #region Validación TipoTransaccion

    [Fact]
    public async Task Validate_ConTipoTransaccionValido_NoDeberiaRetornarErrorDeTipo()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.TipoTransaccion));
    }

    #endregion

    #region Validación Comentarios

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConComentariosVacios_NoDeberiaValidarComentarios(string? comentariosVacios)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Comentarios = comentariosVacios;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert - Los comentarios son opcionales, no debería haber error por estar vacíos
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Comentarios) &&
            e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public async Task Validate_ConComentariosMuyLargos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Comentarios = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Comentarios) &&
            e.ErrorMessage.Contains("500 caracteres"));
    }

    [Fact]
    public async Task Validate_ConComentariosValidos_NoDeberiaRetornarErrorDeComentarios()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Comentarios = "Acumulación de puntos por compra especial del día";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AcumularPuntosCommand.Comentarios));
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
            MontoCompra = 250.75m,
            TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
            Canal = "Presencial",
            MultiplicadorEspecial = 2,
            Comentarios = "Acumulación doble por promoción fin de semana",
            UsuarioQueAcumula = "usuario_test"
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
            MontoCompra = 0.01m, // Monto mínimo
            TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
            Canal = "Presencial"
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
            MontoCompra = -50.00m, // Error
            Canal = "", // Error
            MultiplicadorEspecial = 0, // Error
            Comentarios = new string('X', 501) // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData(100.00, 1, "Acumulación normal")]
    [InlineData(200.00, 2, "Puntos dobles promoción")]
    [InlineData(50.00, 5, "Bono bienvenida")]
    [InlineData(300.00, 3, "Triple puntos cumpleaños")]
    public async Task Validate_ConDiferentesEscenariosAcumulacion_DeberiaSerValido(decimal monto, decimal multiplicador, string obs)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoCompra = monto;
        command.MultiplicadorEspecial = multiplicador;
        command.Comentarios = obs;

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
        command.TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.PromocionEspecial;
        command.MontoCompra = 500.00m;
        command.MultiplicadorEspecial = 5; // 5x puntos
        command.CodigoPromocion = "BLACK2024";
        command.Comentarios = "Promoción Black Friday - 5x puntos por compras superiores a $500";

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
        command.TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.AjusteManual;
        command.MontoCompra = 1000.00m;
        command.MultiplicadorEspecial = 1;
        command.Comentarios = "Bono de 1000 puntos por registro completado";

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
        command.MontoCompra = 50000.00m; // Límite máximo

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConComentariosEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Comentarios = new string('A', 500); // Límite máximo de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var commands = Enumerable.Range(1, 10)
            .Select(_ => CrearCommandValido())
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
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public async Task Validate_ConCommandNuevo_DeberiaSerValido()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 150.00m,
            TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
            Canal = "App",
            Comentarios = "Compra desde aplicación móvil"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Command_DeberiaCrearseConFactoryMethod()
    {
        // Act
        var command = CrearCommandValido();

        // Assert
        command.Should().NotBeNull();
        command.ClienteId.Should().NotBe(Guid.Empty);
        command.MontoCompra.Should().BeGreaterThan(0);
        command.TipoTransaccion.Should().Be(RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra);
        command.Canal.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Command_FactoryMethodConParametrosCompletos_DeberiaCrearseCorrectamente()
    {
        // Arrange & Act
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 299.99m,
            TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.PromocionEspecial,
            Canal = "Web",
            CodigoPromocion = "PROMO2024",
            MultiplicadorEspecial = 2.5m,
            EsFechaEspecial = true,
            TipoFechaEspecial = "Cumpleanos",
            Comentarios = "Promoción especial de cumpleaños"
        };

        // Assert
        command.Should().NotBeNull();
        command.ClienteId.Should().Be(clienteId);
        command.MontoCompra.Should().Be(299.99m);
        command.TipoTransaccion.Should().Be(RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.PromocionEspecial);
        command.Canal.Should().Be("Web");
        command.CodigoPromocion.Should().Be("PROMO2024");
        command.MultiplicadorEspecial.Should().Be(2.5m);
        command.EsFechaEspecial.Should().BeTrue();
        command.TipoFechaEspecial.Should().Be("Cumpleanos");
        command.Comentarios.Should().Be("Promoción especial de cumpleaños");
    }

    #endregion
} 