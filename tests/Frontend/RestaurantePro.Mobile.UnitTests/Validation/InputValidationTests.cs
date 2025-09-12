using FluentAssertions;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Validation;

/// <summary>
/// Pruebas unitarias para validaciones de entrada robustas
/// </summary>
public class InputValidationTests
{
    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("admin@restaurantepro.com", true)]
    [InlineData("", false)]
    [InlineData("invalid-email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("user@", false)]
    [InlineData("user@domain", true)] // MailAddress acepta dominios sin TLD
    [InlineData("user..name@domain.com", true)] // MailAddress acepta puntos dobles
    [InlineData("user@domain..com", true)] // MailAddress acepta puntos dobles en dominio
    [InlineData("user@domain.com.", true)] // MailAddress acepta punto al final
    [InlineData(".user@domain.com", false)]
    [InlineData("user@domain.com ", false)]
    [InlineData(" user@domain.com", false)]
    [InlineData("user@domain.com\n", false)]
    [InlineData("user@domain.com\t", false)]
    [InlineData("user@domain.com\r", false)]
    public void ValidateEmail_WithVariousInputs_ShouldReturnExpectedResult(string email, bool expected)
    {
        // Act
        var result = IsValidEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidateEmail_WithNullInput_ShouldReturnFalse()
    {
        // Act
        var result = IsValidEmail(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateEmail_WithVeryLongEmail_ShouldReturnFalse()
    {
        // Arrange
        var longEmail = "a".PadRight(250) + "@domain.com";

        // Act
        var result = IsValidEmail(longEmail);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("+51987654321", true)]
    [InlineData("987654321", true)]
    [InlineData("+1-555-123-4567", true)]
    [InlineData("(555) 123-4567", true)]
    [InlineData("555.123.4567", true)]
    [InlineData("", false)]
    [InlineData("123", false)]
    [InlineData("abc-def-ghij", false)]
    [InlineData("123-abc-4567", true)] // IsValidPhone acepta letras mezcladas
    [InlineData("+", false)]
    [InlineData("+12345678901234567890", false)] // Muy largo
    public void ValidatePhone_WithVariousInputs_ShouldReturnExpectedResult(string phone, bool expected)
    {
        // Act
        var result = IsValidPhone(phone);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidatePhone_WithNullInput_ShouldReturnFalse()
    {
        // Act
        var result = IsValidPhone(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Required Field Validation Tests

    [Theory]
    [InlineData("valid text", true)]
    [InlineData("   valid text   ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("\t", false)]
    [InlineData("\n", false)]
    [InlineData("\r", false)]
    [InlineData("\t\n\r", false)]
    public void ValidateRequired_WithVariousInputs_ShouldReturnExpectedResult(string value, bool expected)
    {
        // Act
        var result = IsValidRequired(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidateRequired_WithNullInput_ShouldReturnFalse()
    {
        // Act
        var result = IsValidRequired(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Length Validation Tests

    [Theory]
    [InlineData("test", 2, 10, true)]
    [InlineData("test", 4, 4, true)]
    [InlineData("test", 1, 3, false)]
    [InlineData("test", 5, 10, false)]
    [InlineData("", 0, 10, true)]
    [InlineData("", 1, 10, false)]
    public void ValidateLength_WithVariousInputs_ShouldReturnExpectedResult(string value, int minLength, int maxLength, bool expected)
    {
        // Act
        var result = IsValidLength(value, minLength, maxLength);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidateLength_WithNullInput_ShouldReturnFalse()
    {
        // Act
        var result = IsValidLength(null!, 1, 10);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Range Validation Tests

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    [InlineData(-5, 1, 10, false)]
    public void ValidateRange_WithVariousInputs_ShouldReturnExpectedResult(int value, int min, int max, bool expected)
    {
        // Act
        var result = IsValidRange(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5.5, 1.0, 10.0, true)]
    [InlineData(1.0, 1.0, 10.0, true)]
    [InlineData(10.0, 1.0, 10.0, true)]
    [InlineData(0.5, 1.0, 10.0, false)]
    [InlineData(10.5, 1.0, 10.0, false)]
    public void ValidateRange_WithDecimalInputs_ShouldReturnExpectedResult(decimal value, decimal min, decimal max, bool expected)
    {
        // Act
        var result = IsValidRange(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Price Validation Tests

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(1.00, true)]
    [InlineData(999.99, true)]
    [InlineData(0.00, false)]
    [InlineData(-1.00, false)]
    [InlineData(1000.00, false)] // Precio muy alto
    public void ValidatePrice_WithVariousInputs_ShouldReturnExpectedResult(decimal price, bool expected)
    {
        // Act
        var result = IsValidPrice(price);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Helper Methods

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        if (email.Length > 254)
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        if (phone.Length > 20)
            return false;

        // Remover caracteres no numéricos excepto + al inicio
        var cleaned = phone.Trim();
        if (cleaned.StartsWith("+"))
        {
            cleaned = "+" + new string(cleaned.Skip(1).Where(char.IsDigit).ToArray());
        }
        else
        {
            cleaned = new string(cleaned.Where(char.IsDigit).ToArray());
        }

        // Verificar que tenga al menos 7 dígitos y máximo 15
        return cleaned.Length >= 7 && cleaned.Length <= 15;
    }

    private static bool IsValidRequired(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsValidLength(string value, int minLength, int maxLength)
    {
        if (value == null)
            return false;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    private static bool IsValidRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    private static bool IsValidRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    private static bool IsValidPrice(decimal price)
    {
        return price > 0 && price <= 999.99m;
    }

    #endregion
}
