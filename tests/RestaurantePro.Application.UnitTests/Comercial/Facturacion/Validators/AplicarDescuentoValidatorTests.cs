namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA APLICAR DESCUENTO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de aplicación de descuentos
/// Cobertura: 100% de reglas de negocio del AplicarDescuentoValidator
/// </summary>
public class AplicarDescuentoValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Factura>> _mockFacturas;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;
    private readonly Mock<DbSet<Producto>> _mockProductos;
    private readonly AplicarDescuentoValidator _validator;

    public AplicarDescuentoValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockFacturas = new Mock<DbSet<Factura>>();
        _mockUsuarios = new Mock<DbSet<Usuario>>();
        _mockProductos = new Mock<DbSet<Producto>>();
        
        _mockContext.Setup(c => c.Facturas).Returns(_mockFacturas.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);
        _mockContext.Setup(c => c.Productos).Returns(_mockProductos.Object);
        
        _validator = new AplicarDescuentoValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private AplicarDescuentoCommand CrearCommandValido()
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = Guid.NewGuid(),
            TipoDescuento = "General",
            Concepto = "Descuento promocional",
            Motivo = "Promoción especial del día para clientes frecuentes",
            Porcentaje = 10m,
            MontoFijo = 0m,
            Prioridad = 5,
            UsuarioAutorizaId = Guid.NewGuid(),
            ProductosEspecificos = new List<Guid>(),
            CategoriasAplicables = new List<string>(),
            MontoMinimoFactura = null,
            MontoMaximoDescuento = null,
            FechaExpiracion = null,
            CodigoAutorizacion = null,
            NotasAdicionales = "Descuento aplicado correctamente"
        };
    }

    #endregion

    #region Validación FacturaId

    [Fact]
    public async Task Validate_ConFacturaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.FacturaId) &&
            e.ErrorMessage.Contains("El ID de la factura es requerido"));
    }

    [Fact]
    public async Task Validate_ConFacturaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var facturaIdInexistente = Guid.NewGuid();
        command.FacturaId = facturaIdInexistente;

        _mockFacturas.Setup(f => f.FindAsync(facturaIdInexistente))
            .ReturnsAsync((Factura)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.FacturaId) &&
            e.ErrorMessage.Contains("La factura especificada no existe"));
    }

    #endregion

    #region Validación TipoDescuento

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConTipoDescuentoVacioONull_DeberiaRetornarError(string? tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = tipoInvalido!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.TipoDescuento) &&
            e.ErrorMessage.Contains("El tipo de descuento es requerido"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Validate_ConTipoDescuentoInvalido_DeberiaRetornarError(int tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = tipoInvalido.ToString();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.TipoDescuento) &&
            e.ErrorMessage.Contains("El tipo de descuento debe ser uno de"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task Validate_ConTipoDescuentoValido_NoDeberiaRetornarErrorDeTipo(int tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = tipoValido.ToString();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.TipoDescuento));
    }

    #endregion

    #region Validación Concepto

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConConceptoVacioONull_DeberiaRetornarError(string? conceptoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Concepto = conceptoInvalido!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Concepto) &&
            e.ErrorMessage.Contains("El concepto del descuento es requerido"));
    }

    [Fact]
    public async Task Validate_ConConceptoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Concepto = "Desc"; // Menos de 5 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Concepto) &&
            e.ErrorMessage.Contains("El concepto debe tener al menos 5 caracteres"));
    }

    [Fact]
    public async Task Validate_ConConceptoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Concepto = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Concepto) &&
            e.ErrorMessage.Contains("El concepto no puede exceder 200 caracteres"));
    }

    #endregion

    #region Validación Motivo

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoVacioONull_DeberiaRetornarError(string? motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoInvalido!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = "Descuento"; // Menos de 10 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
    }

    #endregion

    #region Validación TipoValorDescuento

    [Fact]
    public async Task Validate_ConPorcentajeYMontoFijoAmbosConValor_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = 10m;
        command.MontoFijo = 50m; // Ambos con valor - inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "TipoValorDescuento" &&
            e.ErrorMessage.Contains("Debe especificar un porcentaje o un monto fijo, pero no ambos"));
    }

    [Fact]
    public async Task Validate_SinPorcentajeNiMontoFijo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = 0m;
        command.MontoFijo = 0m; // Ambos en cero - inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "TipoValorDescuento" &&
            e.ErrorMessage.Contains("Debe especificar un porcentaje o un monto fijo, pero no ambos"));
    }

    #endregion

    #region Validación Porcentaje

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_ConPorcentajeMenorOIgualACero_DeberiaRetornarError(decimal porcentajeInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = porcentajeInvalido;
        command.MontoFijo = 0m;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Porcentaje) &&
            e.ErrorMessage.Contains("El porcentaje de descuento debe ser mayor a 0"));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(150)]
    public async Task Validate_ConPorcentajeMayorA100_DeberiaRetornarError(decimal porcentajeInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = porcentajeInvalido;
        command.MontoFijo = 0m;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Porcentaje) &&
            e.ErrorMessage.Contains("El porcentaje de descuento no puede exceder 100%"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Validate_ConPorcentajeValido_NoDeberiaRetornarErrorDePorcentaje(decimal porcentajeValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = porcentajeValido;
        command.MontoFijo = 0m;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.Porcentaje));
    }

    #endregion

    #region Validación MontoFijo

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Validate_ConMontoFijoMenorOIgualACero_DeberiaRetornarError(decimal montoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = 0m;
        command.MontoFijo = montoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoFijo) &&
            e.ErrorMessage.Contains("El monto fijo debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConMontoFijoExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = 0m;
        command.MontoFijo = 100001m; // Más de $100,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoFijo) &&
            e.ErrorMessage.Contains("El monto fijo no puede exceder $100,000"));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(500)]
    [InlineData(100000)]
    public async Task Validate_ConMontoFijoValido_NoDeberiaRetornarErrorDeMontoFijo(decimal montoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Porcentaje = 0m;
        command.MontoFijo = montoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.MontoFijo));
    }

    #endregion

    #region Validación Prioridad

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_ConPrioridadMenorAUno_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad mínima es 1"));
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public async Task Validate_ConPrioridadMayorADiez_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad máxima es 10"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Validate_ConPrioridadValida_NoDeberiaRetornarErrorDePrioridad(int prioridadValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.Prioridad));
    }

    #endregion

    #region Validación UsuarioAutorizaId

    [Fact]
    public async Task Validate_ConUsuarioAutorizaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioAutorizaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El ID del usuario que autoriza es requerido"));
    }

    #endregion

    #region Validación CodigoAutorizacion para Cortesía

    [Fact]
    public async Task Validate_ConDescuentoCortesiaSinCodigo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = "Cortesia";
        command.CodigoAutorizacion = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.CodigoAutorizacion) &&
            e.ErrorMessage.Contains("El código de autorización es obligatorio para descuentos de cortesía"));
    }

    [Fact]
    public async Task Validate_ConDescuentoCortesiaCodigoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = "Cortesia";
        command.CodigoAutorizacion = "12345"; // Menos de 6 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.CodigoAutorizacion) &&
            e.ErrorMessage.Contains("El código de autorización debe tener al menos 6 caracteres"));
    }

    #endregion

    #region Validación ProductosEspecificos

    [Fact]
    public async Task Validate_ConMuchosProductosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProductosEspecificos = Enumerable.Range(1, 51).Select(i => Guid.NewGuid()).ToList(); // Más de 50

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.ProductosEspecificos) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 50 productos"));
    }

    #endregion

    #region Validación CategoriasAplicables

    [Fact]
    public async Task Validate_ConMuchasCategoriasAplicables_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CategoriasAplicables = Enumerable.Range(1, 11).Select(i => $"Categoria{i}").ToList(); // Más de 10

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.CategoriasAplicables) &&
            e.ErrorMessage.Contains("No se pueden especificar más de 10 categorías"));
    }

    [Fact]
    public async Task Validate_ConCategoriasInvalidas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CategoriasAplicables = new List<string> { "Comidas", "CategoriaInvalida" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.CategoriasAplicables) &&
            e.ErrorMessage.Contains("Las categorías deben ser válidas"));
    }

    [Fact]
    public async Task Validate_ConCategoriasValidasSimple_NoDeberiaRetornarErrorDeCategorias()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CategoriasAplicables = new List<string> { "Comidas" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.CategoriasAplicables));
    }

    [Fact]
    public async Task Validate_ConCategoriasValidasMultiples_NoDeberiaRetornarErrorDeCategorias()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CategoriasAplicables = new List<string> { "Bebidas", "Postres" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.CategoriasAplicables));
    }

    [Fact]
    public async Task Validate_ConCategoriasValidasCompletas_NoDeberiaRetornarErrorDeCategorias()
    {
        // Arrange
        var command = CrearCommandValido();
        command.CategoriasAplicables = new List<string> { "Comidas", "Bebidas", "Postres", "Entradas", "Especialidades" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.CategoriasAplicables));
    }

    #endregion

    #region Validación MontoMinimoFactura

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Validate_ConMontoMinimoFacturaMenorOIgualACero_DeberiaRetornarError(decimal montoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoMinimoFactura = montoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoMinimoFactura) &&
            e.ErrorMessage.Contains("El monto mínimo debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConMontoMinimoFacturaExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoMinimoFactura = 1000001m; // Más de $1,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoMinimoFactura) &&
            e.ErrorMessage.Contains("El monto mínimo no puede exceder $1,000,000"));
    }

    #endregion

    #region Validación MontoMaximoDescuento

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task Validate_ConMontoMaximoDescuentoMenorOIgualACero_DeberiaRetornarError(decimal montoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoMaximoDescuento = montoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoMaximoDescuento) &&
            e.ErrorMessage.Contains("El monto máximo de descuento debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConMontoMaximoDescuentoExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MontoMaximoDescuento = 500001m; // Más de $500,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.MontoMaximoDescuento) &&
            e.ErrorMessage.Contains("El monto máximo de descuento no puede exceder $500,000"));
    }

    #endregion

    #region Validación FechaExpiracion

    [Fact]
    public async Task Validate_ConFechaExpiracionPasada_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = DateTime.UtcNow.AddDays(-1); // Fecha pasada

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.FechaExpiracion) &&
            e.ErrorMessage.Contains("La fecha de expiración debe ser futura"));
    }

    [Fact]
    public async Task Validate_ConFechaExpiracionMuyLejana_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = DateTime.UtcNow.AddYears(3); // Más de 2 años

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.FechaExpiracion) &&
            e.ErrorMessage.Contains("La fecha de expiración no puede ser más de 2 años en el futuro"));
    }

    #endregion

    #region Validación NotasAdicionales

    [Fact]
    public async Task Validate_ConNotasAdicionalesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NotasAdicionales = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AplicarDescuentoCommand.NotasAdicionales) &&
            e.ErrorMessage.Contains("Las notas adicionales no pueden exceder 1000 caracteres"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Crear entidades usando factorías correctas
        var factura = Factura.Crear("FAC-001", TipoFactura.Normal, "Cliente Test");
        var usuario = Usuario.Crear("testuser", "Usuario Test", "test@test.com", RolUsuario.Administrador);

        _mockFacturas.Setup(f => f.FindAsync(facturaId))
            .ReturnsAsync(factura);
        _mockUsuarios.Setup(u => u.FindAsync(usuarioId))
            .ReturnsAsync(usuario);

        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Concepto = "Descuento promocional especial",
            Motivo = "Promoción del día para clientes frecuentes del restaurante",
            Porcentaje = 15m,
            MontoFijo = 0m,
            Prioridad = 3,
            UsuarioAutorizaId = usuarioId,
            ProductosEspecificos = new List<Guid>(),
            CategoriasAplicables = new List<string>(),
            MontoMinimoFactura = 500m,
            MontoMaximoDescuento = 200m,
            FechaExpiracion = DateTime.UtcNow.AddMonths(1),
            NotasAdicionales = "Descuento aplicado correctamente según política de la empresa"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new AplicarDescuentoCommand
        {
            FacturaId = Guid.Empty, // Error
            TipoDescuento = "", // Error
            Concepto = "", // Error
            Motivo = "", // Error
            Porcentaje = 150m, // Error: excede 100%
            MontoFijo = 50m, // Error: ambos especificados
            Prioridad = 0, // Error: menor que 1
            UsuarioAutorizaId = Guid.Empty, // Error
            CategoriasAplicables = new List<string> { "CategoriaInvalida" }, // Error
            MontoMinimoFactura = -100m, // Error: negativo
            FechaExpiracion = DateTime.UtcNow.AddDays(-1), // Error: pasada
            NotasAdicionales = new string('A', 1001) // Error: muy larga
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(8);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 0, 50)]
    [InlineData(3, 20, 0)]
    [InlineData(4, 15, 0)]
    public async Task Validate_ConDiferentesTiposDescuento_DeberiaSerValido(int tipo, decimal porcentaje, decimal montoFijo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoDescuento = tipo.ToString();
        command.Porcentaje = porcentaje;
        command.MontoFijo = montoFijo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.TipoDescuento));
    }

    [Theory]
    [InlineData(1, "Descuento básico cliente")]
    [InlineData(5, "Descuento promocional estándar")]
    [InlineData(10, "Descuento excepcional autorizado")]
    public async Task Validate_ConDiferentesPrioridadesYConceptos_DeberiaSerValido(int prioridad, string concepto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridad;
        command.Concepto = concepto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.Prioridad));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AplicarDescuentoCommand.Concepto));
    }

    #endregion
} 