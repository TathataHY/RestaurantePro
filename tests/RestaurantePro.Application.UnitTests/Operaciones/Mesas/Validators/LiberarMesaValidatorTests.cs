namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA LIBERAR MESA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de liberación de mesas
/// Cobertura: 100% de reglas de negocio del LiberarMesaValidator
/// </summary>
public class LiberarMesaValidatorTests
{
    private readonly LiberarMesaValidator _validator;

    public LiberarMesaValidatorTests()
    {
        _validator = new LiberarMesaValidator();
    }

    #region Validation Command Helper

    private LiberarMesaCommand CrearCommandValido()
    {
        return new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa liberada correctamente",
            MeseroId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación MesaId

    [Fact]
    public async Task Validate_ConMesaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMesaIdValido_NoDeberiaRetornarErrorDeMesaId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMesaIdEspecifico_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.MesaId));
    }

    #endregion

    #region Validación Observaciones

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Theory]
    [InlineData("Mesa liberada correctamente")]
    [InlineData("Cliente satisfecho, cuenta pagada")]
    [InlineData("Servicio completado sin inconvenientes")]
    [InlineData("A")] // 1 carácter - válido
    public async Task Validate_ConObservacionesValidas_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesValidas;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVaciasONull_NoDeberiaValidarObservaciones(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimite_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', 500); // Exactamente 500 caracteres - límite válido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    #endregion

    #region Validación MeseroId

    [Fact]
    public async Task Validate_ConMeseroIdVacioSiSeEspecifica_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = Guid.Empty; // GUID vacío si se especifica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConMeseroIdValido_NoDeberiaRetornarErrorDeMeseroId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConMeseroIdNull_NoDeberiaValidarMeseroId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = null; // Opcional

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.MeseroId));
    }

    [Fact]
    public async Task Validate_ConMeseroIdEspecifico_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = new Guid("ABCDEFAB-1234-5678-9012-123456789012");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.MeseroId));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa liberada correctamente, cliente satisfecho con el servicio",
            MeseroId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandMinimoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid()
            // Sin observaciones ni meseroId - ambos opcionales
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloMesaIdYObservaciones_DeberiaSerValido()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa liberada sin asignar mesero específico"
            // Sin MeseroId - opcional
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloMesaIdYMeseroId_DeberiaSerValido()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid()
            // Sin observaciones - opcional
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
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.Empty, // Error: vacío
            Observaciones = new string('A', 501), // Error: muy largo
            MeseroId = Guid.Empty // Error: vacío si se especifica
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LiberarMesaCommand.MesaId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LiberarMesaCommand.MeseroId));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - máximo válido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesObservaciones_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Mesa liberada tras finalizar comida")]
    [InlineData("Cliente pagó cuenta y se retiró satisfecho")]
    [InlineData("Servicio completado exitosamente")]
    [InlineData("Mesa disponible para nuevos clientes")]
    [InlineData("Limpieza realizada, mesa lista")]
    public async Task Validate_ConObservacionesRealistasDeRestaurante_DeberiaSerValido(string observacion)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacion;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
    }

    [Fact]
    public async Task Validate_ConLiberacionMesaSinMesero_DeberiaSerValido()
    {
        // Arrange - Mesa liberada por sistema o gerente
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa liberada automáticamente por sistema",
            MeseroId = null // Sin mesero específico
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConLiberacionMesaConMeseroEspecifico_DeberiaSerValido()
    {
        // Arrange - Mesa liberada por mesero específico
        var command = new LiberarMesaCommand
        {
            MesaId = new Guid("MESA0001-1234-5678-9012-123456789012"),
            Observaciones = "Mesa 5 liberada tras servicio excelente",
            MeseroId = new Guid("MESERO01-1234-5678-9012-123456789012")
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConLiberacionRapidaSinObservaciones_DeberiaSerValido()
    {
        // Arrange - Liberación rápida sin detalles
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = null, // Sin observaciones específicas
            MeseroId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Mesa VIP liberada correctamente")]
    [InlineData("Mesa terraza disponible nuevamente")]
    [InlineData("Mesa familiar limpiada y organizada")]
    [InlineData("Mesa bar liberada tras último cliente")]
    public async Task Validate_ConDiferentesTiposDeMesas_DeberiaSerValido(string observacionTipoMesa)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionTipoMesa;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
    }

    #endregion

    #region Tests de Campos Opcionales

    [Fact]
    public async Task Validate_ConTodosCamposOpcionales_SoloValidaLosProporciona()
    {
        // Arrange - Solo MesaId obligatorio
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid()
            // Observaciones y MeseroId omitidos - opcionales
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        
        // Confirmar que campos opcionales no se validaron
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.MeseroId));
    }

    [Fact]
    public async Task Validate_ConCamposOpcionalesNull_NoDeberiaValidarlos()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = null, // Null - no se valida
            MeseroId = null // Null - no se valida
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.Observaciones));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(LiberarMesaCommand.MeseroId));
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Validate_ConMuchosCommandsEnParalelo_DeberiaMantenerse()
    {
        // Arrange
        var commands = Enumerable.Range(1, 100)
            .Select(_ => new LiberarMesaCommand 
            { 
                MesaId = Guid.NewGuid(),
                Observaciones = "Mesa liberada correctamente",
                MeseroId = Guid.NewGuid()
            })
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

    #endregion

    #region Tests de Mensajes de Error

    [Fact]
    public async Task Validate_ConErrores_DeberiaRetornarMensajesEspecificos()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.Empty,
            Observaciones = new string('A', 501),
            MeseroId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        
        // Verificar mensajes específicos
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
            
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
            
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(LiberarMesaCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero no puede ser un GUID vacío"));
    }

    #endregion
} 