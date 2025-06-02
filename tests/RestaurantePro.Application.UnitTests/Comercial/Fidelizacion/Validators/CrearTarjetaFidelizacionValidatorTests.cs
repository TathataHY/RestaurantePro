namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Validators;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Tests para CrearTarjetaFidelizacionValidator
/// Valida reglas de negocio para creación de tarjetas de fidelización
/// </summary>
public class CrearTarjetaFidelizacionValidatorTests
{
    private readonly CrearTarjetaFidelizacionValidator _validator;

    public CrearTarjetaFidelizacionValidatorTests()
    {
        _validator = new CrearTarjetaFidelizacionValidator();
    }

    #region Tests de Validaciones Básicas

    [Fact]
    public async Task ClienteId_DebeSerObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ClienteId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ClienteId));
    }

    [Fact]
    public async Task TipoTarjeta_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = (TipoTarjetaFidelizacion)999;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TipoTarjeta));
    }

    [Theory]
    [InlineData(TipoTarjetaFidelizacion.Estandar)]
    [InlineData(TipoTarjetaFidelizacion.Premium)]
    [InlineData(TipoTarjetaFidelizacion.Vip)]
    [InlineData(TipoTarjetaFidelizacion.Corporativa)]
    [InlineData(TipoTarjetaFidelizacion.Empleado)]
    public async Task TipoTarjeta_DebeAceptarTiposValidos(TipoTarjetaFidelizacion tipo)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = tipo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert - No debe tener errores para TipoTarjeta
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(command.TipoTarjeta));
    }

    [Fact]
    public async Task CodigoTarjeta_PuedeSerOpcional()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoTarjeta = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert - No debe tener errores para CodigoTarjeta cuando es null
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(command.CodigoTarjeta));
    }

    [Fact]
    public async Task CodigoTarjeta_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoTarjeta = new string('1', 21);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CodigoTarjeta));
    }

    [Fact]
    public async Task CodigoTarjeta_DebeSerMinimoLongitud()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoTarjeta = "123";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CodigoTarjeta));
    }

    [Theory]
    [InlineData("12345678", true)]
    [InlineData("FIEL1234567890", true)]
    [InlineData("VIP-2025-001", true)]
    [InlineData("CORP_12345", true)]
    public async Task CodigoTarjeta_DebeValidarFormato(string codigo, bool esValido)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoTarjeta = codigo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (!esValido)
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CodigoTarjeta));
        }
        else
        {
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(command.CodigoTarjeta));
        }
    }

    [Fact]
    public async Task PuntosIniciales_DebeSerPositivo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PuntosIniciales = -1;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PuntosIniciales));
    }

    [Fact]
    public async Task Observaciones_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.Observaciones = new string('A', 501);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Observaciones));
    }

    [Fact]
    public async Task CommandoValido_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Métodos Helper

    private CrearTarjetaFidelizacionCommand CrearCommandoBase()
    {
        return new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = Guid.NewGuid(),
            CodigoTarjeta = "TEST12345678"
        };
    }

    #endregion
} 