namespace RestaurantePro.Application.UnitTests.Core.Productos.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ELIMINAR PRODUCTO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de eliminación de productos
/// Cobertura: 100% de reglas de negocio del EliminarProductoValidator
/// </summary>
public class EliminarProductoValidatorTests
{
    private readonly EliminarProductoValidator _validator;

    public EliminarProductoValidatorTests()
    {
        _validator = new EliminarProductoValidator();
    }

    #region Validation Command Helper

    private EliminarProductoCommand CrearCommandValido()
    {
        return new EliminarProductoCommand
        {
            Id = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación Id - Campo Requerido

    [Fact]
    public async Task Validate_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // Dos reglas de validación para el ID
        
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto es obligatorio"));
            
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto no puede ser vacío"));
    }

    [Fact]
    public async Task Validate_ConIdValidoGenerado_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConIdValidoEspecifico_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Validación Completa de Command

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandCompleto_NoDeberiaRetornarErroresDeId()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto es obligatorio"));
            
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto no puede ser vacío"));
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task Validate_ConIdMinimo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = new Guid("00000000-0000-0000-0000-000000000001"); // GUID mínimo válido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConIdMaximo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"); // GUID máximo válido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesInstancias_DeberiaMantenerse()
    {
        // Arrange
        var command1 = new EliminarProductoCommand { Id = Guid.NewGuid() };
        var command2 = new EliminarProductoCommand { Id = Guid.NewGuid() };
        var command3 = new EliminarProductoCommand { Id = Guid.Empty }; // Inválido

        // Act
        var result1 = await _validator.ValidateAsync(command1);
        var result2 = await _validator.ValidateAsync(command2);
        var result3 = await _validator.ValidateAsync(command3);

        // Assert
        result1.IsValid.Should().BeTrue();
        result2.IsValid.Should().BeTrue();
        result3.IsValid.Should().BeFalse();
        
        result1.Errors.Should().BeEmpty();
        result2.Errors.Should().BeEmpty();
        result3.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Tests de Mensajes de Error

    [Fact]
    public async Task Validate_ConIdVacio_DeberiaRetornarMensajesCorrectosDeError()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        
        var errorObligatorio = result.Errors.FirstOrDefault(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto es obligatorio"));
        errorObligatorio.Should().NotBeNull();
        
        var errorVacio = result.Errors.FirstOrDefault(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del producto no puede ser vacío"));
        errorVacio.Should().NotBeNull();
    }

    [Fact]
    public async Task Validate_ConIdValido_NoDeberiaRetornarMensajesDeError()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarProductoCommand.Id));
    }

    #endregion

    #region Tests de Diferentes Formatos de GUID

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("abcdefab-1234-5678-9012-123456789012")]
    [InlineData("ABCDEFAB-1234-5678-9012-123456789012")]
    [InlineData("11111111-2222-3333-4444-555555555555")]
    [InlineData("99999999-8888-7777-6666-555555555555")]
    public async Task Validate_ConDiferentesFormatosGuidValidos_DeberiaSerValido(string guidString)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = new Guid(guidString);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Tests de Rendimiento y Concurrencia

    [Fact]
    public async Task Validate_ConMuchosCommandsEnParalelo_DeberiaMantenerse()
    {
        // Arrange
        var commands = Enumerable.Range(1, 100)
            .Select(_ => new EliminarProductoCommand { Id = Guid.NewGuid() })
            .ToList();

        // Act
        var tasks = commands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => 
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Validate_ConCommandsValidosEInvalidosEnParalelo_DeberiaDistinguirCorrectamente()
    {
        // Arrange
        var commandsValidos = Enumerable.Range(1, 50)
            .Select(_ => new EliminarProductoCommand { Id = Guid.NewGuid() })
            .ToList();
            
        var commandsInvalidos = Enumerable.Range(1, 50)
            .Select(_ => new EliminarProductoCommand { Id = Guid.Empty })
            .ToList();

        var todosLosCommands = commandsValidos.Concat(commandsInvalidos).ToList();

        // Act
        var tasks = todosLosCommands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        var resultadosValidos = results.Take(50).ToList();
        var resultadosInvalidos = results.Skip(50).Take(50).ToList();

        resultadosValidos.Should().AllSatisfy(result => 
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        });

        resultadosInvalidos.Should().AllSatisfy(result => 
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
        });
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConIdProductoParaEliminacion_DeberiaSerValido()
    {
        // Arrange - Simular un ID de producto real que se va a eliminar
        var command = new EliminarProductoCommand
        {
            Id = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890") // ID típico de producto
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConIdProductoDescontinuado_DeberiaSerValido()
    {
        // Arrange - Simular eliminación de producto descontinuado
        var command = new EliminarProductoCommand
        {
            Id = new Guid("DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF") // ID típico para productos descontinuados
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConIdProductoTemporalParaEliminar_DeberiaSerValido()
    {
        // Arrange - Simular eliminación de producto temporal/promocional
        var command = new EliminarProductoCommand
        {
            Id = new Guid("11110000-1234-5678-9012-123456789012") // ID temporal - GUID válido
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Tests de Validación de Reglas

    [Fact]
    public async Task Validate_DeberiaTenerDosReglasDeValidacionParaId()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.Empty // Violará ambas reglas
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // NotEmpty y NotEqual
        
        // Verificar que ambas reglas están activas
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("obligatorio"));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("vacío"));
    }

    [Fact]
    public async Task Validate_ConIdValido_NoDeberiaActivarNingunaReglaDeError()
    {
        // Arrange
        var command = new EliminarProductoCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        
        // Confirmar que ninguna regla de validación falló
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(EliminarProductoCommand.Id));
    }

    #endregion
} 