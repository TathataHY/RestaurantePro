using FluentAssertions;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Models;

public class ApiResponseTests
{
    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaInicializarCorrectamente()
    {
        // Arrange
        var data = "test data";
        var message = "Success";
        var errors = new List<string> { "Error1", "Error2" };
        var statusCode = 200;

        // Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = data,
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(data);
        apiResponse.Message.Should().Be(message);
        apiResponse.Errors.Should().BeEquivalentTo(errors);
        apiResponse.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void Constructor_ConValoresNulos_DeberiaInicializarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>();

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().BeNull();
        apiResponse.Errors.Should().BeNull();
        apiResponse.StatusCode.Should().Be(0);
    }

    [Fact]
    public void Constructor_ConRespuestaExitosa_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = data,
            Message = "Operation completed successfully",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(data);
        apiResponse.Message.Should().Be("Operation completed successfully");
        apiResponse.Errors.Should().BeNull();
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConRespuestaError_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var errors = new List<string> { "Validation failed", "Invalid input" };

        // Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Operation failed",
            Errors = errors,
            StatusCode = 400
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Operation failed");
        apiResponse.Errors.Should().BeEquivalentTo(errors);
        apiResponse.StatusCode.Should().Be(400);
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public void Constructor_ConDataNula_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = null,
            Message = "Success but no data",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Success but no data");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConMessageVacio_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = "test",
            Message = "",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("test");
        apiResponse.Message.Should().Be("");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConMessageSoloEspacios_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = "test",
            Message = "   ",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("test");
        apiResponse.Message.Should().Be("   ");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConErrorsVacio_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Error occurred",
            Errors = new List<string>(),
            StatusCode = 400
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Error occurred");
        apiResponse.Errors.Should().NotBeNull();
        apiResponse.Errors.Should().BeEmpty();
        apiResponse.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Constructor_ConErrorsNulo_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Error occurred",
            Errors = null,
            StatusCode = 400
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Error occurred");
        apiResponse.Errors.Should().BeNull();
        apiResponse.StatusCode.Should().Be(400);
    }

    // ===== PRUEBAS ROBUSTAS - VALORES EXTREMOS =====

    [Fact]
    public void Constructor_ConStatusCodeMaximo_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = "test",
            Message = "Success",
            StatusCode = int.MaxValue
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("test");
        apiResponse.Message.Should().Be("Success");
        apiResponse.StatusCode.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Constructor_ConStatusCodeMinimo_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Error",
            StatusCode = int.MinValue
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Error");
        apiResponse.StatusCode.Should().Be(int.MinValue);
    }

    [Fact]
    public void Constructor_ConStatusCodeCero_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "No status",
            StatusCode = 0
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("No status");
        apiResponse.StatusCode.Should().Be(0);
    }

    [Fact]
    public void Constructor_ConStatusCodeNegativo_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Negative status",
            StatusCode = -1
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Negative status");
        apiResponse.StatusCode.Should().Be(-1);
    }

    // ===== PRUEBAS ROBUSTAS - TIPOS DIFERENTES =====

    [Fact]
    public void Constructor_ConTipoInt_DeberiaFuncionarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = 42,
            Message = "Success",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(42);
        apiResponse.Message.Should().Be("Success");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConTipoObject_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test", Value = 123.45 };

        // Act
        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = data,
            Message = "Success",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(data);
        apiResponse.Message.Should().Be("Success");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConTipoLista_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var data = new List<string> { "item1", "item2", "item3" };

        // Act
        var apiResponse = new ApiResponse<List<string>>
        {
            Success = true,
            Data = data,
            Message = "Success",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(data);
        apiResponse.Message.Should().Be("Success");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConTipoNullable_DeberiaFuncionarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<int?>
        {
            Success = true,
            Data = null,
            Message = "Success with null data",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Success with null data");
        apiResponse.StatusCode.Should().Be(200);
    }

    // ===== PRUEBAS ROBUSTAS - CASOS ESPECIALES =====

    [Fact]
    public void Constructor_ConSuccessTruePeroDataNull_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = null,
            Message = "Success but no data",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Success but no data");
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConSuccessFalsePeroDataNotNull_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = "partial data",
            Message = "Partial success",
            StatusCode = 206
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().Be("partial data");
        apiResponse.Message.Should().Be("Partial success");
        apiResponse.StatusCode.Should().Be(206);
    }

    [Fact]
    public void Constructor_ConSuccessTruePeroErrorsNotNull_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = "data",
            Message = "Success with warnings",
            Errors = new List<string> { "Warning 1", "Warning 2" },
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("data");
        apiResponse.Message.Should().Be("Success with warnings");
        apiResponse.Errors.Should().BeEquivalentTo(new List<string> { "Warning 1", "Warning 2" });
        apiResponse.StatusCode.Should().Be(200);
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO =====

    [Fact]
    public void Constructor_ConListaGrandeDeErrors_DeberiaManejarCorrectamente()
    {
        // Arrange
        var errors = Enumerable.Range(1, 10000).Select(i => $"Error {i}").ToList();

        // Act
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Data = null,
            Message = "Multiple errors",
            Errors = errors,
            StatusCode = 400
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Data.Should().BeNull();
        apiResponse.Message.Should().Be("Multiple errors");
        apiResponse.Errors.Should().HaveCount(10000);
        apiResponse.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Constructor_ConMessageMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var longMessage = new string('A', 100000);

        // Act
        var apiResponse = new ApiResponse<string>
        {
            Success = true,
            Data = "test",
            Message = longMessage,
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("test");
        apiResponse.Message.Should().Be(longMessage);
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_ConDataCompleja_DeberiaManejarCorrectamente()
    {
        // Arrange
        var complexData = new
        {
            Id = 1,
            Name = "Test",
            Items = new List<object>
            {
                new { Id = 1, Value = "Item 1" },
                new { Id = 2, Value = "Item 2" },
                new { Id = 3, Value = "Item 3" }
            },
            Metadata = new Dictionary<string, object>
            {
                { "CreatedAt", DateTime.Now },
                { "UpdatedAt", DateTime.Now.AddHours(1) },
                { "IsActive", true }
            }
        };

        // Act
        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = complexData,
            Message = "Complex data response",
            StatusCode = 200
        };

        // Assert
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(complexData);
        apiResponse.Message.Should().Be("Complex data response");
        apiResponse.StatusCode.Should().Be(200);
    }

    // ===== PRUEBAS ROBUSTAS - CASOS DE USO COMUNES =====

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(400, false)]
    [InlineData(401, false)]
    [InlineData(403, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(503, false)]
    public void Constructor_ConCodigosDeEstadoComunes_DeberiaConfigurarSuccessCorrectamente(int statusCode, bool expectedSuccess)
    {
        // Arrange & Act
        var apiResponse = new ApiResponse<string>
        {
            Success = expectedSuccess,
            Data = expectedSuccess ? "data" : null,
            Message = expectedSuccess ? "Success" : "Error",
            StatusCode = statusCode
        };

        // Assert
        apiResponse.Success.Should().Be(expectedSuccess);
        apiResponse.StatusCode.Should().Be(statusCode);
    }
}
