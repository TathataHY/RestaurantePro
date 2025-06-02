namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Queries;

/// <summary>
/// Pruebas unitarias para ObtenerMesasDisponiblesHandler
/// Tests que cubren todos los escenarios de obtención de mesas disponibles con filtros
/// </summary>
public class ObtenerMesasDisponiblesHandlerTests
{
    private readonly Mock<IMesaRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerMesasDisponiblesHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ObtenerMesasDisponiblesHandler _handler;
    private readonly List<Mesa> _mesasDisponiblesEjemplo;
    private readonly List<MesaDto> _mesasDtoEjemplo;

    public ObtenerMesasDisponiblesHandlerTests()
    {
        _mockRepository = new Mock<IMesaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerMesasDisponiblesHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new ObtenerMesasDisponiblesHandler(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _mesasDisponiblesEjemplo = CrearMesasDisponiblesEjemplo();
        _mesasDtoEjemplo = CrearMesasDtoEjemplo();
    }

    [Fact]
    public async Task Handle_ConMesasDisponibles_DeberiaRetornarMesasPaginadas()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica(1, 10);

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns(_mesasDtoEjemplo.Take(10).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(10);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(_mesasDisponiblesEjemplo.Count);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCapacidadMinima_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var capacidadMinima = 6;
        var query = ObtenerMesasDisponiblesQuery.ConCapacidad(capacidadMinima);
        var mesasGrandes = _mesasDisponiblesEjemplo.Where(m => m.Capacidad >= capacidadMinima).ToList();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().OnlyContain(dto => dto.Capacidad >= capacidadMinima);
        result.Value.TotalCount.Should().Be(mesasGrandes.Count);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConZonaEspecifica_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var zona = "VIP";
        var query = ObtenerMesasDisponiblesQuery.PorZona(zona);
        var mesasVip = _mesasDisponiblesEjemplo.Where(m => m.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase)).ToList();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().OnlyContain(dto => dto.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase));
        result.Value.TotalCount.Should().Be(mesasVip.Count);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCapacidadYZona_DeberiaAplicarAmbosFiltros()
    {
        // Arrange
        var capacidadMinima = 4;
        var zona = "Terraza";
        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = capacidadMinima,
            Zona = zona,
            Pagina = 1,
            TamanoPagina = 20
        };

        var mesasFiltradas = _mesasDisponiblesEjemplo
            .Where(m => m.Capacidad >= capacidadMinima && 
                       m.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().OnlyContain(dto => 
            dto.Capacidad >= capacidadMinima && 
            dto.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase));
        result.Value.TotalCount.Should().Be(mesasFiltradas.Count);
    }

    [Fact]
    public async Task Handle_ConOrdenPorNumero_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            OrdenarPorNumero = true,
            Pagina = 1,
            TamanoPagina = 20
        };

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns(_mesasDtoEjemplo.OrderBy(m => m.Numero).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var numerosOrdenados = result.Value.Items.Select(m => m.Numero).ToList();
        numerosOrdenados.Should().BeInAscendingOrder();

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPaginacionSegundaPagina_DeberiaRetornarPaginaCorrecta()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica(2, 5);

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.Items.Should().HaveCount(c => c <= 5);
        result.Value.TotalCount.Should().Be(_mesasDisponiblesEjemplo.Count);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SinMesasDisponibles_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(new List<Mesa>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.PageNumber.Should().Be(1);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConRepositorioNull_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync((List<Mesa>?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltrosSinResultados_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = 20, // Capacidad muy alta que ninguna mesa tiene
            Zona = "ZonaInexistente",
            Pagina = 1,
            TamanoPagina = 20
        };

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ThrowsAsync(new Exception("Error en base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al obtener las mesas disponibles");

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnMapper_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Throws(new Exception("Error en mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al obtener las mesas disponibles");

        _mockRepository.Verify(r => r.ObtenerMesasDisponiblesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.ConCapacidad(4);

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns(_mesasDtoEjemplo.Take(5).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se loggea la información inicial
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo mesas disponibles")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar que se loggea el resultado exitoso
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se encontraron")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesasSinFiltros_DeberiLoggearSinFiltros()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns(_mesasDtoEjemplo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se loggea información sin filtros específicos
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Todas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 15)]
    public async Task Handle_ConDiferentesPaginaciones_DeberiaFuncionarCorrectamente(int pagina, int tamanoPagina)
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica(pagina, tamanoPagina);

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        var mesasEsperadas = _mesasDisponiblesEjemplo
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.PageNumber.Should().Be(pagina);
        result.Value.PageSize.Should().Be(tamanoPagina);
        result.Value.Items.Should().HaveCount(c => c <= tamanoPagina);
        result.Value.TotalCount.Should().Be(_mesasDisponiblesEjemplo.Count);
    }

    [Theory]
    [InlineData("Interior")]
    [InlineData("Terraza")]
    [InlineData("VIP")]
    [InlineData("Barra")]
    public async Task Handle_ConDiferentesZonas_DeberiaFiltrarCorrectamente(string zona)
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.PorZona(zona);
        var mesasEsperadas = _mesasDisponiblesEjemplo
            .Where(m => m.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().OnlyContain(dto => dto.Ubicacion.Equals(zona, StringComparison.OrdinalIgnoreCase));
        result.Value.TotalCount.Should().Be(mesasEsperadas.Count);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public async Task Handle_ConDiferentesCapacidades_DeberiaFiltrarCorrectamente(int capacidadMinima)
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.ConCapacidad(capacidadMinima);
        var mesasEsperadas = _mesasDisponiblesEjemplo
            .Where(m => m.Capacidad >= capacidadMinima)
            .ToList();

        _mockRepository.Setup(r => r.ObtenerMesasDisponiblesAsync())
                      .ReturnsAsync(_mesasDisponiblesEjemplo);

        _mockMapper.Setup(m => m.Map<List<MesaDto>>(It.IsAny<List<Mesa>>()))
                   .Returns((List<Mesa> mesas) => 
                       _mesasDtoEjemplo.Where(dto => mesas.Any(m => m.Numero == dto.Numero)).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().OnlyContain(dto => dto.Capacidad >= capacidadMinima);
        result.Value.TotalCount.Should().Be(mesasEsperadas.Count);
    }

    #region Helper Methods

    private List<Mesa> CrearMesasDisponiblesEjemplo()
    {
        return new List<Mesa>
        {
            Mesa.Crear(1, 4, "Interior"),   // Disponible por defecto
            Mesa.Crear(2, 6, "Terraza"),    // Disponible por defecto
            Mesa.Crear(3, 8, "VIP"),        // Disponible por defecto
            Mesa.Crear(4, 2, "Barra"),      // Disponible por defecto
            Mesa.Crear(5, 4, "Interior"),   // Disponible por defecto
            Mesa.Crear(6, 6, "Terraza"),    // Disponible por defecto
            Mesa.Crear(7, 8, "VIP"),        // Disponible por defecto
            Mesa.Crear(8, 2, "Barra"),      // Disponible por defecto
            Mesa.Crear(9, 4, "Interior"),   // Disponible por defecto
            Mesa.Crear(10, 6, "Terraza"),   // Disponible por defecto
            Mesa.Crear(11, 8, "VIP"),       // Disponible por defecto
            Mesa.Crear(12, 2, "Barra"),     // Disponible por defecto
            Mesa.Crear(13, 4, "Interior"),  // Disponible por defecto
            Mesa.Crear(14, 6, "Terraza"),   // Disponible por defecto
            Mesa.Crear(15, 10, "VIP")       // Disponible por defecto
        };
    }

    private List<MesaDto> CrearMesasDtoEjemplo()
    {
        return new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = 1, Capacidad = 4, Ubicacion = "Interior", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 2, Capacidad = 6, Ubicacion = "Terraza", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 3, Capacidad = 8, Ubicacion = "VIP", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 4, Capacidad = 2, Ubicacion = "Barra", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 5, Capacidad = 4, Ubicacion = "Interior", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 6, Capacidad = 6, Ubicacion = "Terraza", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 7, Capacidad = 8, Ubicacion = "VIP", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 8, Capacidad = 2, Ubicacion = "Barra", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 9, Capacidad = 4, Ubicacion = "Interior", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 10, Capacidad = 6, Ubicacion = "Terraza", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 11, Capacidad = 8, Ubicacion = "VIP", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 12, Capacidad = 2, Ubicacion = "Barra", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 13, Capacidad = 4, Ubicacion = "Interior", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 14, Capacidad = 6, Ubicacion = "Terraza", Estado = EstadoMesa.Disponible },
            new MesaDto { Id = Guid.NewGuid(), Numero = 15, Capacidad = 10, Ubicacion = "VIP", Estado = EstadoMesa.Disponible }
        };
    }

    #endregion
} 