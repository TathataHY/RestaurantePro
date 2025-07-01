namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA DESACTIVAR PROVEEDOR VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de desactivación de proveedores
/// Cobertura: 100% de reglas de negocio del DesactivarProveedorValidator
/// </summary>
public class DesactivarProveedorValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly DesactivarProveedorValidator _validator;

    // Fecha base fija para todos los tests de antigüedad
    private static readonly DateTime FechaActualFija = new DateTime(2024, 1, 15, 12, 0, 0);

    public DesactivarProveedorValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDateTimeService = new Mock<IDateTimeService>();
        // Usar la fecha base fija para todos los tests
        _mockDateTimeService.Setup(x => x.Now).Returns(FechaActualFija);
        _validator = new DesactivarProveedorValidator(_mockContext.Object, _mockDateTimeService.Object);
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

    private Domain.Proveedores.Entities.Proveedor CrearProveedorValido(Guid proveedorId, DateTime? fechaCreacion = null)
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
            "Chile",
            "XAXX010102000",
            "Cuenta bancaria test",
            30);
        
        proveedor.SetIdForTesting(proveedorId);
        proveedor.SetFechaCreacionForTesting(fechaCreacion ?? FechaActualFija.AddMonths(-6));
        // Setear también FechaRegistro para los tests de antigüedad
        if (fechaCreacion.HasValue)
            typeof(Domain.Proveedores.Entities.Proveedor)
                .GetProperty("FechaRegistro")!
                .SetValue(proveedor, fechaCreacion.Value);
        return proveedor;
    }

    private OrdenCompra CrearOrdenCompraActiva(Guid proveedorId)
    {
        // Usar factory method para crear orden de compra válida
        var orden = OrdenCompra.Crear(
            proveedorId,
            "Orden de compra de prueba",
            FechaActualFija.AddDays(-5));
        
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

        // Configurar mocks mínimos para evitar NullReferenceException
        ConfigurarProveedorInexistente();
        ConfigurarSinOrdenesActivas(Guid.Empty);

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
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddDays(-6));
        
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
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.Id) &&
            e.ErrorMessage.Contains("El proveedor no fue encontrado"));
    }

    #endregion

    #region Validación Estado del Proveedor

    [Fact]
    public async Task Validate_ConProveedorYaDesactivado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
        proveedor.Desactivar("Proveedor desactivado para prueba"); // Ya desactivado
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

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
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
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
        
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("motivo de desactivación"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = "ABC"; // Menos de 5 caracteres
        
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("debe tener al menos 5 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RazonDesactivacion = new string('A', 1001); // Más de 1000 caracteres
        
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(DesactivarProveedorCommand.RazonDesactivacion) &&
            e.ErrorMessage.Contains("no puede exceder 1000 caracteres"));
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
        
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
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
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
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
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
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
        var proveedor = CrearProveedorValido(command.Id, FechaActualFija.AddMonths(-6));
        
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
    public async Task Validate_ConProveedorRecienCreado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddHours(-12));
        
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
        var fechaCreacion = FechaActualFija.AddHours(-25); // 25 horas de antigüedad
        var proveedor = CrearProveedorValido(command.Id, fechaCreacion);
        
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
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddHours(-1));
        proveedor.Desactivar("Ya desactivado"); // Ya desactivado
        
        var ordenActiva = CrearOrdenCompraActiva(command.Id);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenActiva });

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

        var fechaCreacion = FechaActualFija.AddHours(-48); // 48 horas de antigüedad
        var proveedor = CrearProveedorValido(command.Id, fechaCreacion);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarSinOrdenesActivas(command.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
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
        ConfigurarSinOrdenesActivas(Guid.Empty);

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
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddHours(-horasAntiguedad));
        
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
        
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddMonths(-6));
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

    #region Debug Tests

    [Fact]
    public async Task Debug_VerificarEstadoOrdenCompra()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddMonths(-6));
        
        // Crear una orden con estado Cancelada (NO debe ser considerada activa)
        var ordenCancelada = CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Cancelada);
        
        // Verificar que el estado se estableció correctamente
        ordenCancelada.Estado.Should().Be(EstadoOrdenCompra.Cancelada);
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenCancelada });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        // Esta prueba nos ayuda a ver qué errores se están generando exactamente
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"),
            "Porque una orden CANCELADA no debería considerarse activa");
    }

    [Fact]
    public async Task Debug_VerificarConsultaDirectaOrdenesActivas()
    {
        // Arrange
        var command = CrearCommandValido();
        var proveedor = CrearProveedorValido(command.Id, DateTime.Now.AddMonths(-6));
        
        // Crear órdenes con diferentes estados
        var ordenPendiente = CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Pendiente);    // ACTIVA
        var ordenCancelada = CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Cancelada);   // NO ACTIVA
        var ordenRecibida = CrearOrdenCompraConEstado(command.Id, EstadoOrdenCompra.Recibida);     // NO ACTIVA
        
        ConfigurarProveedorExistente(proveedor);
        ConfigurarConOrdenesActivas(new[] { ordenPendiente, ordenCancelada, ordenRecibida });

        // Act - Simular exactamente la consulta del validador
        var ordenesActivas = await _mockContext.Object.OrdenesCompra
            .Where(o => o.ProveedorId == command.Id &&
                       (o.Estado == EstadoOrdenCompra.Pendiente ||
                        o.Estado == EstadoOrdenCompra.Confirmada ||
                        o.Estado == EstadoOrdenCompra.EnTransito))
            .AnyAsync();

        // Assert
        ordenesActivas.Should().BeTrue("Porque debe encontrar la orden Pendiente como activa");
        
        // Ejecutar validador completo
        var result = await _validator.ValidateAsync(command);
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("No se puede desactivar el proveedor porque tiene órdenes de compra activas"));
    }

    #endregion

    #region Helpers de Configuración

    private void ConfigurarProveedorExistente(Domain.Proveedores.Entities.Proveedor proveedor)
    {
        var mockProveedoresDbSet = MockDbSetHelper.CreateMockDbSet(new List<Domain.Proveedores.Entities.Proveedor> { proveedor }.AsQueryable());
        _mockContext.Setup(c => c.Proveedores).Returns(mockProveedoresDbSet.Object);
    }

    private void ConfigurarProveedorInexistente()
    {
        var mockProveedoresDbSet = MockDbSetHelper.CreateEmptyMockDbSet<Domain.Proveedores.Entities.Proveedor>();
        _mockContext.Setup(c => c.Proveedores).Returns(mockProveedoresDbSet.Object);
    }

    private void ConfigurarSinOrdenesActivas(Guid proveedorId)
    {
        var mockOrdenesCompraDbSet = MockDbSetHelper.CreateEmptyMockDbSet<OrdenCompra>();
        _mockContext.Setup(c => c.OrdenesCompra).Returns(mockOrdenesCompraDbSet.Object);
    }

    private void ConfigurarConOrdenesActivas(OrdenCompra[] ordenes)
    {
        var mockOrdenesCompraDbSet = MockDbSetHelper.CreateMockDbSet(ordenes.AsQueryable());
        _mockContext.Setup(c => c.OrdenesCompra).Returns(mockOrdenesCompraDbSet.Object);
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
            DateTime.Now.AddDays(-5));
        
        // Usar reflexión para establecer el estado después de la creación
        // La propiedad Estado es pública con setter privado, no hay campo _estado
        var estadoProperty = typeof(OrdenCompra).GetProperty("Estado");
        estadoProperty?.SetValue(orden, estado);
        
        // Usar el método nuevo para establecer un ID específico
        orden.SetIdForTesting(Guid.NewGuid());
        
        return orden;
    }

    #endregion
} 