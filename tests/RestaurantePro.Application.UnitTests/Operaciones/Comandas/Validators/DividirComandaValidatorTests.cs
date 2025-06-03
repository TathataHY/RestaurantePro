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
        
        // Configurar mocks básicos por defecto para evitar NullReferenceException
        ConfigurarMocksBasicos();
    }

    [Fact]
    public async Task Validator_ConComandoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMocksParaValidacion(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert - Debug para ver errores específicos
        if (!result.IsValid)
        {
            var errores = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
            throw new Exception($"Validación falló con errores: {errores}");
        }
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConComandaOriginalIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandaOriginalId = Guid.Empty;
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        ConfigurarMocksBasicos(); // Configurar mocks básicos

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
        ConfigurarMocksBasicos(); // Configurar mocks básicos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de división no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksParaValidacion(DividirComandaCommand command)
    {
        // Crear comanda existente para validación
        var comanda = CrearComandaValidaParaDivision();
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, command.ComandaOriginalId);
        
        var comandas = new List<Comanda> { comanda };
        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());

        // Crear items de comanda con IDs específicos que coincidan con los del command
        var itemsComanda = new List<ItemComanda>();
        
        // Obtener todos los ItemIds únicos de las divisiones
        var itemIds = command.DivisionItems
            .SelectMany(d => d.Items)
            .Select(i => i.ItemId)
            .Distinct()
            .ToList();

        // Crear items con esos IDs específicos
        foreach (var itemId in itemIds)
        {
            var item = CrearItemComanda(itemId, 5); // Crear con cantidad suficiente para las divisiones
            // Asignar el item a la comanda original
            typeof(ItemComanda).GetProperty("ComandaId")?.SetValue(item, command.ComandaOriginalId);
            itemsComanda.Add(item);
        }
        
        var itemsComandaMock = MockDbSetHelper.CreateMockDbSet(itemsComanda.AsQueryable());

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.ItemsComanda).Returns(itemsComandaMock.Object);
    }

    private void ConfigurarMockComandaNoExiste()
    {
        // Usar lista vacía para simular que no existe la comanda
        var comandasMock = MockDbSetHelper.CreateEmptyMockDbSet<Comanda>();
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandaNoDivisible(DividirComandaCommand command)
    {
        // Crear comanda no divisible (finalizada)
        var comanda = CrearComandaValidaParaDivision();
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, command.ComandaOriginalId);
        // Marcar como finalizada usando método del dominio 
        comanda.ActualizarEstado(EstadoComanda.Finalizada);

        var comandas = new List<Comanda> { comanda };
        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandaSinItems(DividirComandaCommand command)
    {
        // Crear comanda existente pero sin items
        var comanda = CrearComandaValidaParaDivision();
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, command.ComandaOriginalId);
        
        var comandas = new List<Comanda> { comanda };
        var comandasMock = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());
        
        // Items vacíos para simular que no tiene items
        var itemsComandaMock = MockDbSetHelper.CreateEmptyMockDbSet<ItemComanda>();

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.ItemsComanda).Returns(itemsComandaMock.Object);
    }

    private DividirComandaCommand CrearComandoValido()
    {
        // Usar IDs predefinidos para mantener consistencia
        var item1Id = new Guid("11111111-1111-1111-1111-111111111111");
        var item2Id = new Guid("22222222-2222-2222-2222-222222222222");
        
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
                        new ItemDivisionDto { ItemId = item1Id, Cantidad = 2 } 
                    }
                },
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 2,
                    Items = new List<ItemDivisionDto> 
                    { 
                        new ItemDivisionDto { ItemId = item2Id, Cantidad = 1 } 
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

    #region Helper Methods

    private void ConfigurarMocksBasicos()
    {
        // Configurar DbSets vacíos para evitar NullReferenceException
        var comandasMock = MockDbSetHelper.CreateEmptyMockDbSet<Comanda>();
        var itemsComandaMock = MockDbSetHelper.CreateEmptyMockDbSet<ItemComanda>();
        var mesasMock = MockDbSetHelper.CreateEmptyMockDbSet<Mesa>();
        var usuariosMock = MockDbSetHelper.CreateEmptyMockDbSet<Usuario>();

        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
        _contextMock.Setup(x => x.ItemsComanda).Returns(itemsComandaMock.Object);
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    #endregion
} 