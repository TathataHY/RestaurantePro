using FluentAssertions;
using Microsoft.Maui.Storage;
using System.Net;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Exceptions;

/// <summary>
/// Pruebas unitarias para manejo de excepciones específicas de MAUI
/// </summary>
public class MauiExceptionHandlingTests
{
    #region Network Exception Tests

    [Fact]
    public void HandleHttpRequestException_ShouldReturnUserFriendlyMessage()
    {
        // Arrange
        var exception = new HttpRequestException("Network error occurred");

        // Act
        var result = HandleNetworkException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("conexión");
        result.Should().NotContain("HttpRequestException");
    }

    [Fact]
    public void HandleTaskCanceledException_ShouldReturnTimeoutMessage()
    {
        // Arrange
        var exception = new TaskCanceledException("Request was canceled");

        // Act
        var result = HandleNetworkException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("tiempo");
        result.Should().NotContain("TaskCanceledException");
    }

    [Fact]
    public void HandleWebException_WithTimeout_ShouldReturnTimeoutMessage()
    {
        // Arrange
        var exception = new WebException("The operation has timed out", WebExceptionStatus.Timeout);

        // Act
        var result = HandleNetworkException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("tiempo");
    }

    [Fact]
    public void HandleWebException_WithConnectFailure_ShouldReturnConnectionMessage()
    {
        // Arrange
        var exception = new WebException("Unable to connect", WebExceptionStatus.ConnectFailure);

        // Act
        var result = HandleNetworkException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("conexión");
    }

    [Fact]
    public void HandleSocketException_ShouldReturnConnectionMessage()
    {
        // Arrange
        var exception = new System.Net.Sockets.SocketException(10054); // WSAECONNRESET

        // Act
        var result = HandleNetworkException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("conexión");
    }

    #endregion

    #region Storage Exception Tests

    [Fact]
    public void HandleStorageException_WithUnauthorizedAccess_ShouldReturnPermissionMessage()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access to the path is denied");

        // Act
        var result = HandleStorageException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("permisos");
        result.Should().NotContain("UnauthorizedAccessException");
    }

    [Fact]
    public void HandleStorageException_WithDirectoryNotFound_ShouldReturnStorageMessage()
    {
        // Arrange
        var exception = new DirectoryNotFoundException("Could not find directory");

        // Act
        var result = HandleStorageException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("almacenamiento");
    }

    [Fact]
    public void HandleStorageException_WithIOException_ShouldReturnStorageMessage()
    {
        // Arrange
        var exception = new IOException("The device is not ready");

        // Act
        var result = HandleStorageException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("almacenamiento");
    }

    #endregion

    #region JSON Exception Tests

    [Fact]
    public void HandleJsonException_WithInvalidJson_ShouldReturnDataMessage()
    {
        // Arrange
        var exception = new System.Text.Json.JsonException("The JSON value could not be converted");

        // Act
        var result = HandleDataException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("datos");
        result.Should().NotContain("JsonException");
    }

    [Fact]
    public void HandleJsonException_WithUnexpectedEnd_ShouldReturnDataMessage()
    {
        // Arrange
        var exception = new System.Text.Json.JsonException("Unexpected end of JSON input");

        // Act
        var result = HandleDataException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("datos");
    }

    #endregion

    #region Argument Exception Tests

    [Fact]
    public void HandleArgumentException_WithNullArgument_ShouldReturnValidationMessage()
    {
        // Arrange
        var exception = new ArgumentNullException("parameter", "Value cannot be null");

        // Act
        var result = HandleValidationException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("validación");
        result.Should().NotContain("ArgumentNullException");
    }

    [Fact]
    public void HandleArgumentException_WithInvalidArgument_ShouldReturnValidationMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument provided", "parameter");

        // Act
        var result = HandleValidationException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("validación");
    }

    [Fact]
    public void HandleArgumentOutOfRangeException_ShouldReturnValidationMessage()
    {
        // Arrange
        var exception = new ArgumentOutOfRangeException("index", "Index was out of range");

        // Act
        var result = HandleValidationException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("validación");
    }

    #endregion

    #region Invalid Operation Exception Tests

    [Fact]
    public void HandleInvalidOperationException_WithInvalidState_ShouldReturnOperationMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Operation is not valid in current state");

        // Act
        var result = HandleOperationException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("operación");
        result.Should().NotContain("InvalidOperationException");
    }

    [Fact]
    public void HandleNotSupportedException_ShouldReturnOperationMessage()
    {
        // Arrange
        var exception = new NotSupportedException("Operation is not supported");

        // Act
        var result = HandleOperationException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("operación");
    }

    #endregion

    #region Aggregate Exception Tests

    [Fact]
    public void HandleAggregateException_WithMultipleExceptions_ShouldReturnGeneralMessage()
    {
        // Arrange
        var innerExceptions = new Exception[]
        {
            new HttpRequestException("Network error"),
            new TaskCanceledException("Timeout")
        };
        var exception = new AggregateException("Multiple exceptions occurred", innerExceptions);

        // Act
        var result = HandleAggregateException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("múltiples");
        result.Should().NotContain("AggregateException");
    }

    [Fact]
    public void HandleAggregateException_WithSingleException_ShouldReturnSpecificMessage()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var exception = new AggregateException("Single exception", innerException);

        // Act
        var result = HandleAggregateException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("conexión");
    }

    #endregion

    #region Unknown Exception Tests

    [Fact]
    public void HandleUnknownException_ShouldReturnGenericMessage()
    {
        // Arrange
        var exception = new Exception("Unknown error occurred");

        // Act
        var result = HandleUnknownException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("error");
        result.Should().NotContain("Exception");
    }

    [Fact]
    public void HandleNullException_ShouldReturnGenericMessage()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var result = HandleUnknownException(exception);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("desconocido");
    }

    #endregion

    #region Helper Methods

    private static string HandleNetworkException(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "Error de conexión. Verifica tu internet e intenta nuevamente.",
            TaskCanceledException => "La operación tardó demasiado tiempo. Intenta nuevamente.",
            WebException webEx when webEx.Status == WebExceptionStatus.Timeout => "Tiempo de espera agotado. Intenta nuevamente.",
            WebException webEx when webEx.Status == WebExceptionStatus.ConnectFailure => "No se pudo conectar al servidor. Verifica tu conexión.",
            System.Net.Sockets.SocketException => "Error de conexión. Verifica tu internet e intenta nuevamente.",
            _ => "Error de red. Intenta nuevamente."
        };
    }

    private static string HandleStorageException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "No tienes permisos para acceder al almacenamiento.",
            DirectoryNotFoundException => "Error de almacenamiento. Reinicia la aplicación.",
            IOException => "Error de almacenamiento. Verifica el espacio disponible.",
            _ => "Error de almacenamiento. Intenta nuevamente."
        };
    }

    private static string HandleDataException(Exception exception)
    {
        return exception switch
        {
            System.Text.Json.JsonException => "Error al procesar los datos. Intenta nuevamente.",
            _ => "Error de datos. Intenta nuevamente."
        };
    }

    private static string HandleValidationException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Error de validación. Verifica los datos ingresados.",
            ArgumentOutOfRangeException => "Error de validación. Verifica los datos ingresados.",
            ArgumentException => "Error de validación. Verifica los datos ingresados.",
            _ => "Error de validación. Verifica los datos ingresados."
        };
    }

    private static string HandleOperationException(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException => "Operación no válida en el estado actual.",
            NotSupportedException => "Operación no soportada.",
            _ => "Error en la operación. Intenta nuevamente."
        };
    }

    private static string HandleAggregateException(AggregateException exception)
    {
        if (exception.InnerExceptions.Count == 1)
        {
            return HandleNetworkException(exception.InnerException);
        }

        return "Ocurrieron múltiples errores. Intenta nuevamente.";
    }

    private static string HandleUnknownException(Exception? exception)
    {
        if (exception == null)
            return "Error desconocido. Intenta nuevamente.";

        return "Error inesperado. Intenta nuevamente.";
    }

    #endregion
}
