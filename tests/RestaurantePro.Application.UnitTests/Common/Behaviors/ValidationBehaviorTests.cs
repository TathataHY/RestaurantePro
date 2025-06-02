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
        var behaviorSinValidadores = new ValidationBehavior<CrearProductoCommand, Result<ProductoDto>>(new List<IValidator<CrearProductoCommand>>());
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await behaviorSinValidadores.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_ValidacionExitosa_DeberiaEjecutarNext()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var validationResult = new FluentValidation.Results.ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var nextCalled = false;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => 
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
        _mockValidator.VerifyAll();
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
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        var nextCalled = false;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => 
        {
            nextCalled = true;
            return Task.FromResult(Result.Success(new ProductoDto()));
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Errors.Should().ContainKey("Nombre");
        exception.Errors.Should().ContainKey("Precio");
        exception.Errors["Nombre"].Should().Contain("El nombre es requerido");
        exception.Errors["Precio"].Should().Contain("El precio debe ser mayor a 0");
        
        // Next no debería ejecutarse
        nextCalled.Should().BeFalse();
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
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
            
        mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await behaviorMultiple.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _mockValidator.VerifyAll();
        mockValidator2.VerifyAll();
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
        
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);
            
        mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(Result.Success(new ProductoDto()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behaviorMultiple.Handle(command, nextDelegate, CancellationToken.None));

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
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), cancellationToken))
            .ReturnsAsync(validationResult);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidationExceptionEnValidador_DeberiaLanzarExcepcionOriginal()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en validador"));
        
        var nextCalled = false;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => 
        {
            nextCalled = true;
            return Task.FromResult(Result.Success(new ProductoDto()));
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(command, nextDelegate, CancellationToken.None));
        
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ErroresConPropiedadesVacias_DeberiaFiltrarErroresNull()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Nombre", "El nombre es requerido"),
            new(null, "Error sin propiedad"),
            new("", "Error con propiedad vacía"),
            new("Precio", "")
        };
        
        var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(Result.Success(new ProductoDto()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Debe filtrar errores con propiedades nulas o vacías
        exception.Errors.Should().ContainKey("Nombre");
        exception.Errors.Should().HaveCount(1);
        exception.Errors["Nombre"].Should().Contain("El nombre es requerido");
    }

    [Fact]
    public async Task Handle_ValidationContext_DeberiaConfigurarseBien()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        ValidationContext<CrearProductoCommand> capturedContext = null;
        
        var validationResult = new FluentValidation.Results.ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CrearProductoCommand>>(), It.IsAny<CancellationToken>()))
            .Callback<ValidationContext<CrearProductoCommand>, CancellationToken>((context, ct) => 
            {
                capturedContext = context;
            })
            .ReturnsAsync(validationResult);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(Result.Success(new ProductoDto { Nombre = "Test" }));

        // Act
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext.InstanceToValidate.Should().Be(command);
    }
} 