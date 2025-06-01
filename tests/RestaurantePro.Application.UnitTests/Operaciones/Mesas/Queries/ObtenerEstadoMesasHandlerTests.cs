namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Queries;

/// <summary>
/// Tests unitarios para ObtenerEstadoMesasHandler
/// Cobertura completa de estadísticas de mesas, filtros por zona y métricas empresariales
/// </summary>
public class ObtenerEstadoMesasHandlerTests
{
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerEstadoMesasHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ObtenerEstadoMesasHandler _handler;
    private readonly List<Mesa> _mesasEjemplo;
    private readonly List<MesaDto> _mesasDtoEjemplo;

    public ObtenerEstadoMesasHandlerTests()
    {
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerEstadoMesasHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new ObtenerEstadoMesasHandler(
            _mockMesaRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        _mesasEjemplo = CrearMesasEjemplo();
        _mesasDtoEjemplo = CrearMesasDtoEjemplo();
    }

    [Fact]
    public async Task Handle_ConMesasDisponibles_DeberiaRetornarEstadoCorrectamente()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(_mesasEjemplo))
            .Returns(_mesasDtoEjemplo);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Mesas.Should().HaveCount(_mesasDtoEjemplo.Count);
        resultado.Value.Estadisticas.Should().NotBeNull();
        resultado.Value.FechaConsulta.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

        // Verificar estadísticas calculadas
        resultado.Value.Estadisticas.MesasDisponibles.Should().Be(2);
        resultado.Value.Estadisticas.MesasOcupadas.Should().Be(2);
        resultado.Value.Estadisticas.MesasReservadas.Should().Be(1);
        resultado.Value.Estadisticas.MesasFueraDeServicio.Should().Be(1);
        resultado.Value.Estadisticas.PorcentajeOcupacion.Should().Be(50m); // (2+1)/6 * 100

        _mockMesaRepository.Verify(r => r.ObtenerTodasAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(_mesasEjemplo), Times.Once);
    }

    [Fact]
    public async Task Handle_FiltrandoPorZona_DeberiaRetornarSoloMesasDeLaZona()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            Zona = "Terraza",
            IncluirEstadisticas = true
        };

        var mesasTerraza = _mesasEjemplo.Where(m => m.Ubicacion == "Terraza").ToList();
        var mesasTerrazaDto = _mesasDtoEjemplo.Where(m => m.Ubicacion == "Terraza").ToList();

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
            .Returns(mesasTerrazaDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Zona.Should().Be("Terraza");
        resultado.Value.Mesas.Should().HaveCount(mesasTerrazaDto.Count);
        resultado.Value.Mesas.Should().OnlyContain(m => m.Ubicacion == "Terraza");

        // Verificar estadísticas específicas de la zona
        resultado.Value.Estadisticas.Should().NotBeNull();
        resultado.Value.Estadisticas.PorZona.Should().NotBeEmpty();
        resultado.Value.Estadisticas.PorZona.Should().Contain(z => z.Zona == "Terraza");
    }

    [Fact]
    public async Task Handle_SinIncluirEstadisticas_NoDeberiaCalcularEstadisticas()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            IncluirEstadisticas = false
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(_mesasEjemplo))
            .Returns(_mesasDtoEjemplo);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Mesas.Should().HaveCount(_mesasDtoEjemplo.Count);
        
        // Las estadísticas no deberían estar calculadas (valores por defecto)
        resultado.Value.Estadisticas.Should().NotBeNull();
        resultado.Value.Estadisticas.MesasDisponibles.Should().Be(0);
        resultado.Value.Estadisticas.MesasOcupadas.Should().Be(0);
        resultado.Value.Estadisticas.PorcentajeOcupacion.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SinMesasEnElSistema_DeberiaRetornarEstadoVacio()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(new List<Mesa>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Mesas.Should().BeNull();
        resultado.Value.Estadisticas.Should().NotBeNull();
        resultado.Value.Estadisticas.MesasDisponibles.Should().Be(0);
        resultado.Value.Estadisticas.MesasOcupadas.Should().Be(0);
        resultado.Value.Estadisticas.PorcentajeOcupacion.Should().Be(0);

        // Verificar que se loggeó la advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No se encontraron mesas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConZonaInexistente_DeberiaRetornarEstadoVacio()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            Zona = "ZonaInexistente",
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
            .Returns(new List<MesaDto>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Zona.Should().Be("ZonaInexistente");
        resultado.Value.Mesas.Should().BeEmpty();
        resultado.Value.Estadisticas.MesasDisponibles.Should().Be(0);
        resultado.Value.Estadisticas.PorcentajeOcupacion.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ConEstadisticasCompletas_DeberiaCalcularTodasLasMetricas()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(_mesasEjemplo))
            .Returns(_mesasDtoEjemplo);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var estadisticas = resultado.Value.Estadisticas;
        
        // Verificar conteos por estado
        estadisticas.MesasDisponibles.Should().Be(2);
        estadisticas.MesasOcupadas.Should().Be(2);
        estadisticas.MesasReservadas.Should().Be(1);
        estadisticas.MesasFueraDeServicio.Should().Be(1);
        estadisticas.MesasActivas.Should().Be(6);
        estadisticas.MesasInactivas.Should().Be(0);

        // Verificar porcentajes
        estadisticas.PorcentajeOcupacion.Should().Be(50m); // (2+1)/6 * 100
        estadisticas.PorcentajeDisponibilidad.Should().Be(33.33m); // 2/6 * 100

        // Verificar capacidades
        estadisticas.CapacidadTotalDisponible.Should().Be(8); // Mesa 1 (4) + Mesa 6 (4)
        estadisticas.CapacidadTotalOcupada.Should().Be(10); // Mesa 2 (4) + Mesa 3 (2) + Mesa 4 (4)

        // Verificar estadísticas por zona
        estadisticas.PorZona.Should().NotBeEmpty();
        estadisticas.PorZona.Should().HaveCount(2); // Interior y Terraza
        
        var zonaInterior = estadisticas.PorZona.FirstOrDefault(z => z.Zona == "Interior");
        zonaInterior.Should().NotBeNull();
        zonaInterior!.TotalMesas.Should().Be(4);
        
        var zonaTerraza = estadisticas.PorZona.FirstOrDefault(z => z.Zona == "Terraza");
        zonaTerraza.Should().NotBeNull();
        zonaTerraza!.TotalMesas.Should().Be(2);
    }

    [Theory]
    [InlineData("interior", "Interior")] // Case insensitive
    [InlineData("TERRAZA", "Terraza")]
    [InlineData("Interior", "Interior")]
    public async Task Handle_ConZonaCaseInsensitive_DeberiaFiltrarCorrectamente(string zonaInput, string zonaEsperada)
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            Zona = zonaInput,
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        var mesasFiltradas = _mesasDtoEjemplo.Where(m => m.Ubicacion.Equals(zonaEsperada, StringComparison.OrdinalIgnoreCase)).ToList();
        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
            .Returns(mesasFiltradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Zona.Should().Be(zonaInput);
        resultado.Value.Mesas.Should().OnlyContain(m => m.Ubicacion.Equals(zonaEsperada, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery();

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno del servidor");

        // Verificar que se loggeó el error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al obtener estado de mesas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnMapper_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(_mesasEjemplo))
            .Throws(new Exception("Error de mapeo"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno del servidor");
    }

    [Fact]
    public async Task Handle_ConMesasOrdenadas_DeberiaRetornarMesasOrdenadas()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery();

        var mesasDesordenadas = new List<Mesa>
        {
            CrearMesa(5, "Mesa 5", EstadoMesa.Disponible, "Interior", 4),
            CrearMesa(1, "Mesa 1", EstadoMesa.Ocupada, "Interior", 2),
            CrearMesa(3, "Mesa 3", EstadoMesa.Reservada, "Terraza", 6)
        };

        var mesasDtoOrdenadas = new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = 1, Nombre = "Mesa 1" },
            new() { Id = Guid.NewGuid(), Numero = 3, Nombre = "Mesa 3" },
            new() { Id = Guid.NewGuid(), Numero = 5, Nombre = "Mesa 5" }
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(mesasDesordenadas);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
            .Returns(mesasDtoOrdenadas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Mesas.Should().BeInAscendingOrder(m => m.Numero);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)] // Sin mesas
    [InlineData(1, 0, 0, 0, 0, 0)] // Solo disponibles
    [InlineData(0, 1, 0, 0, 100, 0)] // Solo ocupadas
    [InlineData(0, 0, 1, 0, 100, 0)] // Solo reservadas
    [InlineData(2, 1, 1, 0, 50, 50)] // Mixto
    [InlineData(1, 1, 1, 1, 50, 25)] // Todos los estados
    public async Task Handle_ConDiferentesDistribucionesDeEstado_DeberiaCalcularPorcentajesCorrectamente(
        int disponibles, int ocupadas, int reservadas, int fueraServicio, 
        decimal porcentajeOcupacionEsperado, decimal porcentajeDisponibilidadEsperado)
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery { IncluirEstadisticas = true };
        var mesas = new List<Mesa>();
        var mesasDto = new List<MesaDto>();

        // Crear mesas según la distribución
        int numeroMesa = 1;
        for (int i = 0; i < disponibles; i++)
        {
            mesas.Add(CrearMesa(numeroMesa, $"Mesa {numeroMesa}", EstadoMesa.Disponible, "Interior", 4));
            mesasDto.Add(new MesaDto { Id = Guid.NewGuid(), Numero = numeroMesa, Estado = EstadoMesa.Disponible });
            numeroMesa++;
        }
        for (int i = 0; i < ocupadas; i++)
        {
            mesas.Add(CrearMesa(numeroMesa, $"Mesa {numeroMesa}", EstadoMesa.Ocupada, "Interior", 4));
            mesasDto.Add(new MesaDto { Id = Guid.NewGuid(), Numero = numeroMesa, Estado = EstadoMesa.Ocupada });
            numeroMesa++;
        }
        for (int i = 0; i < reservadas; i++)
        {
            mesas.Add(CrearMesa(numeroMesa, $"Mesa {numeroMesa}", EstadoMesa.Reservada, "Interior", 4));
            mesasDto.Add(new MesaDto { Id = Guid.NewGuid(), Numero = numeroMesa, Estado = EstadoMesa.Reservada });
            numeroMesa++;
        }
        for (int i = 0; i < fueraServicio; i++)
        {
            mesas.Add(CrearMesa(numeroMesa, $"Mesa {numeroMesa}", EstadoMesa.FueraDeServicio, "Interior", 4));
            mesasDto.Add(new MesaDto { Id = Guid.NewGuid(), Numero = numeroMesa, Estado = EstadoMesa.FueraDeServicio });
            numeroMesa++;
        }

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(mesas);
        _mockMapper.Setup(m => m.Map<List<MesaDto>>(mesas)).Returns(mesasDto);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        var estadisticas = resultado.Value.Estadisticas;
        
        estadisticas.MesasDisponibles.Should().Be(disponibles);
        estadisticas.MesasOcupadas.Should().Be(ocupadas);
        estadisticas.MesasReservadas.Should().Be(reservadas);
        estadisticas.MesasFueraDeServicio.Should().Be(fueraServicio);
        estadisticas.PorcentajeOcupacion.Should().Be(porcentajeOcupacionEsperado);
        estadisticas.PorcentajeDisponibilidad.Should().Be(porcentajeDisponibilidadEsperado);
    }

    [Fact]
    public async Task Handle_ConLoggingCompleto_DeberiaLoggearInformacionCorrectamente()
    {
        // Arrange
        var query = new ObtenerEstadoMesasQuery
        {
            Zona = "Interior",
            IncluirEstadisticas = true
        };

        _mockMesaRepository.Setup(r => r.ObtenerTodasAsync())
            .ReturnsAsync(_mesasEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
            .Returns(_mesasDtoEjemplo.Where(m => m.Ubicacion == "Interior").ToList());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo estado de mesas") && v.ToString()!.Contains("Interior")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Estado de mesas obtenido correctamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Métodos de Apoyo

    private List<Mesa> CrearMesasEjemplo()
    {
        return new List<Mesa>
        {
            CrearMesa(1, "Mesa 1", EstadoMesa.Disponible, "Interior", 4),
            CrearMesa(2, "Mesa 2", EstadoMesa.Ocupada, "Interior", 4),
            CrearMesa(3, "Mesa 3", EstadoMesa.Reservada, "Interior", 2),
            CrearMesa(4, "Mesa 4", EstadoMesa.Ocupada, "Interior", 4),
            CrearMesa(5, "Mesa 5", EstadoMesa.FueraDeServicio, "Terraza", 6),
            CrearMesa(6, "Mesa 6", EstadoMesa.Disponible, "Terraza", 4)
        };
    }

    private List<MesaDto> CrearMesasDtoEjemplo()
    {
        return new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = 1, Nombre = "Mesa 1", Estado = EstadoMesa.Disponible, Ubicacion = "Interior", Capacidad = 4 },
            new() { Id = Guid.NewGuid(), Numero = 2, Nombre = "Mesa 2", Estado = EstadoMesa.Ocupada, Ubicacion = "Interior", Capacidad = 4 },
            new() { Id = Guid.NewGuid(), Numero = 3, Nombre = "Mesa 3", Estado = EstadoMesa.Reservada, Ubicacion = "Interior", Capacidad = 2 },
            new() { Id = Guid.NewGuid(), Numero = 4, Nombre = "Mesa 4", Estado = EstadoMesa.Ocupada, Ubicacion = "Interior", Capacidad = 4 },
            new() { Id = Guid.NewGuid(), Numero = 5, Nombre = "Mesa 5", Estado = EstadoMesa.FueraDeServicio, Ubicacion = "Terraza", Capacidad = 6 },
            new() { Id = Guid.NewGuid(), Numero = 6, Nombre = "Mesa 6", Estado = EstadoMesa.Disponible, Ubicacion = "Terraza", Capacidad = 4 }
        };
    }

    private Mesa CrearMesa(int numero, string nombre, EstadoMesa estado, string ubicacion, int capacidad)
    {
        // Crear mesa usando reflection para establecer propiedades privadas
        var mesa = (Mesa)Activator.CreateInstance(typeof(Mesa), true)!;
        
        // Establecer propiedades usando reflection
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, Guid.NewGuid());
        typeof(Mesa).GetProperty("Numero")?.SetValue(mesa, numero);
        typeof(Mesa).GetProperty("Nombre")?.SetValue(mesa, nombre);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, estado);
        typeof(Mesa).GetProperty("Ubicacion")?.SetValue(mesa, ubicacion);
        typeof(Mesa).GetProperty("Capacidad")?.SetValue(mesa, capacidad);
        
        return mesa;
    }

    #endregion
} 