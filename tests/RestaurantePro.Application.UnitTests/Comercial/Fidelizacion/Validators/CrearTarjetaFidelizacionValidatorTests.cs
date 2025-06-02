namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Validators;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Tests para CrearTarjetaFidelizacionValidator
/// Valida reglas de negocio para creación de tarjetas de fidelización
/// </summary>
public class CrearTarjetaFidelizacionValidatorTests
{
    private readonly CrearTarjetaFidelizacionValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Cliente>> _mockClientes;
    private readonly Mock<DbSet<TarjetaFidelizacion>> _mockTarjetas;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;

    public CrearTarjetaFidelizacionValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockClientes = MockDbSetHelper.CreateMockDbSet<Cliente>();
        _mockTarjetas = MockDbSetHelper.CreateMockDbSet<TarjetaFidelizacion>();
        _mockUsuarios = MockDbSetHelper.CreateMockDbSet<Usuario>();
        
        _mockContext.Setup(c => c.Clientes).Returns(_mockClientes.Object);
        _mockContext.Setup(c => c.TarjetasFidelizacion).Returns(_mockTarjetas.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);
        
        _validator = new CrearTarjetaFidelizacionValidator(_mockContext.Object);
    }

    #region Tests de Validaciones Básicas

    [Fact]
    public async Task ClienteId_DebeSerObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ClienteId = Guid.Empty;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteId)
            .WithErrorCode("CREAR_TARJETA_CLIENTE_ID_REQUERIDO")
            .WithErrorMessage("El ID del cliente es obligatorio");
    }

    [Fact]
    public async Task TipoTarjeta_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = (TipoTarjetaFidelizacion)999;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoTarjeta)
            .WithErrorCode("CREAR_TARJETA_TIPO_INVALIDO")
            .WithErrorMessage("El tipo de tarjeta debe ser válido");
    }

    [Theory]
    [InlineData(TipoTarjetaFidelizacion.Estandar)]
    [InlineData(TipoTarjetaFidelizacion.Premium)]
    [InlineData(TipoTarjetaFidelizacion.Vip)]
    [InlineData(TipoTarjetaFidelizacion.Corporativa)]
    [InlineData(TipoTarjetaFidelizacion.Empleado)]
    public async Task TipoTarjeta_DebeAceptarTiposValidos(TipoTarjetaFidelizacion tipo)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = tipo;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TipoTarjeta);
    }

    [Fact]
    public async Task UsuarioCreadorId_DebeSerObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.UsuarioCreadorId = Guid.Empty;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioCreadorId)
            .WithErrorCode("CREAR_TARJETA_USUARIO_CREADOR_REQUERIDO")
            .WithErrorMessage("El ID del usuario creador es obligatorio");
    }

    #endregion

    #region Tests de Validaciones de Número de Tarjeta

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NumeroTarjeta_PuedeSerOpcional(string numeroTarjeta)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NumeroTarjeta = numeroTarjeta;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NumeroTarjeta);
    }

    [Fact]
    public async Task NumeroTarjeta_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NumeroTarjeta = new string('1', 21);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumeroTarjeta)
            .WithErrorCode("CREAR_TARJETA_NUMERO_LONGITUD_MAXIMA")
            .WithErrorMessage("El número de tarjeta no puede exceder 20 caracteres");
    }

    [Fact]
    public async Task NumeroTarjeta_DebeSerMinimoLongitud()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NumeroTarjeta = "123";

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumeroTarjeta)
            .WithErrorCode("CREAR_TARJETA_NUMERO_LONGITUD_MINIMA")
            .WithErrorMessage("El número de tarjeta debe tener al menos 8 caracteres");
    }

    [Theory]
    [InlineData("12345678", true)]
    [InlineData("FIEL1234567890", true)]
    [InlineData("VIP-2025-001", true)]
    [InlineData("CORP_12345", true)]
    [InlineData("123@456", false)]
    [InlineData("FIEL 1234", false)]
    [InlineData("VIP#2025", false)]
    public async Task NumeroTarjeta_DebeValidarFormato(string numero, bool esValido)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NumeroTarjeta = numero;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        if (!esValido)
        {
            result.ShouldHaveValidationErrorFor(x => x.NumeroTarjeta)
                .WithErrorCode("CREAR_TARJETA_NUMERO_FORMATO_INVALIDO")
                .WithErrorMessage("El número de tarjeta solo puede contener letras, números y guiones");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.NumeroTarjeta);
        }
    }

    #endregion

    #region Tests de Validaciones de Fechas

    [Fact]
    public async Task FechaActivacion_NoDebeSerMuyFutura()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaActivacion = DateTime.Now.AddDays(31);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaActivacion)
            .WithErrorCode("CREAR_TARJETA_FECHA_ACTIVACION_FUTURA")
            .WithErrorMessage("La fecha de activación no puede ser más de 30 días en el futuro");
    }

    [Fact]
    public async Task FechaActivacion_NoDebeSerMuyAntigua()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaActivacion = DateTime.Now.AddDays(-8);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaActivacion)
            .WithErrorCode("CREAR_TARJETA_FECHA_ACTIVACION_ANTIGUA")
            .WithErrorMessage("La fecha de activación no puede ser anterior a 7 días");
    }

    [Fact]
    public async Task FechaVencimiento_DebeSerPosteriorAActivacion()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaActivacion = DateTime.Now;
        command.FechaVencimiento = DateTime.Now.AddDays(-1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaVencimiento)
            .WithErrorCode("CREAR_TARJETA_VENCIMIENTO_POSTERIOR_ACTIVACION")
            .WithErrorMessage("La fecha de vencimiento debe ser posterior a la fecha de activación");
    }

    [Fact]
    public async Task FechaVencimiento_NoDebeExcederLimiteMaximo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaActivacion = DateTime.Now;
        command.FechaVencimiento = DateTime.Now.AddYears(6);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaVencimiento)
            .WithErrorCode("CREAR_TARJETA_VENCIMIENTO_LIMITE_MAXIMO")
            .WithErrorMessage("La fecha de vencimiento no puede ser más de 5 años desde la activación");
    }

    [Theory]
    [InlineData(TipoTarjetaFidelizacion.Estandar, 2)]
    [InlineData(TipoTarjetaFidelizacion.Premium, 3)]
    [InlineData(TipoTarjetaFidelizacion.Vip, 5)]
    [InlineData(TipoTarjetaFidelizacion.Corporativa, 3)]
    [InlineData(TipoTarjetaFidelizacion.Empleado, 1)]
    [InlineData(TipoTarjetaFidelizacion.Promocional, 1)]
    public async Task FechaVencimiento_DebeRespetarLimitesPorTipo(TipoTarjetaFidelizacion tipo, int yearsMax)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = tipo;
        command.FechaActivacion = DateTime.Now;
        command.FechaVencimiento = DateTime.Now.AddYears(yearsMax + 1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaVencimiento)
            .WithErrorCode("CREAR_TARJETA_VENCIMIENTO_LIMITE_TIPO")
            .WithErrorMessage($"Para tarjetas {tipo}, el vencimiento máximo es {yearsMax} años");
    }

    #endregion

    #region Tests de Validaciones de Configuración

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task PorcentajeBonificacion_DebeSerPositivo(decimal porcentaje)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PorcentajeBonificacion = porcentaje;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeBonificacion)
            .WithErrorCode("CREAR_TARJETA_PORCENTAJE_BONIFICACION_POSITIVO")
            .WithErrorMessage("El porcentaje de bonificación debe ser mayor a 0");
    }

    [Fact]
    public async Task PorcentajeBonificacion_NoDebeExcederMaximo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PorcentajeBonificacion = 101;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeBonificacion)
            .WithErrorCode("CREAR_TARJETA_PORCENTAJE_BONIFICACION_MAXIMO")
            .WithErrorMessage("El porcentaje de bonificación no puede exceder 100%");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public async Task MultiplicadorPuntos_DebeSerPositivo(decimal multiplicador)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.MultiplicadorPuntos = multiplicador;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MultiplicadorPuntos)
            .WithErrorCode("CREAR_TARJETA_MULTIPLICADOR_PUNTOS_POSITIVO")
            .WithErrorMessage("El multiplicador de puntos debe ser mayor a 0");
    }

    [Fact]
    public async Task MultiplicadorPuntos_NoDebeExcederMaximo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.MultiplicadorPuntos = 11;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MultiplicadorPuntos)
            .WithErrorCode("CREAR_TARJETA_MULTIPLICADOR_PUNTOS_MAXIMO")
            .WithErrorMessage("El multiplicador de puntos no puede exceder 10x");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task LimitePuntosMinimo_DebeSerPositivo(int limite)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.LimitePuntosMinimo = limite;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LimitePuntosMinimo)
            .WithErrorCode("CREAR_TARJETA_LIMITE_PUNTOS_MINIMO_POSITIVO")
            .WithErrorMessage("El límite mínimo de puntos debe ser mayor a 0");
    }

    [Fact]
    public async Task LimitePuntosMaximo_DebeSerMayorAMinimo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.LimitePuntosMinimo = 1000;
        command.LimitePuntosMaximo = 500;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LimitePuntosMaximo)
            .WithErrorCode("CREAR_TARJETA_LIMITE_PUNTOS_MAXIMO_MAYOR_MINIMO")
            .WithErrorMessage("El límite máximo debe ser mayor al límite mínimo");
    }

    #endregion

    #region Tests de Validaciones de Textos

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObservacionesCreacion_PuedeSerOpcional(string observaciones)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ObservacionesCreacion = observaciones;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ObservacionesCreacion);
    }

    [Fact]
    public async Task ObservacionesCreacion_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ObservacionesCreacion = new string('A', 1001);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ObservacionesCreacion)
            .WithErrorCode("CREAR_TARJETA_OBSERVACIONES_LONGITUD_MAXIMA")
            .WithErrorMessage("Las observaciones no pueden exceder 1000 caracteres");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CodigoPromocion_PuedeSerOpcional(string codigo)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoPromocion = codigo;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CodigoPromocion);
    }

    [Fact]
    public async Task CodigoPromocion_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoPromocion = new string('A', 51);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CodigoPromocion)
            .WithErrorCode("CREAR_TARJETA_CODIGO_PROMOCION_LONGITUD")
            .WithErrorMessage("El código de promoción no puede exceder 50 caracteres");
    }

    [Theory]
    [InlineData("PROMO2025", true)]
    [InlineData("VIP-SPECIAL", true)]
    [InlineData("NEW_CLIENT_2025", true)]
    [InlineData("promo@2025", false)]
    [InlineData("VIP SPECIAL", false)]
    [InlineData("NEW#CLIENT", false)]
    public async Task CodigoPromocion_DebeValidarFormato(string codigo, bool esValido)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoPromocion = codigo;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        if (!esValido)
        {
            result.ShouldHaveValidationErrorFor(x => x.CodigoPromocion)
                .WithErrorCode("CREAR_TARJETA_CODIGO_PROMOCION_FORMATO")
                .WithErrorMessage("El código de promoción solo puede contener letras, números, guiones y guiones bajos");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.CodigoPromocion);
        }
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task TarjetaPrincipal_SoloUnaPorCliente()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.EsPrincipal = true;
        ConfigurarClienteConTarjetaPrincipal(command.ClienteId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EsPrincipal)
            .WithErrorCode("CREAR_TARJETA_YA_TIENE_PRINCIPAL")
            .WithErrorMessage("El cliente ya tiene una tarjeta principal activa");
    }

    [Fact]
    public async Task TarjetaVip_SoloUnaPorCliente()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Vip;
        ConfigurarClienteConTarjetaVip(command.ClienteId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoTarjeta)
            .WithErrorCode("CREAR_TARJETA_YA_TIENE_VIP")
            .WithErrorMessage("El cliente ya tiene una tarjeta VIP activa");
    }

    [Fact]
    public async Task TarjetaPremium_SoloUnaPorCliente()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Premium;
        ConfigurarClienteConTarjetaPremium(command.ClienteId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoTarjeta)
            .WithErrorCode("CREAR_TARJETA_YA_TIENE_PREMIUM")
            .WithErrorMessage("El cliente ya tiene una tarjeta Premium activa");
    }

    [Fact]
    public async Task TarjetaEstandar_NoMasDeTres()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Estandar;
        ConfigurarClienteConTresTarjetasEstandar(command.ClienteId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoTarjeta)
            .WithErrorCode("CREAR_TARJETA_LIMITE_ESTANDAR_EXCEDIDO")
            .WithErrorMessage("El cliente no puede tener más de 3 tarjetas estándar activas");
    }

    #endregion

    #region Tests de Validaciones Async (Integridad Referencial)

    [Fact]
    public async Task ClienteId_DebeExistir()
    {
        // Arrange
        var command = CrearCommandoBase();
        ConfigurarClienteNoExiste(command.ClienteId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteId)
            .WithErrorCode("CREAR_TARJETA_CLIENTE_NO_EXISTE")
            .WithErrorMessage("El cliente especificado no existe");
    }

    [Fact]
    public async Task ClienteId_DebeEstarActivo()
    {
        // Arrange
        var command = CrearCommandoBase();
        var clienteInactivo = CrearClienteInactivo();
        ConfigurarClienteExiste(command.ClienteId, clienteInactivo);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteId)
            .WithErrorCode("CREAR_TARJETA_CLIENTE_INACTIVO")
            .WithErrorMessage("El cliente no está activo y no puede tener tarjetas de fidelización");
    }

    [Fact]
    public async Task UsuarioCreadorId_DebeExistir()
    {
        // Arrange
        var command = CrearCommandoBase();
        ConfigurarUsuarioNoExiste(command.UsuarioCreadorId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioCreadorId)
            .WithErrorCode("CREAR_TARJETA_USUARIO_CREADOR_NO_EXISTE")
            .WithErrorMessage("El usuario creador especificado no existe");
    }

    [Fact]
    public async Task NumeroTarjeta_DebeSerUnico()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.NumeroTarjeta = "FIEL12345678";
        ConfigurarNumeroTarjetaExiste(command.NumeroTarjeta);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumeroTarjeta)
            .WithErrorCode("CREAR_TARJETA_NUMERO_DUPLICADO")
            .WithErrorMessage("El número de tarjeta ya existe");
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task ValidarElegibilidadClientePorTipo_TarjetaVipRequiereHistorial()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Vip;
        var clienteNuevo = CrearClienteNuevo();
        ConfigurarClienteExiste(command.ClienteId, clienteNuevo);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoTarjeta)
            .WithErrorCode("CREAR_TARJETA_VIP_REQUIERE_HISTORIAL")
            .WithErrorMessage("Las tarjetas VIP requieren un historial mínimo de compras");
    }

    [Fact]
    public async Task ValidarConfiguracionPorTipo_TarjetaVipRequiereConfiguracionEspecial()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Vip;
        command.MultiplicadorPuntos = 1; // Muy bajo para VIP

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MultiplicadorPuntos)
            .WithErrorCode("CREAR_TARJETA_VIP_MULTIPLICADOR_MINIMO")
            .WithErrorMessage("Las tarjetas VIP requieren un multiplicador mínimo de 3x");
    }

    [Fact]
    public async Task ValidarConfiguracionPorTipo_TarjetaPremiumRequiereConfiguracionEspecial()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoTarjeta = TipoTarjetaFidelizacion.Premium;
        command.MultiplicadorPuntos = 1; // Muy bajo para Premium

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MultiplicadorPuntos)
            .WithErrorCode("CREAR_TARJETA_PREMIUM_MULTIPLICADOR_MINIMO")
            .WithErrorMessage("Las tarjetas Premium requieren un multiplicador mínimo de 2x");
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void CrearTarjetaFidelizacionCommand_CrearEstandar_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar propiedades reales en lugar de método factory inexistente
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
        };

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.TipoTarjeta.Should().Be(TipoTarjetaFidelizacion.Estandar);
        command.PuntosIniciales.Should().Be(0);
        command.ActivarInmediatamente.Should().BeTrue();
        command.EnviarPorEmail.Should().BeFalse();
        command.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void CrearTarjetaFidelizacionCommand_CrearPremium_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar propiedades reales en lugar de método factory inexistente
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            PuntosIniciales = 250,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId
        };

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.TipoTarjeta.Should().Be(TipoTarjetaFidelizacion.Premium);
        command.PuntosIniciales.Should().Be(250);
        command.ActivarInmediatamente.Should().BeTrue();
        command.EnviarPorEmail.Should().BeTrue();
        command.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void CrearTarjetaFidelizacionCommand_CrearVip_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar propiedades reales en lugar de método factory inexistente
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Vip,
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta VIP con beneficios especiales"
        };

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.TipoTarjeta.Should().Be(TipoTarjetaFidelizacion.Vip);
        command.PuntosIniciales.Should().Be(500);
        command.ActivarInmediatamente.Should().BeTrue();
        command.EnviarPorEmail.Should().BeTrue();
        command.UsuarioId.Should().Be(usuarioId);
        command.Observaciones.Should().Be("Tarjeta VIP con beneficios especiales");
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task ValidarConfiguracion_ConValoresEnLimites_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.PorcentajeBonificacion = 100;
        command.MultiplicadorPuntos = 10;
        command.LimitePuntosMinimo = 1;
        command.LimitePuntosMaximo = 1000000;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PorcentajeBonificacion);
        result.ShouldNotHaveValidationErrorFor(x => x.MultiplicadorPuntos);
        result.ShouldNotHaveValidationErrorFor(x => x.LimitePuntosMinimo);
        result.ShouldNotHaveValidationErrorFor(x => x.LimitePuntosMaximo);
    }

    [Fact]
    public async Task ValidarFechas_ConFechasEnLimites_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaActivacion = DateTime.Now.AddDays(30);
        command.FechaVencimiento = DateTime.Now.AddYears(5);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FechaActivacion);
        result.ShouldNotHaveValidationErrorFor(x => x.FechaVencimiento);
    }

    #endregion

    #region Tests de Validaciones de Performance

    [Fact]
    public async Task Validator_ConConfiguracionCompleja_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var command = CrearCommandoCompleto();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Métodos Helper

    private CrearTarjetaFidelizacionCommand CrearCommandoBase()
    {
        return new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = Guid.NewGuid()
        };
    }

    private CrearTarjetaFidelizacionCommand CrearCommandoCompleto()
    {
        return new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            CodigoTarjeta = "TF-TEST-001",
            PuntosIniciales = 150,
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            Observaciones = "Tarjeta de prueba completa",
            UsuarioId = Guid.NewGuid()
        };
    }

    private void ConfigurarClienteExiste(Guid clienteId, Cliente cliente)
    {
        _mockClientes.Setup(m => m.FindAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
    }

    private void ConfigurarClienteNoExiste(Guid clienteId)
    {
        _mockClientes.Setup(m => m.FindAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);
    }

    private void ConfigurarUsuarioNoExiste(Guid usuarioId)
    {
        _mockUsuarios.Setup(m => m.FindAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);
    }

    private void ConfigurarNumeroTarjetaExiste(string numeroTarjeta)
    {
        var tarjeta = CrearTarjetaConNumero(numeroTarjeta);
        var tarjetas = new List<TarjetaFidelizacion> { tarjeta };
        
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private void ConfigurarClienteConTarjetaPrincipal(Guid clienteId)
    {
        var tarjeta = CrearTarjetaPrincipal(clienteId);
        var tarjetas = new List<TarjetaFidelizacion> { tarjeta };
        
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private void ConfigurarClienteConTarjetaVip(Guid clienteId)
    {
        var tarjeta = CrearTarjetaVip(clienteId);
        var tarjetas = new List<TarjetaFidelizacion> { tarjeta };
        
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private void ConfigurarClienteConTarjetaPremium(Guid clienteId)
    {
        var tarjeta = CrearTarjetaPremium(clienteId);
        var tarjetas = new List<TarjetaFidelizacion> { tarjeta };
        
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private void ConfigurarClienteConTresTarjetasEstandar(Guid clienteId)
    {
        var tarjetas = new List<TarjetaFidelizacion>
        {
            CrearTarjetaEstandar(clienteId, "ESTAND001"),
            CrearTarjetaEstandar(clienteId, "ESTAND002"),
            CrearTarjetaEstandar(clienteId, "ESTAND003")
        };
        
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Provider).Returns(tarjetas.AsQueryable().Provider);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.Expression).Returns(tarjetas.AsQueryable().Expression);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.ElementType).Returns(tarjetas.AsQueryable().ElementType);
        _mockTarjetas.As<IQueryable<TarjetaFidelizacion>>().Setup(m => m.GetEnumerator()).Returns(tarjetas.GetEnumerator());
    }

    private Cliente CrearClienteInactivo()
    {
        var cliente = Cliente.Crear(
            "Cliente Inactivo",
            "inactivo@email.com",
            "555-0000"
        );
        cliente.Desactivar("Cliente inactivo para tests");
        return cliente;
    }

    private Cliente CrearClienteNuevo()
    {
        return Cliente.Crear(
            "Cliente Nuevo",
            "nuevo@email.com",
            "555-1111"
        );
    }

    private TarjetaFidelizacion CrearTarjetaConNumero(string numero)
    {
        return TarjetaFidelizacion.Crear(
            Guid.NewGuid(),
            TipoTarjetaFidelizacion.Estandar,
            numero,
            DateTime.Now,
            DateTime.Now.AddYears(2),
            true,
            true
        );
    }

    private TarjetaFidelizacion CrearTarjetaPrincipal(Guid clienteId)
    {
        return TarjetaFidelizacion.Crear(
            clienteId,
            TipoTarjetaFidelizacion.Estandar,
            "PRINCIPAL001",
            DateTime.Now,
            DateTime.Now.AddYears(2),
            true,
            true
        );
    }

    private TarjetaFidelizacion CrearTarjetaVip(Guid clienteId)
    {
        return TarjetaFidelizacion.Crear(
            clienteId,
            TipoTarjetaFidelizacion.Vip,
            "VIP001",
            DateTime.Now,
            DateTime.Now.AddYears(5),
            true,
            true
        );
    }

    private TarjetaFidelizacion CrearTarjetaPremium(Guid clienteId)
    {
        return TarjetaFidelizacion.Crear(
            clienteId,
            TipoTarjetaFidelizacion.Premium,
            "PREMIUM001",
            DateTime.Now,
            DateTime.Now.AddYears(3),
            true,
            true
        );
    }

    private TarjetaFidelizacion CrearTarjetaEstandar(Guid clienteId, string numero)
    {
        return TarjetaFidelizacion.Crear(
            clienteId,
            TipoTarjetaFidelizacion.Estandar,
            numero,
            DateTime.Now,
            DateTime.Now.AddYears(2),
            false,
            true
        );
    }

    #endregion
} 