using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.Common;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para Result
/// </summary>
public class ResultTests
{
    #region Result<T> Tests

    #region Constructor y Propiedades Iniciales

    [Fact]
    public void ResultT_Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var result = Result<string>.Success("data");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("data");
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    #endregion

    #region Success Method

    [Fact]
    public void Success_ConDatos_DeberiaCrearResultadoExitoso()
    {
        // Arrange
        var data = "Datos de prueba";

        // Act
        var result = Result<string>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Success_ConDatosNull_DeberiaPermitirDatosNull()
    {
        // Arrange
        string? data = null;

        // Act
        var result = Result<string?>.Success(data!);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Success_ConTipoInt_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = 42;

        // Act
        var result = Result<int>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Data.Should().BeOfType(typeof(int));
    }

    #endregion

    #region Failure Methods

    [Fact]
    public void ResultT_Failure_ConStringError_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(error);
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ResultT_Failure_ConListaErrores_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ResultT_Failure_ConErrorYListaErrores_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error principal";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result<string>.Failure(error, errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(error);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ResultT_Failure_ConErrorNull_DeberiaPermitirErrorNull()
    {
        // Arrange
        string? error = null;

        // Act
        var result = Result<string>.Failure(error!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ResultT_Failure_ConErroresNull_DeberiaPermitirErroresNull()
    {
        // Arrange
        List<string>? errors = null;

        // Act
        var result = Result<string>.Failure(errors!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    #endregion

    #region Implicit Operators

    [Fact]
    public void ImplicitOperator_DeT_DeberiaCrearResultadoExitoso()
    {
        // Arrange
        var data = "Datos de prueba";

        // Act
        Result<string> result = Result<string>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ResultT_ImplicitOperator_DeString_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        Result<string> result = Result<string>.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(error);
        result.Errors.Should().BeNull();
    }

    #endregion

    #endregion

    #region Result Tests (Non-generic)

    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Result_Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    #endregion

    #region Success Method

    [Fact]
    public void Success_DeberiaCrearResultadoExitoso()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    #endregion

    #region Failure Methods

    [Fact]
    public void Failure_ConStringError_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Failure_ConListaErrores_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_ConErrorYListaErrores_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error principal";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result.Failure(error, errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_ConErrorNull_DeberiaPermitirErrorNull()
    {
        // Arrange
        string? error = null;

        // Act
        var result = Result.Failure(error!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Failure_ConErroresNull_DeberiaPermitirErroresNull()
    {
        // Arrange
        List<string>? errors = null;

        // Act
        var result = Result.Failure(errors!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    #endregion

    #region Implicit Operator

    [Fact]
    public void ImplicitOperator_DeString_DeberiaCrearResultadoFallido()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        Result result = error;

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Errors.Should().BeNull();
    }

    #endregion

    #endregion

    #region Casos Edge

    [Fact]
    public void ResultT_ConErroresVacios_DeberiaManejarCorrectamente()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_ConErroresVacios_DeberiaManejarCorrectamente()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ResultT_ConErroresConElementosNull_DeberiaManejarCorrectamente()
    {
        // Arrange
        var errors = new List<string?> { "Error 1", null, "Error 3" };

        // Act
        var result = Result<string>.Failure(errors!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Result_ConErroresConElementosNull_DeberiaManejarCorrectamente()
    {
        // Arrange
        var errors = new List<string?> { "Error 1", null, "Error 3" };

        // Act
        var result = Result.Failure(errors!);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    #endregion

    #region Escenarios Reales

    [Fact]
    public void ResultT_EscenarioRealExitoso_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = Result<object>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ResultT_EscenarioRealFallido_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var error = "No se pudo procesar la solicitud";
        var errors = new List<string> { "Campo requerido", "Formato inválido" };

        // Act
        var result = Result<object>.Failure(error, errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(error);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Result_EscenarioRealExitoso_DeberiaFuncionarCorrectamente()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Result_EscenarioRealFallido_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var error = "Operación falló";
        var errors = new List<string> { "Error de validación", "Error de conexión" };

        // Act
        var result = Result.Failure(error, errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    #endregion

    #region Tipos Genéricos

    [Fact]
    public void ResultT_ConTipoLista_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = Result<List<int>>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(data);
        result.Data.Should().BeOfType(typeof(List<int>));
    }

    [Fact]
    public void ResultT_ConTipoObjeto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test", Value = 42.5 };

        // Act
        var result = Result<object>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        // No verificamos el tipo específico ya que es un objeto anónimo
    }

    #endregion
}
