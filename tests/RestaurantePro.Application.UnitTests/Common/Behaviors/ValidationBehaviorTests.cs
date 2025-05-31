namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para ValidationBehavior - Validación automática con FluentValidation
/// </summary>
public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<CrearProductoCommand>> _mockValidator;
    private readonly ValidationBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public ValidationBehaviorTests()
    {
        _mockValidator = new Mock<IValidator<CrearProductoCommand>>();
        var validators = new List<IValidator<CrearProductoCommand>> { _mockValidator.Object };
        _behavior = new ValidationBehavior<CrearProductoCommand, Result<ProductoDto>>(validators);
    }

    [Fact]
    public async Task Handle_SinValidadores_DeberiaEjecutarSinValidacion()
    {
        // Arrange
        var emptyValidators = new List<IValidator<CrearProductoCommand>>();
        var behaviorSinValidadores = new ValidationBehavior<CrearProductoCommand, Result<ProductoDto>>(emptyValidators);
        
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behaviorSinValidadores.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        mockNext.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidacionExitosa_DeberiaEjecutarNext()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var validationResult = new FluentValidation.Results.ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        mockNext.Verify(x => x(), Times.Once);
        _mockValidator.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<CrearProductoCommand>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidationFails_DeberiaLanzarValidationException()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "" };
        
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Nombre", "El nombre es requerido"),
            new("Precio", "El precio debe ser mayor a 0")
        };
        
        var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Errors.Should().ContainKey("Nombre");
        exception.Errors.Should().ContainKey("Precio");
        exception.Errors["Nombre"].Should().Contain("El nombre es requerido");
        exception.Errors["Precio"].Should().Contain("El precio debe ser mayor a 0");
        
        // Next no debería ejecutarse
        mockNext.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleValidadores_DeberiaEjecutarTodos()
    {
        // Arrange
        var mockValidator2 = new Mock<IValidator<CrearProductoCommand>>();
        var validators = new List<IValidator<CrearProductoCommand>> 
        { 
            _mockValidator.Object, 
            mockValidator2.Object 
        };
        var behaviorMultiple = new ValidationBehavior<CrearProductoCommand, Result<ProductoDto>>(validators);
        
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        var validationResult = new FluentValidation.Results.ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
            
        mockValidator2.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behaviorMultiple.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _mockValidator.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<CrearProductoCommand>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        mockValidator2.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<CrearProductoCommand>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleValidadoresConErrores_DeberiaAgruparTodosLosErrores()
    {
        // Arrange
        var mockValidator2 = new Mock<IValidator<CrearProductoCommand>>();
        var validators = new List<IValidator<CrearProductoCommand>> 
        { 
            _mockValidator.Object, 
            mockValidator2.Object 
        };
        var behaviorMultiple = new ValidationBehavior<CrearProductoCommand, Result<ProductoDto>>(validators);
        
        var command = new CrearProductoCommand { Nombre = "" };
        
        // Primer validador con errores
        var validationFailures1 = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Nombre", "El nombre es requerido")
        };
        var validationResult1 = new FluentValidation.Results.ValidationResult(validationFailures1);
        
        // Segundo validador con errores diferentes
        var validationFailures2 = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Precio", "El precio debe ser mayor a 0"),
            new("CategoriaId", "La categoría es requerida")
        };
        var validationResult2 = new FluentValidation.Results.ValidationResult(validationFailures2);
        
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);
            
        mockValidator2.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behaviorMultiple.Handle(command, mockNext.Object, CancellationToken.None));

        // Debe contener errores de ambos validadores
        exception.Errors.Should().ContainKey("Nombre");
        exception.Errors.Should().ContainKey("Precio");
        exception.Errors.Should().ContainKey("CategoriaId");
        exception.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaUsarTokenEnValidacion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var cancellationToken = new CancellationToken();
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        var validationResult = new FluentValidation.Results.ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                cancellationToken))
            .ReturnsAsync(validationResult);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        _mockValidator.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<CrearProductoCommand>>(), 
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidationExceptionEnValidador_DeberiaLanzarExcepcionOriginal()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en validador"));
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Be("Error en validador");
        mockNext.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_ErroresConPropiedadesVacias_DeberiaFiltrarErroresNull()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "" };
        
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Nombre", "El nombre es requerido"),
            null!, // Error null que debería filtrarse
            new("Precio", "El precio debe ser mayor a 0")
        };
        
        var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Solo debe contener errores no nulos
        exception.Errors.Should().ContainKey("Nombre");
        exception.Errors.Should().ContainKey("Precio");
        exception.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ValidationContext_DeberiaConfigurarseBien()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        ValidationContext<CrearProductoCommand>? capturedContext = null;
        
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CrearProductoCommand>>(), 
                It.IsAny<CancellationToken>()))
            .Callback<ValidationContext<CrearProductoCommand>, CancellationToken>((context, _) => 
            {
                capturedContext = context;
            })
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.InstanceToValidate.Should().Be(command);
    }
} 