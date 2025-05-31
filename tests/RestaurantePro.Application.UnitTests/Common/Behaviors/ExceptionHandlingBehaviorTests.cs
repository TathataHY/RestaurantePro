namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para ExceptionHandlingBehavior - Comportamiento crítico que maneja todas las excepciones
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
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        mockNext.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_DomainException_DeberiaConvertirAApplicationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var domainException = new DomainException("Error de dominio");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(domainException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Error de dominio");
        exception.ErrorCode.Should().Be("BUSINESS_LOGIC_ERROR");
    }

    [Fact]
    public async Task Handle_BusinessRuleViolationException_DeberiaConvertirAValidationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var businessException = new BusinessRuleViolationException("Regla de negocio violada");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Regla de negocio violada");
    }

    [Fact]
    public async Task Handle_EntityNotFoundException_DeberiaConvertirANotFoundException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var entityException = new EntityNotFoundException("Producto", "123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(entityException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Producto");
        exception.Message.Should().Contain("123");
    }

    [Fact]
    public async Task Handle_ArgumentException_DeberiaConvertirAValidationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var argumentException = new ArgumentException("Parámetro inválido");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(argumentException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Parámetro inválido");
    }

    [Fact]
    public async Task Handle_UnauthorizedAccessException_DeberiaConvertirAAppException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var unauthorizedException = new UnauthorizedAccessException("Acceso denegado");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(unauthorizedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Acceso denegado");
        exception.ErrorSeverity.Should().Be(ErrorSeverity.High);
    }

    [Fact]
    public async Task Handle_ExceptionGenerica_DeberiaConvertirAAppException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var genericException = new Exception("Error genérico");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Error genérico");
        exception.ErrorCode.Should().Be("INTERNAL_ERROR");
    }

    [Fact]
    public async Task Handle_CualquierExcepcion_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var testException = new Exception("Error de test");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(testException);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Verificar que se loggea el error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error procesando")),
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
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(testException);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Verificar que se loggea con RequestId
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConflictException_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var conflictException = ConflictException.ForDuplicate("Producto", "Test", "Ya existe un producto con este nombre");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(conflictException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Should().Be(conflictException);
        exception.ConflictType.Should().Be(ConflictType.Duplicate);
    }

    [Fact]
    public async Task Handle_ValidationExceptionDeApplication_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var validationErrors = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Nombre", "El nombre es requerido")
        };
        var validationException = new ValidationException(validationErrors);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(validationException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Should().Be(validationException);
        exception.Errors.Should().ContainKey("Nombre");
    }

    [Fact]
    public async Task Handle_OperationCanceledException_DeberiaMantenerseSinConversion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var cancellationToken = new CancellationToken(canceled: true);
        var canceledException = new OperationCanceledException(cancellationToken);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(canceledException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, mockNext.Object, cancellationToken));

        exception.Should().Be(canceledException);
    }

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NotSupportedException))]
    [InlineData(typeof(TimeoutException))]
    public async Task Handle_ExcepcionesEspecificas_DeberiaConvertirAAppException(Type exceptionType)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var specificException = (Exception)Activator.CreateInstance(exceptionType, "Mensaje de test")!;
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(specificException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Mensaje de test");
        exception.InnerException.Should().Be(specificException);
    }
} 