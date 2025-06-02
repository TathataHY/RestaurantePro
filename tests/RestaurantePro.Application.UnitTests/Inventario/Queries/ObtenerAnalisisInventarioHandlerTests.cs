namespace RestaurantePro.Application.UnitTests.Inventario.Queries;

/// <summary>
/// Tests unitarios para ObtenerAnalisisInventarioHandler
/// Valida la lógica completa de análisis de inventario con IA máxima y machine learning predictivo
/// </summary>
public class ObtenerAnalisisInventarioHandlerTests
{
    private readonly Mock<IInventarioServiceFacade> _inventarioServiceFacadeMock;
    private readonly Mock<IInventarioIngredientesRepository> _inventarioRepositoryMock;
    private readonly Mock<ILogger<ObtenerAnalisisInventarioHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly ObtenerAnalisisInventarioHandler _handler;

    public ObtenerAnalisisInventarioHandlerTests()
    {
        _inventarioServiceFacadeMock = new Mock<IInventarioServiceFacade>();
        _inventarioRepositoryMock = new Mock<IInventarioIngredientesRepository>();
        _loggerMock = new Mock<ILogger<ObtenerAnalisisInventarioHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();

        _handler = new ObtenerAnalisisInventarioHandler(
            _inventarioServiceFacadeMock.Object,
            _inventarioRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _backgroundJobServiceMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void CrearAnalisisDiario_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisDiario(fecha, true, true);

        // Assert
        Assert.Equal(fecha, query.FechaInicio);
        Assert.Equal(fecha, query.FechaFin);
        Assert.True(query.IncluirTendencias);
        Assert.True(query.IncluirRecomendaciones);
        Assert.Equal(NivelAnalisisInventario.Diario, query.NivelDetalle);
    }

    [Fact]
    public void CrearAnalisisSemanal_ConParametros_DeberiaConfigurarRangoSemanal()
    {
        // Arrange
        var fechaInicio = DateTime.Today.AddDays(-7);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisSemanal(fechaInicio, NivelAnalisisInventario.Completo);

        // Assert
        Assert.Equal(fechaInicio, query.FechaInicio);
        Assert.Equal(fechaInicio.AddDays(7), query.FechaFin);
        Assert.True(query.IncluirTendencias);
        Assert.True(query.IncluirMovimientosDetallados);
        Assert.Equal(NivelAnalisisInventario.Completo, query.NivelDetalle);
    }

    [Fact]
    public void CrearAnalisisCriticos_ConIngredientes_DeberiaEnfocarseEnCriticos()
    {
        // Arrange
        var ingredientesIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var umbralCritico = 5.0m;

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisCriticos(ingredientesIds, umbralCritico, true);

        // Assert
        Assert.Equal(ingredientesIds, query.IngredientesEspecificos);
        Assert.Equal(umbralCritico, query.UmbralStockCritico);
        Assert.Equal(NivelAnalisisInventario.Criticos, query.NivelDetalle);
        Assert.True(query.IncluirMovimientosDetallados);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AnalisisCompletoConIA_DeberiaRetornarAnalisisInteligente()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            UsuarioId = Guid.NewGuid()
        };

        var analisisCompleto = CreateMockAnalisisCompletoIA();
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisCompleto));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(285, result.Value.ResumenExecutivo.TotalIngredientes);
        Assert.Equal(42, result.Value.ResumenExecutivo.IngredientesStockCritico);
        Assert.Equal(15, result.Value.AnalisisIngredientes.Count);
        Assert.Equal(8, result.Value.AnalisisCategorias.Count);
        Assert.NotNull(result.Value.Predicciones);
        Assert.Equal(12, result.Value.AlertasInventario.Count);
        Assert.Equal(18, result.Value.RecomendacionesCompra.Count);
        Assert.True(result.Value.MetricasEficiencia.TasaRotacionGlobal > 0);
    }

    [Fact]
    public async Task Handle_AnalisisConPrediccionesML_DeberiaIncluirPrediccionesIA()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            IncluirTendencias = true,
            UsuarioId = Guid.NewGuid()
        };

        var analisisConIA = CreateMockAnalisisConPrediccionesML();
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisConIA));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.Predicciones);
        Assert.Equal("Alta", result.Value.Predicciones.ConfiabilidadPredicciones);
        Assert.Equal(7, result.Value.Predicciones.PrediccionesPorIngrediente.Count);
        Assert.True(result.Value.Predicciones.PrediccionesPorCategoria.Count > 0);
        Assert.Contains("Machine Learning", result.Value.RecomendacionesCompra.Select(r => r.Descripcion).FirstOrDefault() ?? "");
    }

    [Fact]
    public async Task Handle_AnalisisIngredientesCriticos_DeberiaIdentificarCriticos()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            NivelDetalle = "Críticos",
            SoloCriticos = true,
            SoloAlertaStock = true,
            UsuarioId = Guid.NewGuid()
        };

        var analisisCriticos = CreateMockAnalisisCriticos(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisCriticos));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.AnalisisIngredientes.Count);
        Assert.Contains(result.Value.AlertasInventario, a => a.Prioridad == NivelPrioridad.Critica);
    }

    [Fact]
    public async Task Handle_AnalisisFinanciero_DeberiaIncluirCostosDetallados()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            NivelDetalle = "Financiero",
            IncluirRecomendaciones = true,
            UsuarioId = Guid.NewGuid()
        };

        var analisisFinanciero = CreateMockAnalisisFinanciero();
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisFinanciero));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.AnalisisFinanciero);
        Assert.Equal(285000.75m, result.Value.AnalisisFinanciero.InversionTotalActual);
        Assert.Equal(1416.68m, result.Value.AnalisisFinanciero.CostoPromedioDiario);
        Assert.True(result.Value.AnalisisFinanciero.CostosPorCategoria.Count > 0);
        Assert.Equal("Carnes y Pescados", result.Value.AnalisisFinanciero.CostosPorCategoria.First().NombreCategoria);
    }

    [Fact]
    public async Task Handle_AnalisisBasico_DeberiaRetornarSoloEsencial()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            NivelDetalle = "Básico",
            IncluirTendencias = false,
            IncluirRecomendaciones = false,
            UsuarioId = Guid.NewGuid()
        };

        var analisisBasico = CreateMockAnalisisBasico();
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisisBasico));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.ResumenExecutivo);
        Assert.Null(result.Value.Predicciones);
        Assert.Null(result.Value.AnalisisFinanciero);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_FechasInvalidas_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(-5),
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La fecha de fin debe ser posterior a la fecha de inicio", result.Error);
    }

    [Fact]
    public async Task Handle_RangoMuyAmplio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddYears(-1),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El rango de fechas no puede superar los 180 días", result.Error);
    }

    [Fact]
    public async Task Handle_IngredientesCriticosSinEspecificar_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelDetalle = "Críticos",
            SoloCriticos = true,
            SoloAlertaStock = true,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar ingredientes para análisis crítico", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioInventario_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AnalisisInventarioResult>("Error en análisis predictivo de inventario"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en análisis predictivo de inventario", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionML_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad con servicio de ML"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_AnalisisExitoso_DeberiaLoggearMetricas()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        var analisis = CreateMockAnalisisCompletoIA();
        
        _inventarioServiceFacadeMock.Setup(x => x.GenerarAnalisisInventarioAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(analisis));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Análisis de inventario completado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static AnalisisInventarioResult CreateMockAnalisisCompletoIA()
    {
        return new AnalisisInventarioResult
        {
            FechaInicio = DateTime.Today.AddDays(-7),
            FechaFin = DateTime.Today,
            NivelAnalisis = NivelAnalisisInventario.Completo,
            TiempoGeneracion = TimeSpan.FromMinutes(2.3),
            ResumenExecutivo = new ResumenInventario
            {
                TotalIngredientes = 285,
                IngredientesStockCritico = 42,
                IngredientesEnStock = 195,
                ValorTotalInventario = 285000.75m,
                TasaRotacionInventario = 12.5m
            },
            AnalisisIngredientes = CreateMockAnalisisIngredientes(),
            AnalisisCategorias = CreateMockAnalisisCategorias(),
            AlertasInventario = CreateMockAlertas(),
            Predicciones = new PrediccionesInventario
            {
                ConfiabilidadPredicciones = "Alta",
                PrediccionesPorIngrediente = CreateMockPrediccionesIngredientes(),
                PrediccionesPorCategoria = CreateMockPrediccionesCategorias(),
                PrediccionGeneral = CreateMockPrediccionGeneral()
            },
            RecomendacionesCompra = CreateMockRecomendaciones(),
            MetricasEficiencia = new MetricasEficienciaInventario
            {
                TasaRotacionGlobal = 87.3m,
                TiempoPromedioReposicion = TimeSpan.FromDays(3.5),
                PorcentajeStockOptimo = 92.1m
            }
        };
    }

    private static AnalisisInventarioResult CreateMockAnalisisConPrediccionesML()
    {
        var analisis = CreateMockAnalisisCompletoIA();
        analisis.RecomendacionesCompra.Add(new RecomendacionCompra
        {
            IngredienteId = Guid.NewGuid(),
            NombreIngrediente = "Prediction ML: Stock Optimizado",
            Justificacion = "Machine Learning recomienda ajuste de stock basado en patrones históricos",
            CantidadRecomendada = 150,
            PrioridadCompra = NivelPrioridad.Alta
        });
        return analisis;
    }

    private static AnalisisInventarioResult CreateMockAnalisisCriticos(List<Guid> ingredientesIds)
    {
        return new AnalisisInventarioResult
        {
            NivelAnalisis = NivelAnalisisInventario.Criticos,
            ResumenExecutivo = new ResumenInventario
            {
                TotalIngredientes = ingredientesIds.Count,
                IngredientesStockCritico = ingredientesIds.Count
            },
            AnalisisIngredientes = ingredientesIds.Select(id => new AnalisisIngrediente
            {
                IngredienteId = id,
                NombreIngrediente = $"Ingrediente Crítico {id.ToString()[..8]}",
                StockActual = 5.2m,
                StockMinimo = 10.0m,
                EstadoStock = "Crítico"
            }).ToList(),
            AlertasInventario = new List<AlertaInventario>
            {
                new() { Prioridad = NivelPrioridad.Critica, Titulo = "Stock crítico detectado" }
            }
        };
    }

    private static AnalisisInventarioResult CreateMockAnalisisFinanciero()
    {
        return new AnalisisInventarioResult
        {
            NivelAnalisis = NivelAnalisisInventario.Financiero,
            ResumenExecutivo = new ResumenInventario
            {
                ValorTotalInventario = 285000.75m,
                TotalIngredientes = 285
            },
            AnalisisFinanciero = new AnalisisFinancieroInventario
            {
                InversionTotalActual = 285000.75m,
                CostoPromedioDiario = 1416.68m,
                CostosPorCategoria = new List<AnalisisCostoCategoria>
                {
                    new() { NombreCategoria = "Carnes y Pescados", CostoTotal = 125000.00m },
                    new() { NombreCategoria = "Lácteos", CostoTotal = 45000.00m }
                }
            }
        };
    }

    private static AnalisisInventarioResult CreateMockAnalisisBasico()
    {
        return new AnalisisInventarioResult
        {
            NivelAnalisis = NivelAnalisisInventario.Basico,
            ResumenExecutivo = new ResumenInventario
            {
                TotalIngredientes = 185,
                IngredientesStockCritico = 15,
                IngredientesEnStock = 150
            },
            Predicciones = null,
            AnalisisFinanciero = null,
            MovimientosDetallados = null
        };
    }

    private static List<AnalisisIngrediente> CreateMockAnalisisIngredientes()
    {
        return new List<AnalisisIngrediente>
        {
            new() { IngredienteId = Guid.NewGuid(), NombreIngrediente = "Tomate", StockActual = 25.5m, StockOptimo = 30.0m, EstadoStock = EstadoStock.BajoStock },
            new() { IngredienteId = Guid.NewGuid(), NombreIngrediente = "Cebolla", StockActual = 45.2m, StockOptimo = 40.0m, EstadoStock = EstadoStock.Optimo },
            new() { IngredienteId = Guid.NewGuid(), NombreIngrediente = "Pollo", StockActual = 15.8m, StockOptimo = 20.0m, EstadoStock = EstadoStock.BajoStock }
        };
    }

    private static List<AnalisisCategoria> CreateMockAnalisisCategorias()
    {
        return new List<AnalisisCategoria>
        {
            new() { Categoria = "Verduras", TotalIngredientes = 45, IngredientesCriticos = 8, ValorTotal = 12500.00m },
            new() { Categoria = "Carnes", TotalIngredientes = 25, IngredientesCriticos = 5, ValorTotal = 85000.00m }
        };
    }

    private static List<AlertaInventario> CreateMockAlertas()
    {
        return new List<AlertaInventario>
        {
            new() { Prioridad = NivelPrioridad.Critica, Titulo = "Stock crítico: Tomate", IngredienteId = Guid.NewGuid() },
            new() { Prioridad = NivelPrioridad.Alta, Titulo = "Vencimiento próximo: Lácteos", IngredienteId = Guid.NewGuid() }
        };
    }

    private static List<PrediccionIngrediente> CreateMockPrediccionesIngredientes()
    {
        return new List<PrediccionIngrediente>
        {
            new() { 
                IngredienteId = Guid.NewGuid(), 
                NombreIngrediente = "Tomate", 
                DiasRestantesStock = 5,
                FechaAgotamientoEstimada = DateTime.Today.AddDays(5),
                ConsumoProyectado7Dias = 25.5m,
                ConsumoProyectado30Dias = 100.0m,
                CantidadOptimalPedido = 50.0m,
                FechaOptimalPedido = DateTime.Today.AddDays(3),
                ConfiabilidadPrediccion = 0.85m
            },
            new() { 
                IngredienteId = Guid.NewGuid(), 
                NombreIngrediente = "Cebolla", 
                DiasRestantesStock = 12,
                FechaAgotamientoEstimada = DateTime.Today.AddDays(12),
                ConsumoProyectado7Dias = 15.2m,
                ConsumoProyectado30Dias = 60.0m,
                CantidadOptimalPedido = 30.0m,
                FechaOptimalPedido = DateTime.Today.AddDays(8),
                ConfiabilidadPrediccion = 0.92m
            }
        };
    }

    private static List<PrediccionCategoria> CreateMockPrediccionesCategorias()
    {
        return new List<PrediccionCategoria>
        {
            new() { 
                NombreCategoria = "Verduras", 
                InversionRecomendada7Dias = 5000.00m,
                InversionRecomendada30Dias = 20000.00m,
                IngredientesCriticosProyectados = 8,
                IngredientesPrioritarios = new List<string> { "Tomate", "Lechuga", "Cebolla" }
            },
            new() { 
                NombreCategoria = "Carnes", 
                InversionRecomendada7Dias = 15000.00m,
                InversionRecomendada30Dias = 60000.00m,
                IngredientesCriticosProyectados = 3,
                IngredientesPrioritarios = new List<string> { "Pollo", "Res" }
            }
        };
    }

    private static PrediccionGeneral CreateMockPrediccionGeneral()
    {
        return new PrediccionGeneral
        {
            InversionTotalRecomendada = 85000.00m,
            DiasAutonomiaPropedio = 15,
            RiesgoDesabastecimiento = 0.25m,
            RecomendacionGeneral = "Inventario en estado óptimo con algunas alertas menores"
        };
    }

    private static List<RecomendacionCompra> CreateMockRecomendaciones()
    {
        return new List<RecomendacionCompra>
        {
            new() { IngredienteId = Guid.NewGuid(), NombreIngrediente = "Tomate", CantidadRecomendada = 50.0m, PrioridadCompra = NivelPrioridad.Alta },
            new() { IngredienteId = Guid.NewGuid(), NombreIngrediente = "Lechuga", CantidadRecomendada = 30.0m, PrioridadCompra = NivelPrioridad.Media }
        };
    }

    #endregion
} 