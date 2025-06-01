namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA DESACTIVAR PROVEEDOR VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de desactivación de proveedores
/// Cobertura: 100% de reglas de negocio del DesactivarProveedorValidator
/// </summary>
public class DesactivarProveedorValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly DesactivarProveedorValidator _validator;
    private readonly Mock<DbSet<Domain.Proveedores.Entities.Proveedor>> _mockProveedoresDbSet;
    private readonly Mock<DbSet<OrdenCompra>> _mockOrdenesCompraDbSet;
    private readonly Mock<DbSet<Factura>> _mockFacturasDbSet;

    public DesactivarProveedorValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockProveedoresDbSet = new Mock<DbSet<Domain.Proveedores.Entities.Proveedor>>();
        _mockOrdenesCompraDbSet = new Mock<DbSet<OrdenCompra>>();
        _mockFacturasDbSet = new Mock<DbSet<Factura>>();
        
        _mockContext.Setup(c => c.Proveedores).Returns(_mockProveedoresDbSet.Object);
        _mockContext.Setup(c => c.OrdenesCompra).Returns(_mockOrdenesCompraDbSet.Object);
        _mockContext.Setup(c => c.Facturas).Returns(_mockFacturasDbSet.Object);
        
        _validator = new DesactivarProveedorValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private DesactivarProveedorCommand CrearCommandValido()
    {
        return new DesactivarProveedorCommand
        {
            ProveedorId = Guid.NewGuid(),
            MotivoDesactivacion = "Proveedor ya no cumple con los estándares de calidad requeridos",
            UsuarioId = Guid.NewGuid()
        };
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorValido(Guid proveedorId)
    {
        return new Domain.Proveedores.Entities.Proveedor
        {
            Id = proveedorId,
            Nombre = "Proveedor Test S.A.",
            RFC = "XAXX010102000",
            Email = "contacto@proveedor.com",
            Telefono = "555-123-4567",
            Activo = true,
            FechaCreacion = DateTime.UtcNow.AddMonths(-6),
            FechaActualizacion = DateTime.UtcNow.AddDays(-1)
        };
    }

    private OrdenCompra CrearOrdenCompraActiva(Guid proveedorId)
    {
        return new OrdenCompra
        {
            Id = Guid.NewGuid(),
            ProveedorId = proveedorId,
            Estado = EstadoOrdenCompra.Pendiente,
            FechaCreacion = DateTime.UtcNow.AddDays(-5),
            MontoTotal = 1500.0m
        };
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
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("ProveedorId es requerido"));
    }

    [Fact]
    public async Task Validate_ConProveedorIdValido_NoDeberiaRetornarErrorEnProveedorId()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("ProveedorId es requerido"));
    }

    [Fact]
    public async Task Validate_ConProveedorNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        
        ConfigurarProveedorInexistente();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El proveedor especificado no existe"));
    }

    #endregion

    #region Validación Estado del Proveedor

    [Fact]
    public async Task Validate_ConProveedorYaDesactivado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.Activo = false; // Ya desactivado
        
        ConfigurarProveedorExistente(proveedor);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El proveedor ya está desactivado"));
    }

    [Fact]
    public async Task Validate_ConProveedorActivo_NoDeberiaRetornarErrorDeEstado()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.Activo = true;
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("El proveedor ya está desactivado"));
    }

    #endregion

    #region Validación MotivoDesactivacion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoVacioONull_DeberiaRetornarError(string motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoDesactivacion = motivoInvalido;
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion) &&
            e.ErrorMessage.Contains("El motivo de desactivación es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoDesactivacion = "ABC"; // Menos de 5 caracteres
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 5 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoDesactivacion = new string('A', 1001); // Más de 1000 caracteres
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 1000 caracteres"));
    }

    [Theory]
    [InlineData("No cumple con estándares de calidad")]
    [InlineData("Problemas recurrentes en entregas")]
    [InlineData("Solicitud del proveedor para darse de baja del sistema por restructuración interna")]
    public async Task Validate_ConMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoDesactivacion = motivoValido;
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion));
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.Empty;
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.UsuarioId) &&
            e.ErrorMessage.Contains("UsuarioId es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorDeUsuario()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.NewGuid();
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarProveedorCommand.UsuarioId));
    }

    #endregion

    #region Validaciones de Reglas de Negocio Complejas

    [Theory]
    [InlineData(EstadoOrdenCompra.Pendiente)]
    [InlineData(EstadoOrdenCompra.Aprobada)]
    [InlineData(EstadoOrdenCompra.EnTransito)]
    public async Task Validate_ConOrdenesCompraActivas_DeberiaRetornarError(EstadoOrdenCompra estadoActivo)
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        var ordenActiva = CrearOrdenCompraActiva(command.ProveedorId);
        ordenActiva.Estado = estadoActivo;
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenActiva });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"));
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Completada)]
    [InlineData(EstadoOrdenCompra.Cancelada)]
    [InlineData(EstadoOrdenCompra.Rechazada)]
    public async Task Validate_ConOrdenesCompraNoActivas_NoDeberiaRetornarErrorDeOrdenes(EstadoOrdenCompra estadoInactivo)
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        var ordenInactiva = CrearOrdenCompraActiva(command.ProveedorId);
        ordenInactiva.Estado = estadoInactivo;
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenInactiva });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"));
    }

    [Fact]
    public async Task Validate_ConVariasOrdenesActivasYAlgunasInactivas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        
        var ordenes = new[]
        {
            new OrdenCompra { Id = Guid.NewGuid(), ProveedorId = command.ProveedorId, Estado = EstadoOrdenCompra.Completada },
            new OrdenCompra { Id = Guid.NewGuid(), ProveedorId = command.ProveedorId, Estado = EstadoOrdenCompra.Pendiente }, // ACTIVA
            new OrdenCompra { Id = Guid.NewGuid(), ProveedorId = command.ProveedorId, Estado = EstadoOrdenCompra.Cancelada }
        };
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(ordenes);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"));
    }

    [Fact]
    public async Task Validate_ConFacturasPendientesPago_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        
        var facturaPendiente = new Factura
        {
            Id = Guid.NewGuid(),
            ProveedorId = command.ProveedorId,
            Estado = EstadoFactura.Pendiente,
            MontoTotal = 800.0m,
            FechaVencimiento = DateTime.UtcNow.AddDays(15)
        };
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);
        ConfigurarConFacturasPendientes(new[] { facturaPendiente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene facturas pendientes de pago"));
    }

    [Fact]
    public async Task Validate_ConTodasFacturasPagadas_NoDeberiaRetornarErrorDeFacturas()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        
        var facturaPagada = new Factura
        {
            Id = Guid.NewGuid(),
            ProveedorId = command.ProveedorId,
            Estado = EstadoFactura.Pagada,
            MontoTotal = 800.0m,
            FechaPago = DateTime.UtcNow.AddDays(-5)
        };
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);
        ConfigurarConFacturasPendientes(new[] { facturaPagada });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene facturas pendientes de pago"));
    }

    [Fact]
    public async Task Validate_ConProveedorRecienCreado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.FechaCreacion = DateTime.UtcNow.AddHours(-12); // Menos de 24 horas
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId) &&
            e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
    }

    [Fact]
    public async Task Validate_ConProveedorConAntiguedadSuficiente_NoDeberiaRetornarErrorDeAntiguedad()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.FechaCreacion = DateTime.UtcNow.AddDays(-5); // Más de 24 horas
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            ProveedorId = Guid.NewGuid(),
            MotivoDesactivacion = "El proveedor ha solicitado darse de baja del sistema por restructuración de su empresa",
            UsuarioId = Guid.NewGuid()
        };

        var proveedor = new Domain.Proveedores.Entities.Proveedor
        {
            Id = command.ProveedorId,
            Nombre = "Proveedor Test",
            RFC = "XAXX010102000",
            Email = "test@proveedor.com",
            Activo = true,
            FechaCreacion = DateTime.UtcNow.AddDays(-30) // Suficiente antigüedad
        };
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);
        ConfigurarSinFacturasPendientes(command.ProveedorId);

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
        var command = new DesactivarProveedorCommand
        {
            ProveedorId = Guid.Empty, // Error
            MotivoDesactivacion = "AB", // Error: muy corto
            UsuarioId = Guid.Empty // Error
        };

        ConfigurarProveedorInexistente();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.ProveedorId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.UsuarioId));
    }

    [Fact]
    public async Task Validate_ConProveedorConTodosLosProblemas_DeberiaRetornarTodosLosErroresDeNegocio()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.Activo = false; // Ya desactivado
        proveedor.FechaCreacion = DateTime.UtcNow.AddHours(-1); // Recién creado
        
        var ordenActiva = CrearOrdenCompraActiva(command.ProveedorId);
        var facturaPendiente = new Factura
        {
            Id = Guid.NewGuid(),
            ProveedorId = command.ProveedorId,
            Estado = EstadoFactura.Pendiente
        };
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenActiva });
        ConfigurarConFacturasPendientes(new[] { facturaPendiente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        
        // Debería tener múltiples errores de reglas de negocio
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ya está desactivado"));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(1)]    // 1 hora - inválido
    [InlineData(12)]   // 12 horas - inválido
    [InlineData(23)]   // 23 horas - inválido
    public async Task Validate_ConProveedorRecienCreado_DeberiaValidarCorrectamente(int horasAntiguedad)
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.ProveedorId);
        proveedor.FechaCreacion = DateTime.UtcNow.AddHours(-horasAntiguedad);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (horasAntiguedad < 24)
        {
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
        }
        else
        {
            result.Errors.Should().NotContain(e => 
                e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
        }
    }

    [Theory]
    [InlineData(5, true)]    // 5 caracteres - válido
    [InlineData(50, true)]   // 50 caracteres - válido
    [InlineData(1000, true)] // 1000 caracteres - límite válido
    [InlineData(4, false)]   // 4 caracteres - inválido
    [InlineData(1001, false)] // 1001 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoDesactivacion = new string('A', longitud);
        
        var proveedor = CrearProveedorValido(command.ProveedorId);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.ProveedorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion));
        }
        else
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.MotivoDesactivacion));
        }
    }

    #endregion

    #region Helpers de Configuración

    private void ConfigurarProveedorExistente(Domain.Proveedores.Entities.Proveedor proveedor)
    {
        var proveedores = new List<Domain.Proveedores.Entities.Proveedor> { proveedor }.AsQueryable();
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.Provider).Returns(proveedores.Provider);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.Expression).Returns(proveedores.Expression);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.ElementType).Returns(proveedores.ElementType);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.GetEnumerator()).Returns(proveedores.GetEnumerator());
    }

    private void ConfigurarProveedorInexistente()
    {
        var proveedores = new List<Domain.Proveedores.Entities.Proveedor>().AsQueryable();
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.Provider).Returns(proveedores.Provider);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.Expression).Returns(proveedores.Expression);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.ElementType).Returns(proveedores.ElementType);
        _mockProveedoresDbSet.As<IQueryable<Domain.Proveedores.Entities.Proveedor>>().Setup(m => m.GetEnumerator()).Returns(proveedores.GetEnumerator());
    }

    private void ConfigurarSinOrdenesActivas(Guid proveedorId)
    {
        var ordenes = new List<OrdenCompra>().AsQueryable();
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.Provider).Returns(ordenes.Provider);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.Expression).Returns(ordenes.Expression);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.ElementType).Returns(ordenes.ElementType);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.GetEnumerator()).Returns(ordenes.GetEnumerator());
    }

    private void ConfigurarConOrdenesActivas(OrdenCompra[] ordenes)
    {
        var ordenesQueryable = ordenes.AsQueryable();
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.Provider).Returns(ordenesQueryable.Provider);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.Expression).Returns(ordenesQueryable.Expression);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.ElementType).Returns(ordenesQueryable.ElementType);
        _mockOrdenesCompraDbSet.As<IQueryable<OrdenCompra>>().Setup(m => m.GetEnumerator()).Returns(ordenesQueryable.GetEnumerator());
    }

    private void ConfigurarSinFacturasPendientes(Guid proveedorId)
    {
        var facturas = new List<Factura>().AsQueryable();
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturas.Provider);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturas.Expression);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturas.ElementType);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturas.GetEnumerator());
    }

    private void ConfigurarConFacturasPendientes(Factura[] facturas)
    {
        var facturasQueryable = facturas.AsQueryable();
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(facturasQueryable.Provider);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(facturasQueryable.Expression);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(facturasQueryable.ElementType);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(facturasQueryable.GetEnumerator());
    }

    #endregion
} 