namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// Tests unitarios para DividirComandaValidator
/// Validación completa de reglas de negocio para división de comandas
/// </summary>
public class DividirComandaValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DividirComandaValidator _validator;

    public DividirComandaValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new DividirComandaValidator(_contextMock.Object);
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
    public async Task Validator_ConComandaOriginalIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandaOriginalId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID de la comanda original no puede ser un GUID vacío.");
    }

    [Fact]
    public async Task Validator_ConTipoDivisionInvalido_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoDivision = (TipoDivisionComanda)999; // Valor inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El tipo de división especificado no es válido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoDivision = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_SinDivisionItems_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.DivisionItems = new List<DivisionComandaDto>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos una división de items.");
    }

    [Fact]
    public async Task Validator_ConDemasiadasDivisiones_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.DivisionItems = new List<DivisionComandaDto>();
        
        // Agregar más de 10 divisiones
        for (int i = 0; i < 11; i++)
        {
            command.DivisionItems.Add(new DivisionComandaDto 
            { 
                NumeroComandaNueva = i + 1,
                Items = new List<ItemDivisionDto> 
                { 
                    new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 } 
                }
            });
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden crear más de 10 comandas nuevas.");
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
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda original especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConComandaNoDivisible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaNoDivisible(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda no puede ser dividida en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConComandaSinItems_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandaSinItems(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La comanda debe tener items para poder ser dividida.");
    }

    [Fact]
    public async Task Validator_ConNumerosComandaNoUnicos_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.DivisionItems = new List<DivisionComandaDto>
        {
            new DivisionComandaDto 
            { 
                NumeroComandaNueva = 1,
                Items = new List<ItemDivisionDto> { new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 } }
            },
            new DivisionComandaDto 
            { 
                NumeroComandaNueva = 1, // Número duplicado
                Items = new List<ItemDivisionDto> { new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 } }
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Los números de comanda nueva deben ser únicos.");
    }

    [Fact]
    public async Task Validator_ConNotasDivisionMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasDivision = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de división no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksParaValidacion(DividirComandaCommand command)
    {
        // Mock para comandas
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var comanda = CrearComandaValidaParaDivision();
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, command.ComandaOriginalId);
        
        comandasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Mock para items de comanda
        var itemsComandaMock = new Mock<DbSet<ItemComanda>>();
        itemsComandaMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<ItemComanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var itemsComanda = new List<ItemComanda>
        {
            CrearItemComanda(Guid.NewGuid(), 2),
            CrearItemComanda(Guid.NewGuid(), 1)
        };
        itemsComandaMock.Setup(x => x.Where(It.IsAny<Expression<Func<ItemComanda, bool>>>()).ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsComanda);

        // TODO: Descomentar cuando FacturaItems esté disponible en el contexto
        // Mock para factura items (no facturada)
        // var facturaItemsMock = new Mock<DbSet<FacturaItem>>();
        // facturaItemsMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<FacturaItem, bool>>>(), It.IsAny<CancellationToken>()))
        //     .ReturnsAsync(false);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.ItemsComanda).Returns(itemsComandaMock.Object);
        // _contextMock.Setup(x => x.FacturaItems).Returns(facturaItemsMock.Object);
    }

    private void ConfigurarMockComandaNoExiste()
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandaNoDivisible(DividirComandaCommand command)
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var comanda = CrearComandaValidaParaDivision();
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, command.ComandaOriginalId);
        // Marcar como finalizada usando método del dominio 
        comanda.ActualizarEstado(EstadoComanda.Finalizada);

        comandasMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandaSinItems(DividirComandaCommand command)
    {
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Comanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var itemsComandaMock = new Mock<DbSet<ItemComanda>>();
        itemsComandaMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<ItemComanda, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // No tiene items

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.ItemsComanda).Returns(itemsComandaMock.Object);
    }

    private DividirComandaCommand CrearComandoValido()
    {
        return new DividirComandaCommand
        {
            ComandaOriginalId = Guid.NewGuid(),
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Cliente solicita cuentas separadas",
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 1,
                    Items = new List<ItemDivisionDto> 
                    { 
                        new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 } 
                    }
                },
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 2,
                    Items = new List<ItemDivisionDto> 
                    { 
                        new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 } 
                    }
                }
            },
            MantenerComandaOriginal = false,
            DistribuirDescuentos = true
        };
    }

    private Comanda CrearComandaValidaParaDivision()
    {
        // Usar factory method en lugar de inicializador de objetos
        var comanda = Comanda.Crear(
            mesaId: Guid.NewGuid(),
            meseroId: Guid.NewGuid(), 
            clienteId: null,
            observaciones: "Comanda para división");

        // Usar reflection para setear el ID específico requerido para tests
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, Guid.NewGuid());

        // Agregar items usando métodos del dominio
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test 1", 2, 25.00m);
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test 2", 3, 15.00m);

        return comanda;
    }

    private ItemComanda CrearItemComanda(Guid id, int cantidad)
    {
        var item = new ItemComanda(
            comandaId: Guid.NewGuid(),
            productoId: Guid.NewGuid(),
            cantidad: cantidad,
            precioUnitario: 25.00m,
            observaciones: "Item de prueba");

        // Usar reflection para setear el ID específico requerido para tests
        typeof(EntityBase).GetProperty("Id")?.SetValue(item, id);

        return item;
    }
} 