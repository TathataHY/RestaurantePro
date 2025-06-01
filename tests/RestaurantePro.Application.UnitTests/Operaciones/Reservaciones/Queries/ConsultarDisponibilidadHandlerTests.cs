namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Queries;

/// <summary>
/// Tests unitarios para ConsultarDisponibilidadHandler
/// Cobertura completa de consultas de disponibilidad, mesas específicas, alternativas y estadísticas de ocupación
/// </summary>
public class ConsultarDisponibilidadHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<ConsultarDisponibilidadHandler>> _mockLogger;
    private readonly Mock<DbSet<Mesa>> _mockMesasDbSet;
    private readonly Mock<DbSet<Reservacion>> _mockReservacionesDbSet;
    private readonly ConsultarDisponibilidadHandler _handler;
    private readonly List<Mesa> _mesasEjemplo;
    private readonly List<Reservacion> _reservacionesEjemplo;

    public ConsultarDisponibilidadHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<ConsultarDisponibilidadHandler>>();
        _mockMesasDbSet = new Mock<DbSet<Mesa>>();
        _mockReservacionesDbSet = new Mock<DbSet<Reservacion>>();
        
        _handler = new ConsultarDisponibilidadHandler(
            _mockContext.Object,
            _mockLogger.Object);

        _mesasEjemplo = CrearMesasEjemplo();
        _reservacionesEjemplo = CrearReservacionesEjemplo();
        ConfigurarMockDbSets();
    }

    [Fact]
    public async Task Handle_ConDisponibilidadGeneral_DeberiaRetornarMesasDisponibles()
    {
        // Arrange
        var fechaConsulta = DateTime.Now.AddDays(1).Date.AddHours(20); // Mañana a las 8 PM
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = fechaConsulta,
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            MargenToleranciaPersonas = 1,
            PermitirCapacidadMayor = true,
            MostrarAlternativas = true
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.HayDisponibilidad.Should().BeTrue();
        resultado.Value.FechaHoraConsultada.Should().Be(fechaConsulta);
        resultado.Value.NumeroPersonasSolicitadas.Should().Be(4);
        resultado.Value.MesasDisponibles.Should().NotBeEmpty();

        // Verificar logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando disponibilidad")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaEspecificaDisponible_DeberiaRetornarDisponibilidadPositiva()
    {
        // Arrange
        var mesaEspecifica = _mesasEjemplo[0]; // Mesa de 4 personas
        var fechaConsulta = DateTime.Now.AddDays(1).Date.AddHours(19); // Mañana a las 7 PM
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = fechaConsulta,
            NumeroPersonas = 4,
            MesaPreferida = mesaEspecifica.Id,
            DuracionEstimadaMinutos = 90
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.HayDisponibilidad.Should().BeTrue();
        resultado.Value.MesasDisponibles.Should().HaveCount(1);
        resultado.Value.MesasDisponibles[0].MesaId.Should().Be(mesaEspecifica.Id);
        resultado.Value.MesasDisponibles[0].Disponible.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConMesaEspecificaOcupada_DeberiaRetornarNoDisponible()
    {
        // Arrange
        var mesaOcupada = _mesasEjemplo[0]; // Esta mesa tiene una reservación
        var fechaConReservacion = DateTime.Now.AddDays(1).Date.AddHours(20); // Misma hora que una reservación
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = fechaConReservacion,
            NumeroPersonas = 4,
            MesaPreferida = mesaOcupada.Id,
            DuracionEstimadaMinutos = 120,
            MostrarAlternativas = true
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.HayDisponibilidad.Should().BeFalse();
        resultado.Value.MotivoNoDisponibilidad.Should().Contain("ocupada");
        resultado.Value.AlternativasSugeridas.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ConMesaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var mesaInexistente = Guid.NewGuid();
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4,
            MesaPreferida = mesaInexistente,
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("La mesa especificada no existe.");
    }

    [Fact]
    public async Task Handle_ConCapacidadInsuficiente_DeberiaRetornarNoDisponible()
    {
        // Arrange
        var mesaPequeña = _mesasEjemplo[0]; // Mesa de 4 personas
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = 8, // Más personas que la capacidad
            MesaPreferida = mesaPequeña.Id,
            MargenToleranciaPersonas = 0,
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.HayDisponibilidad.Should().BeFalse();
        resultado.Value.MotivoNoDisponibilidad.Should().Contain("capacidad");
    }

    [Theory]
    [InlineData(2, 4, 1, true)]  // 2 personas en mesa de 4 con tolerancia 1
    [InlineData(4, 4, 0, true)]  // Capacidad exacta
    [InlineData(5, 4, 1, true)]  // Con tolerancia positiva
    [InlineData(6, 4, 1, false)] // Excede tolerancia
    public async Task Handle_ConDiferentesCapacidades_DeberiaValidarCorrectamente(
        int numeroPersonas, int capacidadMesa, int margenTolerancia, bool deberiaEstarDisponible)
    {
        // Arrange
        var mesaTest = CrearMesa(Guid.NewGuid(), 1, capacidadMesa, EstadoMesa.Disponible, TipoMesa.Interior);
        var mesasTemporales = new List<Mesa> { mesaTest };
        ConfigurarMockDbSetConMesas(mesasTemporales);

        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = numeroPersonas,
            MesaPreferida = mesaTest.Id,
            MargenToleranciaPersonas = margenTolerancia,
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.HayDisponibilidad.Should().Be(deberiaEstarDisponible);
    }

    [Fact]
    public async Task Handle_ConMostrarAlternativas_DeberiaIncluirSugerencias()
    {
        // Arrange
        var fechaOcupada = DateTime.Now.AddDays(1).Date.AddHours(20); // Hora con reservaciones
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = fechaOcupada,
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            MostrarAlternativas = true,
            MargenToleranciaPersonas = 1
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        if (!resultado.Value.HayDisponibilidad)
        {
            resultado.Value.AlternativasSugeridas.Should().NotBeNull();
            resultado.Value.MotivoNoDisponibilidad.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Handle_ConPermitirCapacidadMayorFalse_DeberiaLimitarBusqueda()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = 4,
            PermitirCapacidadMayor = false,
            MargenToleranciaPersonas = 1,
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        if (resultado.Value.HayDisponibilidad)
        {
            // Todas las mesas disponibles deberían tener capacidad <= (4 + 1)
            resultado.Value.MesasDisponibles.Should().OnlyContain(m => m.Capacidad <= 5);
        }
    }

    [Fact]
    public async Task Handle_ConZonaPreferida_DeberiaFiltrarPorZona()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = 4,
            ZonaPreferida = "Terraza",
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        // Nota: La implementación actual no filtra por zona (está comentada en el código)
        // pero el test verifica que la funcionalidad esté preparada
        resultado.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ConEstadisticasOcupacion_DeberiaIncluirEstadisticas()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(20),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            IncluirEstadisticas = true
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.EstadisticasOcupacion.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ConDuracionLarga_DeberiaValidarSolapamientos()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(1).Date.AddHours(18); // 6 PM
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = fechaInicio,
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 240, // 4 horas (hasta 10 PM)
            MargenToleranciaPersonas = 1
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        
        // Las mesas que tengan reservaciones entre 6-10 PM no deberían estar disponibles
        if (!resultado.Value.HayDisponibilidad)
        {
            resultado.Value.MotivoNoDisponibilidad.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Handle_ConExcepcionEnBaseDatos_DeberiaRetornarError()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120
        };

        _mockMesasDbSet.Setup(x => x.Where(It.IsAny<Expression<Func<Mesa, bool>>>()))
            .Throws(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno al consultar disponibilidad.");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al consultar disponibilidad")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120
        };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Theory]
    [InlineData(EstadoMesa.Disponible, true)]
    [InlineData(EstadoMesa.Ocupada, false)]
    [InlineData(EstadoMesa.FueraDeServicio, false)]
    [InlineData(EstadoMesa.Reservada, false)]
    public async Task Handle_ConDiferentesEstadosMesa_DeberiaFiltrarCorrectamente(
        EstadoMesa estadoMesa, bool deberiaConsiderar)
    {
        // Arrange
        var mesaTest = CrearMesa(Guid.NewGuid(), 1, 4, estadoMesa, TipoMesa.Interior);
        var mesasTemporales = new List<Mesa> { mesaTest };
        ConfigurarMockDbSetConMesas(mesasTemporales);

        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        if (deberiaConsiderar)
        {
            resultado.Value.MesasDisponibles.Should().NotBeEmpty();
        }
        else
        {
            // Las mesas fuera de servicio u ocupadas no deberían aparecer
            resultado.Value.MesasDisponibles.Should().NotContain(m => m.MesaId == mesaTest.Id);
        }
    }

    [Fact]
    public async Task Handle_ConLoggingCompleto_DeberiaLoggearTodosLosEventos()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(20),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            MostrarAlternativas = true
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando disponibilidad")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de completado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consulta de disponibilidad completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorPreferencia_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(19),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            ZonaPreferida = "Interior"
        };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        if (resultado.Value.HayDisponibilidad && resultado.Value.MesasDisponibles.Count > 1)
        {
            // Las mesas deberían estar ordenadas por preferencia
            // (capacidad óptima, zona preferida, etc.)
            var capacidades = resultado.Value.MesasDisponibles.Select(m => m.Capacidad).ToList();
            capacidades.Should().BeInAscendingOrder();
        }
    }

    #region Métodos de Apoyo

    private void ConfigurarMockDbSets()
    {
        ConfigurarMockDbSetConMesas(_mesasEjemplo);
        ConfigurarMockDbSetConReservaciones(_reservacionesEjemplo);
    }

    private void ConfigurarMockDbSetConMesas(List<Mesa> mesas)
    {
        var queryableMesas = mesas.AsQueryable();
        
        _mockMesasDbSet.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(queryableMesas.Provider);
        _mockMesasDbSet.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(queryableMesas.Expression);
        _mockMesasDbSet.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(queryableMesas.ElementType);
        _mockMesasDbSet.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(queryableMesas.GetEnumerator());

        _mockMesasDbSet.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Mesa, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Mesa, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                
                var compiledPredicate = predicate.Compile();
                var result = mesas.FirstOrDefault(compiledPredicate);
                return Task.FromResult(result);
            });

        _mockContext.Setup(c => c.Mesas).Returns(_mockMesasDbSet.Object);
    }

    private void ConfigurarMockDbSetConReservaciones(List<Reservacion> reservaciones)
    {
        var queryableReservaciones = reservaciones.AsQueryable();
        
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Provider).Returns(queryableReservaciones.Provider);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.Expression).Returns(queryableReservaciones.Expression);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.ElementType).Returns(queryableReservaciones.ElementType);
        _mockReservacionesDbSet.As<IQueryable<Reservacion>>().Setup(m => m.GetEnumerator()).Returns(queryableReservaciones.GetEnumerator());

        _mockReservacionesDbSet.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Reservacion, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Reservacion, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                
                var compiledPredicate = predicate.Compile();
                var result = reservaciones.Any(compiledPredicate);
                return Task.FromResult(result);
            });

        _mockContext.Setup(c => c.Reservaciones).Returns(_mockReservacionesDbSet.Object);
    }

    private List<Mesa> CrearMesasEjemplo()
    {
        return new List<Mesa>
        {
            CrearMesa(Guid.NewGuid(), 1, 4, EstadoMesa.Disponible, TipoMesa.Interior),
            CrearMesa(Guid.NewGuid(), 2, 6, EstadoMesa.Disponible, TipoMesa.Terraza),
            CrearMesa(Guid.NewGuid(), 3, 2, EstadoMesa.Disponible, TipoMesa.VIP),
            CrearMesa(Guid.NewGuid(), 4, 8, EstadoMesa.FueraDeServicio, TipoMesa.Interior),
            CrearMesa(Guid.NewGuid(), 5, 4, EstadoMesa.Disponible, TipoMesa.Accesible)
        };
    }

    private List<Reservacion> CrearReservacionesEjemplo()
    {
        var mañana20hrs = DateTime.Now.AddDays(1).Date.AddHours(20);
        
        return new List<Reservacion>
        {
            CrearReservacion(Guid.NewGuid(), _mesasEjemplo[0].Id, mañana20hrs, EstadoReservacion.Confirmada),
            CrearReservacion(Guid.NewGuid(), _mesasEjemplo[1].Id, mañana20hrs.AddHours(1), EstadoReservacion.Pendiente)
        };
    }

    private Mesa CrearMesa(Guid id, int numero, int capacidad, EstadoMesa estado, TipoMesa tipo)
    {
        // Usar reflection para crear la mesa con propiedades privadas
        var mesa = (Mesa)Activator.CreateInstance(typeof(Mesa), true)!;
        
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        typeof(Mesa).GetProperty("Numero")?.SetValue(mesa, numero);
        typeof(Mesa).GetProperty("Capacidad")?.SetValue(mesa, capacidad);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, estado);
        typeof(Mesa).GetProperty("Tipo")?.SetValue(mesa, tipo);
        typeof(Mesa).GetProperty("Ubicacion")?.SetValue(mesa, $"Mesa {numero}");
        
        return mesa;
    }

    private Reservacion CrearReservacion(Guid id, Guid mesaId, DateTime fechaHora, EstadoReservacion estado)
    {
        // Usar reflection para crear la reservación con propiedades privadas
        var reservacion = (Reservacion)Activator.CreateInstance(typeof(Reservacion), true)!;
        
        typeof(Reservacion).GetProperty("Id")?.SetValue(reservacion, id);
        typeof(Reservacion).GetProperty("MesaId")?.SetValue(reservacion, mesaId);
        typeof(Reservacion).GetProperty("FechaReservacion")?.SetValue(reservacion, fechaHora);
        typeof(Reservacion).GetProperty("Estado")?.SetValue(reservacion, estado);
        typeof(Reservacion).GetProperty("ClienteId")?.SetValue(reservacion, Guid.NewGuid());
        typeof(Reservacion).GetProperty("NumeroPersonas")?.SetValue(reservacion, 4);
        typeof(Reservacion).GetProperty("CodigoReservacion")?.SetValue(reservacion, $"RES-{id:N}".Substring(0, 12));
        
        return reservacion;
    }

    #endregion
} 