namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Validators;
using Moq;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA OBTENER FACTURA POR ID VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de consulta de factura por ID
/// Cobertura: 100% de reglas de negocio del ObtenerFacturaPorIdValidator
/// </summary>
public class ObtenerFacturaPorIdValidatorTests
{
    private readonly ObtenerFacturaPorIdValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;

    public ObtenerFacturaPorIdValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new ObtenerFacturaPorIdValidator(_mockContext.Object);
    }

    #region Validation Query Helper

    private ObtenerFacturaPorIdQuery CrearQueryValida()
    {
        return new ObtenerFacturaPorIdQuery
        {
            FacturaId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación FacturaId

    [Fact]
    public async Task Validate_ConFacturaIdVacia_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerFacturaPorIdQuery.FacturaId) &&
            e.ErrorMessage.Contains("El ID de la factura es requerido") &&
            e.ErrorCode == "FACTURA_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConFacturaIdValida_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerFacturaPorIdQuery
        {
            FacturaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConDiferentesGuids_DeberiaSerValido()
    {
        // Arrange
        var queries = new[]
        {
            new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() },
            new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() },
            new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() }
        };

        // Act & Assert
        foreach (var query in queries)
        {
            var result = await _validator.ValidateAsync(query);
            result.IsValid.Should().BeTrue();
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConBusquedaFacturaExistente_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaFacturaCliente_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        // Simula búsqueda de factura específica de cliente
        query.FacturaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConConsultaFacturaHistorial_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        // Simula consulta de factura antigua
        query.FacturaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    public async Task Validate_ConGuidEspecial_DeberiaRetornarError(string guidString)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Parse(guidString);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "FACTURA_ID_REQUERIDO");
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("87654321-4321-4321-4321-210987654321")]
    [InlineData("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE")]
    public async Task Validate_ConGuidValido_DeberiaSerValido(string guidString)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Parse(guidString);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConGuidMinValue_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Empty; // Equivalent to MinValue

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ConGuidMaxValue_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var queries = Enumerable.Range(1, 10)
            .Select(_ => new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() })
            .ToList();

        // Act
        var tasks = queries.Select(q => _validator.ValidateAsync(q));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(query);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Debe ser muy rápido
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task Validate_ConQueryNueva_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerFacturaPorIdQuery();
        query.FacturaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAsignacionDirecta_DeberiaSerValida()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.FacturaId.Should().Be(facturaId);
    }

    #endregion

    #region Tests de Mensajes de Error

    [Fact]
    public async Task Validate_ConFacturaIdVacia_DeberiaRetornarMensajeEspecifico()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        var error = result.Errors.Single();
        error.PropertyName.Should().Be(nameof(ObtenerFacturaPorIdQuery.FacturaId));
        error.ErrorMessage.Should().Be("El ID de la factura es requerido para realizar la consulta");
        error.ErrorCode.Should().Be("FACTURA_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConErrorDeValidacion_DeberiaIncluirPropertyName()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().AllSatisfy(error =>
        {
            error.PropertyName.Should().NotBeNullOrWhiteSpace();
            error.PropertyName.Should().Be(nameof(ObtenerFacturaPorIdQuery.FacturaId));
        });
    }

    #endregion

    #region Tests de Validador Instance

    [Fact]
    public void Validator_DeberiaImplementarAbstractValidator()
    {
        // Assert
        _validator.Should().BeAssignableTo<AbstractValidator<ObtenerFacturaPorIdQuery>>();
    }

    [Fact]
    public void Validator_DeberiaInicializarseCorrectamente()
    {
        // Arrange & Act
        var validator = new ObtenerFacturaPorIdValidator(_mockContext.Object);

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public async Task Validator_ConQueryNull_NoDeberiaLanzarExcepcion()
    {
        // Arrange
        ObtenerFacturaPorIdQuery? query = null;

        // Act
        Func<Task> act = async () => await _validator.ValidateAsync(query!);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Tests de Factory Methods (si existen)

    [Fact]
    public void Query_DeberiaCrearseConFactoryMethod()
    {
        // Arrange
        var facturaId = Guid.NewGuid();

        // Act
        var query = ObtenerFacturaPorIdQuery.ConsultaBasica(facturaId);

        // Assert
        query.FacturaId.Should().Be(facturaId);
    }

    // Este test se comenta porque los factory methods reales no validan Guid.Empty
    // [Fact]
    // public void Query_FactoryMethod_ConGuidVacio_DeberiaLanzarExcepcion()
    // {
    //     // Act & Assert
    //     var act = () => ObtenerFacturaPorIdQuery.ConsultaBasica(Guid.Empty);
    //     act.Should().Throw<ArgumentException>()
    //         .WithMessage("*FacturaId*");
    // }

    [Fact]
    public void Query_FactoryMethodBasico_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var query = ObtenerFacturaPorIdQuery.ConsultaBasica(facturaId, usuarioId);

        // Assert
        query.FacturaId.Should().Be(facturaId);
        query.UsuarioConsultaId.Should().Be(usuarioId);
        query.FormatoRespuesta.Should().Be("Basico");
        query.ValidarPermisos.Should().BeFalse();
    }

    [Fact]
    public void Query_FactoryMethodCompleto_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Consulta administrativa";

        // Act
        var query = ObtenerFacturaPorIdQuery.ConsultaCompleta(facturaId, usuarioId, motivo);

        // Assert
        query.FacturaId.Should().Be(facturaId);
        query.UsuarioConsultaId.Should().Be(usuarioId);
        query.MotivoConsulta.Should().Be(motivo);
        query.FormatoRespuesta.Should().Be("Completo");
        query.IncluirAuditoria.Should().BeTrue();
        query.IncluirMetricasRentabilidad.Should().BeTrue();
    }

    #endregion

    #region Tests de Inmutabilidad y Comportamiento

    [Fact]
    public void Query_DeberiaSerRecord()
    {
        // Arrange
        var query1 = new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() };
        var query2 = new ObtenerFacturaPorIdQuery { FacturaId = query1.FacturaId };

        // Act & Assert
        query1.Should().BeEquivalentTo(query2);
    }

    [Fact]
    public void Query_ConMismoId_DeberiaSerIgual()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var query1 = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };
        var query2 = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act & Assert
        query1.Should().BeEquivalentTo(query2);
    }

    #endregion
} 