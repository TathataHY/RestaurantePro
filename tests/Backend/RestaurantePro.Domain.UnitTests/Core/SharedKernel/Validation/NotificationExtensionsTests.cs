namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Validation
{
    public class NotificationExtensionsTests
    {
        [Fact]
        public void Require_WhenConditionIsTrue_ShouldNotAddError()
        {
            // Arrange
            var notification = new Notification();
            
            // Act
            notification.Require(true, "This error should not be added");
            
            // Assert
            notification.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void Require_WhenConditionIsFalse_ShouldAddError()
        {
            // Arrange
            var notification = new Notification();
            var errorMessage = "This error should be added";
            
            // Act
            notification.Require(false, errorMessage);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
        
        [Fact]
        public void RequireNotNull_WhenValueIsNotNull_ShouldNotAddError()
        {
            // Arrange
            var notification = new Notification();
            var value = new object();
            
            // Act
            notification.RequireNotNull(value, "This error should not be added");
            
            // Assert
            notification.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void RequireNotNull_WhenValueIsNull_ShouldAddError()
        {
            // Arrange
            var notification = new Notification();
            object? value = null;
            var errorMessage = "Value cannot be null";
            
            // Act
            notification.RequireNotNull(value, errorMessage);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
        
        [Fact]
        public void RequireNotEmpty_WhenStringIsNotEmpty_ShouldNotAddError()
        {
            // Arrange
            var notification = new Notification();
            var value = "Not empty";
            
            // Act
            notification.RequireNotEmpty(value, "This error should not be added");
            
            // Assert
            notification.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void RequireNotEmpty_WhenStringIsEmpty_ShouldAddError()
        {
            // Arrange
            var notification = new Notification();
            string value = string.Empty;
            var errorMessage = "String cannot be empty";
            
            // Act
            notification.RequireNotEmpty(value, errorMessage);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
        
        [Fact]
        public void RequireNotEmpty_WhenStringIsNull_ShouldAddError()
        {
            // Arrange
            var notification = new Notification();
            string? value = null;
            var errorMessage = "String cannot be null";
            
            // Act
            notification.RequireNotEmpty(value, errorMessage);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
        
        [Fact]
        public void OnSuccess_WhenNotificationHasNoErrors_ShouldExecuteAction()
        {
            // Arrange
            var notification = new Notification();
            var actionExecuted = false;
            
            // Act
            notification.OnSuccess(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeTrue();
        }
        
        [Fact]
        public void OnSuccess_WhenNotificationHasErrors_ShouldNotExecuteAction()
        {
            // Arrange
            var notification = new Notification();
            notification.AddError("Error");
            var actionExecuted = false;
            
            // Act
            notification.OnSuccess(() => actionExecuted = true);
            
            // Assert
            actionExecuted.Should().BeFalse();
        }
        
        [Fact]
        public void ValidateAnd_ShouldExecuteValidationFunction()
        {
            // Arrange
            var notification = new Notification();
            var validationCalled = false;
            
            // Act
            notification.ValidateAnd("test", (n, v) => {
                validationCalled = true;
                n.Require(v.Length > 2, "Value should have more than 2 characters");
            });
            
            // Assert
            validationCalled.Should().BeTrue();
            notification.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void ValidateAnd_WithFailingValidation_ShouldAddError()
        {
            // Arrange
            var notification = new Notification();
            var errorMessage = "Value should have more than 2 characters";
            
            // Act
            notification.ValidateAnd("a", (n, v) => {
                n.Require(v.Length > 2, errorMessage);
            });
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
    }
} 