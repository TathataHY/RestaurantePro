namespace RestaurantePro.Application.UnitTests.Proveedores.Queries;

/// <summary>
/// Tests unitarios para ObtenerProveedorPorIdHandler
/// Valida la lógica completa de consulta de proveedores con información detallada
/// </summary>
public class ObtenerProveedorPorIdHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerProveedorPorIdHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<Proveedor>> _proveedoresDbSetMock;
    private readonly ObtenerProveedorPorIdHandler _handler;

    public ObtenerProveedorPorIdHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerProveedorPorIdHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _proveedoresDbSetMock = new Mock<DbSet<Proveedor>>();

        // Setup DbContext
        _contextMock.Setup(x => x.Proveedores).Returns(_proveedoresDbSetMock.Object);

        _handler = new ObtenerProveedorPorIdHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Query Factory Methods

    [Fact]
    public void ConsultaBasica_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var query = ObtenerProveedorPorIdQuery.ConsultaBasica(proveedorId, usuarioId);

        // Assert
        Assert.Equal(proveedorId, query.ProveedorId);
        Assert.Equal(usuarioId, query.UsuarioConsultaId);
        Assert.Equal("Basico", query.NivelDetalle);
        Assert.False(query.IncluirContactos);
        Assert.False(query.IncluirHistorialOrdenes);
        Assert.False(query.IncluirMetricasDesempeno);
        Assert.False(query.ValidarPermisos);
    }

    [Fact]
    public void ConsultaCompleta_ConParametrosValidos_DeberiaIncluirTodosLosDetalles()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Evaluación de proveedor estratégico";

        // Act
        var query = ObtenerProveedorPorIdQuery.ConsultaCompleta(proveedorId, usuarioId, motivo);

        // Assert
        Assert.Equal("Completo", query.NivelDetalle);
        Assert.True(query.IncluirContactos);
        Assert.True(query.IncluirHistorialOrdenes);
        Assert.True(query.IncluirMetricasDesempeno);
        Assert.True(query.IncluirAnalisisCalidad);
        Assert.True(query.ValidarPermisos);
        Assert.Equal(motivo, query.MotivoConsulta);
    }

    [Fact]
    public void ConsultaComercial_ConParametrosValidos_DeberiaConfigurarNivelComercial()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var query = ObtenerProveedorPorIdQuery.ConsultaComercial(proveedorId, usuarioId);

        // Assert
        Assert.Equal("Comercial", query.NivelDetalle);
        Assert.True(query.IncluirContactos);
        Assert.True(query.IncluirHistorialOrdenes);
        Assert.False(query.IncluirMetricasDesempeno);
        Assert.Equal("Gestión comercial", query.MotivoConsulta);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ProveedorExistenteConsultaBasica_DeberiaRetornarProveedorDto()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            UsuarioConsultaId = usuarioId,
            NivelDetalle = "Basico",
            IncluirContactos = false,
            IncluirHistorialOrdenes = false,
            ValidarPermisos = false
        };

        var proveedor = CreateMockProveedorActivo(proveedorId, "Distribuidora Central");
        var proveedorDto = CreateMockProveedorDto(proveedorId, "Distribuidora Central");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(proveedorId, result.Value.Id);
        Assert.Equal("Distribuidora Central", result.Value.NombreComercial);
    }

    [Fact]
    public async Task Handle_ProveedorConContactosCompletos_DeberiaIncluirContactos()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Completo",
            IncluirContactos = true,
            IncluirHistorialOrdenes = true,
            IncluirMetricasDesempeno = true
        };

        var proveedor = CreateMockProveedorConContactos(proveedorId, "Proveedor Premium SA");
        var proveedorDto = CreateMockProveedorDtoCompleto(proveedorId, "Proveedor Premium SA");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value.Contactos.Count); // Mock con 3 contactos
        Assert.Contains("Juan Pérez", result.Value.Contactos.Select(c => c.Nombre));
        Assert.Contains("María González", result.Value.Contactos.Select(c => c.Nombre));
    }

    [Fact]
    public async Task Handle_ProveedorConHistorialOrdenes_DeberiaIncluirMetricas()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Completo",
            IncluirHistorialOrdenes = true,
            IncluirMetricasDesempeno = true,
            IncluirAnalisisCalidad = true
        };

        var proveedor = CreateMockProveedorConHistorial(proveedorId, "Proveedor Confiable LTDA");
        var proveedorDto = CreateMockProveedorDtoConMetricas(proveedorId, "Proveedor Confiable LTDA");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value.MetricasDesempeno);
        Assert.True(result.Value.MetricasDesempeno.PorcentajeCumplimiento >= 0);
        Assert.True(result.Value.MetricasDesempeno.TiempoPromedioEntrega > 0);
        Assert.NotEmpty(result.Value.HistorialOrdenes);
    }

    [Fact]
    public async Task Handle_ProveedorEstrategico_DeberiaIncluirAnalisisCompleto()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Estrategico",
            IncluirContactos = true,
            IncluirHistorialOrdenes = true,
            IncluirMetricasDesempeno = true,
            IncluirAnalisisCalidad = true,
            IncluirAnalisisFinanciero = true
        };

        var proveedor = CreateMockProveedorEstrategico(proveedorId, "Proveedor Estratégico Global");
        var proveedorDto = CreateMockProveedorDtoEstrategico(proveedorId, "Proveedor Estratégico Global");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(ClasificacionProveedor.Estrategico, result.Value.Clasificacion);
        Assert.NotNull(result.Value.AnalisisFinanciero);
        Assert.True(result.Value.AnalisisFinanciero.FacturacionAnual > 0);
        Assert.NotEmpty(result.Value.AnalisisFinanciero.IndicadoresRiesgo);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ProveedorNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Basico"
        };

        SetupProveedoresDbSet(new List<Proveedor>()); // Lista vacía

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no encontrado", result.Error);
    }

    [Fact]
    public async Task Handle_ProveedorInactivo_DeberiaRetornarError()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Basico"
        };

        var proveedor = CreateMockProveedorInactivo(proveedorId, "Proveedor Inactivo");
        SetupProveedoresDbSet(new List<Proveedor> { proveedor });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("inactivo", result.Error);
    }

    [Fact]
    public async Task Handle_ProveedorSuspendido_DeberiaRetornarAdvertencia()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Basico"
        };

        var proveedor = CreateMockProveedorSuspendido(proveedorId, "Proveedor Suspendido");
        var proveedorDto = CreateMockProveedorDto(proveedorId, "Proveedor Suspendido");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(EstadoProveedor.Suspendido, result.Value.Estado);
        
        // Verificar que se loggeó una advertencia
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("suspendido")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ExcepcionBaseDatos_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Basico"
        };

        _proveedoresDbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Proveedor, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión a la base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionMapper_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Basico"
        };

        var proveedor = CreateMockProveedorActivo(proveedorId, "Proveedor Test");
        SetupProveedoresDbSet(new List<Proveedor> { proveedor });

        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Throws(new Exception("Error en mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_ConsultaExitosa_DeberiaLoggearInformacion()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            NivelDetalle = "Completo",
            MotivoConsulta = "Test de logging"
        };

        var proveedor = CreateMockProveedorActivo(proveedorId, "Proveedor Test");
        var proveedorDto = CreateMockProveedorDto(proveedorId, "Proveedor Test");

        SetupProveedoresDbSet(new List<Proveedor> { proveedor });
        _mapperMock.Setup(x => x.Map<ProveedorDto>(It.IsAny<Proveedor>()))
            .Returns(proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando proveedor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("consultado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private void SetupProveedoresDbSet(List<Proveedor> proveedores)
    {
        var queryable = proveedores.AsQueryable();
        
        _proveedoresDbSetMock.As<IQueryable<Proveedor>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _proveedoresDbSetMock.As<IQueryable<Proveedor>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _proveedoresDbSetMock.As<IQueryable<Proveedor>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _proveedoresDbSetMock.As<IQueryable<Proveedor>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _proveedoresDbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Proveedor, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Proveedor, bool>> expr, CancellationToken ct) =>
                proveedores.AsQueryable().FirstOrDefault(expr.Compile()));
    }

    private static Proveedor CreateMockProveedorActivo(Guid id, string nombre)
    {
        return new Proveedor
        {
            Id = id,
            NombreComercial = nombre,
            RazonSocial = $"{nombre} S.A.S.",
            Estado = EstadoProveedor.Activo,
            Clasificacion = ClasificacionProveedor.Regular,
            FechaRegistro = DateTime.UtcNow.AddDays(-30),
            Activo = true
        };
    }

    private static Proveedor CreateMockProveedorInactivo(Guid id, string nombre)
    {
        return new Proveedor
        {
            Id = id,
            NombreComercial = nombre,
            Estado = EstadoProveedor.Inactivo,
            Activo = false
        };
    }

    private static Proveedor CreateMockProveedorSuspendido(Guid id, string nombre)
    {
        return new Proveedor
        {
            Id = id,
            NombreComercial = nombre,
            Estado = EstadoProveedor.Suspendido,
            Activo = true
        };
    }

    private static Proveedor CreateMockProveedorConContactos(Guid id, string nombre)
    {
        var proveedor = CreateMockProveedorActivo(id, nombre);
        proveedor.Clasificacion = ClasificacionProveedor.Premium;
        return proveedor;
    }

    private static Proveedor CreateMockProveedorConHistorial(Guid id, string nombre)
    {
        var proveedor = CreateMockProveedorActivo(id, nombre);
        proveedor.Clasificacion = ClasificacionProveedor.Confiable;
        return proveedor;
    }

    private static Proveedor CreateMockProveedorEstrategico(Guid id, string nombre)
    {
        var proveedor = CreateMockProveedorActivo(id, nombre);
        proveedor.Clasificacion = ClasificacionProveedor.Estrategico;
        return proveedor;
    }

    private static ProveedorDto CreateMockProveedorDto(Guid id, string nombre)
    {
        return new ProveedorDto
        {
            Id = id,
            NombreComercial = nombre,
            RazonSocial = $"{nombre} S.A.S.",
            Estado = EstadoProveedor.Activo,
            Clasificacion = ClasificacionProveedor.Regular,
            Contactos = new List<ContactoProveedorDto>(),
            HistorialOrdenes = new List<OrdenCompraResumenDto>()
        };
    }

    private static ProveedorDto CreateMockProveedorDtoCompleto(Guid id, string nombre)
    {
        return new ProveedorDto
        {
            Id = id,
            NombreComercial = nombre,
            Estado = EstadoProveedor.Activo,
            Clasificacion = ClasificacionProveedor.Premium,
            Contactos = new List<ContactoProveedorDto>
            {
                new() { Nombre = "Juan Pérez", Cargo = "Gerente Comercial" },
                new() { Nombre = "María González", Cargo = "Coordinadora Logística" },
                new() { Nombre = "Carlos Rodríguez", Cargo = "Servicio al Cliente" }
            },
            HistorialOrdenes = new List<OrdenCompraResumenDto>()
        };
    }

    private static ProveedorDto CreateMockProveedorDtoConMetricas(Guid id, string nombre)
    {
        return new ProveedorDto
        {
            Id = id,
            NombreComercial = nombre,
            Estado = EstadoProveedor.Activo,
            Clasificacion = ClasificacionProveedor.Confiable,
            Contactos = new List<ContactoProveedorDto>(),
            HistorialOrdenes = new List<OrdenCompraResumenDto>
            {
                new() { NumeroOrden = "ORD-001", Total = 15000m },
                new() { NumeroOrden = "ORD-002", Total = 23000m }
            },
            MetricasDesempeno = new MetricasDesempenoProveedorDto
            {
                PorcentajeCumplimiento = 95.5m,
                TiempoPromedioEntrega = 3.2m,
                CalificacionPromedio = 4.7m
            }
        };
    }

    private static ProveedorDto CreateMockProveedorDtoEstrategico(Guid id, string nombre)
    {
        return new ProveedorDto
        {
            Id = id,
            NombreComercial = nombre,
            Estado = EstadoProveedor.Activo,
            Clasificacion = ClasificacionProveedor.Estrategico,
            Contactos = new List<ContactoProveedorDto>(),
            HistorialOrdenes = new List<OrdenCompraResumenDto>(),
            AnalisisFinanciero = new AnalisisFinancieroProveedorDto
            {
                FacturacionAnual = 500000m,
                IndicadoresRiesgo = new List<string> { "Riesgo Bajo", "Liquidez Alta" }
            }
        };
    }

    #endregion
} 