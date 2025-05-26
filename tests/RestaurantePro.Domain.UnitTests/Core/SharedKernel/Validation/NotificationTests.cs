using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using Xunit;
using FluentAssertions;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Validation
{
    public class NotificationTests
    {
        [Fact]
        public void Constructor_ShouldCreateEmptyNotification()
        {
            // Arrange & Act
            var notification = new Notification();
            
            // Assert
            notification.HasErrors.Should().BeFalse();
            notification.Errors.Should().BeEmpty();
        }
        
        [Fact]
        public void AddError_ShouldAddErrorToNotification()
        {
            // Arrange
            var notification = new Notification();
            var errorMessage = "Test error";
            
            // Act
            notification.AddError(errorMessage);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(1);
            notification.Errors.First().Message.Should().Be(errorMessage);
        }
        
        [Fact]
        public void AddErrors_WithErrorCollection_ShouldAddAllErrors()
        {
            // Arrange
            var notification = new Notification();
            var errors = new List<Error>
            {
                new Error("Error 1"),
                new Error("Error 2")
            };
            
            // Act
            notification.AddErrors(errors);
            
            // Assert
            notification.HasErrors.Should().BeTrue();
            notification.Errors.Should().HaveCount(2);
        }
        
        [Fact]
        public void AddErrors_WithNotification_ShouldAddAllErrorsFromOtherNotification()
        {
            // Arrange
            var notification1 = new Notification();
            notification1.AddError("Error 1");
            
            var notification2 = new Notification();
            notification2.AddError("Error 2");
            
            // Act
            notification1.AddErrors(notification2);
            
            // Assert
            notification1.HasErrors.Should().BeTrue();
            notification1.Errors.Should().HaveCount(2);
        }
        
        [Fact]
        public void ClearErrors_ShouldRemoveAllErrors()
        {
            // Arrange
            var notification = new Notification();
            notification.AddError("Error 1");
            notification.AddError("Error 2");
            
            // Act
            notification.ClearErrors();
            
            // Assert
            notification.HasErrors.Should().BeFalse();
            notification.Errors.Should().BeEmpty();
        }
        
        [Fact]
        public void ToResult_WithNoErrors_ShouldReturnSuccessResult()
        {
            // Arrange
            var notification = new Notification();
            
            // Act
            var result = notification.ToResult();
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
        }
        
        [Fact]
        public void ToResult_WithErrors_ShouldReturnFailureResult()
        {
            // Arrange
            var notification = new Notification();
            notification.AddError("Error 1");
            notification.AddError("Error 2");
            
            // Act
            var result = notification.ToResult();
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(new[] { "Error 1", "Error 2" });
        }
        
        [Fact]
        public void ToResult_WithGenericType_AndNoErrors_ShouldReturnSuccessResultWithValue()
        {
            // Arrange
            var notification = new Notification();
            var value = "test value";
            
            // Act
            var result = notification.ToResult(value);
            
            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(value);
        }
        
        [Fact]
        public void ToResult_WithGenericType_AndErrors_ShouldReturnFailureResult()
        {
            // Arrange
            var notification = new Notification();
            notification.AddError("Error 1");
            var value = "test value";
            
            // Act
            var result = notification.ToResult(value);
            
            // Assert
            result.Succeeded.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
        }
    }
} 