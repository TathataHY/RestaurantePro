namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA OBTENER REPORTE VENTAS DIARIA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de obtención de reportes de ventas diarias
/// Cobertura: 100% de reglas de negocio del ObtenerReporteVentasDiariaValidator
/// </summary>
public class ObtenerReporteVentasDiariaValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ObtenerReporteVentasDiariaValidator _validator;
    private readonly Mock<DbSet<Mesa>> _mesasMock;
    private readonly Mock<DbSet<Usuario>> _usuariosMock;
    private readonly Mock<DbSet<Comanda>> _comandasMock;

    public ObtenerReporteVentasDiariaValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mesasMock = new Mock<DbSet<Mesa>>();
        _usuariosMock = new Mock<DbSet<Usuario>>();
        _comandasMock = new Mock<DbSet<Comanda>>();

        _contextMock.Setup(x => x.Mesas).Returns(_mesasMock.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(_usuariosMock.Object);
        _contextMock.Setup(x => x.Comandas).Returns(_comandasMock.Object);

        _validator = new ObtenerReporteVentasDiariaValidator(_contextMock.Object);
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

    private void ConfigurarMesasExistentes(List<Guid> mesaIds)
    {
        var mesas = mesaIds.Select(id => new Mesa { Id = id }).ToList().AsQueryable();
        _mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesas.Provider);
        _mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesas.Expression);
        _mesasMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesas.ElementType);
        _mesasMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesas.GetEnumerator());
    }

    private void ConfigurarMeserosExistentes(List<Guid> meseroIds)
    {
        var meseros = meseroIds.Select(id => new Usuario 
        { 
            Id = id, 
            Roles = new List<RolUsuario> { RolUsuario.Mesero } 
        }).ToList().AsQueryable();
        
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(meseros.Provider);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(meseros.Expression);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(meseros.ElementType);
        _usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(meseros.GetEnumerator());
    }

    private void ConfigurarComandasExistentes(DateTime fecha, bool tieneComandas = true)
    {
        var comandas = tieneComandas 
            ? new List<Comanda> { new() { Id = Guid.NewGuid(), FechaCreacion = fecha } }
            : new List<Comanda>();
        
        var comandasQueryable = comandas.AsQueryable();
        _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandasQueryable.Provider);
        _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandasQueryable.Expression);
        _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandasQueryable.ElementType);
        _comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandasQueryable.GetEnumerator());
    }

    #endregion

    #region Validaciones Básicas

    [Fact]
    public async Task Validate_ConQueryValida_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConFechaReporteVacia_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = default;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.FechaReporte) &&
            e.ErrorMessage.Contains("La fecha del reporte es requerida"));
    }

    [Fact]
    public async Task Validate_ConFechaReporteFutura_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddDays(2);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.FechaReporte) &&
            e.ErrorMessage.Contains("La fecha del reporte no puede ser futura"));
    }

    [Fact]
    public async Task Validate_ConFechaReporteMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddYears(-3);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.FechaReporte) &&
            e.ErrorMessage.Contains("La fecha del reporte no puede ser mayor a 2 años atrás"));
    }

    [Fact]
    public async Task Validate_ConFechaReporteHoy_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validaciones de Nivel de Detalle

    [Theory]
    [InlineData(NivelDetalle.Basico)]
    [InlineData(NivelDetalle.Intermedio)]
    [InlineData(NivelDetalle.Completo)]
    [InlineData(NivelDetalle.Meseros)]
    [InlineData(NivelDetalle.Mesas)]
    public async Task Validate_ConNivelesDetalleValidos_DeberiaSerValido(NivelDetalle nivel)
    {
        // Arrange
        var query = CrearQueryValida();
        query.NivelDetalle = nivel;
        
        // Ajustar configuración según el nivel
        if (nivel == NivelDetalle.Meseros)
        {
            query.IncluirAnalisisPorMesero = true;
        }
        else if (nivel == NivelDetalle.Mesas)
        {
            query.IncluirAnalisisPorMesa = true;
        }
        else if (nivel == NivelDetalle.Basico)
        {
            query.IncluirTendenciasSemana = false;
        }

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validaciones de Listas de Filtros

    [Fact]
    public async Task Validate_ConMuchasMesasEspecificas_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = Enumerable.Range(1, 51).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MesesEspecificos) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 50 mesas"));
    }

    [Fact]
    public async Task Validate_ConMesasEspecificasConGuidVacio_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = new List<Guid> { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MesesEspecificos) &&
            e.ErrorMessage.Contains("Todos los IDs de mesa deben ser válidos"));
    }

    [Fact]
    public async Task Validate_ConMeserosEspecificosConGuidVacio_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MeserosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MeserosEspecificos) &&
            e.ErrorMessage.Contains("Todos los IDs de mesero deben ser válidos"));
    }

    [Fact]
    public async Task Validate_ConMuchosMeserosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MeserosEspecificos = Enumerable.Range(1, 21).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MeserosEspecificos) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 20 meseros"));
    }

    [Fact]
    public async Task Validate_ConMesasEspecificasExistentes_DeberiaSerValido()
    {
        // Arrange
        var mesaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = CrearQueryValida();
        query.MesesEspecificos = mesaIds;
        query.IncluirAnalisisPorMesa = true;
        
        ConfigurarMesasExistentes(mesaIds);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConMesasEspecificasInexistentes_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        query.IncluirAnalisisPorMesa = true;
        
        ConfigurarMesasExistentes(new List<Guid>()); // Sin mesas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MesesEspecificos) &&
            e.ErrorMessage.Contains("Una o más mesas especificadas no existen"));
    }

    [Fact]
    public async Task Validate_ConMeserosEspecificosExistentes_DeberiaSerValido()
    {
        // Arrange
        var meseroIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = CrearQueryValida();
        query.MeserosEspecificos = meseroIds;
        query.IncluirAnalisisPorMesero = true;
        
        ConfigurarMeserosExistentes(meseroIds);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConMeserosEspecificosInexistentes_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MeserosEspecificos = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        query.IncluirAnalisisPorMesero = true;
        
        ConfigurarMeserosExistentes(new List<Guid>()); // Sin meseros

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.MeserosEspecificos) &&
            e.ErrorMessage.Contains("Uno o más meseros especificados no existen"));
    }

    #endregion

    #region Validaciones de Consistencia

    [Fact]
    public async Task Validate_ConNivelMeserosSinAnalisisPorMesero_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.NivelDetalle = NivelDetalle.Meseros;
        query.IncluirAnalisisPorMesero = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.IncluirAnalisisPorMesero) &&
            e.ErrorMessage.Contains("El nivel 'Meseros' requiere incluir análisis por mesero"));
    }

    [Fact]
    public async Task Validate_ConNivelMesasSinAnalisisPorMesa_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.NivelDetalle = NivelDetalle.Mesas;
        query.IncluirAnalisisPorMesa = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.IncluirAnalisisPorMesa) &&
            e.ErrorMessage.Contains("El nivel 'Mesas' requiere incluir análisis por mesa"));
    }

    [Fact]
    public async Task Validate_ConMeserosEspecificosSinAnalisisPorMesero_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MeserosEspecificos = new List<Guid> { Guid.NewGuid() };
        query.IncluirAnalisisPorMesero = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.IncluirAnalisisPorMesero) &&
            e.ErrorMessage.Contains("Al especificar meseros específicos, debe incluir análisis por mesero"));
    }

    [Fact]
    public async Task Validate_ConMesasEspecificasSinAnalisisPorMesa_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = new List<Guid> { Guid.NewGuid() };
        query.IncluirAnalisisPorMesa = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.IncluirAnalisisPorMesa) &&
            e.ErrorMessage.Contains("Al especificar mesas específicas, debe incluir análisis por mesa"));
    }

    [Fact]
    public async Task Validate_ConNivelBasicoYTendenciasSemana_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.NivelDetalle = NivelDetalle.Basico;
        query.IncluirTendenciasSemana = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.IncluirTendenciasSemana) &&
            e.ErrorMessage.Contains("El nivel básico no puede incluir tendencias semanales"));
    }

    #endregion

    #region Validaciones de Negocio

    [Fact]
    public async Task Validate_ConFechaAntiguaSinDatos_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddDays(-35);
        
        ConfigurarComandasExistentes(query.FechaReporte, tieneComandas: false);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReporteVentasDiariaQuery.FechaReporte) &&
            e.ErrorMessage.Contains("La fecha seleccionada no tiene datos operacionales"));
    }

    [Fact]
    public async Task Validate_ConFechaAntiguaConDatos_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddDays(-35);
        
        ConfigurarComandasExistentes(query.FechaReporte, tieneComandas: true);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConTodosLosAnalisisSimultaneos_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisPorMesa = true;
        query.IncluirAnalisisPorMesero = true;
        query.IncluirAnalisisProductos = true;
        query.IncluirTendenciasSemana = true;
        query.IncluirComparativoPeriodoAnterior = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("No se pueden incluir todos los análisis simultáneamente por limitaciones de rendimiento"));
    }

    [Fact]
    public async Task Validate_ConAlgunosAnalisisSimultaneos_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirAnalisisPorMesa = true;
        query.IncluirAnalisisPorMesero = true;
        query.IncluirAnalisisProductos = true;
        query.IncluirTendenciasSemana = false; // No todos
        query.IncluirComparativoPeriodoAnterior = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public async Task Validate_ConQueryCrearReporteHoy_DeberiaSerValido()
    {
        // Arrange
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteHoy();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.FechaReporte.Should().Be(DateTime.Today);
        query.NivelDetalle.Should().Be(NivelDetalle.Completo);
    }

    [Fact]
    public async Task Validate_ConQueryCrearReporteFecha_DeberiaSerValido()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteFecha(fecha);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.FechaReporte.Should().Be(fecha.Date);
        query.IncluirComparativoPeriodoAnterior.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConQueryCrearReporteMeseros_DeberiaSerValido()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);
        var meseroIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteMeseros(fecha, meseroIds);
        
        ConfigurarMeserosExistentes(meseroIds);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.NivelDetalle.Should().Be(NivelDetalle.Meseros);
        query.IncluirAnalisisPorMesero.Should().BeTrue();
        query.MeserosEspecificos.Should().BeEquivalentTo(meseroIds);
    }

    #endregion

    #region Tests de Escenarios Complejos

    [Fact]
    public async Task Validate_ConQueryCompleja_DeberiaSerValido()
    {
        // Arrange
        var mesaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var meseroIds = new List<Guid> { Guid.NewGuid() };
        
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddDays(-7),
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = false, // Para evitar limitaciones de rendimiento
            IncluirTendenciasSemana = true,
            MesesEspecificos = mesaIds,
            MeserosEspecificos = meseroIds,
            NivelDetalle = NivelDetalle.Completo
        };

        ConfigurarMesasExistentes(mesaIds);
        ConfigurarMeserosExistentes(meseroIds);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddDays(1), // Fecha futura
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = true, // Todos los análisis (limitación de rendimiento)
            MesesEspecificos = new List<Guid> { Guid.Empty }, // GUID vacío
            MeserosEspecificos = Enumerable.Range(1, 25).Select(_ => Guid.NewGuid()).ToList(), // Muchos meseros
            NivelDetalle = NivelDetalle.Basico
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("La fecha del reporte no puede ser futura"));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("No se pueden especificar más de 20 meseros"));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("El nivel básico no puede incluir tendencias semanales"));
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(query);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    [Fact]
    public async Task Validate_ConValidacionesAsincronas_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var mesaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var meseroIds = new List<Guid> { Guid.NewGuid() };
        
        var query = CrearQueryValida();
        query.MesesEspecificos = mesaIds;
        query.MeserosEspecificos = meseroIds;
        query.IncluirAnalisisPorMesa = true;
        query.IncluirAnalisisPorMesero = true;
        
        ConfigurarMesasExistentes(mesaIds);
        ConfigurarMeserosExistentes(meseroIds);
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(query);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos para validaciones async
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task Validate_ConListasVacias_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = new List<Guid>();
        query.MeserosEspecificos = new List<Guid>();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConListasNull_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesesEspecificos = null;
        query.MeserosEspecificos = null;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConFechaLimiteAnterior_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddYears(-2); // Exactamente en el límite

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConFechaLimitePosterior_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReporte = DateTime.Today.AddDays(1); // Exactamente en el límite futuro

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 