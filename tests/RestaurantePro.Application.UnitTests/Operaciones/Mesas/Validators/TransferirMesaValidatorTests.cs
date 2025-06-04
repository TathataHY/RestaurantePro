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
        
        // Configurar mocks por defecto para evitar NullReferenceException
        ConfigurarMocksDefecto();
        
        _validator = new TransferirMesaValidator(_contextMock.Object);
    }

    private void ConfigurarMocksDefecto()
    {
        // Configurar DbSets vacíos por defecto
        var comandasVacias = new List<Comanda>().AsQueryable().BuildMockDbSet();
        var mesasVacias = new List<Mesa>().AsQueryable().BuildMockDbSet();
        var usuariosVacios = new List<Usuario>().AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Comandas).Returns(comandasVacias.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasVacias.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosVacios.Object);
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
        // Crear entidades de prueba
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        // Usar reflection para establecer el ID y estado si es necesario
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        comanda.ActualizarEstado(EstadoComanda.Creada);

        var mesaOrigen = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaOrigen, command.MesaOrigenId);
        mesaOrigen.MarcarComoOcupada();

        var mesaDestino = Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaDestino, command.MesaDestinoId);
        mesaDestino.MarcarComoOcupada();

        // Configurar mocks usando MockQueryable
        var comandasList = new List<Comanda> { comanda };
        var mesasList = new List<Mesa> { mesaOrigen, mesaDestino };

        var comandasMock = comandasList.AsQueryable().BuildMockDbSet();
        var mesasMock = mesasList.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockComandaNoExiste()
    {
        // Configurar DbSet vacío
        var comandasMock = new List<Comanda>().AsQueryable().BuildMockDbSet();
        var mesasList = new List<Mesa>
        {
            Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior"),
            Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior")
        };
        var mesasMock = mesasList.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockComandaNoTransferible(TransferirMesaCommand command)
    {
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        comanda.ActualizarEstado(EstadoComanda.Cancelada); // Estado no transferible

        var mesaOrigen = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaOrigen, command.MesaOrigenId);
        mesaOrigen.MarcarComoOcupada();

        var mesaDestino = Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaDestino, command.MesaDestinoId);
        mesaDestino.MarcarComoOcupada();

        var comandasList = new List<Comanda> { comanda };
        var mesasList = new List<Mesa> { mesaOrigen, mesaDestino };

        var comandasMock = comandasList.AsQueryable().BuildMockDbSet();
        var mesasMock = mesasList.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockMesaDestinoNoDisponible(TransferirMesaCommand command)
    {
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaOrigenId,
            observaciones: "Test comanda",
            numeroComanda: "TEST-001");
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, command.ComandaId);
        comanda.ActualizarEstado(EstadoComanda.Creada);

        // Crear comanda activa en mesa destino para hacerla no disponible
        var comandaEnDestino = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: command.MesaDestinoId,
            observaciones: "Comanda existente",
            numeroComanda: "TEST-002");
        comandaEnDestino.ActualizarEstado(EstadoComanda.EnProceso);

        var mesaOrigen = Mesa.Crear(numero: 1, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaOrigen, command.MesaOrigenId);
        mesaOrigen.MarcarComoOcupada();

        var mesaDestino = Mesa.Crear(numero: 2, capacidad: 4, ubicacion: "Interior");
        typeof(Mesa).GetProperty("Id")?.SetValue(mesaDestino, command.MesaDestinoId);
        mesaDestino.MarcarComoOcupada();

        var comandasList = new List<Comanda> { comanda, comandaEnDestino };
        var mesasList = new List<Mesa> { mesaOrigen, mesaDestino };

        var comandasMock = comandasList.AsQueryable().BuildMockDbSet();
        var mesasMock = mesasList.AsQueryable().BuildMockDbSet();

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