namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACTUALIZAR CLIENTE VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de actualización de clientes
/// Cobertura: 100% de reglas de negocio del ActualizarClienteValidator
/// </summary>
public class ActualizarClienteValidatorTests
{
    private readonly ActualizarClienteValidator _validator;

    public ActualizarClienteValidatorTests()
    {
        _validator = new ActualizarClienteValidator();
    }

    #region Validation Command Helper

    private ActualizarClienteCommand CrearCommandValido()
    {
        return new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Actualizado",
            Email = "cliente.actualizado@email.com",
            Telefono = "+521234567890",
            FechaNacimiento = DateTime.Now.AddYears(-25),
            EstaActivo = true
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
            e.PropertyName == nameof(ActualizarClienteCommand.Id) &&
            e.ErrorMessage.Contains("El ID del cliente no puede ser un GUID vacío"));
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
            e.PropertyName == nameof(ActualizarClienteCommand.Id) &&
            e.ErrorMessage.Contains("El ID del cliente no puede ser un GUID vacío"));
    }

    #endregion

    #region Validación AlMenosUnCampo

    [Fact]
    public async Task Validate_SinNingunCampoParaActualizar_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid()
            // Sin ningún campo para actualizar
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("Debe proporcionar al menos un campo para actualizar"));
    }

    [Fact]
    public async Task Validate_ConAlMenosUnCampoParaActualizar_NoDeberiaRetornarErrorDeAlMenosUnCampo()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente con al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("Debe proporcionar al menos un campo para actualizar"));
    }

    #endregion

    #region Validación Nombre

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConNombreVacioSiSeProporcionaCometed_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre no puede estar vacío si se proporciona"));
    }

    [Theory]
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre debe tener al menos 2 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre no puede exceder 200 caracteres"));
    }

    [Theory]
    [InlineData("María José García")]
    [InlineData("Juan Carlos Pérez")]
    [InlineData("Cliente Válido")]
    [InlineData("Ab")]  // Mínimo válido
    public async Task Validate_ConNombreValido_NoDeberiaRetornarErrorDeNombre(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
    }

    [Fact]
    public async Task Validate_ConNombreNull_NoDeberiaValidarNombre()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Email = "cliente@email.com" // Otro campo para que pase validación de "al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
    }

    #endregion

    #region Validación Email

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConEmailVacioSiSeProporcionada_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede estar vacío si se proporciona"));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [InlineData("email.domain.com")]
    [InlineData("email@@domain.com")]
    public async Task Validate_ConEmailFormatoInvalido_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Email) &&
            e.ErrorMessage.Contains("El formato del email no es válido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var nombreLargo = new string('a', 310);
        command.Email = $"{nombreLargo}@domain.com"; // Más de 320 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 320 caracteres"));
    }

    [Theory]
    [InlineData("cliente@restaurante.com")]
    [InlineData("maria.jose@empresa.cl")]
    [InlineData("test+cliente@sistema.net")]
    [InlineData("a@b.co")]  // Mínimo válido
    public async Task Validate_ConEmailValido_NoDeberiaRetornarErrorDeEmail(string emailValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Email));
    }

    [Fact]
    public async Task Validate_ConEmailNull_NoDeberiaValidarEmail()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Sin Email" // Otro campo para que pase validación de "al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Email));
    }

    #endregion

    #region Validación Telefono

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConTelefonoVacioSiSeProporcionada_DeberiaRetornarError(string telefonoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono no puede estar vacío si se proporciona"));
    }

    [Theory]
    [InlineData("123")]            // Muy corto
    [InlineData("abcdefgh")]       // Letras
    [InlineData("555@123456")]     // Símbolos inválidos
    [InlineData("++123456789")]    // Doble +
    public async Task Validate_ConTelefonoFormatoInvalido_DeberiaRetornarError(string telefonoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.Telefono) &&
            e.ErrorMessage.Contains("El formato del teléfono no es válido"));
    }

    [Theory]
    [InlineData("+521234567890")]   // Con código país
    [InlineData("1234567890")]      // Sin código país
    [InlineData("12345678901234")]  // Hasta 14 dígitos
    [InlineData("1234567")]         // Mínimo válido
    public async Task Validate_ConTelefonoValido_NoDeberiaRetornarErrorDeTelefono(string telefonoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Telefono));
    }

    [Fact]
    public async Task Validate_ConTelefonoNull_NoDeberiaValidarTelefono()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Sin Teléfono" // Otro campo para que pase validación de "al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Telefono));
    }

    #endregion

    #region Validación FechaNacimiento

    [Fact]
    public async Task Validate_ConFechaNacimientoMenorEdad_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaNacimiento = DateTime.Now.AddYears(-17); // Menor de 18 años

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento) &&
            e.ErrorMessage.Contains("El cliente debe ser mayor de 18 años"));
    }

    [Fact]
    public async Task Validate_ConFechaNacimientoMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaNacimiento = DateTime.Now.AddYears(-121); // Más de 120 años

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento) &&
            e.ErrorMessage.Contains("La fecha de nacimiento no puede ser mayor a 120 años"));
    }

    [Theory]
    [InlineData(-19)]   // 19 años - claramente válido
    [InlineData(-25)]   // 25 años
    [InlineData(-50)]   // 50 años
    [InlineData(-120)]  // Exactamente 120 años - límite válido
    public async Task Validate_ConFechaNacimientoValida_NoDeberiaRetornarErrorDeFechaNacimiento(int anosAtras)
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaNacimiento = DateTime.Today.AddYears(anosAtras);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento));
    }

    [Fact]
    public async Task Validate_ConFechaNacimientoNull_NoDeberiaValidarFechaNacimiento()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Sin Fecha" // Otro campo para que pase validación de "al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento));
    }

    #endregion

    #region Validación EstaActivo

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConEstaActivoValido_NoDeberiaRetornarErrorDeEstaActivo(bool estaActivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.EstaActivo = estaActivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.EstaActivo));
    }

    [Fact]
    public async Task Validate_ConEstaActivoNull_NoDeberiaValidarEstaActivo()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Sin Estado" // Otro campo para que pase validación de "al menos un campo"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.EstaActivo));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Completamente Actualizado",
            Email = "cliente.completo@restaurante.cl",
            Telefono = "+521234567890",
            FechaNacimiento = DateTime.Now.AddYears(-30),
            EstaActivo = true
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandParcialValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Solo Nombre Actualizado"
            // Sin otros campos - válido porque tiene al menos uno
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloEmailActualizado_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Email = "solo.email@actualizado.com"
            // Sin otros campos - válido porque tiene al menos uno
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloTelefonoActualizado_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Telefono = "+521987654321"
            // Sin otros campos - válido porque tiene al menos uno
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloFechaNacimientoActualizada_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            FechaNacimiento = DateTime.Now.AddYears(-35)
            // Sin otros campos - válido porque tiene al menos uno
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloEstaActivoActualizado_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            EstaActivo = false
            // Sin otros campos - válido porque tiene al menos uno
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
        var command = new ActualizarClienteCommand
        {
            Id = Guid.Empty, // Error: ID vacío
            Nombre = "", // Error: nombre vacío si se proporciona
            Email = "email-invalido", // Error: formato de email inválido
            Telefono = "123", // Error: teléfono muy corto
            FechaNacimiento = DateTime.Now.AddYears(-15) // Error: menor de edad
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.Telefono));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(2, true)]    // 2 caracteres - mínimo válido
    [InlineData(100, true)]  // 100 caracteres - válido
    [InlineData(200, true)]  // 200 caracteres - máximo válido
    [InlineData(201, false)] // 201 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesNombre_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(ActualizarClienteCommand.Nombre) &&
                (e.ErrorMessage.Contains("El nombre debe tener al menos 2 caracteres") ||
                 e.ErrorMessage.Contains("El nombre no puede exceder 200 caracteres")));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(ActualizarClienteCommand.Nombre) &&
                e.ErrorMessage.Contains("El nombre no puede exceder 200 caracteres"));
        }
    }

    [Theory]
    [InlineData(10, true)]   // 10 caracteres - válido
    [InlineData(320, true)]  // 320 caracteres - máximo válido
    [InlineData(321, false)] // 321 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesEmail_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        var nombreEmail = new string('a', Math.Max(1, longitud - 9)); // Ajustar para @test.com (9 caracteres)
        command.Email = $"{nombreEmail}@test.com";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(ActualizarClienteCommand.Email) &&
                e.ErrorMessage.Contains("El email no puede exceder 320 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(ActualizarClienteCommand.Email) &&
                e.ErrorMessage.Contains("El email no puede exceder 320 caracteres"));
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("María José García López")]
    [InlineData("Juan Carlos Pérez Hernández")]
    [InlineData("Ana Sofía Martínez")]
    [InlineData("José Luis Rodríguez")]
    public async Task Validate_ConNombresRealistasDeClientes_DeberiaSerValido(string nombre)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombre;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
    }

    [Theory]
    [InlineData("cliente@gmail.com")]
    [InlineData("maria.jose@empresa.cl")]
    [InlineData("juan.carlos+personal@correo.org")]
    [InlineData("cliente123@restaurante.net")]
    public async Task Validate_ConEmailsRealistasDeClientes_DeberiaSerValido(string email)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = email;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Email));
    }

    [Theory]
    [InlineData("+56987654321")]      // Celular Chile
    [InlineData("5551234567")]         // Local
    [InlineData("+1234567890")]        // Internacional
    [InlineData("(555) 123-4567")]     // Con formato
    [InlineData("555-123-4567")]       // Con guiones
    public async Task Validate_ConTelefonosRealistasDeClientes_DeberiaSerValido(string telefono)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefono;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Telefono));
    }

    [Theory]
    [InlineData(-19, true)]   // Recién mayor de edad - activo
    [InlineData(-25, true)]   // Adulto joven - activo
    [InlineData(-40, true)]   // Adulto - activo
    [InlineData(-65, false)]  // Adulto mayor - inactivo
    public async Task Validate_ConDiferentesEdadesYEstados_DeberiaSerValido(int anosAtras, bool estaActivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaNacimiento = DateTime.Today.AddYears(anosAtras);
        command.EstaActivo = estaActivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.EstaActivo));
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task Validate_ConCamposOpcionales_SoloValidaLosProporciona()
    {
        // Arrange - Proporcionar algunos campos, omitir otros
        var command = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Solo Nombre",
            Telefono = "+521234567890"
            // Email y FechaNacimiento omitidos intencionalmente
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        
        // Confirmar que no validó campos no proporcionados
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Email));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.FechaNacimiento));
    }

    [Fact]
    public async Task Validate_ConCamposNulosVsVacios_DeberiaDistinguirCorrectamente()
    {
        // Arrange - Null no se valida, string vacío sí
        var command1 = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = null, // Null - no se valida
            Email = "cliente@email.com" // Para pasar validación de "al menos un campo"
        };
        
        var command2 = new ActualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "", // Vacío - se valida y falla
            Email = "cliente@email.com"
        };

        // Act
        var result1 = await _validator.ValidateAsync(command1);
        var result2 = await _validator.ValidateAsync(command2);

        // Assert
        result1.IsValid.Should().BeTrue(); // Null no se valida
        result2.IsValid.Should().BeFalse(); // Vacío sí se valida y falla
        
        result1.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
        result2.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarClienteCommand.Nombre));
    }

    #endregion
} 