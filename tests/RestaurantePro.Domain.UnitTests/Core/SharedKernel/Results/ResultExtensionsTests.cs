using RestaurantePro.Domain.Core.SharedKernel.Results;
namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Results
{
    public class ResultExtensionsTests
    {
        [Fact]
        public void OnSuccess_WithAction_ShouldExecuteActionOnSuccessResult()
        {
            // Arrange
            var result = Result.Success();
            var actionExecuted = false;
            
            // Act
            result.OnSuccess(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeTrue();
        }
        
        [Fact]
        public void OnSuccess_WithAction_ShouldNotExecuteActionOnFailureResult()
        {
            // Arrange
            var result = Result.Failure("Error");
            var actionExecuted = false;
            
            // Act
            result.OnSuccess(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeFalse();
        }
        
        [Fact]
        public void OnSuccess_WithActionAndValue_ShouldExecuteActionWithValueOnSuccessResult()
        {
            // Arrange
            var value = "test value";
            var result = Result.Success(value);
            var capturedValue = string.Empty;
            
            // Act
            result.OnSuccess(v => capturedValue = v);
            
            // Assert
            capturedValue.Should().Be(value);
        }
        
        [Fact]
        public void OnSuccess_WithFunctionReturningValue_ShouldReturnSuccessResultWithTransformedValue()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            
            // Act
            var transformedResult = result.OnSuccess(v => v * 2);
            
            // Assert
            transformedResult.Succeeded.Should().BeTrue();
            transformedResult.Value.Should().Be(10);
        }
        
        [Fact]
        public void OnFailure_WithAction_ShouldExecuteActionOnFailureResult()
        {
            // Arrange
            var result = Result.Failure("Error");
            var actionExecuted = false;
            
            // Act
            result.OnFailure(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeTrue();
        }
        
        [Fact]
        public void OnFailure_WithAction_ShouldNotExecuteActionOnSuccessResult()
        {
            // Arrange
            var result = Result.Success();
            var actionExecuted = false;
            
            // Act
            result.OnFailure(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeFalse();
        }
        
        [Fact]
        public void OnFailure_WithActionAndErrorMessage_ShouldExecuteActionWithErrorMessageOnFailureResult()
        {
            // Arrange
            var errorMessage = "Test error";
            var result = Result.Failure(errorMessage);
            var capturedError = string.Empty;
            
            // Act
            result.OnFailure(e => capturedError = e ?? string.Empty);
            
            // Assert
            capturedError.Should().Be(errorMessage);
        }
        
        [Fact]
        public void Combine_WithAllSuccessResults_ShouldReturnSuccessResult()
        {
            // Arrange
            var result1 = Result.Success();
            var result2 = Result.Success();
            var result3 = Result.Success();
            
            // Act
            var combinedResult = ResultExtensions.Combine(result1, result2, result3);
            
            // Assert
            combinedResult.Succeeded.Should().BeTrue();
            combinedResult.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void Combine_WithSomeFailureResults_ShouldReturnFailureResultWithCombinedErrors()
        {
            // Arrange
            var result1 = Result.Success();
            var result2 = Result.Failure("Error 2");
            var result3 = Result.Failure("Error 3");
            
            // Act
            var combinedResult = ResultExtensions.Combine(result1, result2, result3);
            
            // Assert
            combinedResult.Succeeded.Should().BeFalse();
            combinedResult.HasErrors.Should().BeTrue();
            combinedResult.Errors.Should().HaveCount(2);
            combinedResult.Errors.Should().Contain(new[] { "Error 2", "Error 3" });
        }
    }
} 