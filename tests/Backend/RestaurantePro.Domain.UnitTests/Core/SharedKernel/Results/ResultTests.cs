namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Results
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldReturnSuccessfulResult()
        {
            // Arrange & Act
            var result = Result.Success();
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void Success_WithValue_ShouldReturnSuccessfulResultWithValue()
        {
            // Arrange & Act
            var value = "test value";
            var result = Result.Success(value);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.Value.Should().Be(value);
        }
        
        [Fact]
        public void Failure_ShouldReturnFailureResult()
        {
            // Arrange & Act
            var errorMessage = "Test error";
            var result = Result.Failure(errorMessage);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Error.Should().Be(errorMessage);
        }
        
        [Fact]
        public void Failure_WithGenericType_ShouldReturnFailureResultWithDefaultValue()
        {
            // Arrange & Act
            var errorMessage = "Test error";
            var result = Result.Failure<string>(errorMessage);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Error.Should().Be(errorMessage);
            result.Value.Should().Be(default);
        }
        
        [Fact]
        public void Failure_WithMultipleErrors_ShouldReturnFailureResultWithErrorsList()
        {
            // Arrange & Act
            var errors = new List<string> { "Error 1", "Error 2" };
            var result = Result.Failure(errors);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(errors);
        }
    }
} 