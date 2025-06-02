namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Queries;

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

        var analisisCompleto = CreateMockAnalisisCompletoML();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisCompleto));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(1250, result.Value.EstadisticasGenerales.TotalClientes);
        Assert.Equal(85.5m, result.Value.EstadisticasGenerales.TasaRetencion);
        Assert.Equal(4, result.Value.AnalisisPorNivel.Count);
        Assert.NotNull(result.Value.Tendencias);
        Assert.NotNull(result.Value.Proyecciones);
        Assert.True(result.Value.Alertas.Count > 0);
        Assert.True(result.Value.Recomendaciones.Count > 0);
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

        var analisisEspecifico = CreateMockAnalisisClientesEspecificos(clienteIds);
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisEspecifico));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.EstadisticasGenerales.TotalClientes);
        Assert.True(result.Value.ClientesTop.All(c => clienteIds.Contains(c.ClienteId)));
        Assert.Equal(TipoAnalisis.ClientesEspecificos, result.Value.TipoAnalisis);
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

        var analisisIA = CreateMockAnalisisConIA();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisIA));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.Proyecciones);
        Assert.Equal("Alta", result.Value.Proyecciones.ConfiabilidadProyeccion);
        Assert.True(result.Value.Proyecciones.ClientesProyectados3Meses > 0);
        Assert.True(result.Value.Proyecciones.VentasProyectadas3Meses > 0);
        Assert.Equal(3, result.Value.Proyecciones.ProyeccionMensual.Count);
        Assert.Contains("Machine Learning", result.Value.Recomendaciones.Select(r => r.Descripcion).FirstOrDefault() ?? "");
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

        var analisisConAlertas = CreateMockAnalisisConAlertas();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisConAlertas));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Value.Alertas.Count);
        Assert.Contains(result.Value.Alertas, a => a.Tipo == TipoAlerta.ClientesInactivos && a.Prioridad == NivelPrioridad.Alta);
        Assert.Contains(result.Value.Alertas, a => a.Tipo == TipoAlerta.BajaTasaCanje && a.Prioridad == NivelPrioridad.Media);
        Assert.Contains(result.Value.Alertas, a => a.Tipo == TipoAlerta.TendenciaNegativa && a.Prioridad == NivelPrioridad.Critica);
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

        var analisisBasico = CreateMockAnalisisBasico();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisBasico));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.EstadisticasGenerales);
        Assert.Null(result.Value.Tendencias);
        Assert.Null(result.Value.Proyecciones);
        Assert.Equal(TipoAnalisis.Basico, result.Value.TipoAnalisis);
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
        Assert.Contains("La fecha de fin debe ser posterior a la fecha de inicio", result.Error);
    }

    [Fact]
    public async Task Handle_RangoFechasMuyAmplio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddYears(-2), // Rango muy amplio
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El rango de fechas no puede superar los 365 días", result.Error);
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
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar al menos un cliente para análisis específico", result.Error);
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
        Assert.False(result.Succeeded);
        Assert.Contains("No se pueden analizar más de 100 clientes específicos", result.Error);
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

        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AnalisisFidelizacionResult>("Error en análisis de machine learning"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en análisis de machine learning", result.Error);
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

        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad con servicio de IA"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorProcesamientoAsincronoML_DeberiaLogearYContinuar()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Predictivo,
            IncluirProyecciones = true
        };

        var analisisCompleto = CreateMockAnalisisCompletoML();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisCompleto));
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("Error en job ML"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debe continuar exitosamente
        
        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al programar análisis avanzado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging y Performance

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

        var analisis = CreateMockAnalisisCompletoML();
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisis));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando análisis de fidelización")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de métricas
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Análisis completado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AnalisisComplejo_DeberiaLoggearTiempoGeneracion()
    {
        // Arrange
        var query = new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Today.AddMonths(-3),
            FechaFin = DateTime.Today,
            TipoAnalisis = TipoAnalisis.Predictivo,
            IncluirProyecciones = true
        };

        var analisis = CreateMockAnalisisCompletoML();
        analisis.TiempoGeneracion = TimeSpan.FromMinutes(2.5); // Análisis complejo que toma tiempo
        
        _comercialServiceFacadeMock.Setup(x => x.GenerarAnalisisFidelizacionAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisis));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se loggeó el tiempo de generación
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tiempo de generación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static AnalisisFidelizacionResult CreateMockAnalisisCompletoML()
    {
        return new AnalisisFidelizacionResult
        {
            EstadisticasGenerales = new EstadisticasGenerales
            {
                TotalClientes = 1250,
                ClientesActivos = 1068,
                ClientesInactivos = 182,
                TasaRetencion = 85.5m,
                ValorVidaPromedio = 2875.50m,
                PuntosAcumuladosTotal = 458900,
                PuntosCanjeadosTotal = 127650,
                TasaCanjePromedio = 27.8m,
                TicketPromedioClienteFiel = 45.75m,
                IncrementoVentasVsFrecuente = 23.5m
            },
            AnalisisPorNivel = new List<AnalisisNivel>
            {
                new() { Nivel = NivelFidelizacion.Basico, CantidadClientes = 650, PorcentajeTotalClientes = 52.0m },
                new() { Nivel = NivelFidelizacion.Plata, CantidadClientes = 380, PorcentajeTotalClientes = 30.4m },
                new() { Nivel = NivelFidelizacion.Oro, CantidadClientes = 180, PorcentajeTotalClientes = 14.4m },
                new() { Nivel = NivelFidelizacion.Platino, CantidadClientes = 40, PorcentajeTotalClientes = 3.2m }
            },
            ClientesTop = new List<ClienteTop>
            {
                new() { ClienteId = Guid.NewGuid(), NombreCompleto = "Juan Pérez", PuntosAcumulados = 15800, VentasTotales = 4250.00m },
                new() { ClienteId = Guid.NewGuid(), NombreCompleto = "María González", PuntosAcumulados = 12400, VentasTotales = 3675.50m }
            },
            Tendencias = new TendenciasFidelizacion
            {
                EvolucionClientes = new List<PuntoTendencia>
                {
                    new() { Fecha = DateTime.Today.AddDays(-30), Valor = 1200, Etiqueta = "Hace 30 días" },
                    new() { Fecha = DateTime.Today.AddDays(-15), Valor = 1250, Etiqueta = "Hace 15 días" },
                    new() { Fecha = DateTime.Today, Valor = 1068, Etiqueta = "Hoy" }
                },
                EvolucionPuntos = new List<PuntoTendencia>
                {
                    new() { Fecha = DateTime.Today.AddDays(-30), Valor = 420000, Etiqueta = "Hace 30 días" },
                    new() { Fecha = DateTime.Today.AddDays(-15), Valor = 440000, Etiqueta = "Hace 15 días" },
                    new() { Fecha = DateTime.Today, Valor = 458900, Etiqueta = "Hoy" }
                },
                EvolucionCanjes = new List<PuntoTendencia>
                {
                    new() { Fecha = DateTime.Today.AddDays(-30), Valor = 110000, Etiqueta = "Hace 30 días" },
                    new() { Fecha = DateTime.Today.AddDays(-15), Valor = 120000, Etiqueta = "Hace 15 días" },
                    new() { Fecha = DateTime.Today, Valor = 127650, Etiqueta = "Hoy" }
                },
                CambiosNiveles = new List<CambioNivel>
                {
                    new() { Fecha = DateTime.Today.AddDays(-15), NivelAnterior = NivelFidelizacion.Plata, NivelNuevo = NivelFidelizacion.Oro, CantidadClientes = 25 }
                },
                CrecimientoMensual = 8.5m,
                TendenciaGeneral = "Positiva"
            },
            Proyecciones = new ProyeccionesFidelizacion
            {
                ProyeccionMensual = new List<ProyeccionMes>
                {
                    new() { Mes = DateTime.Today.AddMonths(1), ClientesProyectados = 1290, VentasProyectadas = 58750.00m, PuntosProyectados = 48500 },
                    new() { Mes = DateTime.Today.AddMonths(2), ClientesProyectados = 1315, VentasProyectadas = 61200.00m, PuntosProyectados = 51200 },
                    new() { Mes = DateTime.Today.AddMonths(3), ClientesProyectados = 1340, VentasProyectadas = 63800.00m, PuntosProyectados = 54100 }
                },
                ClientesProyectados3Meses = 1340,
                VentasProyectadas3Meses = 183750.00m,
                ConfiabilidadProyeccion = "Alta"
            },
            Alertas = new List<AlertaFidelizacion>
            {
                new() { Tipo = TipoAlerta.ClientesInactivos, Titulo = "Clientes inactivos creciendo", Prioridad = NivelPrioridad.Alta },
                new() { Tipo = TipoAlerta.BajaTasaCanje, Titulo = "Baja en tasa de canje", Prioridad = NivelPrioridad.Media }
            },
            Recomendaciones = new List<RecomendacionFidelizacion>
            {
                new() { Titulo = "Campaña reactivación", Descripcion = "Machine Learning detectó patrón de inactividad", Categoria = "Retención", Prioridad = NivelPrioridad.Alta },
                new() { Titulo = "Optimizar programa puntos", Descripcion = "IA sugiere ajuste en equivalencias", Categoria = "Monetización", Prioridad = NivelPrioridad.Media }
            },
            TipoAnalisis = TipoAnalisis.Completo,
            TiempoGeneracion = TimeSpan.FromMinutes(1.5),
            VersionAnalisis = "2.1"
        };
    }

    private static AnalisisFidelizacionResult CreateMockAnalisisClientesEspecificos(List<Guid> clienteIds)
    {
        return new AnalisisFidelizacionResult
        {
            EstadisticasGenerales = new EstadisticasGenerales
            {
                TotalClientes = clienteIds.Count,
                ClientesActivos = clienteIds.Count,
                TasaRetencion = 92.0m,
                ValorVidaPromedio = 4250.75m
            },
            ClientesTop = clienteIds.Select(id => new ClienteTop 
            { 
                ClienteId = id, 
                NombreCompleto = $"Cliente {id.ToString()[..8]}", 
                PuntosAcumulados = Random.Shared.Next(5000, 20000),
                VentasTotales = Random.Shared.Next(2000, 8000)
            }).ToList(),
            TipoAnalisis = TipoAnalisis.ClientesEspecificos,
            Alertas = new List<AlertaFidelizacion>(),
            Recomendaciones = new List<RecomendacionFidelizacion>()
        };
    }

    private static AnalisisFidelizacionResult CreateMockAnalisisConIA()
    {
        var analisis = CreateMockAnalisisCompletoML();
        analisis.TipoAnalisis = TipoAnalisis.Predictivo;
        analisis.Proyecciones!.ConfiabilidadProyeccion = "Alta";
        analisis.Recomendaciones.Add(new RecomendacionFidelizacion
        {
            Titulo = "Predicción ML: Oportunidad VIP",
            Descripcion = "Machine Learning identifica 45 clientes elegibles para upgrade a VIP",
            Categoria = "Acquisition",
            Prioridad = NivelPrioridad.Alta
        });
        return analisis;
    }

    private static AnalisisFidelizacionResult CreateMockAnalisisConAlertas()
    {
        return new AnalisisFidelizacionResult
        {
            EstadisticasGenerales = new EstadisticasGenerales
            {
                TotalClientes = 800,
                ClientesActivos = 650,
                TasaRetencion = 78.5m
            },
            Alertas = new List<AlertaFidelizacion>
            {
                new() { Tipo = TipoAlerta.ClientesInactivos, Prioridad = NivelPrioridad.Alta, Titulo = "Incremento clientes inactivos" },
                new() { Tipo = TipoAlerta.BajaTasaCanje, Prioridad = NivelPrioridad.Media, Titulo = "Disminución en canjes" },
                new() { Tipo = TipoAlerta.DisminucionNivel, Prioridad = NivelPrioridad.Media, Titulo = "Clientes bajando de nivel" },
                new() { Tipo = TipoAlerta.PuntosProximosVencer, Prioridad = NivelPrioridad.Baja, Titulo = "Puntos por vencer" },
                new() { Tipo = TipoAlerta.TendenciaNegativa, Prioridad = NivelPrioridad.Critica, Titulo = "Tendencia negativa detectada" }
            },
            Recomendaciones = new List<RecomendacionFidelizacion>(),
            TipoAnalisis = TipoAnalisis.Completo
        };
    }

    private static AnalisisFidelizacionResult CreateMockAnalisisBasico()
    {
        return new AnalisisFidelizacionResult
        {
            EstadisticasGenerales = new EstadisticasGenerales
            {
                TotalClientes = 450,
                ClientesActivos = 380,
                TasaRetencion = 84.4m,
                ValorVidaPromedio = 1850.25m
            },
            AnalisisPorNivel = new List<AnalisisNivel>
            {
                new() { Nivel = NivelFidelizacion.Basico, CantidadClientes = 280, PorcentajeTotalClientes = 62.2m },
                new() { Nivel = NivelFidelizacion.Plata, CantidadClientes = 120, PorcentajeTotalClientes = 26.7m }
            },
            ClientesTop = new List<ClienteTop>(),
            Tendencias = null,
            Proyecciones = null,
            Alertas = new List<AlertaFidelizacion>(),
            Recomendaciones = new List<RecomendacionFidelizacion>(),
            TipoAnalisis = TipoAnalisis.Basico
        };
    }

    #endregion
} 