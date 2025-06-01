namespace RestaurantePro.Application.UnitTests.Comercial.Promociones.Validators;

/// <summary>
/// Tests para AplicarPromocionValidator
/// Valida reglas de negocio para aplicación de promociones comerciales
/// </summary>
public class AplicarPromocionValidatorTests
{
    private readonly AplicarPromocionValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Promocion>> _mockPromociones;
    private readonly Mock<DbSet<Cliente>> _mockClientes;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;

    public AplicarPromocionValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPromociones = MockDbSetHelper.CreateMockDbSet<Promocion>();
        _mockClientes = MockDbSetHelper.CreateMockDbSet<Cliente>();
        _mockUsuarios = MockDbSetHelper.CreateMockDbSet<Usuario>();
        
        _mockContext.Setup(c => c.Promociones).Returns(_mockPromociones.Object);
        _mockContext.Setup(c => c.Clientes).Returns(_mockClientes.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);
        
        _validator = new AplicarPromocionValidator(_mockContext.Object);
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
            .WithErrorCode("APLICAR_PROMOCION_CLIENTE_ID_REQUERIDO")
            .WithErrorMessage("El ID del cliente es obligatorio");
    }

    [Fact]
    public async Task CodigoPromocion_DebeSerObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CodigoPromocion = string.Empty;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CodigoPromocion)
            .WithErrorCode("APLICAR_PROMOCION_CODIGO_REQUERIDO")
            .WithErrorMessage("El código de promoción es obligatorio");
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
            .WithErrorCode("APLICAR_PROMOCION_CODIGO_LONGITUD_MAXIMA")
            .WithErrorMessage("El código de promoción no puede exceder 50 caracteres");
    }

    [Theory]
    [InlineData("DESC123", true)]
    [InlineData("PROMO-2025", true)]
    [InlineData("VIP_SPECIAL", true)]
    [InlineData("2x1COMBO", true)]
    [InlineData("desc@123", false)]
    [InlineData("PROMO 2025", false)]
    [InlineData("VIP#SPECIAL", false)]
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
                .WithErrorCode("APLICAR_PROMOCION_CODIGO_FORMATO_INVALIDO")
                .WithErrorMessage("El código solo puede contener letras, números, guiones y guiones bajos");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.CodigoPromocion);
        }
    }

    [Fact]
    public async Task TipoPromocion_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "TipoInvalido";

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoPromocion)
            .WithErrorCode("APLICAR_PROMOCION_TIPO_INVALIDO")
            .WithErrorMessage("El tipo de promoción debe ser: Descuento, DosXUno, TresXDos, PuntosDoboles, CanjePuntos, ComboEspecial");
    }

    [Theory]
    [InlineData("Descuento")]
    [InlineData("DosXUno")]
    [InlineData("TresXDos")]
    [InlineData("PuntosDobles")]
    [InlineData("CanjePuntos")]
    [InlineData("ComboEspecial")]
    public async Task TipoPromocion_DebeAceptarTiposValidos(string tipo)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = tipo;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TipoPromocion);
    }

    [Fact]
    public async Task UsuarioId_DebeSerObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId)
            .WithErrorCode("APLICAR_PROMOCION_USUARIO_ID_REQUERIDO")
            .WithErrorMessage("El ID del usuario es obligatorio");
    }

    #endregion

    #region Tests de Validaciones de Montos

    [Fact]
    public async Task MontoMinimo_DebeSerPositivo()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.MontoMinimo = -100;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MontoMinimo)
            .WithErrorCode("APLICAR_PROMOCION_MONTO_MINIMO_POSITIVO")
            .WithErrorMessage("El monto mínimo debe ser mayor a 0");
    }

    [Fact]
    public async Task MontoMinimo_NoDebeExcederLimite()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.MontoMinimo = 100001;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MontoMinimo)
            .WithErrorCode("APLICAR_PROMOCION_MONTO_MINIMO_LIMITE")
            .WithErrorMessage("El monto mínimo no puede exceder $100,000");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task ValorDescuento_DebeSerPositivo(decimal valor)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "Descuento";
        command.ValorDescuento = valor;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorDescuento)
            .WithErrorCode("APLICAR_PROMOCION_VALOR_DESCUENTO_POSITIVO")
            .WithErrorMessage("El valor del descuento debe ser mayor a 0");
    }

    [Fact]
    public async Task ValorDescuento_PorcentajeNoDebeExceder100()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "Descuento";
        command.EsPorcentaje = true;
        command.ValorDescuento = 150;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorDescuento)
            .WithErrorCode("APLICAR_PROMOCION_PORCENTAJE_LIMITE")
            .WithErrorMessage("El porcentaje no puede ser mayor a 100%");
    }

    [Fact]
    public async Task ValorDescuento_MontoFijoNoDebeExcederLimite()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "Descuento";
        command.EsPorcentaje = false;
        command.ValorDescuento = 50001;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorDescuento)
            .WithErrorCode("APLICAR_PROMOCION_MONTO_FIJO_LIMITE")
            .WithErrorMessage("El monto fijo no puede exceder $50,000");
    }

    #endregion

    #region Tests de Validaciones de Fechas

    [Fact]
    public async Task FechaAplicacion_NoDebeSerFutura()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaAplicacion = DateTime.Now.AddDays(1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaAplicacion)
            .WithErrorCode("APLICAR_PROMOCION_FECHA_FUTURA")
            .WithErrorMessage("La fecha de aplicación no puede ser futura");
    }

    [Fact]
    public async Task FechaAplicacion_NoDebeSerMuyAntigua()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.FechaAplicacion = DateTime.Now.AddDays(-31);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FechaAplicacion)
            .WithErrorCode("APLICAR_PROMOCION_FECHA_ANTIGUA")
            .WithErrorMessage("La fecha de aplicación no puede ser anterior a 30 días");
    }

    #endregion

    #region Tests de Validaciones de Productos

    [Fact]
    public async Task ProductosEspecificos_NoDebeExcederLimite()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ProductosEspecificos = CrearListaProductos(21);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductosEspecificos)
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_LIMITE")
            .WithErrorMessage("No se pueden especificar más de 20 productos");
    }

    [Fact]
    public async Task ProductosEspecificos_DebenTenerIdsValidos()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ProductosEspecificos = new List<Guid> { Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductosEspecificos)
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_IDS_INVALIDOS")
            .WithErrorMessage("Todos los IDs de productos deben ser válidos");
    }

    [Theory]
    [InlineData("Bebidas")]
    [InlineData("Comidas")]
    [InlineData("Postres")]
    [InlineData("Entradas")]
    [InlineData("Especialidades")]
    public async Task CategoriasEspecificas_DebeValidarCategoriasValidas(string categoria)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CategoriasEspecificas = new List<string> { categoria };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoriasEspecificas);
    }

    [Fact]
    public async Task CategoriasEspecificas_DebeRechazarCategoriasInvalidas()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CategoriasEspecificas = new List<string> { "CategoriaInvalida" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoriasEspecificas)
            .WithErrorCode("APLICAR_PROMOCION_CATEGORIAS_INVALIDAS")
            .WithErrorMessage("Las categorías deben ser válidas: Bebidas, Comidas, Postres, Entradas, Especialidades");
    }

    #endregion

    #region Tests de Validaciones de Textos

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObservacionesAplicacion_PuedeSerOpcional(string observaciones)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ObservacionesAplicacion = observaciones;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ObservacionesAplicacion);
    }

    [Fact]
    public async Task ObservacionesAplicacion_NoDebeExcederLongitudMaxima()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ObservacionesAplicacion = new string('A', 1001);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ObservacionesAplicacion)
            .WithErrorCode("APLICAR_PROMOCION_OBSERVACIONES_LONGITUD")
            .WithErrorMessage("Las observaciones no pueden exceder 1000 caracteres");
    }

    [Fact]
    public async Task CanalAplicacion_DebeSerValido()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CanalAplicacion = "CanalInvalido";

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CanalAplicacion)
            .WithErrorCode("APLICAR_PROMOCION_CANAL_INVALIDO")
            .WithErrorMessage("Canal no válido. Valores permitidos: Presencial, App, Web, Telefono, WhatsApp");
    }

    [Theory]
    [InlineData("Presencial")]
    [InlineData("App")]
    [InlineData("Web")]
    [InlineData("Telefono")]
    [InlineData("WhatsApp")]
    public async Task CanalAplicacion_DebeAceptarCanalesValidos(string canal)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.CanalAplicacion = canal;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CanalAplicacion);
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task TipoDescuento_ValorDescuentoEsObligatorio()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "Descuento";
        command.ValorDescuento = null;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorDescuento)
            .WithErrorCode("APLICAR_PROMOCION_DESCUENTO_VALOR_REQUERIDO")
            .WithErrorMessage("Para promociones de descuento debe especificar el valor");
    }

    [Fact]
    public async Task TipoCanje_PuntosObligatorios()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "CanjePuntos";
        command.PuntosRequeridos = null;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PuntosRequeridos)
            .WithErrorCode("APLICAR_PROMOCION_CANJE_PUNTOS_REQUERIDOS")
            .WithErrorMessage("Para promociones de canje debe especificar los puntos requeridos");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task PuntosRequeridos_DebeSerPositivo(int puntos)
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "CanjePuntos";
        command.PuntosRequeridos = puntos;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PuntosRequeridos)
            .WithErrorCode("APLICAR_PROMOCION_PUNTOS_POSITIVOS")
            .WithErrorMessage("Los puntos requeridos deben ser mayor a 0");
    }

    [Fact]
    public async Task PuntosRequeridos_NoDebeExcederLimite()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.TipoPromocion = "CanjePuntos";
        command.PuntosRequeridos = 100001;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PuntosRequeridos)
            .WithErrorCode("APLICAR_PROMOCION_PUNTOS_LIMITE")
            .WithErrorMessage("Los puntos requeridos no pueden exceder 100,000");
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
            .WithErrorCode("APLICAR_PROMOCION_CLIENTE_NO_EXISTE")
            .WithErrorMessage("El cliente especificado no existe");
    }

    [Fact]
    public async Task UsuarioId_DebeExistir()
    {
        // Arrange
        var command = CrearCommandoBase();
        ConfigurarUsuarioNoExiste(command.UsuarioId);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId)
            .WithErrorCode("APLICAR_PROMOCION_USUARIO_NO_EXISTE")
            .WithErrorMessage("El usuario especificado no existe");
    }

    [Fact]
    public async Task CodigoPromocion_DebeExistir()
    {
        // Arrange
        var command = CrearCommandoBase();
        ConfigurarPromocionNoExiste(command.CodigoPromocion);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CodigoPromocion)
            .WithErrorCode("APLICAR_PROMOCION_CODIGO_NO_EXISTE")
            .WithErrorMessage("El código de promoción no existe o ha expirado");
    }

    [Fact]
    public async Task ClienteElegible_DebeValidarElegibilidad()
    {
        // Arrange
        var command = CrearCommandoBase();
        var cliente = CrearClienteInactivo();
        ConfigurarClienteExiste(command.ClienteId, cliente);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClienteId)
            .WithErrorCode("APLICAR_PROMOCION_CLIENTE_NO_ELEGIBLE")
            .WithErrorMessage("El cliente no es elegible para esta promoción");
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task ValidarLimitesPromocion_NoDebeExcederLimites()
    {
        // Arrange
        var command = CrearCommandoBase();
        var promocion = CrearPromocionConLimites(usosMaximos: 1, usosActuales: 1);
        ConfigurarPromocionExiste(command.CodigoPromocion, promocion);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("APLICAR_PROMOCION_LIMITE_EXCEDIDO")
            .WithErrorMessage("La promoción ha alcanzado su límite de usos");
    }

    [Fact]
    public async Task ValidarFechasPromocion_DebeEstarVigente()
    {
        // Arrange
        var command = CrearCommandoBase();
        var promocion = CrearPromocionExpirada();
        ConfigurarPromocionExiste(command.CodigoPromocion, promocion);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("APLICAR_PROMOCION_EXPIRADA")
            .WithErrorMessage("La promoción ha expirado");
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void AplicarPromocionCommand_CrearDescuento_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var codigo = "DESC20";
        var valor = 20m;
        var usuarioId = Guid.NewGuid();

        // Act
        var command = AplicarPromocionCommand.CrearDescuento(clienteId, codigo, valor, true, usuarioId);

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.CodigoPromocion.Should().Be(codigo);
        command.TipoPromocion.Should().Be("Descuento");
        command.ValorDescuento.Should().Be(valor);
        command.EsPorcentaje.Should().BeTrue();
        command.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void AplicarPromocionCommand_CrearCanjePuntos_DebeConfigurarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var codigo = "CANJE500";
        var puntos = 500;
        var usuarioId = Guid.NewGuid();

        // Act
        var command = AplicarPromocionCommand.CrearCanjePuntos(clienteId, codigo, puntos, usuarioId);

        // Assert
        command.ClienteId.Should().Be(clienteId);
        command.CodigoPromocion.Should().Be(codigo);
        command.TipoPromocion.Should().Be("CanjePuntos");
        command.PuntosRequeridos.Should().Be(puntos);
        command.UsuarioId.Should().Be(usuarioId);
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task ValidarPromocion_ConProductosYCategorias_DebeValidarCoherencia()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ProductosEspecificos = CrearListaProductos(5);
        command.CategoriasEspecificas = new List<string> { "Bebidas", "Comidas" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode("APLICAR_PROMOCION_PRODUCTOS_CATEGORIAS_EXCLUSIVOS")
            .WithErrorMessage("No se pueden especificar productos específicos y categorías al mismo tiempo");
    }

    [Fact]
    public async Task ValidarMontoMinimo_ConMontoInsuficiente_DebeRechazar()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.MontoMinimo = 1000;
        command.MontoTransaccion = 500;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MontoTransaccion)
            .WithErrorCode("APLICAR_PROMOCION_MONTO_INSUFICIENTE")
            .WithErrorMessage("El monto de la transacción no alcanza el mínimo requerido");
    }

    #endregion

    #region Tests de Validaciones de Performance

    [Fact]
    public async Task Validator_ConDatosMasivos_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var command = CrearCommandoBase();
        command.ProductosEspecificos = CrearListaProductos(20);
        command.CategoriasEspecificas = new List<string> { "Bebidas", "Comidas", "Postres" };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Métodos Helper

    private AplicarPromocionCommand CrearCommandoBase()
    {
        return new AplicarPromocionCommand
        {
            ClienteId = Guid.NewGuid(),
            CodigoPromocion = "PROMO2025",
            TipoPromocion = "Descuento",
            ValorDescuento = 15m,
            EsPorcentaje = true,
            MontoMinimo = 100,
            MontoTransaccion = 200,
            FechaAplicacion = DateTime.Now,
            CanalAplicacion = "Presencial",
            UsuarioId = Guid.NewGuid(),
            ObservacionesAplicacion = "Promoción aplicada correctamente"
        };
    }

    private List<Guid> CrearListaProductos(int cantidad)
    {
        return Enumerable.Range(1, cantidad).Select(_ => Guid.NewGuid()).ToList();
    }

    private void ConfigurarClienteExiste(Guid clienteId, Cliente cliente)
    {
        var clientes = new List<Cliente> { cliente };
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

    private void ConfigurarPromocionExiste(string codigo, Promocion promocion)
    {
        var promociones = new List<Promocion> { promocion };
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.Provider).Returns(promociones.AsQueryable().Provider);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.Expression).Returns(promociones.AsQueryable().Expression);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.ElementType).Returns(promociones.AsQueryable().ElementType);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.GetEnumerator()).Returns(promociones.GetEnumerator());
    }

    private void ConfigurarPromocionNoExiste(string codigo)
    {
        var promociones = new List<Promocion>();
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.Provider).Returns(promociones.AsQueryable().Provider);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.Expression).Returns(promociones.AsQueryable().Expression);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.ElementType).Returns(promociones.AsQueryable().ElementType);
        _mockPromociones.As<IQueryable<Promocion>>().Setup(m => m.GetEnumerator()).Returns(promociones.GetEnumerator());
    }

    private Cliente CrearClienteInactivo()
    {
        return Cliente.Crear(
            "Cliente Test",
            "test@email.com",
            "555-1234"
        );
        // Asumir que el cliente se crea activo, luego se desactiva
    }

    private Promocion CrearPromocionConLimites(int usosMaximos, int usosActuales)
    {
        return Promocion.Crear(
            "PROMO2025",
            "Promoción Test",
            TipoPromocion.PorcentajeTotal,
            15m,
            DateTime.Now.AddDays(-10),
            DateTime.Now.AddDays(10),
            100m,
            usosMaximos,
            0
        );
        // Simular usos actuales
    }

    private Promocion CrearPromocionExpirada()
    {
        return Promocion.Crear(
            "EXPIRED",
            "Promoción Expirada",
            TipoPromocion.PorcentajeTotal,
            10m,
            DateTime.Now.AddDays(-30),
            DateTime.Now.AddDays(-1),
            50m,
            100,
            0
        );
    }

    #endregion
} 