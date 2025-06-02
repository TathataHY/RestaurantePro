namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Validators;

/// <summary>
/// Tests unitarios para CrearFacturaValidator
/// Validación completa de reglas complejas de facturación y negocio
/// </summary>
public class CrearFacturaValidatorTests
{
    private readonly CrearFacturaValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;

    public CrearFacturaValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new CrearFacturaValidator(_mockContext.Object);
    }

    #region ComandasIds Validations

    [Fact]
    public void Validator_ConComandasIdsValidas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConComandasIdsVacia_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Debe especificar al menos una comanda para facturar.");
    }

    [Fact]
    public void Validator_ConComandasIdsNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Debe especificar al menos una comanda para facturar.");
    }

    [Fact]
    public void Validator_ConComandasConGuidVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Todas las comandas deben tener IDs válidos.");
    }

    #endregion

    #region TipoFactura Validations

    [Fact]
    public void Validator_ConTipoFacturaValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = "Normal";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConTipoFacturaVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura))
            .Which.ErrorMessage.Should().Be("El tipo de factura es requerido.");
    }

    [Fact]
    public void Validator_ConTipoFacturaNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura))
            .Which.ErrorMessage.Should().Be("El tipo de factura es requerido.");
    }

    [Theory]
    [InlineData("Normal")]
    [InlineData("Fiscal")]
    [InlineData("Global")]
    [InlineData("NotaCredito")]
    [InlineData("NotaDebito")]
    public void Validator_ConTiposFacturaValidos_DeberiaSerValido(string tipoFactura)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = tipoFactura;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("TipoInvalido")]
    [InlineData("Credito")]
    [InlineData("Debito")]
    [InlineData("NORMAL")]
    [InlineData("normal")]
    public void Validator_ConTiposFacturaInvalidos_DeberiaFallar(string tipoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = tipoInvalido;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura))
            .Which.ErrorMessage.Should().Be("El tipo de factura debe ser uno de: Normal, Fiscal, Global, NotaCredito, NotaDebito.");
    }

    #endregion

    #region NombreCliente Validations

    [Fact]
    public void Validator_ConNombreClienteValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "Juan Pérez";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConNombreClienteVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es requerido.");
    }

    [Fact]
    public void Validator_ConNombreClienteNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es requerido.");
    }

    [Fact]
    public void Validator_ConNombreClienteMuyCorto_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "A";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente debe tener al menos 2 caracteres.");
    }

    [Fact]
    public void Validator_ConNombreClienteMuyLargo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = new string('A', 201);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente no puede exceder 200 caracteres.");
    }

    #endregion

    #region Moneda Validations

    [Theory]
    [InlineData("MXN")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("CAD")]
    public void Validator_ConMonedasValidas_DeberiaSerValido(string moneda)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Moneda = moneda;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("GBP")]
    [InlineData("COP")]
    [InlineData("mxn")]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ConMonedasInvalidas_DeberiaFallar(string? monedaInvalida)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Moneda = monedaInvalida!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region MetodoPagoPreferido Validations

    [Theory]
    [InlineData("Efectivo")]
    [InlineData("TarjetaCredito")]
    [InlineData("TarjetaDebito")]
    [InlineData("Transferencia")]
    public void Validator_ConMetodosPagoValidos_DeberiaSerValido(string metodoPago)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoPagoPreferido = metodoPago;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("MetodoInvalido")]
    [InlineData("Bitcoin")]
    public void Validator_ConMetodosPagoInvalidos_DeberiaFallar(string? metodoPagoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoPagoPreferido = metodoPagoInvalido;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Validator_ConTodosLosCamposValidos_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConMultiplesErrores_DeberiaListarTodos()
    {
        // Arrange
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid>(), // Error: vacía
            TipoFactura = "TipoInvalido", // Error: tipo inválido
            NombreCliente = "", // Error: vacío
            Moneda = "JPY", // Error: moneda inválida
            MetodoPagoPreferido = "Bitcoin" // Error: método inválido
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente));
    }

    [Fact]
    public void Validator_ConFacturaFiscalCompleta_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid() },
            TipoFactura = "Fiscal",
            NombreCliente = "Empresa ABC S.A. de C.V.",
            IdentificacionFiscal = "ABC123456789",
            DireccionCliente = "Av. Principal 123, Col. Centro",
            Moneda = "MXN",
            MetodoPagoPreferido = "Transferencia",
            Observaciones = "Factura fiscal empresarial"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConNotaCreditoConFacturaOriginal_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid() },
            TipoFactura = "NotaCredito",
            NombreCliente = "Cliente Ejemplo",
            Observaciones = "Devolución de producto defectuoso",
            Moneda = "MXN",
            MetodoPagoPreferido = "Efectivo"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Validator_RendimientoValidacion_DeberiaSerRapido()
    {
        // Arrange
        var command = CrearComandoValido();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _validator.Validate(command);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200); // Menos de 200ms para 1000 validaciones (complejo)
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validator_ConComandasDuplicadas_DeberiaSerValido()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { comandaId, comandaId }; // Duplicadas

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // El validator básico no valida duplicados
    }

    [Fact]
    public void Validator_ConMuchasComandas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = Enumerable.Range(1, 50).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConCaracteresEspecialesEnNombre_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "José María Péñez & Asociados S.A. de C.V.";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private CrearFacturaCommand CrearComandoValido()
    {
        return new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid() },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Ejemplo",
            Moneda = "MXN",
            MetodoPagoPreferido = "Efectivo",
            Observaciones = "Factura de ejemplo"
        };
    }

    #endregion
} 