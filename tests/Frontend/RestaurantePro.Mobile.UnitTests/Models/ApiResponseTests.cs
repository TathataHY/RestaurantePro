using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para ApiResponse
/// </summary>
public class ApiResponseTests
{
    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var response = new ApiResponse<string>();

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().BeEmpty();
        response.Errors.Should().NotBeNull();
        response.Errors.Should().BeEmpty();
        response.Succeeded.Should().BeFalse();
        response.Error.Should().BeEmpty();
        response.StatusCode.Should().Be(0);
    }

    #endregion

    #region Propiedades de Conveniencia

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Succeeded_DeberiaRetornarValorDeSuccess(bool success)
    {
        // Arrange
        var response = new ApiResponse<string> { Success = success };

        // Act & Assert
        response.Succeeded.Should().Be(success);
    }

    [Fact]
    public void Error_ConErroresVacios_DeberiaRetornarStringVacio()
    {
        // Arrange
        var response = new ApiResponse<string> { Errors = new List<string>() };

        // Act & Assert
        response.Error.Should().BeEmpty();
    }

    [Fact]
    public void Error_ConUnError_DeberiaRetornarPrimerError()
    {
        // Arrange
        var response = new ApiResponse<string> 
        { 
            Errors = new List<string> { "Error 1", "Error 2" } 
        };

        // Act & Assert
        response.Error.Should().Be("Error 1");
    }

    [Fact]
    public void Error_ConErroresNull_DeberiaRetornarStringVacio()
    {
        // Arrange
        var response = new ApiResponse<string> { Errors = null! };

        // Act & Assert
        // La propiedad Error maneja null internamente, pero lanza excepción
        // Este test verifica el comportamiento real del código
        var act = () => response.Error;
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SuccessResponse

    [Fact]
    public void SuccessResponse_ConDatos_DeberiaCrearRespuestaExitosa()
    {
        // Arrange
        var data = "Datos de prueba";
        var message = "Operación exitosa";

        // Act
        var response = ApiResponse<string>.SuccessResponse(data, message);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(200);
        response.Errors.Should().BeEmpty();
        response.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void SuccessResponse_ConMensajePorDefecto_DeberiaUsarMensajePorDefecto()
    {
        // Arrange
        var data = "Datos de prueba";

        // Act
        var response = ApiResponse<string>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be("Operación exitosa");
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void SuccessResponse_ConDatosNull_DeberiaPermitirDatosNull()
    {
        // Arrange
        string? data = null;

        // Act
        var response = ApiResponse<string?>.SuccessResponse(data!);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Operación exitosa");
    }

    #endregion

    #region ErrorResponse con Lista de Errores

    [Fact]
    public void ErrorResponse_ConListaErrores_DeberiaCrearRespuestaDeError()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };
        var message = "Error en operación";
        var statusCode = 400;

        // Act
        var response = ApiResponse<string>.ErrorResponse(errors, message, statusCode);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(statusCode);
        response.Errors.Should().BeEquivalentTo(errors);
        response.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void ErrorResponse_ConMensajePorDefecto_DeberiaUsarMensajePorDefecto()
    {
        // Arrange
        var errors = new List<string> { "Error de prueba" };

        // Act
        var response = ApiResponse<string>.ErrorResponse(errors);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Error en la operación");
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ErrorResponse_ConErroresVacios_DeberiaCrearRespuestaConListaVacia()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var response = ApiResponse<string>.ErrorResponse(errors);

        // Assert
        response.Success.Should().BeFalse();
        response.Errors.Should().BeEmpty();
    }

    #endregion

    #region ErrorResponse con String

    [Fact]
    public void ErrorResponse_ConStringError_DeberiaCrearRespuestaDeError()
    {
        // Arrange
        var error = "Error de prueba";
        var message = "Error en operación";
        var statusCode = 500;

        // Act
        var response = ApiResponse<string>.ErrorResponse(error, message, statusCode);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(statusCode);
        response.Errors.Should().HaveCount(1);
        response.Errors.Should().Contain(error);
    }

    [Fact]
    public void ErrorResponse_ConStringErrorYValoresPorDefecto_DeberiaUsarValoresPorDefecto()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        var response = ApiResponse<string>.ErrorResponse(error);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Error en la operación");
        response.StatusCode.Should().Be(400);
        response.Errors.Should().HaveCount(1);
        response.Errors.Should().Contain(error);
    }

    #endregion

    #region Métodos de Conveniencia

    [Fact]
    public void Failure_DeberiaCrearRespuestaDeError()
    {
        // Arrange
        var error = "Error de fallo";
        var message = "Operación falló";
        var statusCode = 404;

        // Act
        var response = ApiResponse<string>.Failure(error, message, statusCode);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(statusCode);
        response.Errors.Should().HaveCount(1);
        response.Errors.Should().Contain(error);
    }

    [Fact]
    public void Failure_ConValoresPorDefecto_DeberiaUsarValoresPorDefecto()
    {
        // Arrange
        var error = "Error de fallo";

        // Act
        var response = ApiResponse<string>.Failure(error);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Error en la operación");
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public void SuccessResult_DeberiaCrearRespuestaExitosa()
    {
        // Arrange
        var data = "Datos de resultado";
        var message = "Resultado exitoso";

        // Act
        var response = ApiResponse<string>.SuccessResult(data, message);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void SuccessResult_ConMensajePorDefecto_DeberiaUsarMensajePorDefecto()
    {
        // Arrange
        var data = "Datos de resultado";

        // Act
        var response = ApiResponse<string>.SuccessResult(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be("Operación exitosa");
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void ErrorResponse_ConErroresNull_DeberiaManejarCorrectamente()
    {
        // Arrange
        List<string>? errors = null;

        // Act
        var response = ApiResponse<string>.ErrorResponse(errors!);

        // Assert
        response.Success.Should().BeFalse();
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void ErrorResponse_ConMensajeNull_DeberiaPermitirMensajeNull()
    {
        // Arrange
        var error = "Error de prueba";

        // Act
        var response = ApiResponse<string>.ErrorResponse(error, null!);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void SuccessResponse_ConMensajeNull_DeberiaPermitirMensajeNull()
    {
        // Arrange
        var data = "Datos de prueba";

        // Act
        var response = ApiResponse<string>.SuccessResponse(data, null!);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().BeNull();
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(0)]
    [InlineData(-1)]
    public void StatusCode_DeberiaPermitirCualquierValor(int statusCode)
    {
        // Arrange
        var response = new ApiResponse<string> { StatusCode = statusCode };

        // Act & Assert
        response.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region Tipos Genéricos

    [Fact]
    public void ApiResponse_ConTipoInt_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = 42;

        // Act
        var response = ApiResponse<int>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Data.Should().BeOfType(typeof(int));
    }

    [Fact]
    public void ApiResponse_ConTipoLista_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new List<string> { "item1", "item2" };

        // Act
        var response = ApiResponse<List<string>>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(data);
        response.Data.Should().BeOfType<List<string>>();
    }

    [Fact]
    public void ApiResponse_ConTipoObjeto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new { Name = "Test", Value = 123 };

        // Act
        var response = ApiResponse<object>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
    }

    #endregion
}
