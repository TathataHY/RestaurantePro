namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CANCELAR RESERVACION VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de cancelación de reservaciones
/// Cobertura: 100% de reglas de negocio del CancelarReservacionValidator
/// </summary>
public class CancelarReservacionValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CancelarReservacionValidator _validator;
    private readonly Mock<DbSet<Reservacion>> _mockReservacionesDbSet;

    public CancelarReservacionValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockReservacionesDbSet = new Mock<DbSet<Reservacion>>();
        
        _mockContext.Setup(c => c.Reservaciones).Returns(_mockReservacionesDbSet.Object);
        
        _validator = new CancelarReservacionValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private CancelarReservacionCommand CrearCommandValido()
    {
        return new CancelarReservacionCommand
        {
            ReservacionId = Guid.NewGuid(),
            MotivoTexto = "Cliente canceló por cambio de planes de viaje",
            CanceladoPor = Guid.NewGuid(),
            NotificarCliente = true
        };
    }

    private Reservacion CrearReservacionValida(Guid reservacionId)
    {
        return new Reservacion
        {
            Id = reservacionId,
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            Fecha = DateTime.UtcNow.AddHours(3), // 3 horas en el futuro
            CantidadPersonas = 4,
            Estado = EstadoReservacion.Confirmada,
            FechaCreacion = DateTime.UtcNow.AddDays(-1),
            DuracionEstimada = TimeSpan.FromHours(2),
            NombreCliente = "Juan Pérez",
            TelefonoCliente = "555-123-4567"
        };
    }

    #endregion

    #region Helper Methods para Mocks

    private void ConfigurarReservacionExistente(Reservacion reservacion)
    {
        var data = new List<Reservacion> { reservacion }.AsQueryable();
        
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    private void ConfigurarReservacionInexistente()
    {
        var data = new List<Reservacion>().AsQueryable();
        
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion

    #region Validación ReservacionId

    [Fact]
    public async Task Validate_ConReservacionIdVacia_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ReservacionId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("El ID de la reservación es requerido"));
    }

    [Fact]
    public async Task Validate_ConReservacionIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarReservacionInexistente();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("La reservación especificada no existe"));
    }

    [Fact]
    public async Task Validate_ConReservacionIdValido_NoDeberiaRetornarErrorDeId()
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("El ID de la reservación es requerido"));
    }

    #endregion

    #region Validación Estado de Reservación

    [Theory]
    [InlineData(EstadoReservacion.Cancelada)]
    [InlineData(EstadoReservacion.Completada)]
    [InlineData(EstadoReservacion.NoShow)]
    public async Task Validate_ConReservacionNoCancelable_DeberiaRetornarError(EstadoReservacion estadoNoCancelable)
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Estado = estadoNoCancelable;
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("La reservación no puede ser cancelada en su estado actual"));
    }

    [Theory]
    [InlineData(EstadoReservacion.Pendiente)]
    [InlineData(EstadoReservacion.Confirmada)]
    public async Task Validate_ConReservacionCancelable_NoDeberiaRetornarErrorDeEstado(EstadoReservacion estadoCancelable)
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Estado = estadoCancelable;
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("La reservación no puede ser cancelada en su estado actual"));
    }

    #endregion

    #region Validación Fecha Vencida

    [Fact]
    public async Task Validate_ConReservacionVencida_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddHours(-2); // 2 horas en el pasado
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId) &&
            e.ErrorMessage.Contains("No se puede cancelar una reservación que ya pasó"));
    }

    [Theory]
    [InlineData(1)] // 1 hora en el futuro
    [InlineData(24)] // 1 día en el futuro
    [InlineData(168)] // 1 semana en el futuro
    public async Task Validate_ConReservacionFutura_NoDeberiaRetornarErrorDeFecha(int horasFuturas)
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddHours(horasFuturas);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede cancelar una reservación que ya pasó"));
    }

    #endregion

    #region Validación MotivoTexto

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoTextoVacioONull_DeberiaRetornarError(string motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = motivoInvalido;
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto) &&
            e.ErrorMessage.Contains("El motivo de cancelación es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivoTextoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = "Corto"; // Menos de 10 caracteres
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoTextoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = new string('A', 501); // Más de 500 caracteres
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("Cliente canceló por enfermedad")]
    [InlineData("Cambio de planes de viaje de último momento")]
    [InlineData("Emergencia familiar que requiere cancelar la reservación")]
    public async Task Validate_ConMotivoTextoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = motivoValido;
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto));
    }

    #endregion

    #region Validación CanceladoPor

    [Fact]
    public async Task Validate_ConCanceladoPorVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CanceladoPor = Guid.Empty;
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.CanceladoPor) &&
            e.ErrorMessage.Contains("El usuario que cancela es requerido"));
    }

    [Fact]
    public async Task Validate_ConCanceladoPorValido_NoDeberiaRetornarErrorDeCanceladoPor()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CanceladoPor = Guid.NewGuid();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.CanceladoPor) &&
            e.ErrorMessage.Contains("El usuario que cancela es requerido"));
    }

    #endregion

    #region Validación NotificarCliente

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConNotificarClienteValido_NoDeberiaRetornarError(bool notificarCliente)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NotificarCliente = notificarCliente;
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CancelarReservacionCommand.NotificarCliente) &&
            e.ErrorMessage.Contains("Debe especificar si notificar al cliente"));
    }

    #endregion

    #region Validación Política de Cancelación

    [Fact]
    public async Task Validate_ConCancelacionDentroDelLimiteDeAnticipacion_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddHours(3); // 3 horas de anticipación
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("La cancelación no cumple con la política establecida"));
    }

    [Fact]
    public async Task Validate_ConCancelacionFueraDelLimiteDeAnticipacion_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddMinutes(90); // Solo 1.5 horas de anticipación (menos del mínimo de 2)
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "PoliticaCancelacion" &&
            e.ErrorMessage.Contains("La cancelación no cumple con la política establecida (mínimo 2 horas de anticipación)"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new CancelarReservacionCommand
        {
            ReservacionId = Guid.NewGuid(),
            MotivoTexto = "Cliente canceló debido a emergencia familiar",
            CanceladoPor = Guid.NewGuid(),
            NotificarCliente = true
        };

        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddHours(4); // 4 horas de anticipación
        ConfigurarReservacionExistente(reservacion);

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
        var command = new CancelarReservacionCommand
        {
            ReservacionId = Guid.NewGuid(),
            MotivoTexto = "Cancelación solicitada por cliente",
            CanceladoPor = Guid.NewGuid(),
            NotificarCliente = false
        };

        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

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
        var command = new CancelarReservacionCommand
        {
            ReservacionId = Guid.Empty, // Error
            MotivoTexto = "Corto", // Error - muy corto
            CanceladoPor = Guid.Empty // Error
            // NotificarCliente tiene valor por defecto
        };

        ConfigurarReservacionInexistente();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Cliente enfermó y no puede asistir")]
    [InlineData("Cambio de planes de último momento")]
    [InlineData("Problemas de transporte impiden llegar a tiempo")]
    [InlineData("Cancelación solicitada por emergencia familiar")]
    public async Task Validate_ConDiferentesMotivos_DeberiaSerValido(string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = motivo;
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCancelacionAdministrativa_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = "Cancelación administrativa por mantenimiento del restaurante";
        command.NotificarCliente = true; // Importante notificar en cancelaciones administrativas
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCancelacionPorCliente_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = "Cliente solicitó cancelación por cambio de planes";
        command.NotificarCliente = false; // No es necesario notificar si el cliente canceló
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(10, true)]   // 10 caracteres - mínimo válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - máximo válido
    [InlineData(9, false)]   // 9 caracteres - inválido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoTexto = new string('M', longitud);
        var reservacion = CrearReservacionValida(command.ReservacionId);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(CancelarReservacionCommand.MotivoTexto));
        }
    }

    [Theory]
    [InlineData(2.1, true)]  // 2.1 horas - válido
    [InlineData(3, true)]    // 3 horas - válido
    [InlineData(24, true)]   // 24 horas - válido
    [InlineData(1.9, false)] // 1.9 horas - inválido
    [InlineData(1, false)]   // 1 hora - inválido
    public async Task Validate_ConDiferentesHorasDeAnticipacion_DeberiaValidarCorrectamente(double horasAnticipacion, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        var reservacion = CrearReservacionValida(command.ReservacionId);
        reservacion.Fecha = DateTime.UtcNow.AddHours(horasAnticipacion);
        ConfigurarReservacionExistente(reservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == "PoliticaCancelacion");
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == "PoliticaCancelacion" &&
                e.ErrorMessage.Contains("La cancelación no cumple con la política establecida"));
        }
    }

    #endregion
} 