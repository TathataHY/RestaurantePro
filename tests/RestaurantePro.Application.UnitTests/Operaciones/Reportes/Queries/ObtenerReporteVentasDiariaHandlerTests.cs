namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Queries;

/// <summary>
/// 🔥 Tests exhaustivos para ObtenerReporteVentasDiariaHandler
/// Validación completa de análisis de ventas diarias con diferentes configuraciones
/// </summary>
public class ObtenerReporteVentasDiariaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerReporteVentasDiariaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<DbSet<Comanda>> _mockComandas;
    private readonly ObtenerReporteVentasDiariaHandler _handler;

    public ObtenerReporteVentasDiariaHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerReporteVentasDiariaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockComandas = new Mock<DbSet<Comanda>>();

        _mockContext.Setup(x => x.Comandas).Returns(_mockComandas.Object);

        _handler = new ObtenerReporteVentasDiariaHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);
    }

    #region Helper Methods

    private ObtenerReporteVentasDiariaQuery CrearQueryValida()
    {
        return new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddDays(-1),
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };
    }

    private void ConfigurarComandasMock(List<Comanda> comandas)
    {
        var comandasQueryable = comandas.AsQueryable();
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandasQueryable.Provider);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandasQueryable.Expression);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandasQueryable.ElementType);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandasQueryable.GetEnumerator());
    }

    private List<Comanda> CrearComandasDePrueba(DateTime fecha, int cantidad = 3)
    {
        var comandas = new List<Comanda>();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        for (int i = 0; i < cantidad; i++)
        {
            var comanda = new Comanda
            {
                Id = Guid.NewGuid(),
                FechaCreacion = fecha.AddHours(i * 2),
                MontoTotal = 1500m + (i * 500m),
                MesaId = mesaId,
                MeseroId = meseroId,
                Mesa = new Mesa { Id = mesaId, Numero = 1 + i },
                Mesero = new Usuario { Id = meseroId, Nombre = $"Mesero {i + 1}" },
                DetalleComandas = new List<DetalleComanda>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = productoId,
                        Cantidad = 2,
                        PrecioUnitario = 750m + (i * 250m),
                        Producto = new Producto { Id = productoId, Nombre = $"Producto {i + 1}" }
                    }
                }
            };
            comandas.Add(comanda);
        }

        return comandas;
    }

    #endregion

    #region Tests Básicos

    [Fact]
    public async Task Handle_ReporteDiarioBasico_DeberiaGenerarExitosamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FechaReporte.Should().Be(query.FechaReporte);
        result.Value.NivelDetalle.Should().Be(query.NivelDetalle);
        result.Value.MetricasBasicas.Should().NotBeNull();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(2);
        result.Value.MetricasBasicas.MontoTotalVentas.Should().Be(3500m);
        result.Value.FechaGeneracion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_SinComandasEnFecha_DeberiaRetornarReporteVacio()
    {
        // Arrange
        var query = CrearQueryValida();

        ConfigurarComandasMock(new List<Comanda>()); // Sin comandas

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(0);
        result.Value.MetricasBasicas.MontoTotalVentas.Should().Be(0);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No se encontraron comandas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(NivelDetalle.Basico)]
    [InlineData(NivelDetalle.Intermedio)]
    [InlineData(NivelDetalle.Completo)]
    [InlineData(NivelDetalle.Meseros)]
    [InlineData(NivelDetalle.Mesas)]
    public async Task Handle_DiferentesNivelesDetalle_DeberiaGenerarCorrectamente(NivelDetalle nivel)
    {
        // Arrange
        var query = CrearQueryValida();
        query.NivelDetalle = nivel;
        
        // Ajustar configuración según nivel
        if (nivel == NivelDetalle.Meseros)
        {
            query.IncluirAnalisisPorMesero = true;
        }
        else if (nivel == NivelDetalle.Mesas)
        {
            query.IncluirAnalisisPorMesa = true;
        }

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.NivelDetalle.Should().Be(nivel);
    }

    #endregion

    #region Tests de Análisis Específicos

    [Fact]
    public async Task Handle_ConAnalisisPorMesa_DeberiaGenerarAnalisisMesas()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisPorMesa = true;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesa.Should().NotBeNull();
        result.Value.AnalisisPorMesa.Should().HaveCount(3);
        result.Value.AnalisisPorMesa[0].TotalComandas.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesa[0].MontoTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ConAnalisisPorMesero_DeberiaGenerarAnalisisMeseros()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisPorMesero = true;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesero.Should().NotBeNull();
        result.Value.AnalisisPorMesero.Should().HaveCount(3);
        result.Value.AnalisisPorMesero[0].TotalComandas.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesero[0].MontoTotal.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesero[0].EficienciaVentas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ConAnalisisProductos_DeberiaGenerarAnalisisProductos()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisProductos = true;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisProductos.Should().NotBeNull();
        result.Value.AnalisisProductos.Should().HaveCount(3);
        result.Value.AnalisisProductos[0].CantidadVendida.Should().BeGreaterThan(0);
        result.Value.AnalisisProductos[0].MontoTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_SinAnalisisEspecificos_NoDeberiaGenerarAnalisis()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisPorMesa = false;
        query.IncluirAnalisisPorMesero = false;
        query.IncluirAnalisisProductos = false;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesa.Should().BeNull();
        result.Value.AnalisisPorMesero.Should().BeNull();
        result.Value.AnalisisProductos.Should().BeNull();
        result.Value.DistribucionHoraria.Should().NotBeNull(); // Siempre se genera
    }

    #endregion

    #region Tests de Distribución Horaria

    [Fact]
    public async Task Handle_ConComandas_DeberiaGenerarDistribucionHoraria()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 4);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.DistribucionHoraria.Should().NotBeNull();
        result.Value.DistribucionHoraria.Should().HaveCount(4);
        result.Value.DistribucionHoraria.Should().BeInAscendingOrder(d => d.Hora);
        result.Value.DistribucionHoraria[0].TotalComandas.Should().BeGreaterThan(0);
    }

    #endregion

    #region Tests de Comparativo Período Anterior

    [Fact]
    public async Task Handle_ConComparativoPeriodoAnterior_DeberiaGenerarComparativo()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirComparativoPeriodoAnterior = true;

        var comandasHoy = CrearComandasDePrueba(query.FechaReporte, 2);
        var comandasAyer = CrearComandasDePrueba(query.FechaReporte.AddDays(-1), 1);
        
        var todasComandas = comandasHoy.Concat(comandasAyer).ToList();
        ConfigurarComandasMock(todasComandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.ComparativoPeriodoAnterior.Should().NotBeNull();
        result.Value.ComparativoPeriodoAnterior.FechaAnterior.Should().Be(query.FechaReporte.AddDays(-1));
        result.Value.ComparativoPeriodoAnterior.MetricasAnteriores.Should().NotBeNull();
        result.Value.ComparativoPeriodoAnterior.VariacionComandas.Should().NotBe(0);
        result.Value.ComparativoPeriodoAnterior.VariacionVentas.Should().NotBe(0);
    }

    [Fact]
    public async Task Handle_SinComparativoPeriodoAnterior_NoDeberiaGenerarComparativo()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirComparativoPeriodoAnterior = false;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.ComparativoPeriodoAnterior.Should().BeNull();
    }

    #endregion

    #region Tests de Tendencias Semana

    [Fact]
    public async Task Handle_ConTendenciasSemana_DeberiaGenerarTendencias()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirTendenciasSemana = true;

        var comandasSemana = new List<Comanda>();
        for (int i = 0; i < 7; i++)
        {
            var fecha = query.FechaReporte.AddDays(-6 + i);
            comandasSemana.AddRange(CrearComandasDePrueba(fecha, 1));
        }

        ConfigurarComandasMock(comandasSemana);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TendenciasSemana.Should().NotBeNull();
        result.Value.TendenciasSemana.Should().HaveCount(7);
        result.Value.TendenciasSemana.Should().BeInAscendingOrder(t => t.Fecha);
        result.Value.TendenciasSemana[0].TotalComandas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_SinTendenciasSemana_NoDeberiaGenerarTendencias()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirTendenciasSemana = false;

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TendenciasSemana.Should().BeNull();
    }

    #endregion

    #region Tests de Filtros

    [Fact]
    public async Task Handle_ConMesasEspecificas_DeberiaFiltrarPorMesas()
    {
        // Arrange
        var query = CrearQueryValida();
        var mesaId = Guid.NewGuid();
        query.MesesEspecificos = new List<Guid> { mesaId };
        query.IncluirAnalisisPorMesa = true;

        var comandasFiltradas = new List<Comanda>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FechaCreacion = query.FechaReporte.AddHours(10), 
                MontoTotal = 1500m, 
                MesaId = mesaId,
                Mesa = new Mesa { Id = mesaId, Numero = 5 }
            }
        };

        ConfigurarComandasMock(comandasFiltradas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConMeserosEspecificos_DeberiaFiltrarPorMeseros()
    {
        // Arrange
        var query = CrearQueryValida();
        var meseroId = Guid.NewGuid();
        query.MeserosEspecificos = new List<Guid> { meseroId };
        query.IncluirAnalisisPorMesero = true;

        var comandasFiltradas = new List<Comanda>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FechaCreacion = query.FechaReporte.AddHours(10), 
                MontoTotal = 2000m, 
                MeseroId = meseroId,
                Mesero = new Usuario { Id = meseroId, Nombre = "Mesero Específico" }
            }
        };

        ConfigurarComandasMock(comandasFiltradas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(1);
    }

    #endregion

    #region Tests de Métricas Calculadas

    [Fact]
    public async Task Handle_ConComandas_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = new List<Comanda>
        {
            new() { Id = Guid.NewGuid(), FechaCreacion = query.FechaReporte.AddHours(10), MontoTotal = 1000m },
            new() { Id = Guid.NewGuid(), FechaCreacion = query.FechaReporte.AddHours(14), MontoTotal = 2000m },
            new() { Id = Guid.NewGuid(), FechaCreacion = query.FechaReporte.AddHours(18), MontoTotal = 1500m }
        };

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var metricas = result.Value.MetricasBasicas;
        metricas.TotalComandas.Should().Be(3);
        metricas.MontoTotalVentas.Should().Be(4500m);
        metricas.PromedioVentaPorComanda.Should().Be(1500m);
        metricas.HoraPico.Should().NotBe(TimeSpan.Zero);
        metricas.ProductoMasVendido.Should().NotBeEmpty();
    }

    #endregion

    #region Tests de Errores

    [Fact]
    public async Task Handle_ExcepcionEnBaseDatos_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();

        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Provider)
            .Throws(new InvalidOperationException("Error de conexión"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno generando reporte");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error generando reporte")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_GeneracionExitosa_DeberiaLoguearCorrectamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar log de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generando reporte de ventas diarias")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Verificar log de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reporte de ventas diarias generado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public async Task Handle_QueryCrearReporteHoy_DeberiaFuncionar()
    {
        // Arrange
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteHoy();
        var comandas = CrearComandasDePrueba(DateTime.Today, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.FechaReporte.Should().Be(DateTime.Today);
        result.Value.NivelDetalle.Should().Be(NivelDetalle.Completo);
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Handle_GeneracionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 5);

        ConfigurarComandasMock(comandas);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion
} 