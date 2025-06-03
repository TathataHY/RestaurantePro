using System.Reflection;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Queries;

/// <summary>
/// Tests unitarios para ObtenerReservacionesPorFechaHandler
/// Cobertura completa de consulta de reservaciones por fecha, filtros avanzados y estadísticas
/// ¡ÚLTIMO HANDLER PARA PERFECCIÓN ABSOLUTA! 🏆
/// </summary>
public class ObtenerReservacionesPorFechaHandlerTests
{
    private readonly Mock<IReservacionRepository> _mockReservacionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerReservacionesPorFechaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ObtenerReservacionesPorFechaHandler _handler;
    private readonly List<Reservacion> _reservacionesEjemplo;

    public ObtenerReservacionesPorFechaHandlerTests()
    {
        _mockReservacionRepository = new Mock<IReservacionRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerReservacionesPorFechaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new ObtenerReservacionesPorFechaHandler(
            _mockReservacionRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        _reservacionesEjemplo = CrearReservacionesEjemplo();
    }

    [Fact]
    public async Task Handle_ConFechaConReservaciones_DeberiaRetornarReservacionesPaginadas()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo reservaciones para fecha")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFechaSinReservaciones_DeberiaRetornarListaVacia()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(30);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Pagina = 1,
            TamanoPagina = 10
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No se encontraron reservaciones para la fecha")),
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
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Estado = estado,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesFiltradas = _reservacionesEjemplo.Where(r => r.Estado == estado).ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesFiltradas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ConFiltroMesa_DeberiaFiltrarPorMesa()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var mesaId = Guid.NewGuid();
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            MesaId = mesaId,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(1).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Filtro aplicado - Mesa: {mesaId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroSoloActivas_DeberiaFiltrarReservacionesActivas()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            SoloActivas = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesActivas = _reservacionesEjemplo
            .Where(r => r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada)
            .ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesActivas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ConFiltroSoloFuturas_DeberiaFiltrarReservacionesFuturas()
    {
        // Arrange
        var fecha = DateTime.Today;
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            SoloFuturas = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesFuturas = _reservacionesEjemplo.Where(r => r.FechaReservacion > DateTime.Now).ToList();
        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(reservacionesFuturas.Count).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ConOrdenamientoPorHora_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            OrdenarPorHora = true,
            Pagina = 1,
            TamanoPagina = 10
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ordenamiento aplicado por hora")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConPaginacion_DeberiaAplicarPaginacionCorrectamente()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Pagina = 2,
            TamanoPagina = 2
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(2).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Pagina = 1,
            TamanoPagina = 10
            // Sin filtro de estado para que genere estadísticas
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de estadísticas por estado
        var estadosExistentes = _reservacionesEjemplo.Select(r => r.Estado).Distinct();
        foreach (var estado in estadosExistentes)
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"{estado}:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ConFiltrosSinResultados_DeberiaRetornarListaVaciaConMensaje()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Estado = EstadoReservacion.NoShow, // Estado que no existe en nuestros ejemplos
            Pagina = 1,
            TamanoPagina = 10
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery { Fecha = fecha };

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno del servidor al obtener las reservaciones");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al obtener reservaciones por fecha")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new ObtenerReservacionesPorFechaQuery { Fecha = DateTime.Today };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Fact]
    public async Task Handle_ConResultadosCompletos_DeberiaLoggearInformacionDetallada()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Pagina = 1,
            TamanoPagina = 3
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(3).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
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
                                             v.ToString()!.Contains("reservaciones para")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltrosMultiples_DeberiaAplicarTodosLosFiltros()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var mesaId = Guid.NewGuid();
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Estado = EstadoReservacion.Confirmada,
            MesaId = mesaId,
            SoloActivas = true,
            OrdenarPorHora = true,
            Pagina = 1,
            TamanoPagina = 5
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo().Take(1).ToList();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se aplicaron todos los filtros
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filtro aplicado - Estado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filtro aplicado - Mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solo reservaciones activas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ordenamiento aplicado por hora")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConLoggingParametrosCompletos_DeberiaLoggearTodosLosParametros()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(1);
        var mesaId = Guid.NewGuid();
        var query = new ObtenerReservacionesPorFechaQuery 
        { 
            Fecha = fecha,
            Estado = EstadoReservacion.Confirmada,
            MesaId = mesaId,
            Pagina = 2,
            TamanoPagina = 5
        };

        var reservacionesDto = CrearReservacionesDtoEjemplo();

        _mockReservacionRepository.Setup(r => r.ObtenerPorFechaAsync(fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_reservacionesEjemplo);

        _mockMapper.Setup(m => m.Map<List<ReservacionDto>>(It.IsAny<List<Reservacion>>()))
            .Returns(reservacionesDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de parámetros iniciales
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Página: 2") &&
                    v.ToString()!.Contains($"Estado: Confirmada") &&
                    v.ToString()!.Contains($"Mesa: {mesaId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Métodos de Apoyo

    private List<Reservacion> CrearReservacionesEjemplo()
    {
        var fechaBase = DateTime.Today.AddDays(1);
        return new List<Reservacion>
        {
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Confirmada, fechaBase.AddHours(18)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, fechaBase.AddHours(19)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Completada, fechaBase.AddHours(20)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Cancelada, fechaBase.AddHours(21)),
            CrearReservacion(Guid.NewGuid(), EstadoReservacion.Confirmada, fechaBase.AddHours(22))
        };
    }

    private List<ReservacionDto> CrearReservacionesDtoEjemplo()
    {
        var fechaBase = DateTime.Today.AddDays(1);
        return new List<ReservacionDto>
        {
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Confirmada, FechaHora = fechaBase.AddHours(18) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Pendiente, FechaHora = fechaBase.AddHours(19) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Completada, FechaHora = fechaBase.AddHours(20) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Cancelada, FechaHora = fechaBase.AddHours(21) },
            new() { Id = Guid.NewGuid(), Estado = EstadoReservacion.Confirmada, FechaHora = fechaBase.AddHours(22) }
        };
    }

    private Reservacion CrearReservacion(Guid id, EstadoReservacion estado, DateTime fechaReservacion)
    {
        // Usar el factory method de la entidad Reservacion
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var duracion = TimeSpan.FromHours(2);
        var cantidadPersonas = 4;
        var telefono = "123456789";
        var email = "test@example.com";
        var observaciones = "Reservación de prueba";
        
        var reservacion = Reservacion.Crear(
            mesaId,
            clienteId,
            fechaReservacion,
            duracion,
            cantidadPersonas,
            telefono,
            email,
            observaciones);
        
        // Usar reflection solo para el ID y estado si es necesario
        if (id != Guid.Empty)
        {
            var idProperty = typeof(Reservacion).BaseType?.GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            idProperty?.SetValue(reservacion, id);
        }
        
        // Cambiar estado si no es el por defecto (Pendiente)
        if (estado != EstadoReservacion.Pendiente)
        {
            switch (estado)
            {
                case EstadoReservacion.Confirmada:
                    reservacion.Confirmar();
                    break;
                case EstadoReservacion.Cancelada:
                    reservacion.Cancelar("Test cancelación");
                    break;
                case EstadoReservacion.Completada:
                    reservacion.Confirmar();
                    reservacion.Completar();
                    break;
                case EstadoReservacion.NoShow:
                    reservacion.MarcarNoAsistio();
                    break;
            }
        }
        
        return reservacion;
    }

    #endregion
} 