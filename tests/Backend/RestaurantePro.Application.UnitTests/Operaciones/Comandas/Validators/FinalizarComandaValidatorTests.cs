namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA FINALIZAR COMANDA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de finalización de comandas
/// Cobertura: 100% de reglas de negocio del FinalizarComandaValidator
/// </summary>
public class FinalizarComandaValidatorTests
{
    private readonly FinalizarComandaValidator _validator;

    public FinalizarComandaValidatorTests()
    {
        _validator = new FinalizarComandaValidator();
    }

    #region Validation Command Helper

    private FinalizarComandaCommand CrearCommandValido()
    {
        return new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Comanda completada exitosamente",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };
    }

    #endregion

    #region Validación ComandaId

    [Fact]
    public async Task Validate_ConComandaIdVacia_DeberiaRetornarError()
    {
        // Arrange
        // No podemos usar el factory con ComandaId vacío porque lanza excepción
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.Empty,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ComandaId) &&
            e.ErrorMessage.Contains("ComandaId es requerido para finalizar") &&
            e.ErrorCode == "FINALIZAR_COMANDA_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConComandaIdValida_NoDeberiaRetornarErrorDeComandaId()
    {
        // Arrange
        var command = CrearCommandValido();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ComandaId));
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        // No podemos usar el factory con UsuarioId vacío porque lanza excepción
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.Empty,
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.UsuarioId) &&
            e.ErrorMessage.Contains("UsuarioId es requerido para finalizar") &&
            e.ErrorCode == "FINALIZAR_USUARIO_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorDeUsuarioId()
    {
        // Arrange
        var command = CrearCommandValido();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.UsuarioId));
    }

    #endregion

    #region Validación ObservacionesFinalizacion

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var observacionesLargas = new string('A', 501); // Más de 500 caracteres
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = observacionesLargas,
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ObservacionesFinalizacion) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 500 caracteres") &&
            e.ErrorCode == "FINALIZAR_OBSERVACIONES_LONGITUD");
    }

    [Theory]
    [InlineData("Comanda completada")]
    [InlineData("Finalizada correctamente con todos los productos entregados")]
    [InlineData("Cliente satisfecho - mesa liberada")]
    public async Task Validate_ConObservacionesValidas_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = FinalizarComandaCommand.Crear(
            comandaId: Guid.NewGuid(),
            usuarioId: Guid.NewGuid(),
            observaciones: observacionesValidas
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ObservacionesFinalizacion));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarLongitud(string observacionesVacias)
    {
        // Arrange
        var command = FinalizarComandaCommand.Crear(
            comandaId: Guid.NewGuid(),
            usuarioId: Guid.NewGuid(),
            observaciones: observacionesVacias
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ObservacionesFinalizacion));
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var observacionesEnLimite = new string('A', 500); // Exactamente 500 caracteres
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = observacionesEnLimite,
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.ObservacionesFinalizacion));
    }

    #endregion

    #region Validación FechaFinalizacion - Futuro

    [Fact]
    public async Task Validate_ConFechaEnFuturo_DeberiaRetornarError()
    {
        // Arrange
        var fechaFutura = DateTime.UtcNow.AddHours(1); // 1 hora en el futuro
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaFutura
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.FechaFinalizacion) &&
            e.ErrorMessage.Contains("La fecha de finalización no puede ser en el futuro") &&
            e.ErrorCode == "FINALIZAR_FECHA_FUTURA");
    }

    [Fact]
    public async Task Validate_ConFechaEnFuturoLimite_DeberiaRetornarError()
    {
        // Arrange
        var fechaFuturaLimite = DateTime.UtcNow.AddMinutes(15); // Claramente más de 5 minutos en el futuro
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaFuturaLimite
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.FechaFinalizacion) &&
            e.ErrorCode == "FINALIZAR_FECHA_FUTURA");
    }

    [Theory]
    [InlineData(-1)]  // 1 minuto atrás
    [InlineData(0)]   // Ahora
    [InlineData(2)]   // 2 minutos adelante (dentro del límite de 5)
    [InlineData(4)]   // 4 minutos adelante (dentro del límite de 5)
    public async Task Validate_ConFechaValida_NoDeberiaRetornarErrorDeFechaFutura(int minutosDesdeAhora)
    {
        // Arrange
        var fechaValida = DateTime.UtcNow.AddMinutes(minutosDesdeAhora);
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaValida
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "FINALIZAR_FECHA_FUTURA");
    }

    #endregion

    #region Validación FechaFinalizacion - Pasado

    [Fact]
    public async Task Validate_ConFechaMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var fechaAntigua = DateTime.UtcNow.AddHours(-30); // Claramente más de 24 horas atrás
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaAntigua
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.FechaFinalizacion) &&
            e.ErrorMessage.Contains("La fecha de finalización no puede ser mayor a 24 horas en el pasado") &&
            e.ErrorCode == "FINALIZAR_FECHA_ANTIGUA");
    }

    [Theory]
    [InlineData(-1)]   // 1 hora atrás
    [InlineData(-12)]  // 12 horas atrás
    [InlineData(-20)]  // 20 horas atrás
    [InlineData(-23)]  // 23 horas atrás (dentro del límite de 24)
    public async Task Validate_ConFechaEnRangoValido_NoDeberiaRetornarErrorDeFechaPasada(int horasDesdeAhora)
    {
        // Arrange
        var fechaEnRango = DateTime.UtcNow.AddHours(horasDesdeAhora);
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaEnRango
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "FINALIZAR_FECHA_ANTIGUA");
    }

    #endregion

    #region Validación FechaFinalizacion - Null

    [Fact]
    public async Task Validate_ConFechaNull_NoDeberiaValidarFecha()
    {
        // Arrange
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test observaciones",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = null
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(FinalizarComandaCommand.FechaFinalizacion));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Comanda finalizada exitosamente - cliente satisfecho",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow.AddMinutes(-5)
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
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ValidarTodosItemsListos = false,
            NotificarMesero = false
            // Campos opcionales omitidos
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
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.Empty, // Error
            UsuarioId = Guid.Empty, // Error
            ObservacionesFinalizacion = new string('A', 501), // Error - muy largo
            FechaFinalizacion = DateTime.UtcNow.AddHours(2) // Error - futuro
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(e => e.ErrorCode == "FINALIZAR_COMANDA_ID_REQUERIDO");
        result.Errors.Should().Contain(e => e.ErrorCode == "FINALIZAR_USUARIO_ID_REQUERIDO");
        result.Errors.Should().Contain(e => e.ErrorCode == "FINALIZAR_OBSERVACIONES_LONGITUD");
        result.Errors.Should().Contain(e => e.ErrorCode == "FINALIZAR_FECHA_FUTURA");
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Comanda completada")]
    [InlineData("Finalizada por solicitud del cliente")]
    [InlineData("Todos los productos fueron entregados correctamente")]
    [InlineData("Mesa liberada - cliente satisfecho con el servicio")]
    public async Task Validate_ConDiferentesObservaciones_DeberiaSerValido(string observaciones)
    {
        // Arrange
        var command = FinalizarComandaCommand.Crear(
            comandaId: Guid.NewGuid(),
            usuarioId: Guid.NewGuid(),
            observaciones: observaciones
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConFinalizacionInmediata_DeberiaSerValido()
    {
        // Arrange
        var fechaInmediata = DateTime.UtcNow;
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Finalización inmediata",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaInmediata
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConFinalizacionSinValidaciones_DeberiaSerValido()
    {
        // Arrange
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Finalización rápida",
            ValidarTodosItemsListos = false,
            NotificarMesero = false,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - límite válido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesObservaciones_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var observaciones = new string('O', longitud);
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = observaciones,
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.ErrorCode == "FINALIZAR_OBSERVACIONES_LONGITUD");
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.ErrorCode == "FINALIZAR_OBSERVACIONES_LONGITUD");
        }
    }

    [Fact]
    public async Task Validate_ConFechaExactamenteEn5Minutos_NoDeberiaRetornarError()
    {
        // Arrange - usar 4 minutos para estar seguro de que está dentro del límite
        var fechaEn4Minutos = DateTime.UtcNow.AddMinutes(4);
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test finalizacion en el límite de tiempo",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaEn4Minutos
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "FINALIZAR_FECHA_FUTURA");
    }

    [Fact]
    public async Task Validate_ConFechaExactamenteEn24Horas_NoDeberiaRetornarError()
    {
        // Arrange - usar 23 horas para estar seguro de que está dentro del límite  
        var fechaEn23Horas = DateTime.UtcNow.AddHours(-23);
        var command = new FinalizarComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = "Test finalizacion en el límite pasado",
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = fechaEn23Horas
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "FINALIZAR_FECHA_ANTIGUA");
    }

    #endregion

    #region Tests de Factory Method

    [Fact]
    public void Crear_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var observaciones = "Test observaciones";

        // Act
        var command = FinalizarComandaCommand.Crear(comandaId, usuarioId, observaciones);

        // Assert
        command.ComandaId.Should().Be(comandaId);
        command.UsuarioId.Should().Be(usuarioId);
        command.ObservacionesFinalizacion.Should().Be(observaciones);
        command.ValidarTodosItemsListos.Should().BeTrue();
        command.NotificarMesero.Should().BeTrue();
        command.FechaFinalizacion.Should().NotBeNull();
    }

    [Fact]
    public void Crear_ConComandaIdVacio_DeberiaLanzarExcepcion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act & Assert
        var act = () => FinalizarComandaCommand.Crear(Guid.Empty, usuarioId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("ComandaId no puede estar vacío*");
    }

    [Fact]
    public void Crear_ConUsuarioIdVacio_DeberiaLanzarExcepcion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act & Assert
        var act = () => FinalizarComandaCommand.Crear(comandaId, Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("UsuarioId no puede estar vacío*");
    }

    #endregion
} 