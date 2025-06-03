using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
using MediatR;

namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para ExceptionHandlingBehavior - Manejo centralizado de excepciones con conversión a errores de aplicación
/// </summary>
public class ExceptionHandlingBehaviorTests
{
    private readonly Mock<ILogger<ExceptionHandlingBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly ExceptionHandlingBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public ExceptionHandlingBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _behavior = new ExceptionHandlingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_RequestExitoso_DeberiaEjecutarSinExcepciones()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_DomainException_DeberiaConvertirAApplicationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var domainException = new BusinessRuleViolationException("REGLA_VIOLADA", "Error de dominio", "Detalle del error");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw domainException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Error de dominio");
    }

    [Fact]
    public async Task Handle_BusinessRuleViolationException_DeberiaConvertirAValidationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var businessException = new BusinessRuleViolationException("REGLA_VIOLADA", "Regla de negocio violada", "Detalle adicional");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw businessException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Regla de negocio violada");
    }

    [Fact]
    public async Task Handle_EntityNotFoundException_DeberiaConvertirANotFoundException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var entityException = new EntityNotFoundException("Producto", Guid.NewGuid(), "Core");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw entityException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Producto");
    }

    [Fact]
    public async Task Handle_ArgumentException_DeberiaConvertirAValidationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var argumentException = new ArgumentException("Parámetro inválido");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw argumentException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Parámetro inválido");
    }

    [Fact]
    public async Task Handle_UnauthorizedAccessException_DeberiaConvertirAAppException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var unauthorizedException = new UnauthorizedAccessException("Acceso denegado");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw unauthorizedException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Acceso denegado");
    }

    [Fact]
    public async Task Handle_ExceptionGenerica_DeberiaConvertirAAppException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var genericException = new Exception("Error genérico");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw genericException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Error genérico");
    }

    [Fact]
    public async Task Handle_CualquierExcepcion_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var testException = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw testException;

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Verificar que se loggea el error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error no controlado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionConRequestId_DeberiaIncluirRequestIdEnLog()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var testException = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw testException;

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Verificar que se loggea con RequestId
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error no controlado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConflictException_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var conflictException = new ConflictException("Ya existe un usuario con ese email", ConflictType.DuplicateEntity);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw conflictException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Ya existe un usuario");
        exception.ConflictType.Should().Be(ConflictType.DuplicateEntity);
    }

    [Fact]
    public async Task Handle_ValidationExceptionDeApplication_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var validationException = new ValidationException("Error de validación", "Campo", "Error de validación");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw validationException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Error de validación");
    }

    [Fact]
    public async Task Handle_OperationCanceledException_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var operationCanceledException = new OperationCanceledException("Operación cancelada");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw operationCanceledException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Operación cancelada");
    }

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NotSupportedException))]
    [InlineData(typeof(TimeoutException))]
    public async Task Handle_ExcepcionesEspecificas_DeberiaConvertirAAppException(Type exceptionType)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Error específico")!;
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        var resultException = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        resultException.Message.Should().Contain("Error específico");
    }
} 
