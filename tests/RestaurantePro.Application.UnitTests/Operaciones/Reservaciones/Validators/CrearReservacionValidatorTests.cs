namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// Tests unitarios para CrearReservacionValidator
/// Validación completa de reglas de negocio para sistema de reservaciones
/// </summary>
public class CrearReservacionValidatorTests
{
    private readonly CrearReservacionValidator _validator;

    public CrearReservacionValidatorTests()
    {
        _validator = new CrearReservacionValidator();
    }

    #region FechaReservacion Validations

    [Fact]
    public void Validator_ConFechaReservacionValida_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddDays(1); // Mañana

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConFechaReservacionPasada_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddDays(-1); // Ayer

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.FechaHoraReservacion))
            .Which.ErrorMessage.Should().Be("La fecha de reservación debe ser futura");
    }

    [Fact]
    public void Validator_ConFechaReservacionHoy_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddHours(2); // Hoy, 2 horas después

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConFechaReservacionMuyLejana_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddDays(91); // Más de 3 meses

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        // Buscamos cualquier error relacionado con la fecha de reservación que mencione "90 días"
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.FechaHoraReservacion) &&
                                         x.ErrorMessage.Contains("90 días"));
    }

    [Theory]
    [InlineData(1)]   // Mañana
    [InlineData(7)]   // Una semana
    [InlineData(30)]  // Un mes
    [InlineData(90)]  // Límite máximo
    public void Validator_ConFechasValidasEnRango_DeberiaSerValido(int diasAdelante)
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddDays(diasAdelante);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region HoraReservacion Validations

    [Theory]
    [InlineData(12, 0)] // 12:00 PM
    [InlineData(13, 30)] // 1:30 PM
    [InlineData(18, 0)] // 6:00 PM
    [InlineData(21, 30)] // 9:30 PM
    public void Validator_ConHorariosValidos_DeberiaSerValido(int hora, int minuto)
    {
        // Arrange
        var command = CrearComandoValido();
        var fechaBase = DateTime.Now.AddDays(1).Date; // Mañana a medianoche
        command.FechaHoraReservacion = fechaBase.AddHours(hora).AddMinutes(minuto);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 0)]  // Medianoche
    [InlineData(1, 0)]  // 1:00 AM
    [InlineData(2, 0)]  // 2:00 AM
    [InlineData(3, 0)]  // 3:00 AM
    public void Validator_ConHorariosInvalidos_DeberiaFallar(int hora, int minuto)
    {
        // Arrange
        var command = CrearComandoValido();
        var fechaBase = DateTime.Now.AddDays(1).Date; // Mañana a medianoche
        command.FechaHoraReservacion = fechaBase.AddHours(hora).AddMinutes(minuto);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.FechaHoraReservacion))
            .Which.ErrorMessage.Should().Be("La hora de reservación debe estar entre las 12:00 PM y 10:00 PM");
    }

    #endregion

    #region NumeroPersonas Validations

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    public void Validator_ConNumeroPersonasValido_DeberiaSerValido(int numeroPersonas)
    {
        // Arrange
        var command = CrearComandoValido();
        command.NumeroPersonas = numeroPersonas;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConNumeroPersonasCero_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NumeroPersonas = 0;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NumeroPersonas))
            .Which.ErrorMessage.Should().Be("El número de personas debe ser mayor a 0");
    }

    [Fact]
    public void Validator_ConNumeroPersonasNegativo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NumeroPersonas = -5;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NumeroPersonas))
            .Which.ErrorMessage.Should().Be("El número de personas debe ser mayor a 0");
    }

    [Fact]
    public void Validator_ConDemasiadasPersonas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NumeroPersonas = 21; // Máximo 20

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NumeroPersonas))
            .Which.ErrorMessage.Should().Be("El número máximo de personas por reservación es 20");
    }

    #endregion

    #region NombreCliente Validations

    [Fact]
    public void Validator_ConNombreClienteValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "Juan Carlos Pérez García";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConNombreClienteVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es obligatorio");
    }

    [Fact]
    public void Validator_ConNombreClienteNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es obligatorio");
    }

    [Fact]
    public void Validator_ConNombreClienteMuyCorto_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "A";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente debe tener al menos 2 caracteres");
    }

    [Fact]
    public void Validator_ConNombreClienteMuyLargo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = new string('A', 201);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente no puede exceder 200 caracteres");
    }

    #endregion

    #region TelefonoContacto Validations

    [Fact]
    public void Validator_ConTelefonoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TelefonoContacto = "+52-55-1234-5678";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConTelefonoVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TelefonoContacto = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.TelefonoContacto))
            .Which.ErrorMessage.Should().Be("El teléfono de contacto es obligatorio");
    }

    [Theory]
    [InlineData("+52-55-1234-5678")]
    [InlineData("5512345678")]
    [InlineData("(55) 1234-5678")]
    [InlineData("55 1234 5678")]
    [InlineData("+57300123456")]
    public void Validator_ConTelefonosValidos_DeberiaSerValido(string telefono)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TelefonoContacto = telefono;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")]        // Muy corto
    [InlineData("abcdefghij")] // No numérico
    [InlineData("0123456789")] // Empieza con 0
    public void Validator_ConTelefonosInvalidos_DeberiaFallar(string telefonoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TelefonoContacto = telefonoInvalido;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.TelefonoContacto));
    }

    #endregion

    #region Observaciones Validations

    [Fact]
    public void Validator_ConObservacionesValidas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Observaciones = "Mesa cerca de la ventana, cumpleaños, necesitamos silla alta para bebé";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConObservacionesVacias_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Observaciones = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // Observaciones opcionales
    }

    [Fact]
    public void Validator_ConObservacionesNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Observaciones = null;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // Observaciones opcionales
    }

    [Fact]
    public void Validator_ConObservacionesMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Observaciones = new string('A', 1001); // Máximo 1000

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearReservacionCommand.Observaciones))
            .Which.ErrorMessage.Should().Be("Las observaciones no pueden exceder 1000 caracteres");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Validator_ConReservacionCompleta_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearReservacionCommand
        {
            FechaHoraReservacion = DateTime.Now.AddDays(3).Date.AddHours(19), // 3 días, 7:00 PM
            NumeroPersonas = 4,
            NombreCliente = "María José González Hernández",
            TelefonoContacto = "+52-55-9876-5432",
            Email = "maria.gonzalez@email.com",
            Observaciones = "Celebración de aniversario, mesa romántica si es posible"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConMultiplesErrores_DeberiaListarTodos()
    {
        // Arrange
        var command = new CrearReservacionCommand
        {
            FechaHoraReservacion = DateTime.Now.AddDays(-1), // Error: fecha pasada
            NumeroPersonas = 0, // Error: número inválido
            NombreCliente = "", // Error: nombre vacío
            TelefonoContacto = "123", // Error: teléfono inválido
            Observaciones = new string('A', 1001) // Error: muy largas
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.FechaHoraReservacion));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.NumeroPersonas));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.NombreCliente));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.TelefonoContacto));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.Observaciones));
    }

    [Fact]
    public void Validator_ConReservacionParaHoy_DeberiaValidarHoraMinima()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddHours(3); // Bien por encima del mínimo de 1 hora

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConReservacionParaHoyMuyProxima_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaHoraReservacion = DateTime.Now.AddMinutes(30); // Solo 30 minutos después

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        // Buscamos cualquier error relacionado con la fecha de reservación sin importar cuántos haya
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearReservacionCommand.FechaHoraReservacion) &&
                                           x.ErrorMessage.Contains("anticipación"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validator_ConReservacionEnDomingo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var proximoDomingo = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek).Date.AddHours(14); // Próximo domingo 2:00 PM
        command.FechaHoraReservacion = proximoDomingo;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConCaracteresEspecialesEnNombre_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "José María Péñez-González & Asociados";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConEmailOpcional_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Email = null; // Email opcional

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Validator_RendimientoValidacion_DeberiaSerRapido()
    {
        // Arrange
        var command = CrearComandoValido();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _validator.Validate(command);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 validaciones
    }

    #endregion

    #region Helper Methods

    private CrearReservacionCommand CrearComandoValido()
    {
        return new CrearReservacionCommand
        {
            FechaHoraReservacion = DateTime.Now.AddDays(2).Date.AddHours(19), // Pasado mañana 7:00 PM
            NumeroPersonas = 4,
            NombreCliente = "Juan Pérez García",
            TelefonoContacto = "+52-55-1234-5678",
            Email = "juan.perez@email.com",
            Observaciones = "Reservación para cena familiar"
        };
    }

    #endregion
} 