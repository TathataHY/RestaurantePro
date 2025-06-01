namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// Tests unitarios para UnificarComandasValidator
/// Validación completa de reglas de negocio para unificación de comandas
/// </summary>
public class UnificarComandasValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UnificarComandasValidator _validator;

    public UnificarComandasValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new UnificarComandasValidator(_contextMock.Object);
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
    public async Task Validator_SinComandasIds_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos una comanda para unificar.");
    }

    [Fact]
    public async Task Validator_ConSoloUnaComanda_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos 2 comandas para unificar.");
    }

    [Fact]
    public async Task Validator_ConDemasiadasComandas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();
        
        // Agregar más de 10 comandas
        for (int i = 0; i < 11; i++)
        {
            command.ComandasIds.Add(Guid.NewGuid());
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar más de 10 comandas a la vez.");
    }

    [Fact]
    public async Task Validator_ConIdsComandaDuplicados_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var comandaId = Guid.NewGuid();
        command.ComandasIds = new List<Guid> { comandaId, comandaId }; // ID duplicado

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Los IDs de comandas deben ser únicos.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaDestinoId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID de la mesa de destino no puede ser un GUID vacío.");
    }

    [Fact]
    public async Task Validator_ConMeseroIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MeseroId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID del mesero no puede ser un GUID vacío.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoUnificacion = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_ConEstrategiaDescuentosInvalida_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.EstrategiaDescuentos = (EstrategiaDescuentos)999; // Valor inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La estrategia de descuentos especificada no es válida.");
    }

    [Fact]
    public async Task Validator_ConComandaNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoExisten();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas especificadas no existen.");
    }

    [Fact]
    public async Task Validator_ConComandasNoUnificables_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoUnificables(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas no pueden ser unificadas en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConComandasFacturadas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasFacturadas(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar comandas que ya han sido facturadas.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConMeseroNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMeseroNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El mesero especificado no existe.");
    }

    [Fact]
    public async Task Validator_ConNotasUnificacionMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasUnificacion = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de unificación no pueden exceder 500 caracteres.");
    }

    [Fact]
    public async Task Validator_ConObservacionesUnificadaMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ObservacionesUnificada = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las observaciones de la comanda unificada no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksParaValidacion(UnificarComandasCommand command)
    {
        // Mock para comandas
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.Where(It.IsAny<Expression<Func<Comanda, bool>>>()).CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(command.ComandasIds.Count);

        var comandas = command.ComandasIds.Select(id => new Comanda 
        { 
            Id = id, 
            Estado = EstadoComanda.Creada 
        }).ToList();
        
        comandasMock.Setup(x => x.Where(It.IsAny<Expression<Func<Comanda, bool>>>()).ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        // Mock para mesas
        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mesa = new Mesa { Id = command.MesaDestinoId, Estado = EstadoMesa.Disponible };
        mesasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        // Mock para usuarios (meseros)
        var usuariosMock = new Mock<DbSet<Usuario>>();
        usuariosMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var usuario = new Usuario { Id = command.MeseroId, Activo = true };
        usuariosMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Mock para factura items (no facturadas)
        var facturaItemsMock = new Mock<DbSet<FacturaItem>>();
        facturaItemsMock.Setup(x => x.Where(It.IsAny<Expression<Func<FacturaItem, bool>>>()).AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
        _contextMock.Setup(x => x.FacturaItems).Returns(facturaItemsMock.Object);
    }

    private void ConfigurarMockComandasNoExisten()
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.Where(It.IsAny<Expression<Func<Comanda, bool>>>()).CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // No encuentra comandas

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandasNoUnificables(UnificarComandasCommand command)
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.Where(It.IsAny<Expression<Func<Comanda, bool>>>()).CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(command.ComandasIds.Count);

        var comandas = command.ComandasIds.Select(id => new Comanda 
        { 
            Id = id, 
            Estado = EstadoComanda.Finalizada // Estado no unificable
        }).ToList();
        
        comandasMock.Setup(x => x.Where(It.IsAny<Expression<Func<Comanda, bool>>>()).ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandasFacturadas(UnificarComandasCommand command)
    {
        var facturaItemsMock = new Mock<DbSet<FacturaItem>>();
        facturaItemsMock.Setup(x => x.Where(It.IsAny<Expression<Func<FacturaItem, bool>>>()).AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Hay comandas facturadas

        _contextMock.Setup(x => x.FacturaItems).Returns(facturaItemsMock.Object);
    }

    private void ConfigurarMockMesaNoExiste()
    {
        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockMeseroNoExiste()
    {
        var usuariosMock = new Mock<DbSet<Usuario>>();
        usuariosMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    private UnificarComandasCommand CrearComandoValido()
    {
        return new UnificarComandasCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Cliente solicita unificar cuentas",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            MantenerHistorico = true,
            NotasUnificacion = "Unificación por solicitud del cliente",
            ObservacionesUnificada = "Mesa familiar"
        };
    }
} 