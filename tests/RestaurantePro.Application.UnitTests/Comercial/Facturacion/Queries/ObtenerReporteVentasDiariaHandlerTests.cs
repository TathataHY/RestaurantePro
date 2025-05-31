namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Queries;

/// <summary>
/// Tests unitarios para ObtenerReporteVentasDiariaHandler
/// Valida la lógica completa de reportes diarios con business intelligence y analytics
/// </summary>
public class ObtenerReporteVentasDiariaHandlerTests
{
    private readonly Mock<IOperacionesServiceFacade> _operacionesServiceFacadeMock;
    private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<ILogger<ObtenerReporteVentasDiariaHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly ObtenerReporteVentasDiariaHandler _handler;

    public ObtenerReporteVentasDiariaHandlerTests()
    {
        _operacionesServiceFacadeMock = new Mock<IOperacionesServiceFacade>();
        _facturaRepositoryMock = new Mock<IFacturaRepository>();
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _loggerMock = new Mock<ILogger<ObtenerReporteVentasDiariaHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();

        _handler = new ObtenerReporteVentasDiariaHandler(
            _operacionesServiceFacadeMock.Object,
            _facturaRepositoryMock.Object,
            _comandaRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _backgroundJobServiceMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void CrearReporteHoy_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange & Act
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteHoy(true, true, NivelDetalle.Completo);

        // Assert
        Assert.Equal(DateTime.Today, query.FechaReporte);
        Assert.True(query.IncluirComparativoPeriodoAnterior);
        Assert.True(query.IncluirAnalisisPorMesa);
        Assert.True(query.IncluirAnalisisPorMesero);
        Assert.True(query.IncluirAnalisisProductos);
        Assert.True(query.IncluirTendenciasSemana);
        Assert.Equal(NivelDetalle.Completo, query.NivelDetalle);
    }

    [Fact]
    public void CrearReporteFecha_ConFechaEspecifica_DeberiaConfigurarFechaCorrectamente()
    {
        // Arrange
        var fechaEspecifica = DateTime.Today.AddDays(-5);

        // Act
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteFecha(fechaEspecifica, false, NivelDetalle.Intermedio);

        // Assert
        Assert.Equal(fechaEspecifica.Date, query.FechaReporte);
        Assert.False(query.IncluirComparativoPeriodoAnterior);
        Assert.Equal(NivelDetalle.Intermedio, query.NivelDetalle);
        Assert.False(query.IncluirTendenciasSemana);
    }

    [Fact]
    public void CrearReporteMeseros_ConListaMeseros_DeberiaEnfocarseEnMeseros()
    {
        // Arrange
        var meseroIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteMeseros(fecha, meseroIds, false);

        // Assert
        Assert.Equal(meseroIds, query.MeserosEspecificos);
        Assert.Equal(NivelDetalle.Meseros, query.NivelDetalle);
        Assert.False(query.IncluirAnalisisPorMesa);
        Assert.True(query.IncluirAnalisisPorMesero);
        Assert.False(query.IncluirAnalisisProductos);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ReporteCompletoExitoso_DeberiaRetornarAnalisisCompleto()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true
        };

        var reporteCompleto = CreateMockReporteCompletoBI();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteCompleto));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(45850.75m, result.Value.ResumenEjecutivo.VentasTotalDia);
        Assert.Equal(38.25m, result.Value.ResumenEjecutivo.TicketPromedio);
        Assert.Equal(120, result.Value.ResumenEjecutivo.TotalComandas);
        Assert.Equal(8, result.Value.VentasPorMesa?.Count);
        Assert.Equal(5, result.Value.VentasPorMesero?.Count);
        Assert.Equal(12, result.Value.VentasPorProducto?.Count);
        Assert.NotNull(result.Value.Comparativo);
        Assert.True(result.Value.Alertas.Count > 0);
    }

    [Fact]
    public async Task Handle_ReporteConComparativo_DeberiaIncluirAnalisisComparativo()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            IncluirComparativoPeriodoAnterior = true
        };

        var reporteConComparativo = CreateMockReporteConComparativo();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteConComparativo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.Comparativo);
        Assert.Equal(42300.50m, result.Value.Comparativo.VentasPeriodoAnterior);
        Assert.Equal(3550.25m, result.Value.Comparativo.CambioAbsoluto);
        Assert.Equal(8.4m, result.Value.Comparativo.CambioPorcentual);
        Assert.Equal("Positiva", result.Value.ResumenEjecutivo.TendenciaGeneral);
        Assert.True(result.Value.Comparativo.CambiosDetallados.Count > 0);
    }

    [Fact]
    public async Task Handle_ReporteMeserosEspecificos_DeberiaFocalizarEnMeseros()
    {
        // Arrange
        var meseroIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            MeserosEspecificos = meseroIds,
            NivelDetalle = NivelDetalle.Meseros,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisPorMesa = false
        };

        var reporteMeseros = CreateMockReporteMeserosEspecificos(meseroIds);
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteMeseros));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.VentasPorMesero?.Count);
        Assert.Null(result.Value.VentasPorMesa);
        Assert.True(result.Value.VentasPorMesero?.All(m => meseroIds.Contains(m.MeseroId)));
        Assert.Contains("Carlos Rodríguez", result.Value.VentasPorMesero?.Select(m => m.NombreMesero));
    }

    [Fact]
    public async Task Handle_ReporteConTendenciasSemana_DeberiaIncluirAnalisisTendencias()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            IncluirTendenciasSemana = true
        };

        var reporteConTendencias = CreateMockReporteConTendencias();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteConTendencias));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.TendenciasSemana);
        Assert.Equal(7, result.Value.TendenciasSemana.VentasPorDia.Count);
        Assert.True(result.Value.TendenciasSemana.TendenciaGeneral > 0);
        Assert.Equal("Lunes", result.Value.TendenciasSemana.MejorDiaSemana);
    }

    [Fact]
    public async Task Handle_ReporteBasico_DeberiaRetornarSoloResumenEjecutivo()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Basico,
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = false,
            IncluirAnalisisProductos = false
        };

        var reporteBasico = CreateMockReporteBasico();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteBasico));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.ResumenEjecutivo);
        Assert.Null(result.Value.VentasPorMesa);
        Assert.Null(result.Value.VentasPorMesero);
        Assert.Null(result.Value.VentasPorProducto);
        Assert.Equal(NivelDetalle.Basico, result.Value.NivelDetalle);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_FechaFutura_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddDays(5), // Fecha futura
            NivelDetalle = NivelDetalle.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La fecha del reporte no puede ser futura", result.Error);
    }

    [Fact]
    public async Task Handle_FechaMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddYears(-2), // Muy antigua
            NivelDetalle = NivelDetalle.Completo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La fecha del reporte no puede ser anterior a 365 días", result.Error);
    }

    [Fact]
    public async Task Handle_MeserosEspecificosSinMeseros_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Meseros,
            MeserosEspecificos = new List<Guid>() // Lista vacía
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar al menos un mesero para reporte específico", result.Error);
    }

    [Fact]
    public async Task Handle_DemasiadosMeserosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var meseroIds = Enumerable.Range(1, 51).Select(_ => Guid.NewGuid()).ToList(); // Más de 50 meseros
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Meseros,
            MeserosEspecificos = meseroIds
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se pueden analizar más de 50 meseros específicos", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioOperaciones_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo
        };

        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ReporteVentasDiariaResult>("Error en analytics de business intelligence"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en analytics de business intelligence", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo
        };

        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad con servicio de analytics"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorReporteAsincrono_DeberiaLogearYContinuar()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            IncluirTendenciasSemana = true
        };

        var reporteCompleto = CreateMockReporteCompletoBI();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteCompleto));
        
        _backgroundJobServiceMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<string>(), It.IsAny<object>()))
            .Throws(new Exception("Error en job de analytics"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debe continuar exitosamente
        
        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al programar reporte avanzado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Performance y Logging

    [Fact]
    public async Task Handle_ReporteExitoso_DeberiaLoggearMetricas()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo
        };

        var reporte = CreateMockReporteCompletoBI();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporte));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generando reporte de ventas diarias")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de métricas
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reporte generado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReporteComplejo_DeberiaLoggearTiempoGeneracion()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            IncluirTendenciasSemana = true,
            IncluirComparativoPeriodoAnterior = true
        };

        var reporte = CreateMockReporteCompletoBI();
        reporte.TiempoGeneracion = TimeSpan.FromMinutes(3.2); // Reporte complejo que toma tiempo
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporte));

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

    [Fact]
    public async Task Handle_ReporteConAlertas_DeberiaLoggearAlertas()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo
        };

        var reporteConAlertas = CreateMockReporteConAlertas();
        
        _operacionesServiceFacadeMock.Setup(x => x.GenerarReporteVentasDiariaAsync(
            It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(reporteConAlertas));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(4, result.Value.Alertas.Count);
        
        // Verificar que se loggearon las alertas críticas
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("alertas críticas detectadas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static ReporteVentasDiariaResult CreateMockReporteCompletoBI()
    {
        return new ReporteVentasDiariaResult
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Completo,
            TiempoGeneracion = TimeSpan.FromMinutes(1.8),
            ResumenEjecutivo = new ResumenEjecutivo
            {
                VentasTotalDia = 45850.75m,
                TicketPromedio = 38.25m,
                TotalComandas = 120,
                TotalClientes = 95,
                MesasAtendidas = 25,
                MeserosActivos = 8,
                TiempoPromedioServicio = TimeSpan.FromMinutes(18.5),
                EficienciaOperacional = 87.3m,
                ProductosVendidos = 485,
                ProductoMasVendido = "Hamburguesa Clásica",
                VentasProductoTop = 8950.00m,
                CambioPorcentualVentas = 8.4m,
                TendenciaGeneral = "Positiva"
            },
            VentasPorMesa = CreateMockVentasPorMesa(),
            VentasPorMesero = CreateMockVentasPorMesero(),
            VentasPorProducto = CreateMockVentasPorProducto(),
            Comparativo = new ComparativoPeriodos
            {
                FechaPeriodoAnterior = DateTime.Today.AddDays(-1),
                VentasPeriodoAnterior = 42300.50m,
                CambioAbsoluto = 3550.25m,
                CambioPorcentual = 8.4m,
                CambioComandas = 8,
                CambioTicketPromedio = 2.15m,
                AnalisisComparativo = "Mejora significativa en ventas y eficiencia operacional"
            },
            Alertas = new List<AlertaOperacional>
            {
                new() { Tipo = "Rendimiento", Titulo = "Mesa 15 con alta rotación", Prioridad = NivelPrioridad.Media },
                new() { Tipo = "Eficiencia", Titulo = "Mesero Juan necesita apoyo", Prioridad = NivelPrioridad.Baja }
            },
            MetricasRendimiento = new MetricasRendimiento
            {
                EficienciaGlobal = 87.3m,
                TiempoPromedioAtencion = TimeSpan.FromMinutes(8.2),
                SatisfaccionClientes = 4.6m
            }
        };
    }

    private static ReporteVentasDiariaResult CreateMockReporteConComparativo()
    {
        var reporte = CreateMockReporteCompletoBI();
        reporte.Comparativo!.CambiosDetallados = new List<CambioMetrica>
        {
            new() { NombreMetrica = "Ventas Totales", ValorActual = 45850.75m, ValorAnterior = 42300.50m, CambioPorcentual = 8.4m, TipoCambio = "Mejora" },
            new() { NombreMetrica = "Ticket Promedio", ValorActual = 38.25m, ValorAnterior = 36.10m, CambioPorcentual = 6.0m, TipoCambio = "Mejora" },
            new() { NombreMetrica = "Eficiencia", ValorActual = 87.3m, ValorAnterior = 83.1m, CambioPorcentual = 5.1m, TipoCambio = "Mejora" }
        };
        return reporte;
    }

    private static ReporteVentasDiariaResult CreateMockReporteMeserosEspecificos(List<Guid> meseroIds)
    {
        return new ReporteVentasDiariaResult
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Meseros,
            ResumenEjecutivo = new ResumenEjecutivo
            {
                VentasTotalDia = 18750.50m,
                TotalComandas = 48,
                MeserosActivos = 2
            },
            VentasPorMesero = meseroIds.Select((id, index) => new VentaPorMesero
            {
                MeseroId = id,
                NombreMesero = index == 0 ? "Carlos Rodríguez" : "Ana García",
                VentasTotal = index == 0 ? 10200.25m : 8550.25m,
                ComandasAtendidas = index == 0 ? 28 : 20,
                TicketPromedio = index == 0 ? 36.43m : 42.75m,
                EficienciaMesero = index == 0 ? 91.2m : 87.8m,
                RankingRendimiento = index == 0 ? "Excelente" : "Muy Bueno"
            }).ToList(),
            VentasPorMesa = null,
            VentasPorProducto = null
        };
    }

    private static ReporteVentasDiariaResult CreateMockReporteConTendencias()
    {
        var reporte = CreateMockReporteCompletoBI();
        reporte.TendenciasSemana = new TendenciasSemana
        {
            VentasPorDia = new List<VentaDiaria>
            {
                new() { Fecha = DateTime.Today.AddDays(-6), VentasTotal = 38200.00m, DiaSemana = "Lunes" },
                new() { Fecha = DateTime.Today.AddDays(-5), VentasTotal = 41500.00m, DiaSemana = "Martes" },
                new() { Fecha = DateTime.Today.AddDays(-4), VentasTotal = 43800.00m, DiaSemana = "Miércoles" },
                new() { Fecha = DateTime.Today.AddDays(-3), VentasTotal = 46200.00m, DiaSemana = "Jueves" },
                new() { Fecha = DateTime.Today.AddDays(-2), VentasTotal = 52300.00m, DiaSemana = "Viernes" },
                new() { Fecha = DateTime.Today.AddDays(-1), VentasTotal = 48900.00m, DiaSemana = "Sábado" },
                new() { Fecha = DateTime.Today, VentasTotal = 45850.75m, DiaSemana = "Domingo" }
            },
            TendenciaGeneral = 5.2m,
            MejorDiaSemana = "Lunes",
            PromedioSemanal = 45250.11m
        };
        return reporte;
    }

    private static ReporteVentasDiariaResult CreateMockReporteBasico()
    {
        return new ReporteVentasDiariaResult
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = NivelDetalle.Basico,
            ResumenEjecutivo = new ResumenEjecutivo
            {
                VentasTotalDia = 32500.00m,
                TicketPromedio = 35.50m,
                TotalComandas = 92,
                TotalClientes = 76,
                TendenciaGeneral = "Estable"
            },
            VentasPorMesa = null,
            VentasPorMesero = null,
            VentasPorProducto = null,
            Comparativo = null,
            TendenciasSemana = null
        };
    }

    private static ReporteVentasDiariaResult CreateMockReporteConAlertas()
    {
        var reporte = CreateMockReporteCompletoBI();
        reporte.Alertas = new List<AlertaOperacional>
        {
            new() { Tipo = "Crítica", Titulo = "Mesa 8 tiempo excesivo", Prioridad = NivelPrioridad.Critica },
            new() { Tipo = "Rendimiento", Titulo = "Bajo rendimiento cocina", Prioridad = NivelPrioridad.Alta },
            new() { Tipo = "Eficiencia", Titulo = "Mesero Ana requiere apoyo", Prioridad = NivelPrioridad.Media },
            new() { Tipo = "Inventario", Titulo = "Producto agotándose", Prioridad = NivelPrioridad.Baja }
        };
        return reporte;
    }

    private static List<VentaPorMesa> CreateMockVentasPorMesa()
    {
        return new List<VentaPorMesa>
        {
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M01", CapacidadMesa = 4, VentasTotal = 6200.50m, ComandasAtendidas = 8, EficienciaMesa = 92.5m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M02", CapacidadMesa = 2, VentasTotal = 3800.25m, ComandasAtendidas = 12, EficienciaMesa = 88.3m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M03", CapacidadMesa = 6, VentasTotal = 8950.00m, ComandasAtendidas = 6, EficienciaMesa = 95.1m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M04", CapacidadMesa = 4, VentasTotal = 5400.75m, ComandasAtendidas = 10, EficienciaMesa = 85.7m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M05", CapacidadMesa = 8, VentasTotal = 11200.00m, ComandasAtendidas = 4, EficienciaMesa = 91.8m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M06", CapacidadMesa = 2, VentasTotal = 2950.25m, ComandasAtendidas = 15, EficienciaMesa = 82.4m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M07", CapacidadMesa = 4, VentasTotal = 4350.00m, ComandasAtendidas = 11, EficienciaMesa = 89.2m },
            new() { MesaId = Guid.NewGuid(), NumeroMesa = "M08", CapacidadMesa = 6, VentasTotal = 3000.00m, ComandasAtendidas = 5, EficienciaMesa = 76.1m }
        };
    }

    private static List<VentaPorMesero> CreateMockVentasPorMesero()
    {
        return new List<VentaPorMesero>
        {
            new() { MeseroId = Guid.NewGuid(), NombreMesero = "Carlos Rodríguez", VentasTotal = 12500.50m, ComandasAtendidas = 32, TicketPromedio = 39.06m, EficienciaMesero = 93.2m, RankingRendimiento = "Excelente" },
            new() { MeseroId = Guid.NewGuid(), NombreMesero = "Ana García", VentasTotal = 10800.25m, ComandasAtendidas = 28, TicketPromedio = 38.58m, EficienciaMesero = 89.7m, RankingRendimiento = "Muy Bueno" },
            new() { MeseroId = Guid.NewGuid(), NombreMesero = "Luis Martínez", VentasTotal = 9750.00m, ComandasAtendidas = 25, TicketPromedio = 39.00m, EficienciaMesero = 87.1m, RankingRendimiento = "Bueno" },
            new() { MeseroId = Guid.NewGuid(), NombreMesero = "María López", VentasTotal = 8500.00m, ComandasAtendidas = 22, TicketPromedio = 38.64m, EficienciaMesero = 85.5m, RankingRendimiento = "Bueno" },
            new() { MeseroId = Guid.NewGuid(), NombreMesero = "Pedro Sánchez", VentasTotal = 4300.00m, ComandasAtendidas = 13, TicketPromedio = 33.08m, EficienciaMesero = 78.9m, RankingRendimiento = "Regular" }
        };
    }

    private static List<VentaPorProducto> CreateMockVentasPorProducto()
    {
        return new List<VentaPorProducto>
        {
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa Clásica", CategoriaProducto = "Platos Principales", CantidadVendida = 45, VentasTotal = 8950.00m, PrecioPromedio = 19.89m, TendenciaVenta = "Alta" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pizza Margherita", CategoriaProducto = "Pizzas", CantidadVendida = 32, VentasTotal = 6850.00m, PrecioPromedio = 21.41m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Ensalada César", CategoriaProducto = "Ensaladas", CantidadVendida = 28, VentasTotal = 4200.00m, PrecioPromedio = 15.00m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Pasta Carbonara", CategoriaProducto = "Pastas", CantidadVendida = 22, VentasTotal = 3850.00m, PrecioPromedio = 17.50m, TendenciaVenta = "Baja" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Coca Cola", CategoriaProducto = "Bebidas", CantidadVendida = 85, VentasTotal = 2550.00m, PrecioPromedio = 3.00m, TendenciaVenta = "Alta" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Cerveza Corona", CategoriaProducto = "Bebidas", CantidadVendida = 48, VentasTotal = 2400.00m, PrecioPromedio = 5.00m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Tiramisu", CategoriaProducto = "Postres", CantidadVendida = 18, VentasTotal = 1800.00m, PrecioPromedio = 10.00m, TendenciaVenta = "Baja" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Salmón Grillado", CategoriaProducto = "Platos Principales", CantidadVendida = 15, VentasTotal = 4200.00m, PrecioPromedio = 28.00m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Agua Mineral", CategoriaProducto = "Bebidas", CantidadVendida = 95, VentasTotal = 1425.00m, PrecioPromedio = 1.50m, TendenciaVenta = "Alta" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Café Americano", CategoriaProducto = "Bebidas", CantidadVendida = 65, VentasTotal = 1950.00m, PrecioPromedio = 3.00m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Tacos Mexicanos", CategoriaProducto = "Platos Principales", CantidadVendida = 20, VentasTotal = 3200.00m, PrecioPromedio = 16.00m, TendenciaVenta = "Media" },
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Cheesecake", CategoriaProducto = "Postres", CantidadVendida = 12, VentasTotal = 1080.00m, PrecioPromedio = 9.00m, TendenciaVenta = "Baja" }
        };
    }

    #endregion
} 