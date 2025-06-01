namespace RestaurantePro.Application.UnitTests.Inventario.Reportes.Validators;

/// <summary>
/// Tests para ObtenerAnalisisInventarioValidator
/// Valida reglas de negocio para obtener análisis completo de inventario
/// </summary>
public class ObtenerAnalisisInventarioValidatorTests
{
    private readonly ObtenerAnalisisInventarioValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Ingrediente>> _mockIngredientes;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;

    public ObtenerAnalisisInventarioValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockIngredientes = MockDbSetHelper.CreateMockDbSet<Ingrediente>();
        _mockUsuarios = MockDbSetHelper.CreateMockDbSet<Usuario>();
        
        _mockContext.Setup(c => c.Ingredientes).Returns(_mockIngredientes.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);
        
        _validator = new ObtenerAnalisisInventarioValidator(_mockContext.Object);
    }

    #region Tests de Validaciones de Fechas

    [Fact]
    public async Task FechaInicio_NoDebeSerMuyAntigua()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddYears(-3);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaInicio)
            .WithErrorCode("ANALISIS_INVENTARIO_FECHA_INICIO_ANTIGUA")
            .WithErrorMessage("La fecha de inicio no puede ser anterior a 2 años");
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
            .WithErrorCode("ANALISIS_INVENTARIO_FECHA_INICIO_FUTURA")
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
            .WithErrorCode("ANALISIS_INVENTARIO_FECHA_FIN_FUTURA")
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
            .WithErrorCode("ANALISIS_INVENTARIO_FECHA_FIN_POSTERIOR_INICIO")
            .WithErrorMessage("La fecha de fin debe ser posterior a la fecha de inicio");
    }

    [Fact]
    public async Task RangoFechas_NoDebeExcederLimiteMaximo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaInicio = DateTime.Now.AddYears(-1).AddDays(-1);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_INVENTARIO_RANGO_FECHAS_EXCEDE_LIMITE")
            .WithErrorMessage("El rango de fechas no puede exceder 1 año");
    }

    [Theory]
    [InlineData(1)]    // 1 día - válido
    [InlineData(30)]   // 30 días - válido
    [InlineData(90)]   // 90 días - válido
    [InlineData(365)]  // 365 días - límite
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

    #region Tests de Validaciones de Ingredientes Específicos

    [Fact]
    public async Task IngredientesEspecificos_NoDebeExcederLimite()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IngredientesEspecificos = CrearListaIngredientes(101);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_LIMITE")
            .WithErrorMessage("No se pueden especificar más de 100 ingredientes específicos");
    }

    [Fact]
    public async Task IngredientesEspecificos_DebenTenerIdsValidos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IngredientesEspecificos = new List<Guid> { Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_IDS_INVALIDOS")
            .WithErrorMessage("Todos los IDs de ingredientes deben ser válidos");
    }

    [Fact]
    public async Task IngredientesEspecificos_NoDebenTenerDuplicados()
    {
        // Arrange
        var query = CrearQueryBase();
        var ingredienteId = Guid.NewGuid();
        query.IngredientesEspecificos = new List<Guid> { ingredienteId, ingredienteId };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_DUPLICADOS")
            .WithErrorMessage("No se pueden especificar ingredientes duplicados");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task IngredientesEspecificos_PuedeSerVacioONulo(object ingredientes)
    {
        // Arrange
        var query = CrearQueryBase();
        query.IngredientesEspecificos = ingredientes as List<Guid>;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IngredientesEspecificos);
    }

    #endregion

    #region Tests de Validaciones de Categorías Específicas

    [Fact]
    public async Task CategoriasEspecificas_NoDebeExcederLimite()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriasEspecificas = CrearListaCategorias(21);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoriasEspecificas)
            .WithErrorCode("ANALISIS_INVENTARIO_CATEGORIAS_LIMITE")
            .WithErrorMessage("No se pueden especificar más de 20 categorías específicas");
    }

    [Theory]
    [InlineData("Carnes")]
    [InlineData("Verduras")]
    [InlineData("Lacteos")]
    [InlineData("Cereales")]
    [InlineData("Especias")]
    [InlineData("Bebidas")]
    [InlineData("Condimentos")]
    public async Task CategoriasEspecificas_DebeValidarCategoriasValidas(string categoria)
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriasEspecificas = new List<string> { categoria };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoriasEspecificas);
    }

    [Fact]
    public async Task CategoriasEspecificas_DebeRechazarCategoriasInvalidas()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriasEspecificas = new List<string> { "CategoriaInvalida" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoriasEspecificas)
            .WithErrorCode("ANALISIS_INVENTARIO_CATEGORIAS_INVALIDAS")
            .WithErrorMessage("Las categorías deben ser válidas: Carnes, Verduras, Lacteos, Cereales, Especias, Bebidas, Condimentos");
    }

    [Fact]
    public async Task CategoriasEspecificas_NoDebenTenerDuplicadas()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriasEspecificas = new List<string> { "Carnes", "Carnes" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoriasEspecificas)
            .WithErrorCode("ANALISIS_INVENTARIO_CATEGORIAS_DUPLICADAS")
            .WithErrorMessage("No se pueden especificar categorías duplicadas");
    }

    #endregion

    #region Tests de Validaciones de Nivel de Análisis

    [Fact]
    public async Task NivelAnalisis_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelAnalisis = (NivelAnalisisInventario)999;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NivelAnalisis)
            .WithErrorCode("ANALISIS_INVENTARIO_NIVEL_INVALIDO")
            .WithErrorMessage("El nivel de análisis debe ser válido");
    }

    [Theory]
    [InlineData(NivelAnalisisInventario.Basico)]
    [InlineData(NivelAnalisisInventario.Diario)]
    [InlineData(NivelAnalisisInventario.Semanal)]
    [InlineData(NivelAnalisisInventario.Completo)]
    [InlineData(NivelAnalisisInventario.Criticos)]
    [InlineData(NivelAnalisisInventario.Financiero)]
    public async Task NivelAnalisis_DebeAceptarNivelesValidos(NivelAnalisisInventario nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelAnalisis = nivel;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelAnalisis);
    }

    #endregion

    #region Tests de Validaciones de Umbral Stock Crítico

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UmbralStockCritico_DebeSerPositivo(decimal umbral)
    {
        // Arrange
        var query = CrearQueryBase();
        query.UmbralStockCritico = umbral;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UmbralStockCritico)
            .WithErrorCode("ANALISIS_INVENTARIO_UMBRAL_STOCK_POSITIVO")
            .WithErrorMessage("El umbral de stock crítico debe ser mayor a 0");
    }

    [Fact]
    public async Task UmbralStockCritico_NoDebeExcederMaximo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.UmbralStockCritico = 10001;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UmbralStockCritico)
            .WithErrorCode("ANALISIS_INVENTARIO_UMBRAL_STOCK_MAXIMO")
            .WithErrorMessage("El umbral de stock crítico no puede exceder 10,000 unidades");
    }

    [Theory]
    [InlineData(null)]
    public async Task UmbralStockCritico_PuedeSerOpcional(decimal? umbral)
    {
        // Arrange
        var query = CrearQueryBase();
        query.UmbralStockCritico = umbral;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UmbralStockCritico);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task UmbralStockCritico_DebeAceptarValoresValidos(decimal umbral)
    {
        // Arrange
        var query = CrearQueryBase();
        query.UmbralStockCritico = umbral;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UmbralStockCritico);
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task AnalisisCriticos_RequiereIngredientesEspecificos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelAnalisis = NivelAnalisisInventario.Criticos;
        query.IngredientesEspecificos = null;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_CRITICOS_REQUIERE_INGREDIENTES")
            .WithErrorMessage("El análisis de críticos requiere especificar ingredientes específicos");
    }

    [Fact]
    public async Task AnalisisFinanciero_RequiereCalculosCostos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelAnalisis = NivelAnalisisInventario.Financiero;
        query.IncluirAnalisisCostos = false;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IncluirAnalisisCostos)
            .WithErrorCode("ANALISIS_INVENTARIO_FINANCIERO_REQUIERE_COSTOS")
            .WithErrorMessage("El análisis financiero requiere incluir análisis de costos");
    }

    [Fact]
    public async Task AnalisisCompleto_RequiereTodasLasOpciones()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelAnalisis = NivelAnalisisInventario.Completo;
        query.IncluirPrediccionesStock = false;
        query.IncluirAnalisisCostos = false;
        query.IncluirAlertasAvanzadas = false;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_INVENTARIO_COMPLETO_REQUIERE_OPCIONES")
            .WithErrorMessage("El análisis completo requiere incluir predicciones, costos y alertas avanzadas");
    }

    [Fact]
    public async Task MovimientosDetallados_RequiereRangoLimitado()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IncluirMovimientosDetallados = true;
        query.FechaInicio = DateTime.Now.AddDays(-91);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_INVENTARIO_MOVIMIENTOS_RANGO_LIMITADO")
            .WithErrorMessage("Los movimientos detallados requieren un rango máximo de 90 días");
    }

    #endregion

    #region Tests de Validaciones de Combinaciones Lógicas

    [Fact]
    public async Task IngredientesYCategorias_NoDebenEspecificarseSimultaneamente()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IngredientesEspecificos = CrearListaIngredientes(5);
        query.CategoriasEspecificas = new List<string> { "Carnes", "Verduras" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_CATEGORIAS_EXCLUSIVOS")
            .WithErrorMessage("No se pueden especificar ingredientes específicos y categorías al mismo tiempo");
    }

    [Fact]
    public async Task PrediccionesConRangoCorto_DebeAdvertir()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IncluirPrediccionesStock = true;
        query.FechaInicio = DateTime.Now.AddDays(-6);
        query.FechaFin = DateTime.Now;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("ANALISIS_INVENTARIO_PREDICCIONES_RANGO_MINIMO")
            .WithErrorMessage("Las predicciones requieren un rango mínimo de 7 días para ser precisas");
    }

    #endregion

    #region Tests de Validaciones Async (Integridad Referencial)

    [Fact]
    public async Task IngredientesEspecificos_DebenExistir()
    {
        // Arrange
        var query = CrearQueryBase();
        var ingredienteId = Guid.NewGuid();
        query.IngredientesEspecificos = new List<Guid> { ingredienteId };
        ConfigurarIngredienteNoExiste(ingredienteId);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_NO_EXISTEN")
            .WithErrorMessage("Uno o más ingredientes especificados no existen");
    }

    [Fact]
    public async Task IngredientesEspecificos_DebenEstarActivos()
    {
        // Arrange
        var query = CrearQueryBase();
        var ingredienteId = Guid.NewGuid();
        query.IngredientesEspecificos = new List<Guid> { ingredienteId };
        var ingredienteInactivo = CrearIngredienteInactivo();
        ConfigurarIngredienteExiste(ingredienteId, ingredienteInactivo);

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientesEspecificos)
            .WithErrorCode("ANALISIS_INVENTARIO_INGREDIENTES_INACTIVOS")
            .WithErrorMessage("Uno o más ingredientes especificados están inactivos");
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void ObtenerAnalisisInventarioQuery_CrearAnalisisDiario_DebeConfigurarCorrectamente()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisDiario(fecha, true, true);

        // Assert
        query.FechaInicio.Should().Be(fecha);
        query.FechaFin.Should().Be(fecha);
        query.IncluirPrediccionesStock.Should().BeTrue();
        query.IncluirAnalisisCostos.Should().BeTrue();
        query.IncluirMovimientosDetallados.Should().BeFalse();
        query.IncluirAlertasAvanzadas.Should().BeTrue();
        query.IncluirRecomendacionesCompra.Should().BeTrue();
        query.NivelAnalisis.Should().Be(NivelAnalisisInventario.Diario);
    }

    [Fact]
    public void ObtenerAnalisisInventarioQuery_CrearAnalisisSemanal_DebeConfigurarCorrectamente()
    {
        // Arrange
        var fechaInicio = DateTime.Today.AddDays(-7);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisSemanal(fechaInicio, NivelAnalisisInventario.Completo);

        // Assert
        query.FechaInicio.Should().Be(fechaInicio);
        query.FechaFin.Should().Be(fechaInicio.AddDays(7));
        query.IncluirPrediccionesStock.Should().BeTrue();
        query.IncluirAnalisisCostos.Should().BeTrue();
        query.IncluirMovimientosDetallados.Should().BeTrue();
        query.IncluirAlertasAvanzadas.Should().BeTrue();
        query.IncluirRecomendacionesCompra.Should().BeTrue();
        query.NivelAnalisis.Should().Be(NivelAnalisisInventario.Completo);
    }

    [Fact]
    public void ObtenerAnalisisInventarioQuery_CrearAnalisisCriticos_DebeConfigurarCorrectamente()
    {
        // Arrange
        var ingredientes = CrearListaIngredientes(10);
        var umbral = 5m;

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisCriticos(ingredientes, umbral, true);

        // Assert
        query.IngredientesEspecificos.Should().BeEquivalentTo(ingredientes);
        query.UmbralStockCritico.Should().Be(umbral);
        query.IncluirPrediccionesStock.Should().BeTrue();
        query.IncluirAnalisisCostos.Should().BeFalse();
        query.IncluirMovimientosDetallados.Should().BeTrue();
        query.IncluirAlertasAvanzadas.Should().BeTrue();
        query.IncluirRecomendacionesCompra.Should().BeTrue();
        query.NivelAnalisis.Should().Be(NivelAnalisisInventario.Criticos);
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
        result.ShouldNotHaveValidationErrorFor(x => x.NivelAnalisis);
        result.ShouldNotHaveValidationErrorFor(x => x.UmbralStockCritico);
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
        result.ShouldNotHaveValidationErrorFor(x => x.NivelAnalisis);
    }

    #endregion

    #region Tests de Validaciones de Performance

    [Fact]
    public async Task Validator_ConDatosMasivos_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var query = CrearQueryBase();
        query.IngredientesEspecificos = CrearListaIngredientes(100);
        query.CategoriasEspecificas = null; // Para evitar conflicto
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
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

    #endregion

    #region Métodos Helper

    private ObtenerAnalisisInventarioQuery CrearQueryBase()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Now.AddDays(-30),
            FechaFin = DateTime.Now,
            IngredientesEspecificos = null,
            CategoriasEspecificas = null,
            IncluirPrediccionesStock = true,
            IncluirAnalisisCostos = true,
            IncluirMovimientosDetallados = false,
            IncluirAlertasAvanzadas = true,
            IncluirRecomendacionesCompra = true,
            NivelAnalisis = NivelAnalisisInventario.Completo,
            UmbralStockCritico = 10
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryCompleto()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Now.AddDays(-90),
            FechaFin = DateTime.Now,
            IngredientesEspecificos = CrearListaIngredientes(50),
            CategoriasEspecificas = null, // Exclusivo con ingredientes específicos
            IncluirPrediccionesStock = true,
            IncluirAnalisisCostos = true,
            IncluirMovimientosDetallados = true,
            IncluirAlertasAvanzadas = true,
            IncluirRecomendacionesCompra = true,
            NivelAnalisis = NivelAnalisisInventario.Completo,
            UmbralStockCritico = 5
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryMinimo()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Now.AddDays(-7),
            FechaFin = DateTime.Now,
            IngredientesEspecificos = null,
            CategoriasEspecificas = null,
            IncluirPrediccionesStock = false,
            IncluirAnalisisCostos = false,
            IncluirMovimientosDetallados = false,
            IncluirAlertasAvanzadas = false,
            IncluirRecomendacionesCompra = false,
            NivelAnalisis = NivelAnalisisInventario.Basico,
            UmbralStockCritico = null
        };
    }

    private List<Guid> CrearListaIngredientes(int cantidad)
    {
        return Enumerable.Range(1, cantidad).Select(_ => Guid.NewGuid()).ToList();
    }

    private List<string> CrearListaCategorias(int cantidad)
    {
        var categorias = new[] { "Carnes", "Verduras", "Lacteos", "Cereales", "Especias", "Bebidas", "Condimentos" };
        var resultado = new List<string>();
        
        for (int i = 0; i < cantidad; i++)
        {
            resultado.Add($"{categorias[i % categorias.Length]}_{i}");
        }
        
        return resultado;
    }

    private void ConfigurarIngredienteExiste(Guid ingredienteId, Ingrediente ingrediente)
    {
        _mockIngredientes.Setup(m => m.FindAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);
    }

    private void ConfigurarIngredienteNoExiste(Guid ingredienteId)
    {
        _mockIngredientes.Setup(m => m.FindAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingrediente?)null);
    }

    private Ingrediente CrearIngredienteInactivo()
    {
        var ingrediente = Ingrediente.Crear(
            "Ingrediente Inactivo",
            "INACT001",
            UnidadMedida.Kilogramos,
            CategoriaIngrediente.Especias,
            RotacionIngrediente.Baja,
            TemporadaIngrediente.TodoElAño,
            100,
            10,
            500,
            1.50m
        );
        ingrediente.Desactivar("Ingrediente inactivo para tests");
        return ingrediente;
    }

    private Ingrediente CrearIngredienteActivo()
    {
        return Ingrediente.Crear(
            "Ingrediente Activo",
            "ACT001",
            UnidadMedida.Kilogramos,
            CategoriaIngrediente.Carnes,
            RotacionIngrediente.Alta,
            TemporadaIngrediente.TodoElAño,
            100,
            10,
            500,
            2.50m
        );
    }

    #endregion
} 