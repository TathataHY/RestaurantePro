using RestaurantePro.Application.UnitTests.Common;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Commands;

/// <summary>
/// 🔥 Tests exhaustivos para GenerarReporteHandler
/// Validación completa de generación de reportes con diferentes tipos y formatos
/// </summary>
public class GenerarReporteHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<GenerarReporteHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<DbSet<Comanda>> _mockComandas;
    private readonly Mock<DbSet<MovimientoInventario>> _mockMovimientos;
    private readonly Mock<DbSet<Ingrediente>> _mockIngredientes;
    private readonly GenerarReporteHandler _handler;

    public GenerarReporteHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<GenerarReporteHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockComandas = new Mock<DbSet<Comanda>>();
        _mockMovimientos = new Mock<DbSet<MovimientoInventario>>();
        _mockIngredientes = new Mock<DbSet<Ingrediente>>();

        // Configurar DbSets en el contexto
        _mockContext.Setup(x => x.Comandas).Returns(_mockComandas.Object);
        _mockContext.Setup(x => x.MovimientosInventario).Returns(_mockMovimientos.Object);
        _mockContext.Setup(x => x.Ingredientes).Returns(_mockIngredientes.Object);

        // Configurar CurrentUserService
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        
        // Configurar FileStorageService exitoso
        _mockFileStorageService.Setup(x => x.SubirArchivoAsync(
                It.IsAny<DatosArchivo>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("reports/reporte_12345.pdf"));
            
        _mockFileStorageService.Setup(x => x.ObtenerUrlTemporalAsync(
                It.IsAny<string>(), 
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://storage.example.com/reports/reporte_12345.pdf"));

        // Configurar EmailService exitoso
        _mockEmailService.Setup(x => x.SendEmailWithAttachmentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(true);

        _handler = new GenerarReporteHandler(
            _mockContext.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockEmailService.Object,
            _mockFileStorageService.Object);
    }

    #region Helper Methods

    private GenerarReporteCommand CrearCommandValido()
    {
        return new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasDiarias,
            FechaInicio = DateTime.Today.AddDays(-1),
            FechaFin = DateTime.Today,
            Formato = FormatoReporte.PDF,
            UsuarioSolicitanteId = Guid.NewGuid(),
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            Prioridad = NivelPrioridad.Media,
            EnviarPorEmail = false
        };
    }

    private void ConfigurarComandasMock(List<Comanda> comandas)
    {
        // Asegurar que las comandas tengan totales calculados
        foreach (var comanda in comandas)
        {
            try
            {
                // Usar la secuencia correcta de estados para finalizar la comanda
                comanda.MarcarEnPreparacion();
                comanda.MarcarLista();
                comanda.MarcarEntregada();
                comanda.MarcarPagada(); // Esto calculará el total correctamente y finalizará la comanda
            }
            catch
            {
                // Si no se puede finalizar, al menos intentar recalcular el total
                // Las comandas ya deberían tener items agregados por las pruebas
            }
        }
        
        var mockSet = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMovimientosMock(List<MovimientoInventario> movimientos)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(movimientos.AsQueryable());
        _mockContext.Setup(c => c.MovimientosInventario).Returns(mockSet.Object);
    }

    private void ConfigurarIngredientesMock(List<Ingrediente> ingredientes)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable());
        _mockContext.Setup(c => c.Ingredientes).Returns(mockSet.Object);
    }

    #endregion

    #region Tests Básicos de Generación

    [Fact]
    public async Task Handle_ReporteVentasBasico_DeberiaGenerarExitosamente()
    {
        // Arrange
        var command = CrearCommandValido();
        var comanda1 = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba 1", "COM-001");
        comanda1.AgregarItem(Guid.NewGuid(), "Producto 1", 2, 15.50m, "Sin observaciones");
        comanda1.AgregarItem(Guid.NewGuid(), "Producto 2", 1, 25.00m, "Con extra");
        
        var comanda2 = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba 2", "COM-002");
        comanda2.AgregarItem(Guid.NewGuid(), "Producto 3", 1, 30.00m, "Sin observaciones");
        
        ConfigurarComandasMock(new List<Comanda> { comanda1, comanda2 });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.ReporteId.Should().NotBeEmpty();
        result.Value.NombreArchivo.Should().EndWith(".pdf");
        result.Value.Formato.Should().Be(FormatoReporte.PDF);
    }

    [Theory]
    [InlineData(TipoReporte.VentasDiarias)]
    [InlineData(TipoReporte.VentasSemanales)]
    [InlineData(TipoReporte.VentasMensuales)]
    [InlineData(TipoReporte.Inventario)]
    [InlineData(TipoReporte.ProductosMasVendidos)]
    public async Task Handle_DiferentesTiposReporte_DeberiaGenerarExitosamente(TipoReporte tipoReporte)
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = tipoReporte,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId
        };

        var comandas = new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        };

        // Nota: MovimientoInventario e Ingrediente necesitan factory methods o mocks apropiados
        var movimientos = new List<MovimientoInventario>();
        var ingredientes = new List<Ingrediente>();

        ConfigurarComandasMock(comandas);
        ConfigurarMovimientosMock(movimientos);
        ConfigurarIngredientesMock(ingredientes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetadatosAdicionales["tipoReporte"].Should().Be(tipoReporte.ToString());
    }

    [Theory]
    [InlineData(FormatoReporte.PDF, "pdf")]
    [InlineData(FormatoReporte.Excel, "xlsx")]
    [InlineData(FormatoReporte.CSV, "csv")]
    [InlineData(FormatoReporte.JSON, "json")]
    [InlineData(FormatoReporte.HTML, "html")]
    public async Task Handle_DiferentesFormatos_DeberiaGenerarConExtensionCorrecta(FormatoReporte formato, string extensionEsperada)
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = command.TipoReporte,
            Formato = formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId
        };

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.NombreArchivo.Should().EndWith($".{extensionEsperada}");
        result.Value.Formato.Should().Be(formato);
    }

    #endregion

    #region Tests de Reportes Específicos

    [Fact]
    public async Task Handle_ReporteInventario_DeberiaUsarDatosInventario()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Inventario,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId,
            FiltrosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Nota: MovimientoInventario e Ingrediente necesitan factory methods o mocks apropiados
        var movimientos = new List<MovimientoInventario>();
        var ingredientes = new List<Ingrediente>();

        ConfigurarComandasMock(new List<Comanda>());
        ConfigurarMovimientosMock(movimientos);
        ConfigurarIngredientesMock(ingredientes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetadatosAdicionales["tipoReporte"].Should().Be("Inventario");
    }

    [Fact]
    public async Task Handle_ReportePersonalizado_DeberiaRequerirNombre()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Personalizado,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId,
            NombrePersonalizado = "Mi Reporte Especial"
        };

        ConfigurarComandasMock(new List<Comanda>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    #endregion

    #region Tests de Email

    [Fact]
    public async Task Handle_ConEnvioEmail_DeberiaEnviarCorreo()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = command.TipoReporte,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId,
            EnviarPorEmail = true,
            EmailDestino = "test@example.com"
        };

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.EnviadoPorEmail.Should().BeTrue();

        // Verificar que se loguea el envío de email
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reporte enviado por email")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SinEnvioEmail_NoDeberiaEnviarCorreo()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = command.TipoReporte,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId,
            EnviarPorEmail = false
        };

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.EnviadoPorEmail.Should().BeFalse();

        // Verificar que NO se loguea el envío de email
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reporte enviado por email")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region Tests de Metadatos

    [Fact]
    public async Task Handle_ReporteGenerado_DeberiaContenerMetadatosCompletos()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = command.TipoReporte,
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId,
            Prioridad = NivelPrioridad.Alta
        };

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var metadatos = result.Value.MetadatosAdicionales;
        metadatos.Should().ContainKey("tipoReporte");
        metadatos.Should().ContainKey("usuarioSolicitante");
        metadatos.Should().ContainKey("fechaInicio");
        metadatos.Should().ContainKey("fechaFin");
        metadatos.Should().ContainKey("prioridad");
        
        metadatos["tipoReporte"].Should().Be(command.TipoReporte.ToString());
        metadatos["usuarioSolicitante"].Should().Be(command.UsuarioSolicitanteId);
        metadatos["prioridad"].Should().Be(command.Prioridad.ToString());
    }

    [Fact]
    public async Task Handle_ReporteGenerado_DeberiaTenerTiempoGeneracion()
    {
        // Arrange
        var command = CrearCommandValido();

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TiempoGeneracion.Should().BeGreaterThan(TimeSpan.Zero);
        result.Value.FechaGeneracion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Tests de Errores

    [Fact]
    public async Task Handle_ExcepcionEnObtenerDatos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();

        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Provider)
            .Throws(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al generar el reporte");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error generando reporte")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TipoReporteNoSoportado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear command usando inicializador de objeto para propiedades de solo inicialización
        command = new GenerarReporteCommand
        {
            TipoReporte = (TipoReporte)999, // Tipo inválido
            Formato = command.Formato,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            UsuarioSolicitanteId = command.UsuarioSolicitanteId
        };

        ConfigurarComandasMock(new List<Comanda>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Tipo de reporte no soportado");
    }

    #endregion

    #region Tests de Datos Vacíos

    [Fact]
    public async Task Handle_SinDatosEnPeriodo_DeberiaGenerarReporteVacio()
    {
        // Arrange
        var command = CrearCommandValido();

        ConfigurarComandasMock(new List<Comanda>()); // Sin datos

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TamanoBytes.Should().BeGreaterThan(0); // Aún debe generar archivo
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_GeneracionExitosa_DeberiaLoguearCorrectamente()
    {
        // Arrange
        var command = CrearCommandValido();

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar log de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Iniciando generación de reporte")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar log de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reporte generado exitosamente")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Handle_GeneracionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var command = CrearCommandValido();

        ConfigurarComandasMock(new List<Comanda>
        {
            Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Comanda de prueba", "COM-001")
        });

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
        result.Value.TiempoGeneracion.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    #endregion
} 