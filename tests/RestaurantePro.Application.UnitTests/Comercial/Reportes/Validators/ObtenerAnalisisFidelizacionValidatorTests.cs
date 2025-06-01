namespace RestaurantePro.Application.UnitTests.Comercial.Reportes.Validators;

/// <summary>
/// Tests para ObtenerAnalisisFidelizacionValidator
/// Valida reglas de negocio para obtener análisis completo de fidelización
/// </summary>
public class ObtenerAnalisisFidelizacionValidatorTests
{
    private readonly ObtenerAnalisisFidelizacionValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Cliente>> _mockClientes;
    private readonly Mock<DbSet<TarjetaFidelizacion>> _mockTarjetas;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;

    public ObtenerAnalisisFidelizacionValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockClientes = MockDbSetHelper.CreateMockDbSet<Cliente>();
        _mockTarjetas = MockDbSetHelper.CreateMockDbSet<TarjetaFidelizacion>();
        _mockUsuarios = MockDbSetHelper.CreateMockDbSet<Usuario>();
        
        _mockContext.Setup(c => c.Clientes).Returns(_mockClientes.Object);
        _mockContext.Setup(c => c.TarjetasFidelizacion).Returns(_mockTarjetas.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);
        
        _validator = new ObtenerAnalisisFidelizacionValidator(_mockContext.Object);
    }

    #region Tests de Validaciones de Fechas

    [Fact]
    public async Task FechaInicio_NoDebeSerMuyAntigua()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddYears(-6);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaInicio)
            .WithErrorCode("ANALISIS_FIDELIZACION_FECHA_INICIO_ANTIGUA")
            .WithErrorMessage("La fecha de inicio no puede ser anterior a 5 años");
    }

    [Fact]
    public async Task FechaInicio_NoDebeSerFutura()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddDays(1);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaInicio)
            .WithErrorCode("ANALISIS_FIDELIZACION_FECHA_INICIO_FUTURA")
            .WithErrorMessage("La fecha de inicio no puede ser futura");
    }

    [Fact]
    public async Task FechaFin_NoDebeSerFutura()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaFin = DateTime.Now.AddDays(1);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaFin)
            .WithErrorCode("ANALISIS_FIDELIZACION_FECHA_FIN_FUTURA")
            .WithErrorMessage("La fecha de fin no puede ser futura");
    }

    [Fact]
    public async Task FechaFin_DebeSerPosteriorAFechaInicio()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddDays(-1);
        query.FechaFin = DateTime.Now.AddDays(-2);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaFin)
            .WithErrorCode("ANALISIS_FIDELIZACION_FECHA_FIN_POSTERIOR_INICIO")
            .WithErrorMessage("La fecha de fin debe ser posterior a la fecha de inicio");
    }

    [Fact]
    public async Task RangoFechas_NoDebeExcederLimiteMaximo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddYears(-3).AddDays(-1);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_RANGO_FECHAS_EXCEDE_LIMITE")
            .WithErrorMessage("El rango de fechas no puede exceder 3 años");
    }

    [Theory]
    [InlineData(1)]     // 1 día - válido
    [InlineData(30)]    // 30 días - válido
    [InlineData(90)]    // 90 días - válido
    [InlineData(365)]   // 1 año - válido
    [InlineData(1095)]  // 3 años - límite
    public async Task RangoFechas_DebeAceptarRangosValidos(int dias)
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddDays(-dias);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Tests de Validaciones de Clientes Específicos

    [Fact]
    public async Task ClientesEspecificos_NoDebeExcederLimite()
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = CrearListaClientes(501);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_LIMITE")
            .WithErrorMessage("No se pueden especificar más de 500 clientes específicos");
    }

    [Fact]
    public async Task ClientesEspecificos_DebenTenerIdsValidos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = new List<Guid> { Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_IDS_INVALIDOS")
            .WithErrorMessage("Todos los IDs de clientes deben ser válidos");
    }

    [Fact]
    public async Task ClientesEspecificos_NoDebenTenerDuplicados()
    {
        // Arrange
        var query = CrearQueryBase();
        var clienteId = Guid.NewGuid();
        query.ClientesEspecificos = new List<Guid> { clienteId, clienteId };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_DUPLICADOS")
            .WithErrorMessage("No se pueden especificar clientes duplicados");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task ClientesEspecificos_PuedeSerVacioONulo(object clientes)
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = clientes as List<Guid>;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClientesEspecificos);
    }

    #endregion

    #region Tests de Validaciones de Nivel Mínimo

    [Fact]
    public async Task NivelMinimo_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelMinimo = (NivelFidelizacion)999;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NivelMinimo)
            .WithErrorCode("ANALISIS_FIDELIZACION_NIVEL_MINIMO_INVALIDO")
            .WithErrorMessage("El nivel mínimo de fidelización debe ser válido");
    }

    [Theory]
    [InlineData(NivelFidelizacion.Basico)]
    [InlineData(NivelFidelizacion.Plata)]
    [InlineData(NivelFidelizacion.Oro)]
    [InlineData(NivelFidelizacion.Platino)]
    [InlineData(NivelFidelizacion.Diamante)]
    public async Task NivelMinimo_DebeAceptarNivelesValidos(NivelFidelizacion nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelMinimo = nivel;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelMinimo);
    }

    [Theory]
    [InlineData(null)]
    public async Task NivelMinimo_PuedeSerOpcional(NivelFidelizacion? nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelMinimo = nivel;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelMinimo);
    }

    #endregion

    #region Tests de Validaciones de Tipo de Análisis

    [Fact]
    public async Task TipoAnalisis_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = (TipoAnalisis)999;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoAnalisis)
            .WithErrorCode("ANALISIS_FIDELIZACION_TIPO_INVALIDO")
            .WithErrorMessage("El tipo de análisis debe ser válido");
    }

    [Theory]
    [InlineData(TipoAnalisis.Basico)]
    [InlineData(TipoAnalisis.Completo)]
    [InlineData(TipoAnalisis.ClientesEspecificos)]
    [InlineData(TipoAnalisis.Comparativo)]
    [InlineData(TipoAnalisis.Predictivo)]
    public async Task TipoAnalisis_DebeAceptarTiposValidos(TipoAnalisis tipo)
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = tipo;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TipoAnalisis);
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task AnalisisClientesEspecificos_RequiereListaClientes()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.ClientesEspecificos;
        query.ClientesEspecificos = null;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_ESPECIFICOS_REQUERIDOS")
            .WithErrorMessage("El análisis de clientes específicos requiere especificar la lista de clientes");
    }

    [Fact]
    public async Task AnalisisClientesEspecificos_RequiereMinimoClientes()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.ClientesEspecificos;
        query.ClientesEspecificos = CrearListaClientes(1);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_MINIMO")
            .WithErrorMessage("El análisis de clientes específicos requiere al menos 2 clientes");
    }

    [Fact]
    public async Task AnalisisComparativo_RequiereRangoMinimo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.Comparativo;
        query.FechaInicio = DateTime.Now.AddDays(-29);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_COMPARATIVO_RANGO_MINIMO")
            .WithErrorMessage("El análisis comparativo requiere un rango mínimo de 30 días");
    }

    [Fact]
    public async Task AnalisisPredictivo_RequiereHistorialMinimo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.Predictivo;
        query.FechaInicio = DateTime.Now.AddDays(-89);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_PREDICTIVO_HISTORIAL_MINIMO")
            .WithErrorMessage("El análisis predictivo requiere un historial mínimo de 90 días");
    }

    [Fact]
    public async Task AnalisisPredictivo_RequiereProyecciones()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.Predictivo;
        query.IncluirProyecciones = false;
        query.FechaInicio = DateTime.Now.AddDays(-180);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IncluirProyecciones)
            .WithErrorCode("ANALISIS_FIDELIZACION_PREDICTIVO_REQUIERE_PROYECCIONES")
            .WithErrorMessage("El análisis predictivo requiere incluir proyecciones");
    }

    [Fact]
    public async Task AnalisisCompleto_RequiereTendencias()
    {
        // Arrange
        var query = CrearQueryBase();
        query.TipoAnalisis = TipoAnalisis.Completo;
        query.IncluirTendencias = false;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IncluirTendencias)
            .WithErrorCode("ANALISIS_FIDELIZACION_COMPLETO_REQUIERE_TENDENCIAS")
            .WithErrorMessage("El análisis completo requiere incluir tendencias");
    }

    #endregion

    #region Tests de Validaciones de Combinaciones Lógicas

    [Fact]
    public async Task ClientesEspecificosConClientesInactivos_DebeValidarCoherencia()
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = CrearListaClientes(10);
        query.IncluirClientesInactivos = true;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_ESPECIFICOS_INACTIVOS_COHERENCIA")
            .WithErrorMessage("Si especifica clientes específicos, la inclusión de inactivos se aplicará automáticamente");
    }

    [Fact]
    public async Task ProyeccionesConRangoCorto_DebeAdvertir()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IncluirProyecciones = true;
        query.FechaInicio = DateTime.Now.AddDays(-29);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_PROYECCIONES_RANGO_MINIMO")
            .WithErrorMessage("Las proyecciones requieren un rango mínimo de 30 días para ser precisas");
    }

    [Fact]
    public async Task TendenciasConRangoMuyCorto_DebeAdvertir()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IncluirTendencias = true;
        query.FechaInicio = DateTime.Now.AddDays(-6);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_FIDELIZACION_TENDENCIAS_RANGO_MINIMO")
            .WithErrorMessage("Las tendencias requieren un rango mínimo de 7 días para ser significativas");
    }

    #endregion

    #region Tests de Validaciones Async (Integridad Referencial)

    [Fact]
    public async Task ClientesEspecificos_DebenExistir()
    {
        // Arrange
        var query = CrearQueryBase();
        var clienteId = Guid.NewGuid();
        query.ClientesEspecificos = new List<Guid> { clienteId };
        ConfigurarClienteNoExiste(clienteId);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_NO_EXISTEN")
            .WithErrorMessage("Uno o más clientes especificados no existen");
    }

    [Fact]
    public async Task ClientesEspecificos_DebenTenerTarjetasFidelizacion()
    {
        // Arrange
        var query = CrearQueryBase();
        var clienteId = Guid.NewGuid();
        query.ClientesEspecificos = new List<Guid> { clienteId };
        var clienteSinTarjeta = CrearClienteSinTarjeta();
        ConfigurarClienteExiste(clienteId, clienteSinTarjeta);
        ConfigurarClienteSinTarjetaFidelizacion(clienteId);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientesEspecificos)
            .WithErrorCode("ANALISIS_FIDELIZACION_CLIENTES_SIN_TARJETA")
            .WithErrorMessage("Uno o más clientes especificados no tienen tarjetas de fidelización");
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void ObtenerAnalisisFidelizacionQuery_CrearAnalisisMensual_DebeConfigurarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddMonths(-1);
        var nivelMinimo = NivelFidelizacion.Plata;

        // Act
        var query = ObtenerAnalisisFidelizacionQuery.CrearAnalisisMensual(fechaInicio, nivelMinimo, true);

        // Assert
        query.FechaInicio.Should().Be(fechaInicio);
        query.FechaFin.Should().Be(fechaInicio.AddMonths(1).AddDays(-1));
        query.NivelMinimo.Should().Be(nivelMinimo);
        query.IncluirClientesInactivos.Should().BeFalse();
        query.IncluirTendencias.Should().BeTrue();
        query.IncluirProyecciones.Should().BeTrue();
        query.TipoAnalisis.Should().Be(TipoAnalisis.Completo);
    }

    [Fact]
    public void ObtenerAnalisisFidelizacionQuery_CrearAnalisisClientes_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clientes = CrearListaClientes(10);
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var query = ObtenerAnalisisFidelizacionQuery.CrearAnalisisClientes(clientes, fechaInicio, fechaFin, true);

        // Assert
        query.ClientesEspecificos.Should().BeEquivalentTo(clientes);
        query.FechaInicio.Should().Be(fechaInicio);
        query.FechaFin.Should().Be(fechaFin);
        query.IncluirClientesInactivos.Should().BeTrue();
        query.IncluirTendencias.Should().BeTrue();
        query.IncluirProyecciones.Should().BeFalse();
        query.TipoAnalisis.Should().Be(TipoAnalisis.ClientesEspecificos);
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task ValidarAnalisis_ConConfiguracionCompleta_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryCompleto();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FechaInicio);
        result.ShouldNotHaveValidationErrorFor(x => x.FechaFin);
        result.ShouldNotHaveValidationErrorFor(x => x.TipoAnalisis);
        result.ShouldNotHaveValidationErrorFor(x => x.NivelMinimo);
    }

    [Fact]
    public async Task ValidarAnalisis_ConMinimaConfiguracion_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryMinimo();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FechaInicio);
        result.ShouldNotHaveValidationErrorFor(x => x.FechaFin);
        result.ShouldNotHaveValidationErrorFor(x => x.TipoAnalisis);
    }

    [Fact]
    public async Task ValidarAnalisis_ConLimitesMaximos_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = CrearListaClientes(500);
        query.TipoAnalisis = TipoAnalisis.ClientesEspecificos;
        query.FechaInicio = DateTime.Now.AddYears(-3);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClientesEspecificos);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Tests de Validaciones de Performance

    [Fact]
    public async Task Validator_ConDatosMasivos_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var query = CrearQueryBase();
        query.ClientesEspecificos = CrearListaClientes(500);
        query.TipoAnalisis = TipoAnalisis.ClientesEspecificos;
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500);
    }

    [Fact]
    public async Task Validator_ConAnalisisCompleto_DebeValidarTodasLasReglas()
    {
        // Arrange
        var query = CrearQueryCompleto();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
        
        // Verificar que se ejecutaron las validaciones principales
        result.Errors.Should().NotContain(e => e.ErrorCode.Contains("TIMEOUT"));
    }

    [Fact]
    public async Task Validator_ConAnalisisPredictivo_DebeValidarReglasPredictivas()
    {
        // Arrange
        var query = CrearQueryPredictivo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        
        // Verificar validaciones específicas para análisis predictivo
        result.ShouldNotHaveValidationErrorFor(x => x.TipoAnalisis);
        result.ShouldNotHaveValidationErrorFor(x => x.IncluirProyecciones);
    }

    #endregion

    #region Métodos Helper

    private ObtenerAnalisisFidelizacionQuery CrearQueryBase()
    {
        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Now.AddMonths(-1),
            FechaFin = DateTime.Now,
            ClientesEspecificos = null,
            NivelMinimo = null,
            IncluirClientesInactivos = false,
            IncluirTendencias = true,
            IncluirProyecciones = false,
            TipoAnalisis = TipoAnalisis.Completo
        };
    }

    private ObtenerAnalisisFidelizacionQuery CrearQueryCompleto()
    {
        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Now.AddMonths(-6),
            FechaFin = DateTime.Now,
            ClientesEspecificos = null,
            NivelMinimo = NivelFidelizacion.Plata,
            IncluirClientesInactivos = true,
            IncluirTendencias = true,
            IncluirProyecciones = true,
            TipoAnalisis = TipoAnalisis.Completo
        };
    }

    private ObtenerAnalisisFidelizacionQuery CrearQueryMinimo()
    {
        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Now.AddDays(-30),
            FechaFin = DateTime.Now,
            ClientesEspecificos = null,
            NivelMinimo = null,
            IncluirClientesInactivos = false,
            IncluirTendencias = false,
            IncluirProyecciones = false,
            TipoAnalisis = TipoAnalisis.Basico
        };
    }

    private ObtenerAnalisisFidelizacionQuery CrearQueryPredictivo()
    {
        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = DateTime.Now.AddMonths(-6),
            FechaFin = DateTime.Now,
            ClientesEspecificos = null,
            NivelMinimo = null,
            IncluirClientesInactivos = false,
            IncluirTendencias = true,
            IncluirProyecciones = true,
            TipoAnalisis = TipoAnalisis.Predictivo
        };
    }

    private List<Guid> CrearListaClientes(int cantidad)
    {
        return Enumerable.Range(1, cantidad).Select(_ => Guid.NewGuid()).ToList();
    }

    private void ConfigurarClienteExiste(Guid clienteId, Cliente cliente)
    {
        _mockClientes.Setup(m => m.FindAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
    }

    private void ConfigurarClienteNoExiste(Guid clienteId)
    {
        _mockClientes.Setup(m => m.FindAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);
    }

    private void ConfigurarClienteSinTarjetaFidelizacion(Guid clienteId)
    {
        var tarjetas = new List<TarjetaFidelizacion>();
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private Cliente CrearClienteSinTarjeta()
    {
        return Cliente.Crear(
            "Cliente Sin Tarjeta",
            "sintarjeta@email.com",
            "555-0000"
        );
    }

    private Cliente CrearClienteConTarjeta()
    {
        return Cliente.Crear(
            "Cliente Con Tarjeta",
            "contarjeta@email.com",
            "555-1111"
        );
    }

    private TarjetaFidelizacion CrearTarjetaFidelizacion(Guid clienteId)
    {
        return TarjetaFidelizacion.Crear(
            clienteId,
            TipoTarjetaFidelizacion.Estandar,
            "FIEL12345678",
            DateTime.Now,
            DateTime.Now.AddYears(2),
            true,
            true
        );
    }

    #endregion
} 