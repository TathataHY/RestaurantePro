namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// 🎟️ Tests unitarios para ConfirmarReservacionValidator
/// Validación completa de reglas de negocio para confirmación de reservaciones
/// </summary>
public class ConfirmarReservacionValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ConfirmarReservacionValidator _validator;

    public ConfirmarReservacionValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new ConfirmarReservacionValidator(_contextMock.Object);
    }

    #region Validaciones Básicas

    [Fact]
    public async Task Validator_ConReservacionIdValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMocksParaValidacion(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConReservacionIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ReservacionId))
            .Which.ErrorMessage.Should().Be("El ID de la reservación es requerido.");
    }

    [Fact]
    public async Task Validator_ConCodigoReservacionValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = "RES-123456";
        ConfigurarMocksParaCodigoReservacion(command.CodigoReservacion);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validator_ConCodigoReservacionVacio_DeberiaFallar(string codigoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = codigoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.CodigoReservacion))
            .Which.ErrorMessage.Should().Be("El código de reservación es requerido cuando no se proporciona ID.");
    }

    [Theory]
    [InlineData("ABC")]     // Muy corto
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]  // Muy largo
    public async Task Validator_ConCodigoReservacionLongitudInvalida_DeberiaFallar(string codigoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = codigoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.CodigoReservacion))
            .Which.ErrorMessage.Should().Be("El código de reservación debe tener entre 6 y 20 caracteres.");
    }

    [Theory]
    [InlineData("res-123")]     // Minúsculas
    [InlineData("RES@123")]     // Caracteres especiales
    [InlineData("RES 123")]     // Espacios
    public async Task Validator_ConCodigoReservacionFormatoInvalido_DeberiaFallar(string codigoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = codigoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.CodigoReservacion))
            .Which.ErrorMessage.Should().Be("El código de reservación solo puede contener letras mayúsculas, números y guiones.");
    }

    [Fact]
    public async Task Validator_SinIdentificadores_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ReservacionId = Guid.Empty;
        command.CodigoReservacion = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Identificacion")
            .Which.ErrorMessage.Should().Be("Debe proporcionar el ID de reservación o el código de reservación.");
    }

    #endregion

    #region Validaciones Método Confirmación

    [Theory]
    [InlineData("Manual")]
    [InlineData("Telefono")]
    [InlineData("Email")]
    [InlineData("SMS")]
    [InlineData("App")]
    public async Task Validator_ConMetodoConfirmacionValido_DeberiaSerValido(string metodo)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoConfirmacion = metodo;
        ConfigurarMocksParaValidacion(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validator_ConMetodoConfirmacionVacio_DeberiaFallar(string metodoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoConfirmacion = metodoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.MetodoConfirmacion))
            .Which.ErrorMessage.Should().Be("El método de confirmación es requerido.");
    }

    [Fact]
    public async Task Validator_ConMetodoConfirmacionInvalido_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoConfirmacion = "MetodoInvalido";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.MetodoConfirmacion))
            .Which.ErrorMessage.Should().Be("El método de confirmación debe ser válido: Manual, Telefono, Email, SMS, App.");
    }

    [Fact]
    public async Task Validator_ConMetodoManualSinConfirmadoPor_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoConfirmacion = "Manual";
        command.ConfirmadoPor = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ConfirmadoPor))
            .Which.ErrorMessage.Should().Be("La persona que confirma es requerida para confirmaciones manuales.");
    }

    #endregion

    #region Validaciones de Negocio

    [Fact]
    public async Task Validator_ConReservacionNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockReservacionNoExiste(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ReservacionId))
            .Which.ErrorMessage.Should().Be("La reservación especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConReservacionNoConfirmable_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockReservacionNoConfirmable(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ReservacionId))
            .Which.ErrorMessage.Should().Be("La reservación no puede ser confirmada en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConTiempoLimiteExcedido_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockTiempoLimiteExcedido(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ReservacionId))
            .Which.ErrorMessage.Should().Be("Ha excedido el tiempo límite para confirmar la reservación.");
    }

    [Fact]
    public async Task Validator_ConMesaNoDisponible_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaNoDisponible(command.ReservacionId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.ReservacionId))
            .Which.ErrorMessage.Should().Be("La mesa ya no está disponible para la fecha y hora de la reservación.");
    }

    #endregion

    #region Validaciones Opcionales

    [Fact]
    public async Task Validator_ConNotasConfirmacionLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasConfirmacion = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.NotasConfirmacion))
            .Which.ErrorMessage.Should().Be("Las notas de confirmación no pueden exceder 500 caracteres.");
    }

    [Fact]
    public async Task Validator_ConDatosAdicionalesInvalidos_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.DatosAdicionales = new Dictionary<string, object>();
        
        // Agregar más de 10 elementos
        for (int i = 0; i < 11; i++)
        {
            command.DatosAdicionales.Add($"key{i}", $"value{i}");
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(ConfirmarReservacionCommand.DatosAdicionales))
            .Which.ErrorMessage.Should().Be("Los datos adicionales contienen información inválida.");
    }

    #endregion

    #region Métodos de Configuración de Mocks

    private void ConfigurarMocksParaValidacion(Guid reservacionId)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        // Mock para ReservacionExiste
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Mock para ReservacionEsConfirmable - usar factory method
        var reservacion = CrearReservacionParaTest(reservacionId, EstadoReservacion.Pendiente);
        reservacionesMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    private void ConfigurarMocksParaCodigoReservacion(string codigoReservacion)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var reservacion = CrearReservacionParaTest(Guid.NewGuid(), EstadoReservacion.Pendiente);
        // Usar reflection para setear el código de reservación
        typeof(Reservacion).GetProperty("CodigoReservacion")?.SetValue(reservacion, codigoReservacion);
        
        reservacionesMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    private void ConfigurarMockReservacionNoExiste(Guid reservacionId)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    private void ConfigurarMockReservacionNoConfirmable(Guid reservacionId)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var reservacion = CrearReservacionParaTest(reservacionId, EstadoReservacion.Confirmada);
        reservacionesMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    private void ConfigurarMockTiempoLimiteExcedido(Guid reservacionId)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Crear reservación con fecha/hora específica usando factory method
        var fechaReservacion = DateTime.UtcNow.AddHours(1); // Solo 1 hora antes, límite es 2 horas
        var reservacion = Reservacion.Crear(
            mesaId: Guid.NewGuid(),
            clienteId: Guid.NewGuid(),
            fecha: fechaReservacion,
            duracionEstimada: TimeSpan.FromHours(2),
            cantidadPersonas: 4,
            telefono: "123456789",
            email: "test@test.com");

        // Usar reflection para setear el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(reservacion, reservacionId);
        
        reservacionesMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    private void ConfigurarMockMesaNoDisponible(Guid reservacionId)
    {
        var reservacionesMock = new Mock<DbSet<Reservacion>>();
        
        reservacionesMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Crear reservación para mañana usando factory method
        var fechaReservacion = DateTime.UtcNow.AddDays(1);
        var mesaId = Guid.NewGuid();
        var reservacion = Reservacion.Crear(
            mesaId: mesaId,
            clienteId: Guid.NewGuid(),
            fecha: fechaReservacion,
            duracionEstimada: TimeSpan.FromHours(2),
            cantidadPersonas: 4,
            telefono: "123456789",
            email: "test@test.com");

        // Usar reflection para setear el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(reservacion, reservacionId);
        
        reservacionesMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Reservacion, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        // Mock para conflictos - hay otra reservación confirmada
        reservacionesMock.Setup(x => x.Where(It.IsAny<Expression<Func<Reservacion, bool>>>()).AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);
    }

    /// <summary>
    /// Método helper para crear reservaciones de test usando el factory method
    /// </summary>
    private Reservacion CrearReservacionParaTest(Guid id, EstadoReservacion estado)
    {
        var reservacion = Reservacion.Crear(
            mesaId: Guid.NewGuid(),
            clienteId: Guid.NewGuid(),
            fecha: DateTime.UtcNow.AddDays(1),
            duracionEstimada: TimeSpan.FromHours(2),
            cantidadPersonas: 4,
            telefono: "123456789",
            email: "test@test.com");

        // Usar reflection para setear el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(reservacion, id);

        // Cambiar estado si es necesario
        if (estado == EstadoReservacion.Confirmada)
        {
            reservacion.Confirmar();
        }
        else if (estado == EstadoReservacion.Cancelada)
        {
            reservacion.Cancelar("Test cancelación");
        }
        else if (estado == EstadoReservacion.Completada)
        {
            if (reservacion.Estado == EstadoReservacion.Pendiente)
            {
                reservacion.Confirmar();
            }
            reservacion.Completar();
        }

        return reservacion;
    }

    #endregion

    #region Métodos de Ayuda

    private ConfirmarReservacionCommand CrearComandoValido()
    {
        return new ConfirmarReservacionCommand
        {
            ReservacionId = Guid.NewGuid(),
            MetodoConfirmacion = "Manual",
            ConfirmadoPor = "Juan Pérez",
            NotasConfirmacion = "Confirmación exitosa",
            NotificarCliente = true
        };
    }

    #endregion
}