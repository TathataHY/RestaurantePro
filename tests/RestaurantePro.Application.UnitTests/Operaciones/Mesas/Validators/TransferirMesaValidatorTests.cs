namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// Tests unitarios para TransferirMesaValidator
/// Validación completa de reglas de negocio para transferencia de comandas entre mesas
/// </summary>
public class TransferirMesaValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly TransferirMesaValidator _validator;

    public TransferirMesaValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new TransferirMesaValidator(_contextMock.Object);
    }

    [Fact]
    public async Task Validator_ConComandoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMocksParaValidacion(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConComandaIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID de la comanda no puede ser un GUID vacío.");
    }

    [Fact]
    public async Task Validator_ConMesasIguales_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaDestinoId = command.MesaOrigenId;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino debe ser diferente a la mesa de origen.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoTransferencia = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_ConComandaNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConComandaNoTransferible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaNoTransferible(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda no puede ser transferida en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoNoDisponible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaDestinoNoDisponible(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino no está disponible.");
    }

    [Fact]
    public async Task Validator_ConNotasTransferenciaMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasTransferencia = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de transferencia no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksParaValidacion(TransferirMesaCommand command)
    {
        // Mock para comandas
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        // Usar reflection para establecer el ID y estado si es necesario
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, EstadoComanda.Creada);
        
        comandasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Mock para mesas
        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mesa = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        // Usar reflection para establecer el ID y estado si es necesario
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, command.MesaDestinoId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, EstadoMesa.Disponible);
        
        mesasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockComandaNoExiste()
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandaNoTransferible(TransferirMesaCommand command)
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-002");
        // Usar reflection para establecer el ID y estado finalizado
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, EstadoComanda.Finalizada);
        
        comandasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockMesaDestinoNoDisponible(TransferirMesaCommand command)
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Mesa tiene comanda activa

        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mesa = Mesa.Crear(numero: 2, capacidad: 6, ubicacion: "Terraza");
        // Usar reflection para establecer el ID
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, command.MesaDestinoId);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, EstadoMesa.Disponible);
        
        mesasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private TransferirMesaCommand CrearComandoValido()
    {
        return new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Cliente solicita cambio de mesa",
            NotificarMesero = true,
            MantenerEstado = true
        };
    }
} 