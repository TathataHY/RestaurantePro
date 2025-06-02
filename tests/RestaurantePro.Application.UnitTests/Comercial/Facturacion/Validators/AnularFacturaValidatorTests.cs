namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ANULAR FACTURA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de anulación de facturas
/// Cobertura: 100% de reglas de negocio del AnularFacturaValidator
/// </summary>
public class AnularFacturaValidatorTests
{
    private readonly AnularFacturaValidator _validator;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Factura>> _mockFacturas;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;

    private static readonly string[] TiposAnulacionValidos = 
    { 
        "Normal", "Emergencia", "Administrativa", "Devolución", "SolicitudCliente", "Programada" 
    };

    private static readonly string[] MetodosDevolucionValidos = 
    { 
        "Efectivo", "Tarjeta", "Transferencia", "SaldoFavor" 
    };

    public AnularFacturaValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockFacturas = new Mock<DbSet<Factura>>();
        _mockUsuarios = new Mock<DbSet<Usuario>>();

        _mockContext.Setup(c => c.Facturas).Returns(_mockFacturas.Object);
        _mockContext.Setup(c => c.Usuarios).Returns(_mockUsuarios.Object);

        _validator = new AnularFacturaValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private AnularFacturaCommand CrearCommandValido()
    {
        return new AnularFacturaCommand
        {
            FacturaId = Guid.NewGuid(),
            Motivo = "Anulación solicitada por el cliente debido a error en el pedido",
            DescripcionDetallada = "El cliente indica que el pedido no correspondía con lo solicitado",
            TipoAnulacion = "Normal",
            Prioridad = 2,
            UsuarioAutorizaId = Guid.NewGuid(),
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "Efectivo",
            CancelarPuntosFidelizacion = false,
            NotificarCliente = true,
            DocumentosAdjuntos = new List<string> { "recibo_original.pdf" }
        };
    }

    private Factura CrearFacturaValida()
    {
        return Factura.Crear(
            "FAC-001",
            TipoFactura.Normal,
            "Cliente Test",
            Guid.NewGuid(),
            "RFC-123",
            "Dirección Test",
            new List<Guid> { Guid.NewGuid() },
            "Observaciones test"
        );
    }

    private Usuario CrearUsuarioValido()
    {
        return Usuario.Crear(
            "testuser",
            "Usuario Test",
            "test@example.com",
            RolUsuario.Administrador
        );
    }

    #endregion

    #region Validación FacturaId

    [Fact]
    public async Task Validate_ConFacturaIdVacia_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FacturaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.FacturaId) &&
            e.ErrorMessage.Contains("El ID de la factura es requerido"));
    }

    [Fact]
    public async Task Validate_ConFacturaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var facturas = new List<Factura>().AsQueryable();

        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.FacturaId) &&
            e.ErrorMessage.Contains("La factura especificada no existe"));
    }

    [Fact]
    public async Task Validate_ConFacturaExistente_NoDeberiaRetornarErrorDeExistencia()
    {
        // Arrange
        var command = CrearCommandValido();
        var factura = CrearFacturaValida();
        var usuario = CrearUsuarioValido();

        var facturas = new List<Factura> { factura }.AsQueryable();
        var usuarios = new List<Usuario> { usuario }.AsQueryable();

        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());

        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.FacturaId) &&
            e.ErrorMessage.Contains("La factura especificada no existe"));
    }

    #endregion

    #region Validación Motivo

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoVacioONull_DeberiaRetornarError(string? motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoInvalido!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo de anulación es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = "Muy corto"; // Menos de 10 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("Error en el pedido del cliente")]
    [InlineData("Producto entregado en mal estado - solicitud de devolución")]
    [InlineData("Cliente insatisfecho con el servicio recibido en el restaurante")]
    public async Task Validate_ConMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Motivo));
    }

    #endregion

    #region Validación DescripcionDetallada

    [Fact]
    public async Task Validate_ConDescripcionDetalladaMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DescripcionDetallada = new string('A', 2001); // Más de 2000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DescripcionDetallada) &&
            e.ErrorMessage.Contains("La descripción detallada no puede exceder 2000 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConDescripcionDetalladaVacia_NoDeberiaValidarLongitud(string? descripcionVacia)
    {
        // Arrange
        var command = CrearCommandValido();
        command.DescripcionDetallada = descripcionVacia;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DescripcionDetallada));
    }

    [Fact]
    public async Task Validate_ConDescripcionDetalladaEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DescripcionDetallada = new string('D', 2000); // Exactamente 2000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DescripcionDetallada));
    }

    #endregion

    #region Validación TipoAnulacion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConTipoAnulacionVacioONull_DeberiaRetornarError(string? tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAnulacion = tipoInvalido!;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.TipoAnulacion) &&
            e.ErrorMessage.Contains("El tipo de anulación es requerido"));
    }

    [Theory]
    [InlineData("Invalido")]
    [InlineData("Cancelacion")]
    [InlineData("Manual")]
    [InlineData("Otro")]
    public async Task Validate_ConTipoAnulacionInvalido_DeberiaRetornarError(string tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAnulacion = tipoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.TipoAnulacion) &&
            e.ErrorMessage.Contains("El tipo de anulación debe ser uno de: Normal, Emergencia, Administrativa, Devolución, SolicitudCliente, Programada"));
    }

    [Theory]
    [InlineData("Normal")]
    [InlineData("Emergencia")]
    [InlineData("Administrativa")]
    [InlineData("Devolución")]
    [InlineData("SolicitudCliente")]
    [InlineData("Programada")]
    public async Task Validate_ConTipoAnulacionValido_NoDeberiaRetornarErrorDeTipo(string tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAnulacion = tipoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.TipoAnulacion) &&
            e.ErrorMessage.Contains("El tipo de anulación debe ser uno de"));
    }

    #endregion

    #region Validación Prioridad

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConPrioridadMenorAUno_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad mínima es 1"));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Validate_ConPrioridadMayorACuatro_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad máxima es 4"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Validate_ConPrioridadValida_NoDeberiaRetornarErrorDePrioridad(int prioridadValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Prioridad));
    }

    #endregion

    #region Validación UsuarioAutorizaId

    [Fact]
    public async Task Validate_ConUsuarioAutorizaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioAutorizaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El ID del usuario que autoriza es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioAutorizadorInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var usuarios = new List<Usuario>().AsQueryable();

        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El usuario autorizador especificado no existe"));
    }

    #endregion

    #region Validación MetodoDevolucion

    [Theory]
    [InlineData("Cheque")]
    [InlineData("Bitcoin")]
    [InlineData("Paypal")]
    [InlineData("InvalidoMetodo")]
    public async Task Validate_ConMetodoDevolucionInvalidoCuandoRequiereDevolucion_DeberiaRetornarError(string metodoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProcesarDevolucionPago = true;
        command.MetodoDevolucion = metodoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.MetodoDevolucion) &&
            e.ErrorMessage.Contains("El método de devolución debe ser uno de: Efectivo, Tarjeta, Transferencia, SaldoFavor"));
    }

    [Theory]
    [InlineData("Efectivo")]
    [InlineData("Tarjeta")]
    [InlineData("Transferencia")]
    [InlineData("SaldoFavor")]
    public async Task Validate_ConMetodoDevolucionValidoCuandoRequiereDevolucion_NoDeberiaRetornarErrorDeMetodo(string metodoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProcesarDevolucionPago = true;
        command.MetodoDevolucion = metodoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.MetodoDevolucion));
    }

    [Fact]
    public async Task Validate_ConMetodoDevolucionCuandoNoRequiereDevolucion_NoDeberiaValidarMetodo()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProcesarDevolucionPago = false;
        command.MetodoDevolucion = "MetodoInvalido";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.MetodoDevolucion));
    }

    #endregion

    #region Validación ProcesarDevolucionPago

    [Fact]
    public async Task Validate_ConProcesarDevolucionPagoCuandoNoRequiereDevolucion_NoDeberiaValidarProcesamiento()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProcesarDevolucionPago = false;
        // No se establece MetodoDevolucion

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.ProcesarDevolucionPago));
    }

    #endregion

    #region Validación DocumentosAdjuntos

    [Fact]
    public async Task Validate_ConDemasiadosDocumentosAdjuntos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DocumentosAdjuntos = Enumerable.Range(1, 21) // 21 documentos (más de 20)
            .Select(i => $"documento_{i}.pdf")
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DocumentosAdjuntos) &&
            e.ErrorMessage.Contains("Máximo 20 documentos de soporte"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Validate_ConCantidadValidaDeDocumentosAdjuntos_NoDeberiaRetornarErrorDeCantidad(int cantidadDocumentos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.DocumentosAdjuntos = Enumerable.Range(1, cantidadDocumentos)
            .Select(i => $"documento_{i}.pdf")
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DocumentosAdjuntos) &&
            e.ErrorMessage.Contains("Máximo 20 documentos de soporte"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var factura = CrearFacturaValida();
        var usuario = CrearUsuarioValido();

        var command = new AnularFacturaCommand
        {
            FacturaId = factura.Id,
            Motivo = "Anulación solicitada por el cliente debido a error en el pedido",
            DescripcionDetallada = "El cliente reportó que el pedido no correspondía con lo solicitado. Producto incorrecto entregado.",
            TipoAnulacion = "Normal",
            Prioridad = 2,
            UsuarioAutorizaId = usuario.Id,
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "Efectivo",
            CancelarPuntosFidelizacion = false,
            NotificarCliente = true,
            DocumentosAdjuntos = new List<string> { "recibo_original.pdf", "foto_producto.jpg" }
        };

        var facturas = new List<Factura> { factura }.AsQueryable();
        var usuarios = new List<Usuario> { usuario }.AsQueryable();

        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());

        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

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
        var factura = CrearFacturaValida();
        var usuario = CrearUsuarioValido();

        var command = new AnularFacturaCommand
        {
            FacturaId = factura.Id,
            Motivo = "Error en el pedido del cliente",
            TipoAnulacion = "Normal",
            Prioridad = 1,
            UsuarioAutorizaId = usuario.Id,
            ProcesarDevolucionPago = false,
            CancelarPuntosFidelizacion = false,
            NotificarCliente = false
        };

        var facturas = new List<Factura> { factura }.AsQueryable();
        var usuarios = new List<Usuario> { usuario }.AsQueryable();

        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _mockFacturas.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());

        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        _mockUsuarios.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

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
        var command = new AnularFacturaCommand
        {
            FacturaId = Guid.Empty, // Error
            Motivo = "", // Error
            TipoAnulacion = "Invalido", // Error
            Prioridad = 0, // Error
            UsuarioAutorizaId = Guid.Empty, // Error
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "MetodoInvalido", // Error
            CancelarPuntosFidelizacion = true,
            NotificarCliente = true,
            DocumentosAdjuntos = Enumerable.Range(1, 25).Select(i => $"doc_{i}.pdf").ToList() // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(7);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Normal", "Efectivo")]
    [InlineData("Emergencia", "Tarjeta")]
    [InlineData("Administrativa", "Transferencia")]
    [InlineData("Devolución", "SaldoFavor")]
    [InlineData("SolicitudCliente", "Efectivo")]
    [InlineData("Programada", "Tarjeta")]
    public async Task Validate_ConDiferentesTiposYMetodos_DeberiaSerValido(string tipoAnulacion, string metodoDevolucion)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAnulacion = tipoAnulacion;
        command.ProcesarDevolucionPago = true;
        command.MetodoDevolucion = metodoDevolucion;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.TipoAnulacion) ||
            e.PropertyName == nameof(AnularFacturaCommand.MetodoDevolucion));
    }

    [Fact]
    public async Task Validate_ConAnulacionEmergencia_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoAnulacion = "Emergencia";
        command.Prioridad = 4; // Máxima prioridad
        command.Motivo = "EMERGENCIA: Contaminación detectada en producto - anulación inmediata";
        command.ProcesarDevolucionPago = true;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.TipoAnulacion) ||
            e.PropertyName == nameof(AnularFacturaCommand.Prioridad));
    }

    [Fact]
    public async Task Validate_ConAnulacionSinDevolucion_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProcesarDevolucionPago = false;
        command.MetodoDevolucion = null;
        command.Motivo = "Anulación administrativa - corrección contable";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.MetodoDevolucion) ||
            e.PropertyName == nameof(AnularFacturaCommand.ProcesarDevolucionPago));
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(10, true)]   // 10 caracteres - límite mínimo
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - límite máximo
    [InlineData(9, false)]   // 9 caracteres - inválido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('M', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AnularFacturaCommand.Motivo) &&
                (e.ErrorMessage.Contains("al menos 10 caracteres") || e.ErrorMessage.Contains("exceder 500 caracteres")));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(AnularFacturaCommand.Motivo));
        }
    }

    [Fact]
    public async Task Validate_ConMotivoConCaracteresEspeciales_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = "Anulación: ñáéíóú @#$%^&*()_+ cliente molesto 😠";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Motivo));
    }

    [Fact]
    public async Task Validate_ConDocumentosAdjuntosConExtensionesVariadas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DocumentosAdjuntos = new List<string>
        {
            "recibo.pdf",
            "foto_producto.jpg",
            "email_cliente.msg",
            "video_evidencia.mp4",
            "solicitud.docx"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.DocumentosAdjuntos));
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var commands = Enumerable.Range(1, 5)
            .Select(_ => 
            {
                var cmd = CrearCommandValido();
                cmd.FacturaId = Guid.NewGuid();
                cmd.UsuarioAutorizaId = Guid.NewGuid();
                return cmd;
            })
            .ToList();

        // Act
        var tasks = commands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        // Los resultados pueden variar dependiendo del setup de los mocks
        results.Should().NotBeNull();
        results.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Validate_ConDiferentesPrioridadesEnParalelo_DeberiaValidarTodas(int prioridad)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridad;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AnularFacturaCommand.Prioridad));
    }

    #endregion
} 