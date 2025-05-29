namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Pruebas unitarias para ValidationBehavior
/// Valida el comportamiento del pipeline de validación de MediatR
/// </summary>
public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validator1Mock;
    private readonly Mock<IValidator<TestRequest>> _validator2Mock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;
    private readonly ValidationBehavior<TestRequest, TestResponse> _behavior;

    public ValidationBehaviorTests()
    {
        _validator1Mock = new Mock<IValidator<TestRequest>>();
        _validator2Mock = new Mock<IValidator<TestRequest>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();

        var validators = new List<IValidator<TestRequest>>
        {
            _validator1Mock.Object,
            _validator2Mock.Object
        };

        _behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
    }

    [Fact]
    public async Task Handle_ConRequestValido_DeberiaContinuarPipeline()
    {
        // Arrange
        var request = new TestRequest { Nombre = "Test válido", Edad = 25 };
        var expectedResponse = new TestResponse { Success = true };

        var validationResult1 = new ValidationResult();
        var validationResult2 = new ValidationResult();

        _validator1Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);

        _validator2Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);

        _nextMock.Setup(x => x())
            .ReturnsAsync(expectedResponse);

        // Act
        var resultado = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        resultado.Should().Be(expectedResponse);
        _validator1Mock.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        _validator2Mock.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErroresValidacion_DeberiaLanzarValidationException()
    {
        // Arrange
        var request = new TestRequest { Nombre = "", Edad = -5 };

        var validationFailure1 = new ValidationFailure("Nombre", "El nombre es obligatorio");
        var validationFailure2 = new ValidationFailure("Edad", "La edad debe ser mayor a 0");

        var validationResult1 = new ValidationResult(new[] { validationFailure1 });
        var validationResult2 = new ValidationResult(new[] { validationFailure2 });

        _validator1Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);

        _validator2Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _behavior.Handle(request, _nextMock.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(validationFailure1);
        exception.Errors.Should().Contain(validationFailure2);

        // Verify que no se ejecutó el siguiente paso del pipeline
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_SinValidadores_DeberiaContinuarPipeline()
    {
        // Arrange
        var request = new TestRequest { Nombre = "Test", Edad = 30 };
        var expectedResponse = new TestResponse { Success = true };

        var emptyValidators = new List<IValidator<TestRequest>>();
        var behaviorSinValidadores = new ValidationBehavior<TestRequest, TestResponse>(emptyValidators);

        _nextMock.Setup(x => x())
            .ReturnsAsync(expectedResponse);

        // Act
        var resultado = await behaviorSinValidadores.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        resultado.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConValidadorQueNoTieneErrores_DeberiaContinuar()
    {
        // Arrange
        var request = new TestRequest { Nombre = "Producto válido", Edad = 25 };
        var expectedResponse = new TestResponse { Success = true };

        var validationResultSinErrores = new ValidationResult();

        _validator1Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResultSinErrores);

        _validator2Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResultSinErrores);

        _nextMock.Setup(x => x())
            .ReturnsAsync(expectedResponse);

        // Act
        var resultado = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        resultado.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaUsarloEnValidadores()
    {
        // Arrange
        var request = new TestRequest { Nombre = "Test", Edad = 25 };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = new CancellationToken();

        var validationResult = new ValidationResult();

        _validator1Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            cancellationToken))
            .ReturnsAsync(validationResult);

        _validator2Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            cancellationToken))
            .ReturnsAsync(validationResult);

        _nextMock.Setup(x => x())
            .ReturnsAsync(expectedResponse);

        // Act
        await _behavior.Handle(request, _nextMock.Object, cancellationToken);

        // Assert - Verificar que se usó el CancellationToken correcto
        _validator1Mock.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            cancellationToken), Times.Once);
        _validator2Mock.Verify(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConSoloAlgunosValidadoresConErrores_DeberiaAcumularTodosLosErrores()
    {
        // Arrange
        var request = new TestRequest { Nombre = "", Edad = -5 };

        var validationFailure1 = new ValidationFailure("Nombre", "El nombre es obligatorio");
        var validationFailure2 = new ValidationFailure("Edad", "La edad debe ser mayor a 0");

        var validationResult1 = new ValidationResult(new[] { validationFailure1 });
        var validationResult2 = new ValidationResult(); // Sin errores
        var validationResult3 = new ValidationResult(new[] { validationFailure2 });

        // Agregar un tercer validador para esta prueba
        var validator3Mock = new Mock<IValidator<TestRequest>>();
        var validators = new List<IValidator<TestRequest>>
        {
            _validator1Mock.Object,
            _validator2Mock.Object,
            validator3Mock.Object
        };

        var behaviorConTresValidadores = new ValidationBehavior<TestRequest, TestResponse>(validators);

        _validator1Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);

        _validator2Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);

        validator3Mock.Setup(x => x.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult3);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behaviorConTresValidadores.Handle(request, _nextMock.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(validationFailure1);
        exception.Errors.Should().Contain(validationFailure2);
    }

    #region Test Types

    public class TestRequest : IRequest<TestResponse>
    {
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
    }

    public class TestResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
} 