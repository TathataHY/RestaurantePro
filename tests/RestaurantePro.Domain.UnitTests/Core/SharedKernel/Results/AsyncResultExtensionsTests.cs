using RestaurantePro.Domain.Core.SharedKernel.Results;
namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Results
{
    public class AsyncResultExtensionsTests
    {
        [Fact]
        public async Task AsResultAsync_ShouldReturnSuccessResultWithValue()
        {
            // Arrange
            var task = Task.FromResult("test value");
            
            // Act
            var result = await AsyncResultExtensions.AsResultAsync(task);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be("test value");
        }
        
        [Fact]
        public async Task AsResultAsync_ShouldReturnFailureResultWhenTaskFails()
        {
            // Arrange
            var task = Task.FromException<string>(new Exception("Test exception"));
            
            // Act
            var result = await AsyncResultExtensions.AsResultAsync(task);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Test exception");
        }
        
        [Fact]
        public async Task TryAsync_ShouldReturnSuccessResultWhenTaskCompletes()
        {
            // Arrange
            var task = Task.CompletedTask;
            
            // Act
            var result = await AsyncResultExtensions.TryAsync(task);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public async Task TryAsync_ShouldReturnFailureResultWhenTaskFails()
        {
            // Arrange
            var task = Task.FromException(new Exception("Test exception"));
            
            // Act
            var result = await AsyncResultExtensions.TryAsync(task);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Test exception");
        }
        
        [Fact]
        public async Task TryAsync_WithValue_ShouldReturnSuccessResultWithValue()
        {
            // Arrange
            var task = Task.FromResult("test value");
            
            // Act
            var result = await AsyncResultExtensions.TryAsync(task);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be("test value");
        }
        
        [Fact]
        public async Task TryAsync_WithValue_ShouldReturnFailureResultWhenTaskFails()
        {
            // Arrange
            var task = Task.FromException<string>(new Exception("Test exception"));
            
            // Act
            var result = await AsyncResultExtensions.TryAsync(task);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Test exception");
        }
    }
} 