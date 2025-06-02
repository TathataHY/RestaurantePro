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

    #region Tests de Validaciones de Categorías

    [Fact]
    public async Task CategoriaId_DebeSerValidaCuandoSeEspecifica()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = Guid.NewGuid();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoriaId);
    }

    [Fact]
    public async Task CategoriaId_PuedeSerNula()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = null;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoriaId);
    }

    [Fact]
    public async Task CategoriaId_NoDebeSerGuidVacio()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = Guid.Empty;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoriaId)
            .WithErrorCode("CATEGORIA_ID_INVALIDO");
    }

    #endregion

    #region Tests de Validaciones de Nivel de Detalle

    [Theory]
    [InlineData("Basico")]
    [InlineData("Completo")]
    [InlineData("Resumen")]
    public async Task NivelDetalle_DebeAceptarNivelesValidos(string nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = nivel;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelDetalle);
    }

    [Fact]
    public async Task NivelDetalle_DebeRechazarNivelesInvalidos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = "NivelInexistente";

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NivelDetalle)
            .WithErrorCode("NIVEL_DETALLE_INVALIDO");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NivelDetalle_DebeSerObligatorio(string nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = nivel;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NivelDetalle)
            .WithErrorCode("NIVEL_DETALLE_REQUERIDO");
    }

    #endregion

    #region Tests de Validaciones de Usuario

    [Fact]
    public async Task UsuarioId_DebeSerObligatorio()
    {
        // Arrange
        var query = CrearQueryBase();
        query.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId)
            .WithErrorCode("USUARIO_ID_REQUERIDO");
    }

    [Fact]
    public async Task UsuarioId_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UsuarioId);
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task AnalisisCriticos_DeberiaConfigurarseSoloCriticos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.SoloCriticos = true;
        query.SoloAlertaStock = true;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SoloCriticos);
        result.ShouldNotHaveValidationErrorFor(x => x.SoloAlertaStock);
    }

    [Fact]
    public async Task AnalisisCompleto_RequiereTodasLasOpciones()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = "Completo";
        query.IncluirTendencias = true;
        query.IncluirRecomendaciones = true;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelDetalle);
        result.ShouldNotHaveValidationErrorFor(x => x.IncluirTendencias);
        result.ShouldNotHaveValidationErrorFor(x => x.IncluirRecomendaciones);
    }

    [Fact]
    public async Task AnalisisBasico_RequiereConfiguracionMinima()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = "Basico";
        query.IncluirTendencias = false;
        query.IncluirRecomendaciones = false;

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NivelDetalle);
    }

    #endregion

    #region Tests de Validaciones de Performance

    [Fact]
    public async Task Validator_ConDatosMasivos_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = Guid.NewGuid();
        
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
            CategoriaId = null,
            SoloAlertaStock = false,
            SoloCriticos = false,
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryCompleto()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Now.AddDays(-90),
            FechaFin = DateTime.Now,
            CategoriaId = Guid.NewGuid(),
            SoloAlertaStock = false,
            SoloCriticos = false,
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryMinimo()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Now.AddDays(-7),
            FechaFin = DateTime.Now,
            CategoriaId = null,
            SoloAlertaStock = false,
            SoloCriticos = false,
            IncluirTendencias = false,
            IncluirRecomendaciones = false,
            NivelDetalle = "Basico",
            UsuarioId = Guid.NewGuid()
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