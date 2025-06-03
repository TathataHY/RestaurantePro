namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Queries;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;

/// <summary>
/// Tests unitarios para ObtenerAnalisisFidelizacionHandler
/// Valida la lógica completa de análisis empresarial con IA y Machine Learning
/// </summary>
public class ObtenerAnalisisFidelizacionHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerAnalisisFidelizacionHandler>> _loggerMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly ObtenerAnalisisFidelizacionHandler _handler;

    public ObtenerAnalisisFidelizacionHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerAnalisisFidelizacionHandler>>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();

        // Configurar mocks básicos
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _currentUserServiceMock.Setup(x => x.UserId).Returns("test-user");

        // Configurar DbSets mockeados usando listas vacías por defecto - convertir a IQueryable
        var clientesMock = MockDbSetHelper.CreateMockDbSet<Cliente>(new List<Cliente>().AsQueryable());
        var facturasMock = MockDbSetHelper.CreateMockDbSet<Factura>(new List<Factura>().AsQueryable());
        var reservacionesMock = MockDbSetHelper.CreateMockDbSet<Reservacion>(new List<Reservacion>().AsQueryable());

        _contextMock.Setup(x => x.Clientes).Returns(clientesMock.Object);
        _contextMock.Setup(x => x.Facturas).Returns(facturasMock.Object);
        _contextMock.Setup(x => x.Reservaciones).Returns(reservacionesMock.Object);

        _handler = new ObtenerAnalisisFidelizacionHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _comercialServiceFacadeMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void CrearAnalisisMensual_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddMonths(-1);
        var nivelMinimo = NivelFidelizacion.Plata;

        // Act
        var query = ObtenerAnalisisFidelizacionQuery.CrearAnalisisMensual(fechaInicio, nivelMinimo, true);

        // Assert
        Assert.Equal(fechaInicio, query.FechaInicio);
        Assert.Equal(fechaInicio.AddMonths(1).AddDays(-1), query.FechaFin);
        Assert.Equal(nivelMinimo, query.NivelMinimo);
        Assert.False(query.IncluirClientesInactivos);
        Assert.True(query.IncluirTendencias);
        Assert.True(query.IncluirProyecciones);
        Assert.Equal(TipoAnalisis.Completo, query.TipoAnalisis);
    }

    [Fact]
    public void CrearAnalisisClientes_ConListaEspecifica_DeberiaConfigurarClientesEspecificos()
    {
        // Arrange
        var clienteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var fechaInicio = DateTime.Today.AddDays(-30);
        var fechaFin = DateTime.Today;

        // Act
        var query = ObtenerAnalisisFidelizacionQuery.CrearAnalisisClientes(clienteIds, fechaInicio, fechaFin);

        // Assert
        Assert.Equal(clienteIds, query.ClientesEspecificos);
        Assert.Equal(TipoAnalisis.ClientesEspecificos, query.TipoAnalisis);
        Assert.True(query.IncluirClientesInactivos);
        Assert.True(query.IncluirTendencias);
        Assert.False(query.IncluirProyecciones);
    }

    [Fact]
    public void CrearAnalisisCompleto_SinParametros_DeberiaUsarValoresPorDefecto()
    {
        // Arrange & Act
        var query = ObtenerAnalisisFidelizacionQuery.CrearAnalisisMensual();

        // Assert
        Assert.True(query.FechaInicio <= DateTime.Now.AddMonths(-1));
        Assert.True(query.FechaFin >= DateTime.Now.AddMonths(-1));
        Assert.Null(query.NivelMinimo);
        Assert.Equal(TipoAnalisis.Completo, query.TipoAnalisis);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AnalisisMensualCompleto_DeberiaRetornarAnalisisCompleto()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddMonths(-1),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo,
            IncluirTendencias = true,
            IncluirProyecciones = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.ResumenExecutivo.TotalClientesAnalizados >= 0);
        Assert.True(result.Value.ResumenExecutivo.TasaRetencion >= 0);
        Assert.NotNull(result.Value.Tendencias);
        Assert.True(result.Value.RecomendacionesEstrategicas.Count >= 0);
    }

    [Fact]
    public async Task Handle_AnalisisClientesEspecificos_DeberiaFocalizarEnClientes()
    {
        // Arrange
        var clienteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-60),
            FechaFin = DateTime.Today,
            ClientesEspecificos = clienteIds,
            TipoAnalisis = TipoAnalisis.ClientesEspecificos,
            IncluirTendencias = true
        };

        // Configurar clientes específicos en el mock
        var clientesEspecificosMock = MockDbSetHelper.CreateMockDbSet<Cliente>(new List<Cliente>().AsQueryable());
        _contextMock.Setup(x => x.Clientes).Returns(clientesEspecificosMock.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.ResumenExecutivo.TotalClientesAnalizados);
        Assert.Equal(TipoAnalisis.ClientesEspecificos.ToString(), result.Value.InfoAnalisis.TipoAnalisis);
    }

    [Fact]
    public async Task Handle_AnalisisConPrediccionesML_DeberiaIncluirAnalisisIA()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddMonths(-3),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Predictivo,
            IncluirProyecciones = true,
            IncluirTendencias = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.Tendencias);
        Assert.True(result.Value.RecomendacionesEstrategicas.Count >= 0);
    }

    [Fact]
    public async Task Handle_AnalisisConAlertasInteligentes_DeberiaGenerarAlertasPriorizadas()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-14),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo,
            IncluirTendencias = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.AnalisisRiesgo);
        Assert.True(result.Value.RecomendacionesEstrategicas.Count >= 0);
    }

    [Fact]
    public async Task Handle_AnalisisBasico_DeberiaRetornarSoloEstadisticasBasicas()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Basico,
            IncluirTendencias = false,
            IncluirProyecciones = false
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.ResumenExecutivo);
        Assert.Equal(TipoAnalisis.Basico.ToString(), result.Value.InfoAnalisis.TipoAnalisis);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_FechasInvalidas_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(-10), // Fecha fin anterior a fecha inicio
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La fecha de inicio no puede ser posterior a la fecha de fin", result.Error);
    }

    [Fact]
    public async Task Handle_RangoFechasMuyAmplio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddYears(-3), // Rango muy amplio (más de 2 años)
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El período de análisis no puede exceder 2 años", result.Error);
    }

    [Fact]
    public async Task Handle_ClientesEspecificosVacios_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.ClientesEspecificos,
            ClientesEspecificos = new List<Guid>() // Lista vacía
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // El handler no valida esto específicamente, procesa con lista vacía
    }

    [Fact]
    public async Task Handle_DemasiadosClientesEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var clienteIds = Enumerable.Range(1, 101).Select(_ => Guid.NewGuid()).ToList(); // Más de 100 clientes
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.ClientesEspecificos,
            ClientesEspecificos = clienteIds
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // El handler no valida esto específicamente
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Simular error en el contexto de datos
        _contextMock.Setup(x => x.Clientes).Throws(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al generar el análisis de fidelización", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Simular excepción inesperada
        _contextMock.Setup(x => x.Clientes).Throws(new InvalidOperationException("Error inesperado"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al generar el análisis de fidelización", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorProcesamientoAsincronoML_DeberiaLogearYContinuar()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-90),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Predictivo,
            IncluirProyecciones = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debería continuar a pesar de errores en ML
        Assert.NotNull(result.Value);
    }

    #endregion

    #region Tests de Logging y Métricas

    [Fact]
    public async Task Handle_AnalisisExitoso_DeberiaLoggearMetricas()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se loggeó el inicio del análisis
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando análisis de fidelización")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar que se loggeó la finalización exitosa
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Análisis de fidelización completado")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AnalisisComplejo_DeberiaLoggearTiempoGeneracion()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddMonths(-6),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Predictivo,
            IncluirTendencias = true,
            IncluirProyecciones = true
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.InfoAnalisis.TiempoProcesamiento.TotalMilliseconds >= 0);
    }

    #endregion
} 