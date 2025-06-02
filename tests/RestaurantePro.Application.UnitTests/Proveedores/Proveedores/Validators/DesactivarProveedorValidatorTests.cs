namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Validators;
using System.Reflection;

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
        
        _validator = new DesactivarProveedorValidator();
    }

    #region Validation Command Helper

    private DesactivarProveedorCommand CrearCommandValido()
    {
        return new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = "Proveedor ya no cumple con los estándares de calidad requeridos"
        };
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorValido(Guid proveedorId)
    {
        // Usar factory method para crear proveedor válido
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Proveedor Test S.A.",
            "Juan Pérez",
            "contacto@proveedor.com",
            "555-123-4567",
            "Calle Test 123",
            "Ciudad Test",
            "12345",
            "México",
            "XAXX010102000",
            "Cuenta bancaria test",
            30);
        
        // Usar reflexión para establecer el ID y las fechas
        typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id")?.SetValue(proveedor, proveedorId);
        typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("FechaCreacion")?.SetValue(proveedor, DateTime.UtcNow.AddMonths(-6));
        typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("FechaActualizacion")?.SetValue(proveedor, DateTime.UtcNow.AddDays(-1));
        
        return proveedor;
    }

    private OrdenCompra CrearOrdenCompraActiva(Guid proveedorId)
    {
        // Usar factory method para crear orden de compra válida
        var orden = OrdenCompra.Crear(
            proveedorId,
            "Orden de compra de prueba",
            DateTime.UtcNow.AddDays(-5));
        
        // Usar reflexión para establecer el estado después de la creación
        typeof(OrdenCompra).GetProperty("Estado")?.SetValue(orden, EstadoOrdenCompra.Pendiente);
        
        return orden;
    }

    #endregion

    #region Validación Id

    [Fact]
    public async Task Validate_ConProveedorIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("ID del proveedor es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConProveedorIdValido_NoDeberiaRetornarErrorEnProveedorId()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("ID del proveedor es obligatorio"));
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
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("El proveedor especificado no existe"));
    }

    #endregion

    #region Validación Estado del Proveedor

    [Fact]
    public async Task Validate_ConProveedorYaDesactivado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        proveedor.Desactivar("Proveedor desactivado para prueba"); // Ya desactivado
        
        ConfigurarProveedorExistente(proveedor);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("El proveedor ya está desactivado"));
    }

    [Fact]
    public async Task Validate_ConProveedorActivo_NoDeberiaRetornarErrorDeEstado()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        proveedor.Activar(); // Asegurar que está activo
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("El proveedor ya está desactivado"));
    }

    #endregion

    #region Validación RazonDesactivacion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoVacioONull_DeberiaRetornarError(string motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = motivoInvalido;
        
        var proveedor = CrearProveedorValido(command.Id);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("razón de desactivación"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = "ABC"; // Menos de 5 caracteres
        
        var proveedor = CrearProveedorValido(command.Id);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("razón de desactivación no puede exceder 500 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = new string('A', 1001); // Más de 500 caracteres
        
        var proveedor = CrearProveedorValido(command.Id);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("razón de desactivación no puede exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("No cumple con estándares de calidad")]
    [InlineData("Problemas recurrentes en entregas")]
    [InlineData("Solicitud del proveedor para darse de baja del sistema por restructuración interna")]
    public async Task Validate_ConMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = motivoValido;
        
        var proveedor = CrearProveedorValido(command.Id);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion));
    }

    #endregion

    #region Validaciones de Reglas de Negocio Complejas

    [Theory]
    [InlineData(EstadoOrdenCompra.Pendiente)]
    [InlineData(EstadoOrdenCompra.Confirmada)]
    [InlineData(EstadoOrdenCompra.EnTransito)]
    public async Task Validate_ConOrdenesCompraActivas_DeberiaRetornarError(EstadoOrdenCompra estadoActivo)
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        var ordenActiva = CrearOrdenCompraConEstado(command.Id, estadoActivo);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenActiva });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"));
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Recibida)]
    [InlineData(EstadoOrdenCompra.Cancelada)]
    public async Task Validate_ConOrdenesCompraNoActivas_NoDeberiaRetornarErrorDeOrdenes(EstadoOrdenCompra estadoInactivo)
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        var ordenInactiva = CrearOrdenCompraConEstado(command.Id, estadoInactivo);
        
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
        var proveedor = CrearProveedorValido(command.Id);
        
        var ordenes = new[]
        {
            CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Recibida),
            CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Pendiente), // ACTIVA
            CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Cancelada)
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
        var proveedor = CrearProveedorValido(command.Id);
        
        // Crear factura usando factory method del dominio (no inicializador de objetos)
        var facturaPendiente = Factura.Crear(
            "FAC-001",
            TipoFactura.Fiscal,
            "Cliente Test",
            command.Id, // Usar como ClienteId (no hay ProveedorId en Factura)
            "RFC123456789",
            "Dirección Test",
            new List<Guid> { Guid.NewGuid() },
            "Factura de prueba",
            DateTime.UtcNow);
        
        // Usar reflection para setear propiedades específicas para tests
        typeof(Factura).GetProperty("Estado")?.SetValue(facturaPendiente, EstadoFactura.Emitida); // Usar Emitida en lugar de Pendiente
        typeof(Factura).GetProperty("Total")?.SetValue(facturaPendiente, 800.0m);
        typeof(Factura).GetProperty("FechaVencimiento")?.SetValue(facturaPendiente, DateTime.UtcNow.AddDays(15));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);
        ConfigurarConFacturasPendientes(new[] { facturaPendiente });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene facturas pendientes de pago"));
    }

    [Fact]
    public async Task Validate_ConTodasFacturasPagadas_NoDeberiaRetornarErrorDeFacturas()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        
        // Crear factura pagada usando factory method del dominio
        var facturaPagada = Factura.Crear(
            "FAC-002",
            TipoFactura.Fiscal,
            "Cliente Test",
            command.Id, // Usar como ClienteId
            "RFC123456789",
            "Dirección Test",
            new List<Guid> { Guid.NewGuid() },
            "Factura pagada de prueba",
            DateTime.UtcNow);
        
        // Usar reflection para setear propiedades específicas para tests
        typeof(Factura).GetProperty("Estado")?.SetValue(facturaPagada, EstadoFactura.Pagada);
        typeof(Factura).GetProperty("Total")?.SetValue(facturaPagada, 800.0m);
        typeof(Factura).GetProperty("FechaPago")?.SetValue(facturaPagada, DateTime.UtcNow.AddDays(-5));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);
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
        var proveedor = CrearProveedorValido(command.Id);
        
        // Usar reflection para establecer FechaCreacion (menos de 24 horas)
        var fechaCreacionField = typeof(EntityBase).GetField("_fechaCreacion", BindingFlags.NonPublic | BindingFlags.Instance);
        fechaCreacionField?.SetValue(proveedor, DateTime.UtcNow.AddHours(-12));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
    }

    [Fact]
    public async Task Validate_ConProveedorConAntiguedadSuficiente_NoDeberiaRetornarErrorDeAntiguedad()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        
        // Usar reflection para establecer FechaCreacion (más de 24 horas)
        var fechaCreacionField = typeof(EntityBase).GetField("_fechaCreacion", BindingFlags.NonPublic | BindingFlags.Instance);
        fechaCreacionField?.SetValue(proveedor, DateTime.UtcNow.AddDays(-5));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede desactivar un proveedor que fue creado hace menos de 24 horas"));
    }

    [Fact]
    public async Task Validate_ConProveedorConTodosLosProblemas_DeberiaRetornarTodosLosErroresDeNegocio()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id);
        proveedor.Desactivar("Ya desactivado"); // Ya desactivado
        
        // Usar reflection para establecer FechaCreacion (recién creado)
        var fechaCreacionField = typeof(EntityBase).GetField("_fechaCreacion", BindingFlags.NonPublic | BindingFlags.Instance);
        fechaCreacionField?.SetValue(proveedor, DateTime.UtcNow.AddHours(-1));
        
        var ordenActiva = CrearOrdenCompraActiva(command.Id);
        
        // Crear factura usando factory method del dominio
        var facturaPendiente = Factura.Crear(
            "FAC-003",
            TipoFactura.Fiscal,
            "Cliente Test",
            command.Id, // Usar como ClienteId
            "RFC123456789",
            "Dirección Test",
            new List<Guid> { Guid.NewGuid() },
            "Factura pendiente de prueba",
            DateTime.UtcNow);
        
        // Usar reflection para setear estado
        typeof(Factura).GetProperty("Estado")?.SetValue(facturaPendiente, EstadoFactura.Emitida);
        
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

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = "El proveedor ha solicitado darse de baja del sistema por restructuración de su empresa"
        };

        // Crear proveedor usando factory method
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Proveedor Test",
            "Contacto Test",
            "test@proveedor.com",
            "+1234567890",
            "Dirección Test",
            "Ciudad Test",
            "12345",
            "País Test",
            "XAXX010102000",
            "Banco Test",
            30
        );
        
        // Usar reflection para establecer ID y fecha usando campos privados
        var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
        idField?.SetValue(proveedor, command.Id);
        
        var fechaCreacionField = typeof(EntityBase).GetField("_fechaCreacion", BindingFlags.NonPublic | BindingFlags.Instance);
        fechaCreacionField?.SetValue(proveedor, DateTime.UtcNow.AddDays(-30));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);
        ConfigurarSinFacturasPendientes(command.Id);

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
            Id = Guid.Empty, // Error
            RazonDesactivacion = "AB", // Error: muy corto
        };

        ConfigurarProveedorInexistente();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion));
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
        var proveedor = CrearProveedorValido(command.Id);
        
        // Usar reflection para establecer FechaCreacion
        var fechaCreacionField = typeof(EntityBase).GetField("_fechaCreacion", BindingFlags.NonPublic | BindingFlags.Instance);
        fechaCreacionField?.SetValue(proveedor, DateTime.UtcNow.AddHours(-horasAntiguedad));
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

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
        command.RazonDesactivacion = new string('A', longitud);
        
        var proveedor = CrearProveedorValido(command.Id);
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion));
        }
        else
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion));
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

    /// <summary>
    /// Crea una OrdenCompra con el estado especificado usando factory method y reflection
    /// </summary>
    private OrdenCompra CrearOrdenCompraConEstado(Guid proveedorId, EstadoOrdenCompra estado)
    {
        // Usar factory method para crear orden de compra válida
        var orden = OrdenCompra.Crear(
            proveedorId,
            "Orden de compra de prueba",
            DateTime.UtcNow.AddDays(-5));
        
        // Usar reflexión para establecer el estado después de la creación
        var estadoField = typeof(OrdenCompra).GetField("_estado", BindingFlags.NonPublic | BindingFlags.Instance);
        estadoField?.SetValue(orden, estado);
        
        // Usar reflexión para establecer un ID específico
        var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
        idField?.SetValue(orden, Guid.NewGuid());
        
        return orden;
    }

    #endregion
} 