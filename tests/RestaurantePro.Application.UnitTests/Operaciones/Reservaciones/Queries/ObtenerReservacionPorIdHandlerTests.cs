namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Queries;

/// <summary>
/// Tests unitarios para ObtenerReservacionPorIdHandler
/// Cobertura completa de consulta de reservaciones por ID, validaciones de estado y logging empresarial
/// </summary>
public class ObtenerReservacionPorIdHandlerTests
{
    private readonly Mock<IReservacionRepository> _mockReservacionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerReservacionPorIdHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ObtenerReservacionPorIdHandler _handler;

    public ObtenerReservacionPorIdHandlerTests()
    {
        _mockReservacionRepository = new Mock<IReservacionRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerReservacionPorIdHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new ObtenerReservacionPorIdHandler(
            _mockReservacionRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_ConReservacionExistente_DeberiaRetornarReservacionCorrectamente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Id.Should().Be(reservacionId);
        resultado.Value.Estado.Should().Be(EstadoReservacion.Confirmada);

        // Verificar logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Buscando reservación con ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("obtenida exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConReservacionInexistente_DeberiaRetornarError()
    {
        // Arrange
        var reservacionIdInexistente = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionIdInexistente };

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionIdInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservacion?)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("no encontrada");

        // Verificar logging de advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EstadoReservacion.Pendiente, "Pendiente")]
    [InlineData(EstadoReservacion.Confirmada, "confirmada")]
    [InlineData(EstadoReservacion.Completada, "completada exitosamente")]
    [InlineData(EstadoReservacion.Cancelada, "fue cancelada")]
    [InlineData(EstadoReservacion.NoShow, "marcada como No Show")]
    public async Task Handle_ConDiferentesEstados_DeberiaLoggearCorrectamente(
        EstadoReservacion estado, string mensajeEsperado)
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        var reservacion = CrearReservacion(reservacionId, estado);
        var reservacionDto = CrearReservacionDto(reservacionId, estado);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging específico del estado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(mensajeEsperado)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConReservacionConObservaciones_DeberiaLoggearObservaciones()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };
        var observaciones = "Mesa cerca de la ventana, cumpleaños especial";

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada, observaciones);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de observaciones
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Observaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConReservacionSinObservaciones_NoDeberiaLoggearObservaciones()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada, null);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar que NO se loggearon observaciones
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Observaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConInformacionCompleta_DeberiaLoggearDetallesCompletos()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de detalles básicos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reservación encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de información de capacidad
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cantidad de personas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno del servidor al obtener la reservación");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al obtener reservación por ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Fact]
    public async Task Handle_ConReservacionConfirmada_DeberiaLoggearFechaYHora()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };
        var fechaReservacion = DateTime.Now.AddDays(1);

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada, null, fechaReservacion);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging específico para reservación confirmada con fecha y hora
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("confirmada para")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConValidacionesCompletas_DeberiaMapearCorrectamente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Pendiente);
        var reservacionDto = new ReservacionDto
        {
            Id = reservacionId,
            ClienteId = clienteId,
            MesaId = mesaId,
            Estado = EstadoReservacion.Pendiente,
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4,
            TelefonoContacto = "+1234567890",
            CodigoReservacion = "RES-123"
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeEquivalentTo(reservacionDto);

        // Verificar que se llamó al mapper
        _mockMapper.Verify(m => m.Map<ReservacionDto>(reservacion), Times.Once);
    }

    [Fact]
    public async Task Handle_ConLoggingCompleto_DeberiaLoggearTodosLosNiveles()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };
        var observaciones = "Reservación especial con detalles";

        var reservacion = CrearReservacion(reservacionId, EstadoReservacion.Confirmada, observaciones);
        var reservacionDto = CrearReservacionDto(reservacionId, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(reservacion))
            .Returns(reservacionDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar múltiples niveles de logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Al menos información de búsqueda y éxito

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Al menos detalles de reservación y observaciones
    }

    #region Métodos de Apoyo

    private Reservacion CrearReservacion(
        Guid id, 
        EstadoReservacion estado, 
        string? observaciones = null,
        DateTime? fechaReservacion = null)
    {
        // Usar reflection para crear la entidad con constructor privado (similar a ModificarReservacionValidatorTests)
        var reservacion = (Reservacion)Activator.CreateInstance(typeof(Reservacion), true)!;
        
        // Configurar propiedades básicas usando reflection
        typeof(Reservacion).GetProperty("Id")?.SetValue(reservacion, id);
        typeof(Reservacion).GetProperty("ClienteId")?.SetValue(reservacion, Guid.NewGuid());
        typeof(Reservacion).GetProperty("MesaId")?.SetValue(reservacion, Guid.NewGuid());
        typeof(Reservacion).GetProperty("Fecha")?.SetValue(reservacion, (fechaReservacion ?? DateTime.Now.AddDays(1)).Date);
        typeof(Reservacion).GetProperty("Hora")?.SetValue(reservacion, (fechaReservacion ?? DateTime.Now.AddDays(1)).TimeOfDay);
        typeof(Reservacion).GetProperty("DuracionEstimada")?.SetValue(reservacion, TimeSpan.FromMinutes(120));
        typeof(Reservacion).GetProperty("CantidadPersonas")?.SetValue(reservacion, 4);
        typeof(Reservacion).GetProperty("Telefono")?.SetValue(reservacion, "+1234567890");
        typeof(Reservacion).GetProperty("Email")?.SetValue(reservacion, "test@example.com");
        typeof(Reservacion).GetProperty("Observaciones")?.SetValue(reservacion, observaciones ?? "Test");
        typeof(Reservacion).GetProperty("Estado")?.SetValue(reservacion, estado);
        typeof(Reservacion).GetProperty("FechaCreacion")?.SetValue(reservacion, DateTime.Now);
        
        return reservacion;
    }
    
    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        
        // Para EntityBase, intentar usar backing fields directamente
        if (propertyName == "Id")
        {
            // El Id es protected set en EntityBase, intentamos el field privado
            var idField = type.BaseType?.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ??
                         type.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (idField != null)
            {
                idField.SetValue(obj, value);
                return;
            }
        }
        
        // Fallback: intentar property normal
        var property = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    private ReservacionDto CrearReservacionDto(Guid id, EstadoReservacion estado)
    {
        return new ReservacionDto
        {
            Id = id,
            Estado = estado,
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4,
            TelefonoContacto = "+1234567890",
            CodigoReservacion = $"RES-{id:N}".Substring(0, 12),
            FechaCreacion = DateTime.Now
        };
    }

    #endregion
} 