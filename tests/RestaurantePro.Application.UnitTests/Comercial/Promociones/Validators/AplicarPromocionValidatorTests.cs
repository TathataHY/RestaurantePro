namespace RestaurantePro.Application.UnitTests.Comercial.Promociones.Validators;

/// <summary>
/// Tests para AplicarPromocionValidator
/// Valida reglas de negocio para aplicación de promociones comerciales
/// </summary>
public class AplicarPromocionValidatorTests
{
    private readonly AplicarPromocionValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;

    public AplicarPromocionValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new AplicarPromocionValidator(_mockContext.Object);
    }

    #region Tests de Validaciones Básicas

    [Fact]
    public async Task PromocionId_DebeSerObligatorio_CuandoCodigoEsVacio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PromocionId = Guid.Empty;
        command.CodigoPromocion = string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_ID_REQUERIDO");
    }

    [Fact]
    public async Task CodigoPromocion_DebeSerObligatorio_CuandoIdEsVacio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PromocionId = Guid.Empty;
        command.CodigoPromocion = null!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_CODIGO_REQUERIDO");
    }

    [Fact]
    public async Task DebeEspecificar_FacturaOComanda()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FacturaId = null!;
        command.ComandaId = null!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_DESTINO_REQUERIDO");
    }

    [Fact]
    public async Task NoDebeEspecificar_FacturaYComandaSimultaneamente()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FacturaId = Guid.NewGuid();
        command.ComandaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_DESTINO_MULTIPLE");
    }

    [Fact]
    public async Task TipoAplicacion_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoAplicacion = (TipoAplicacionPromocion)999; // Valor inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_TIPO_INVALIDO");
    }

    [Fact]
    public async Task ProductosIds_DebeSerObligatorio_CuandoTipoEsProductosEspecificos()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos;
        command.ProductosIds = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_PRODUCTOS_REQUERIDOS");
    }

    [Fact]
    public async Task ProductosIds_DebeRespetarLimiteMaximo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos;
        command.ProductosIds = Enumerable.Range(1, 51).Select(_ => Guid.NewGuid()).ToList(); // 51 productos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_PRODUCTOS_LIMITE");
    }

    [Fact]
    public async Task NotasAplicacion_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NotasAplicacion = new string('a', 501); // 501 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "APLICAR_PROMOCION_NOTAS_LONGITUD");
    }

    #endregion

    #region Tests de Validaciones Exitosas

    [Fact]
    public async Task CommandValido_ConFactura_DeberiaSerValido()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            ValidarRestriccionesCliente = true,
            ValidarLimitesUso = true
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CommandValido_ConComanda_DeberiaSerValido()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            CodigoPromocion = "PROMO2025",
            ComandaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.PorCategoria,
            ClienteId = Guid.NewGuid(),
            NotasAplicacion = "Promoción aplicada correctamente"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CommandValido_ConProductosEspecificos_DeberiaSerValido()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos,
            ProductosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            AutorizadoPor = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(TipoAplicacionPromocion.FacturaCompleta)]
    [InlineData(TipoAplicacionPromocion.ProductosEspecificos)]
    [InlineData(TipoAplicacionPromocion.PorCategoria)]
    [InlineData(TipoAplicacionPromocion.PorCantidadMinima)]
    [InlineData(TipoAplicacionPromocion.PorMontoMinimo)]
    public async Task TipoAplicacion_DebeAceptarTiposValidos(TipoAplicacionPromocion tipo)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoAplicacion = tipo;
        
        if (tipo == TipoAplicacionPromocion.ProductosEspecificos)
        {
            command.ProductosIds = new List<Guid> { Guid.NewGuid() };
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task NotasAplicacion_PuedeSerNull()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NotasAplicacion = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ClienteId_PuedeSerNull()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ClienteId = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AutorizadoPor_PuedeSerNull()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.AutorizadoPor = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DatosAdicionales_PuedeSerNull()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.DatosAdicionales = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Métodos Helper

    private AplicarPromocionCommand CrearCommandoBase()
    {
        return new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            ValidarRestriccionesCliente = true,
            ValidarLimitesUso = true
        };
    }

    #endregion
} 