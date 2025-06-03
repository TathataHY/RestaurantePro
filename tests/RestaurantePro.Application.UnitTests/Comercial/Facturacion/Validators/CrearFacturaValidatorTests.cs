using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.UnitTests.Common;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using Xunit;

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
        
        ConfigurarMocks();
    }

    #region ComandasIds Validations

    [Fact]
    public async Task Validator_ConComandasIdsValidas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConComandasIdsVacia_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Debe especificar al menos una comanda para facturar.");
    }

    [Fact]
    public async Task Validator_ConComandasIdsNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = null!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Debe especificar al menos una comanda para facturar.");
    }

    [Fact]
    public async Task Validator_ConComandasConGuidVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.Empty };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds))
            .Which.ErrorMessage.Should().Be("Todas las comandas deben tener IDs válidos.");
    }

    #endregion

    #region TipoFactura Validations

    [Fact]
    public async Task Validator_ConTipoFacturaValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = "Normal";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConTipoFacturaVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = "";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura))
            .Which.ErrorMessage.Should().Be("El tipo de factura es requerido.");
    }

    [Fact]
    public async Task Validator_ConTipoFacturaNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = null!;

        // Act
        var result = await _validator.ValidateAsync(command);

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
    public async Task Validator_ConTiposFacturaValidos_DeberiaSerValido(string tipoFactura)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = tipoFactura;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("TipoInvalido")]
    [InlineData("Credito")]
    [InlineData("Debito")]
    [InlineData("NORMAL")]
    [InlineData("normal")]
    public async Task Validator_ConTiposFacturaInvalidos_DeberiaFallar(string tipoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoFactura = tipoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura))
            .Which.ErrorMessage.Should().Be("El tipo de factura debe ser uno de: Normal, Fiscal, Global, NotaCredito, NotaDebito.");
    }

    #endregion

    #region NombreCliente Validations

    [Fact]
    public async Task Validator_ConNombreClienteValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "Juan Pérez";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConNombreClienteVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es requerido.");
    }

    [Fact]
    public async Task Validator_ConNombreClienteNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = null!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente es requerido.");
    }

    [Fact]
    public async Task Validator_ConNombreClienteMuyCorto_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "A";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente))
            .Which.ErrorMessage.Should().Be("El nombre del cliente debe tener al menos 2 caracteres.");
    }

    [Fact]
    public async Task Validator_ConNombreClienteMuyLargo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = new string('A', 201);

        // Act
        var result = await _validator.ValidateAsync(command);

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
    public async Task Validator_ConMonedasValidas_DeberiaSerValido(string moneda)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Moneda = moneda;

        // Act
        var result = await _validator.ValidateAsync(command);

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
    public async Task Validator_ConMonedasInvalidas_DeberiaFallar(string? monedaInvalida)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Moneda = monedaInvalida!;

        // Act
        var result = await _validator.ValidateAsync(command);

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
    public async Task Validator_ConMetodosPagoValidos_DeberiaSerValido(string metodoPago)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoPagoPreferido = metodoPago;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("MetodoInvalido")]
    [InlineData("Bitcoin")]
    public async Task Validator_ConMetodosPagoInvalidos_DeberiaFallar(string? metodoPagoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MetodoPagoPreferido = metodoPagoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Validator_ConTodosLosCamposValidos_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ConMultiplesErrores_DeberiaListarTodos()
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
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(6); // Actualizado de 5 a 6 porque moneda ahora es obligatoria
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.ComandasIds));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.TipoFactura));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearFacturaCommand.NombreCliente));
    }

    [Fact]
    public async Task Validator_ConFacturaFiscalCompleta_DeberiaSerValido()
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
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validator_ConNotaCreditoConFacturaOriginal_DeberiaSerValido()
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
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Validator_RendimientoValidacion_DeberiaSerRapido()
    {
        // Arrange
        var command = CrearComandoValido();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            await _validator.ValidateAsync(command);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200); // Menos de 200ms para 1000 validaciones (complejo)
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Validator_ConComandasDuplicadas_DeberiaSerValido()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { comandaId, comandaId }; // Duplicadas

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue(); // El validator básico no valida duplicados
    }

    [Fact]
    public async Task Validator_ConMuchasComandas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = Enumerable.Range(1, 50).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ConCaracteresEspecialesEnNombre_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NombreCliente = "José María Péñez & Asociados S.A. de C.V.";

        // Act
        var result = await _validator.ValidateAsync(command);

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

    private void ConfigurarMocks()
    {
        // Mock para comandas - usar factory method Crear y MockDbSetHelper
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: Guid.NewGuid(),
            mesaId: Guid.NewGuid(),
            observaciones: "Comanda de prueba"
        );
        // Forzar el estado usando reflection ya que es propiedad privada
        typeof(Comanda).GetProperty("Estado")!.SetValue(comanda, EstadoComanda.Finalizada);
        
        var comandasData = new List<Comanda> { comanda }.AsQueryable();
        var mockComandas = MockDbSetHelper.CreateMockDbSet(comandasData);
        _mockContext.Setup(c => c.Comandas).Returns(mockComandas.Object);

        // Mock para facturas - usando MockDbSetHelper para lista vacía
        var mockFacturas = MockDbSetHelper.CreateEmptyMockDbSet<Factura>();
        _mockContext.Setup(c => c.Facturas).Returns(mockFacturas.Object);

        // Mock para clientes - usar factory method Crear y MockDbSetHelper
        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Cliente", "Ejemplo"),
            "cliente@ejemplo.com",
            "+52-555-1234567",
            DateTime.Now.AddYears(-25)
        );
        
        var clientesData = new List<Cliente> { cliente }.AsQueryable();
        var mockClientes = MockDbSetHelper.CreateMockDbSet(clientesData);
        _mockContext.Setup(c => c.Clientes).Returns(mockClientes.Object);
    }

    #endregion
} 