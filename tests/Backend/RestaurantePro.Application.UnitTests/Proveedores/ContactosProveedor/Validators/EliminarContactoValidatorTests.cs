namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ELIMINAR CONTACTO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de eliminación de contactos
/// Cobertura: 100% de reglas de negocio del EliminarContactoValidator
/// </summary>
public class EliminarContactoValidatorTests
{
    private readonly EliminarContactoValidator _validator;

    public EliminarContactoValidatorTests()
    {
        _validator = new EliminarContactoValidator();
    }

    #region Validation Command Helper

    private EliminarContactoCommand CrearCommandValido()
    {
        return new EliminarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            TipoEliminacion = TipoEliminacion.Logica,
            MotivoEliminacion = "Contacto ya no trabaja en la empresa proveedora",
            ReasignarContactoPrincipal = true,
            ForzarEliminacion = false
        };
    }

    #endregion

    #region Validación Id

    [Fact]
    public async Task Validate_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del contacto es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConIdValido_NoDeberiaRetornarErrorEnId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del contacto es obligatorio"));
    }

    #endregion

    #region Validación ProveedorId

    [Fact]
    public async Task Validate_ConProveedorIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProveedorId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El ID del proveedor es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConProveedorIdValido_NoDeberiaRetornarErrorEnProveedorId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProveedorId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El ID del proveedor es obligatorio"));
    }

    #endregion

    #region Validación MotivoEliminacion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoEliminacionVacioONull_DeberiaRetornarError(string motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoEliminacion = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion) &&
            e.ErrorMessage.Contains("El motivo de eliminación es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMotivoEliminacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoEliminacion = new string('A', 301); // Más de 300 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion) &&
            e.ErrorMessage.Contains("El motivo de eliminación no puede exceder 300 caracteres"));
    }

    [Theory]
    [InlineData("Contacto ya no trabaja en la empresa")]
    [InlineData("Cambio de responsable de compras")]
    [InlineData("Reestructuración del departamento")]
    [InlineData("A")]  // Mínimo válido
    public async Task Validate_ConMotivoEliminacionValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoEliminacion = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion));
    }

    [Fact]
    public async Task Validate_ConMotivoEliminacionEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoEliminacion = new string('A', 300); // Exactamente 300 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion) &&
            e.ErrorMessage.Contains("El motivo de eliminación no puede exceder 300 caracteres"));
    }

    #endregion

    #region Validación TipoEliminacion

    [Theory]
    [InlineData(TipoEliminacion.Logica)]
    [InlineData(TipoEliminacion.Fisica)]
    public async Task Validate_ConTipoEliminacionValido_NoDeberiaRetornarErrorDeTipo(TipoEliminacion tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = tipoValido;
        command.ForzarEliminacion = tipoValido == TipoEliminacion.Fisica; // Ajustar para física

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.TipoEliminacion) &&
            e.ErrorMessage.Contains("El tipo de eliminación debe ser válido"));
    }

    [Fact]
    public async Task Validate_ConTipoEliminacionInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = (TipoEliminacion)999; // Valor inválido

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.TipoEliminacion) &&
            e.ErrorMessage.Contains("El tipo de eliminación debe ser válido"));
    }

    #endregion

    #region Validación NuevoContactoPrincipalId

    [Fact]
    public async Task Validate_ConNuevoContactoPrincipalIgualAlContactoAEliminar_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var contactoId = Guid.NewGuid();
        command.Id = contactoId;
        command.NuevoContactoPrincipalId = contactoId; // Mismo ID

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.NuevoContactoPrincipalId) &&
            e.ErrorMessage.Contains("El nuevo contacto principal no puede ser el mismo que se está eliminando"));
    }

    [Fact]
    public async Task Validate_ConNuevoContactoPrincipalDiferente_NoDeberiaRetornarErrorDeNuevoContacto()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.NewGuid();
        command.NuevoContactoPrincipalId = Guid.NewGuid(); // Diferente ID

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.NuevoContactoPrincipalId) &&
            e.ErrorMessage.Contains("El nuevo contacto principal no puede ser el mismo que se está eliminando"));
    }

    [Fact]
    public async Task Validate_ConNuevoContactoPrincipalNull_NoDeberiaRetornarErrorDeNuevoContacto()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoContactoPrincipalId = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(EliminarContactoCommand.NuevoContactoPrincipalId) &&
            e.ErrorMessage.Contains("El nuevo contacto principal no puede ser el mismo que se está eliminando"));
    }

    #endregion

    #region Validación Eliminación Física

    [Fact]
    public async Task Validate_ConEliminacionFisicaSinForzar_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = TipoEliminacion.Fisica;
        command.ForzarEliminacion = false; // No forzada

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(EliminarContactoCommand.TipoEliminacion) &&
            e.ErrorMessage.Contains("La eliminación física requiere confirmación forzada"));
    }

    [Fact]
    public async Task Validate_ConEliminacionFisicaForzada_NoDeberiaRetornarErrorDeFuerza()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = TipoEliminacion.Fisica;
        command.ForzarEliminacion = true; // Forzada

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("La eliminación física requiere confirmación forzada"));
    }

    [Fact]
    public async Task Validate_ConEliminacionLogica_NoDeberiaRequerirFuerza()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = TipoEliminacion.Logica;
        command.ForzarEliminacion = false; // No forzada

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("La eliminación física requiere confirmación forzada"));
    }

    #endregion

    #region Validación ReasignarContactoPrincipal

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConReasignarContactoPrincipal_NoDeberiaRetornarErrorDeReasignacion(bool reasignar)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ReasignarContactoPrincipal = reasignar;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(EliminarContactoCommand.ReasignarContactoPrincipal));
    }

    #endregion

    #region Validación ForzarEliminacion

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConForzarEliminacion_NoDeberiaRetornarErrorDeForzar(bool forzar)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ForzarEliminacion = forzar;
        if (forzar)
        {
            command.TipoEliminacion = TipoEliminacion.Fisica; // Ajustar para coherencia
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(EliminarContactoCommand.ForzarEliminacion));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new EliminarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            TipoEliminacion = TipoEliminacion.Logica,
            MotivoEliminacion = "El contacto ya no pertenece a la empresa proveedora debido a reestructuración organizacional",
            ReasignarContactoPrincipal = true,
            NuevoContactoPrincipalId = Guid.NewGuid(),
            ForzarEliminacion = false,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "FechaUltimoContacto", DateTime.UtcNow.AddDays(-30) },
                { "MotivoDetallado", "Cambio de departamento interno" }
            }
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
        var command = new EliminarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            TipoEliminacion = TipoEliminacion.Logica,
            MotivoEliminacion = "A", // Mínimo válido
            ReasignarContactoPrincipal = false,
            ForzarEliminacion = false
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConEliminacionFisicaCompleta_DeberiaSerValido()
    {
        // Arrange
        var command = new EliminarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            TipoEliminacion = TipoEliminacion.Fisica,
            MotivoEliminacion = "Eliminación permanente por solicitud legal de protección de datos",
            ReasignarContactoPrincipal = false,
            ForzarEliminacion = true, // Requerido para física
            DatosAdicionales = new Dictionary<string, object>
            {
                { "SolicitudLegal", true },
                { "NumeroTicket", "LEG-2025-001" }
            }
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
        var command = new EliminarContactoCommand
        {
            Id = Guid.Empty, // Error
            ProveedorId = Guid.Empty, // Error
            TipoEliminacion = (TipoEliminacion)999, // Error
            MotivoEliminacion = "", // Error: vacío
            NuevoContactoPrincipalId = Guid.NewGuid(), // Se establecerá igual a Id después
            ForzarEliminacion = false
        };
        command.NuevoContactoPrincipalId = command.Id; // Error: mismo ID

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EliminarContactoCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EliminarContactoCommand.ProveedorId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EliminarContactoCommand.TipoEliminacion));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(150, true)]  // 150 caracteres - válido
    [InlineData(300, true)]  // 300 caracteres - límite válido
    [InlineData(301, false)] // 301 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoEliminacion = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion) &&
                e.ErrorMessage.Contains("El motivo de eliminación no puede exceder 300 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(EliminarContactoCommand.MotivoEliminacion) &&
                e.ErrorMessage.Contains("El motivo de eliminación no puede exceder 300 caracteres"));
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConConstructorParametrizado_DeberiaSerValido()
    {
        // Arrange
        var contactoId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var motivo = "Contacto cambió de empresa";
        
        var command = new EliminarContactoCommand(contactoId, proveedorId, motivo);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        command.Id.Should().Be(contactoId);
        command.ProveedorId.Should().Be(proveedorId);
        command.MotivoEliminacion.Should().Be(motivo);
    }

    [Fact]
    public async Task Validate_ConDatosAdicionales_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DatosAdicionales = new Dictionary<string, object>
        {
            { "Motivo", "Datos adicionales" },
            { "Usuario", "admin@test.com" },
            { "Fecha", DateTime.UtcNow },
            { "Numero", 12345 },
            { "Activo", true }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(TipoEliminacion.Logica, false, true)]   // Lógica sin forzar - válido
    [InlineData(TipoEliminacion.Logica, true, true)]    // Lógica con forzar - válido
    [InlineData(TipoEliminacion.Fisica, false, false)]  // Física sin forzar - inválido
    [InlineData(TipoEliminacion.Fisica, true, true)]    // Física con forzar - válido
    public async Task Validate_ConDiferentesCombinacionesTipoForzar_DeberiaValidarCorrectamente(
        TipoEliminacion tipo, bool forzar, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoEliminacion = tipo;
        command.ForzarEliminacion = forzar;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.ErrorMessage.Contains("La eliminación física requiere confirmación forzada"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("La eliminación física requiere confirmación forzada"));
        }
    }

    #endregion
} 