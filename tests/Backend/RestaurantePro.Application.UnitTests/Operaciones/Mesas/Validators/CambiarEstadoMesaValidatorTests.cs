namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CAMBIAR ESTADO MESA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de cambio de estado de mesas
/// Cobertura: 100% de reglas de negocio del CambiarEstadoMesaValidator
/// </summary>
public class CambiarEstadoMesaValidatorTests
{
    private readonly CambiarEstadoMesaValidator _validator;

    public CambiarEstadoMesaValidatorTests()
    {
        _validator = new CambiarEstadoMesaValidator();
    }

    #region Validation Command Helper

    private CambiarEstadoMesaCommand CrearCommandValido()
    {
        return new CambiarEstadoMesaCommand
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = EstadoMesa.Ocupada,
            Motivo = "Mesa ocupada por clientes",
            Observaciones = "Cliente VIP, servicio premium",
            UsuarioId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación MesaId

    [Fact]
    public async Task Validate_ConMesaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMesaIdValido_NoDeberiaRetornarErrorDeMesaId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
    }

    #endregion

    #region Validación NuevoEstado

    [Theory]
    [InlineData(EstadoMesa.Disponible)]
    [InlineData(EstadoMesa.Ocupada)]
    [InlineData(EstadoMesa.Reservada)]
    [InlineData(EstadoMesa.FueraDeServicio)]
    public async Task Validate_ConNuevoEstadoValido_NoDeberiaRetornarErrorDeEstado(EstadoMesa estadoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = estadoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.NuevoEstado) &&
            e.ErrorMessage.Contains("El estado especificado no es válido"));
    }

    [Fact]
    public async Task Validate_ConNuevoEstadoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = (EstadoMesa)99; // Estado inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.NuevoEstado) &&
            e.ErrorMessage.Contains("El estado especificado no es válido"));
    }

    #endregion

    #region Validación Motivo (Condicional)

    [Fact]
    public async Task Validate_ConMotivoVacioParaFueraDeServicio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.FueraDeServicio;
        command.Motivo = string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo es obligatorio cuando se marca una mesa como fuera de servicio"));
    }

    [Fact]
    public async Task Validate_ConMotivoNullParaFueraDeServicio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.FueraDeServicio;
        command.Motivo = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo es obligatorio cuando se marca una mesa como fuera de servicio"));
    }

    [Fact]
    public async Task Validate_ConMotivoValidoParaFueraDeServicio_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.FueraDeServicio;
        command.Motivo = "Mesa dañada - necesita reparación";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo es obligatorio cuando se marca una mesa como fuera de servicio"));
    }

    [Theory]
    [InlineData(EstadoMesa.Disponible)]
    [InlineData(EstadoMesa.Ocupada)]
    [InlineData(EstadoMesa.Reservada)]
    public async Task Validate_ConMotivoVacioParaOtrosEstados_NoDeberiaRetornarError(EstadoMesa estado)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = estado;
        command.Motivo = string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo es obligatorio cuando se marca una mesa como fuera de servicio"));
    }

    #endregion

    #region Validación Longitud Motivo

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
    }

    [Theory]
    [InlineData("Motivo corto")]
    [InlineData("Mesa ocupada por clientes VIP")]
    [InlineData("Reparación urgente de la mesa por daño en la superficie")]
    public async Task Validate_ConMotivoLongitudValida_NoDeberiaRetornarErrorDeLongitud(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('A', 200); // Exactamente 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConMotivoVacioParaEstadosQueNoLoRequieren_NoDeberiaValidarLongitud(string motivoVacio)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.Disponible; // No requiere motivo
        command.Motivo = motivoVacio;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
    }

    #endregion

    #region Validación Observaciones

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('B', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Theory]
    [InlineData("Observaciones cortas")]
    [InlineData("Cliente VIP requiere atención especial")]
    [InlineData("Mesa reservada para evento empresarial importante, requiere configuración especial")]
    public async Task Validate_ConObservacionesLongitudValida_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesValidas;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('B', 500); // Exactamente 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarLongitud(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorDeUsuario()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdNull_NoDeberiaValidar()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.UsuarioId));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new CambiarEstadoMesaCommand
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = EstadoMesa.FueraDeServicio,
            Motivo = "Mesa dañada - necesita reparación de la superficie",
            Observaciones = "Cliente reportó que la mesa está inestable",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandMinimoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new CambiarEstadoMesaCommand
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = EstadoMesa.Disponible
            // Campos opcionales omitidos
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new CambiarEstadoMesaCommand
        {
            MesaId = Guid.Empty, // Error
            NuevoEstado = (EstadoMesa)99, // Error
            Motivo = new string('A', 201), // Error
            Observaciones = new string('B', 501), // Error
            UsuarioId = Guid.Empty // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Mesa dañada")]
    [InlineData("Reparación urgente")]
    [InlineData("Mantenimiento preventivo")]
    [InlineData("Mesa bloqueada por evento especial")]
    public async Task Validate_ConDiferentesMotivosParaFueraDeServicio_DeberiaSerValido(string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.FueraDeServicio;
        command.Motivo = motivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo));
    }

    [Theory]
    [InlineData(EstadoMesa.Disponible, "Mesa liberada")]
    [InlineData(EstadoMesa.Ocupada, "Clientes acomodados")]
    [InlineData(EstadoMesa.Reservada, "Mesa apartada para reservación")]
    public async Task Validate_ConDiferentesCombinacionesEstadoMotivo_DeberiaSerValido(EstadoMesa estado, string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = estado;
        command.Motivo = motivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCambioAOcupadaSinMotivo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.Ocupada;
        command.Motivo = null;
        command.Observaciones = "Cliente VIP";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCambioADisponibleMinimo_DeberiaSerValido()
    {
        // Arrange
        var command = new CambiarEstadoMesaCommand
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = EstadoMesa.Disponible
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(100, true)]  // 100 caracteres - válido  
    [InlineData(200, true)]  // 200 caracteres - límite válido
    [InlineData(201, false)] // 201 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoEstado = EstadoMesa.FueraDeServicio;
        command.Motivo = new string('M', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
                e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(CambiarEstadoMesaCommand.Motivo) &&
                e.ErrorMessage.Contains("El motivo no puede exceder los 200 caracteres"));
        }
    }

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - límite válido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesObservaciones_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('O', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(CambiarEstadoMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
    }

    #endregion
} 