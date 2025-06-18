using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Infrastructure.Identity.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Identity.Extensions
{
    public class IdentityResultExtensionsTests
    {
        [Fact]
        public void ToResult_DebeRetornarSuccess_CuandoIdentityResultSucceeded()
        {
            // Arrange
            var identityResult = IdentityResult.Success;

            // Act
            var result = identityResult.ToResult();

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToResult_DebeRetornarFailure_CuandoIdentityResultFailed()
        {
            // Arrange
            var error = new IdentityError { Description = "Test Error" };
            var identityResult = IdentityResult.Failed(error);

            // Act
            var result = identityResult.ToResult();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Test Error");
        }
        
        [Fact]
        public void ToResult_ConMensajeExito_DebeRetornarSuccessConMensaje()
        {
            // Arrange
            var identityResult = IdentityResult.Success;
            var successMessage = "Operación exitosa";

            // Act
            var result = identityResult.ToResult(successMessage);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(successMessage);
        }

        [Fact]
        public void ToResult_Generico_DebeRetornarSuccessConValor()
        {
            // Arrange
            var identityResult = IdentityResult.Success;
            var value = 123;

            // Act
            var result = identityResult.ToResult(value);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        [Fact]
        public void GetErrors_DebeRetornarArrayDeErrores()
        {
            // Arrange
            var errors = new[]
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };
            var identityResult = IdentityResult.Failed(errors);
            
            // Act
            var result = identityResult.GetErrors();

            // Assert
            result.Should().BeEquivalentTo(new[] { "Error 1", "Error 2" });
        }

        [Fact]
        public void GetErrors_DebeRetornarArrayVacio_CuandoEsExitoso()
        {
            // Arrange
            var identityResult = IdentityResult.Success;
            
            // Act
            var result = identityResult.GetErrors();

            // Assert
            result.Should().BeEmpty();
        }
        
        [Fact]
        public void GetErrorsAsString_DebeRetornarStringConcatenado()
        {
            // Arrange
            var errors = new[]
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };
            var identityResult = IdentityResult.Failed(errors);
            
            // Act
            var result = identityResult.GetErrorsAsString();

            // Assert
            result.Should().Be("Error 1, Error 2");
        }

        [Fact]
        public void GetErrorsAsString_DebeUsarSeparadorPersonalizado()
        {
            // Arrange
            var errors = new[]
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };
            var identityResult = IdentityResult.Failed(errors);
            
            // Act
            var result = identityResult.GetErrorsAsString(" | ");

            // Assert
            result.Should().Be("Error 1 | Error 2");
        }
        
        [Fact]
        public void GetErrorsAsString_DebeRetornarStringVacio_CuandoEsExitoso()
        {
            // Arrange
            var identityResult = IdentityResult.Success;
            
            // Act
            var result = identityResult.GetErrorsAsString();

            // Assert
            result.Should().BeEmpty();
        }
    }
} 