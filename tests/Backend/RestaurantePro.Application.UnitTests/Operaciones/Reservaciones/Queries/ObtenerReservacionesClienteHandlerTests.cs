namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Queries;

/// <summary>
/// Tests unitarios para ObtenerReservacionesClienteHandler
/// Cobertura completa de consulta de reservaciones por cliente, filtros avanzados, paginación y estadísticas
/// </summary>
public class ObtenerReservacionesClienteHandlerTests
{
    private readonly Mock<IReservacionRepository> _mockReservacionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerReservacionesClienteHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ObtenerReservacionesClienteHandler _handler;
    private readonly List<Reservacion> _reservacionesEjemplo;

    public ObtenerReservacionesClienteHandlerTests()
    {
        _mockReservacionRepository = new Mock<IReservacionRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerReservacionesClienteHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new ObtenerReservacionesClienteHandler(
            _mockReservacionRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        _reservacionesEjemplo = CrearReservacionesEjemplo();
    }

    [Fact]
    public async Task Handle_ConClienteConReservaciones_DeberiaRetornarReservacionesPaginadas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Items.Should().NotBeEmpty();
        resultado.Value.PageNumber.Should().Be(1);
        resultado.Value.TotalCount.Should().Be(_reservacionesEjemplo.Count);

        // Verificar logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo reservaciones del cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConClienteSinReservaciones_DeberiaRetornarListaVacia()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Pagina = 1,
            TamanoPagina = 10
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().BeEmpty();
        resultado.Value.TotalCount.Should().Be(0);

        // Verificar logging específico
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No se encontraron reservaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EstadoReservacion.Pendiente)]
    [InlineData(EstadoReservacion.Confirmada)]
    [InlineData(EstadoReservacion.Cancelada)]
    [InlineData(EstadoReservacion.Completada)]
    public async Task Handle_ConFiltroEstado_DeberiaFiltrarCorrectamente(EstadoReservacion estado)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Estado = estado,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesFiltradas = _reservacionesEjemplo.Where(r => r.Estado == estado).ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesFiltradas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.TotalCount.Should().Be(reservacionesFiltradas.Count);

        // Verificar logging de filtro
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Filtro aplicado - Estado: {estado}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroSoloFuturas_DeberiaFiltrarReservacionesFuturas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            SoloFuturas = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesFuturas = _reservacionesEjemplo.Where(r => r.FechaReservacion > DateTime.Now).ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesFuturas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de filtro
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solo reservaciones futuras")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroSoloActivas_DeberiaFiltrarReservacionesActivas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            SoloActivas = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesActivas = _reservacionesEjemplo
            .Where(r => r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada)
            .ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesActivas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de filtro
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solo reservaciones activas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroRangoFechas_DeberiaFiltrarPorFechas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fechaDesde = DateTime.Today.AddDays(-30);
        var fechaHasta = DateTime.Today.AddDays(30);
        
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de filtro
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rango de fechas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorFecha_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            OrdenarPorFecha = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de ordenamiento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ordenamiento aplicado por fecha")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConPaginacion_DeberiaAplicarPaginacionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Pagina = 2,
            TamanoPagina = 2
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(2).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.PageNumber.Should().Be(2);
        resultado.Value.PageSize.Should().Be(2);
        resultado.Value.TotalCount.Should().Be(_reservacionesEjemplo.Count);
    }

    [Fact]
    public async Task Handle_ConEstadisticasCompletas_DeberiaLoggearEstadisticas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Pagina = 1,
            TamanoPagina = 10
            // Sin filtro de estado para que genere estadísticas
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de estadísticas
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Estadísticas del cliente {clienteId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de perfil del cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Perfil del cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroSinResultados_DeberiaRetornarListaVaciaConMensaje()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Estado = EstadoReservacion.NoShow, // Estado que no existe en nuestros ejemplos
            Pagina = 1,
            TamanoPagina = 10
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Items.Should().BeEmpty();

        // Verificar logging específico
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("que cumplan los criterios especificados")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery { ClienteId = clienteId };

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno del servidor al obtener las reservaciones del cliente");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al obtener reservaciones del cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new ObtenerReservacionesClienteQuery { ClienteId = Guid.NewGuid() };
        var cancellationToken = new CancellationToken(canceled: true);

        // Limpiar todos los setups previos y configurar solo para lanzar la excepción
        _mockReservacionRepository.Reset();
        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Theory]
    [InlineData(true, false, false, "Futuras")]
    [InlineData(false, true, false, "Activas")]
    [InlineData(false, false, true, "Historial")]
    [InlineData(false, false, false, "Todas")]
    public async Task Handle_ConDiferentesTiposConsulta_DeberiaLoggearTipoCorrectamente(
        bool soloFuturas, bool soloActivas, bool soloHistorial, string tipoEsperado)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            SoloFuturas = soloFuturas,
            SoloActivas = soloActivas,
            SoloHistorial = soloHistorial,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging del tipo de consulta
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Tipo: {tipoEsperado}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConResultadosCompletos_DeberiaLoggearInformacionDetallada()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerReservacionesClienteQuery 
        { 
            ClienteId = clienteId,
            Pagina = 1,
            TamanoPagina = 3
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(3).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging detallado de resultados
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se encontraron") && 
                                             v.ToString()!.Contains("reservaciones para el cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Métodos de Apoyo

    private List<Reservacion> CrearReservacionesEjemplo()
    {
        var clienteId = Guid.NewGuid();
        return new List<Reservacion>
        {
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Confirmada, DateTime.Now.AddDays(5)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, DateTime.Now.AddDays(10)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Completada, DateTime.Now.AddDays(-5)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Cancelada, DateTime.Now.AddDays(-2)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Confirmada, DateTime.Now.AddDays(2))
        };
    }

    private List<ReservacionDto> CrearReservacionesDtoEjemplo()
    {
        return new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Confirmada, FechaHora = DateTime.Now.AddDays(5) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Pendiente, FechaHora = DateTime.Now.AddDays(10) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Completada, FechaHora = DateTime.Now.AddDays(-5) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Cancelada, FechaHora = DateTime.Now.AddDays(-2) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Confirmada, FechaHora = DateTime.Now.AddDays(2) }
        };
    }

    private Reservacion CrearReservacion(Guid id, EstadoReservacion estado, DateTime fechaReservacion)
    {
        // Para poder crear la reservación, primero usamos una fecha futura si la original es pasada
        var fechaParaCrear = fechaReservacion < DateTime.Now.Date ? DateTime.Now.AddDays(1) : fechaReservacion;
        
        // Usar el método estático Crear de la entidad Reservacion
        // El método Crear espera (mesaId, clienteId, fecha, duracionEstimada, cantidadPersonas, telefono, email, observaciones)
        var reservacion = Reservacion.Crear(
            mesaId: Guid.NewGuid(),
            clienteId: Guid.NewGuid(),
            fecha: fechaParaCrear,
            duracionEstimada: TimeSpan.FromMinutes(90),
            cantidadPersonas: 4,
            telefono: "+1234567890",
            email: "cliente@test.com",
            observaciones: "Reservación de prueba"
        );

        // Ahora usamos reflexión para establecer la fecha real que queremos (incluso si es pasada)
        if (fechaReservacion != fechaParaCrear)
        {
            var fechaProperty = typeof(Reservacion).GetProperty("Fecha", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            var horaProperty = typeof(Reservacion).GetProperty("Hora", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (fechaProperty != null)
            {
                fechaProperty.SetValue(reservacion, fechaReservacion.Date);
            }
            
            if (horaProperty != null)
            {
                horaProperty.SetValue(reservacion, fechaReservacion.TimeOfDay);
            }
        }

        // Si necesitamos cambiar el estado después de la creación, usamos reflexión solo para el estado
        if (estado != EstadoReservacion.Pendiente)
        {
            var estadoProperty = typeof(Reservacion).GetProperty("Estado", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (estadoProperty != null)
            {
                estadoProperty.SetValue(reservacion, estado);
            }
        }

        // Si necesitamos cambiar el ID, usamos reflexión solo para el ID
        if (id != Guid.Empty && id != reservacion.Id)
        {
            var idProperty = typeof(Reservacion).GetProperty("Id", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (idProperty != null)
            {
                idProperty.SetValue(reservacion, id);
            }
        }
        
        return reservacion;
    }

    #endregion
} 