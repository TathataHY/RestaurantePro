namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACTUALIZAR STOCK VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de actualización de stock
/// Cobertura: 100% de reglas de negocio del ActualizarStockValidator
/// </summary>
public class ActualizarStockValidatorTests
{
    private readonly ActualizarStockValidator _validator;

    public ActualizarStockValidatorTests()
    {
        _validator = new ActualizarStockValidator();
    }

    #region Validation Command Helper

    private ActualizarStockCommand CrearCommandValido()
    {
        return new ActualizarStockCommand
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = "Ingreso",
            Motivo = "Compra de ingredientes para inventario",
            UsuarioId = Guid.NewGuid(),
            Cantidad = 100m,
            NuevoCosto = 50m,
            FechaMovimiento = DateTime.Now,
            ReferenciaExterna = "FAC-2025-001",
            ProveedorId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación IngredienteId

    [Fact]
    public async Task Validate_ConIngredienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.IngredienteId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.IngredienteId) &&
            e.ErrorMessage.Contains("El ID del ingrediente no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConIngredienteIdValido_NoDeberiaRetornarErrorDeIngredienteId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.IngredienteId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.IngredienteId) &&
            e.ErrorMessage.Contains("El ID del ingrediente es obligatorio"));
    }

    #endregion

    #region Validación TipoMovimiento

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConTipoMovimientoVacioONull_DeberiaRetornarError(string? tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = tipoInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.TipoMovimiento) &&
            e.ErrorMessage.Contains("El tipo de movimiento es obligatorio"));
    }

    [Theory]
    [InlineData("TipoInvalido")]
    [InlineData("Entrada")]
    [InlineData("Salida")]
    [InlineData("Movimiento")]
    public async Task Validate_ConTipoMovimientoInvalido_DeberiaRetornarError(string tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = tipoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.TipoMovimiento) &&
            e.ErrorMessage.Contains("El tipo de movimiento debe ser: Ingreso, Egreso o Ajuste"));
    }

    [Theory]
    [InlineData("Ingreso")]
    [InlineData("Egreso")]
    [InlineData("Ajuste")]
    [InlineData("ingreso")] // Case insensitive
    [InlineData("EGRESO")] // Case insensitive
    public async Task Validate_ConTipoMovimientoValido_NoDeberiaRetornarErrorDeTipoMovimiento(string tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = tipoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.TipoMovimiento) &&
            e.ErrorMessage.Contains("El tipo de movimiento debe ser"));
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
        command.Motivo = motivoInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo del movimiento es obligatorio"));
    }

    [Theory]
    [InlineData("Comp")]  // Menos de 5 caracteres
    [InlineData("Abc")]   // 3 caracteres
    public async Task Validate_ConMotivoMuyCorto_DeberiaRetornarError(string motivoCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 5 caracteres"));
    }

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
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 200 caracteres"));
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
            e.PropertyName == nameof(ActualizarStockCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario no puede ser un GUID vacío"));
    }

    #endregion

    #region Validación Cantidad

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task Validate_ConCantidadMenorOIgualACero_DeberiaRetornarError(decimal cantidadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = cantidadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            e.ErrorMessage.Contains("La cantidad debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConCantidadExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = 1000000m; // Igual o mayor a 1,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            e.ErrorMessage.Contains("La cantidad no puede exceder 999,999 unidades"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]  // Máximo válido
    public async Task Validate_ConCantidadValida_NoDeberiaRetornarErrorDeCantidad(decimal cantidadValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = cantidadValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            (e.ErrorMessage.Contains("La cantidad debe ser mayor a 0") ||
             e.ErrorMessage.Contains("La cantidad no puede exceder 999,999 unidades")));
    }

    #endregion

    #region Validación NuevoCosto

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Validate_ConNuevoCostoMenorOIgualACeroSiSeEspecifica_DeberiaRetornarError(decimal costoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoCosto = costoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto) &&
            e.ErrorMessage.Contains("El nuevo costo debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConNuevoCostoExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoCosto = 1000000m; // Igual o mayor a $1,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto) &&
            e.ErrorMessage.Contains("El nuevo costo no puede exceder $999,999"));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(500)]
    [InlineData(999999)]  // Máximo válido
    public async Task Validate_ConNuevoCostoValido_NoDeberiaRetornarErrorDeNuevoCosto(decimal costoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoCosto = costoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto) &&
            (e.ErrorMessage.Contains("El nuevo costo debe ser mayor a 0") ||
             e.ErrorMessage.Contains("El nuevo costo no puede exceder $999,999")));
    }

    [Fact]
    public async Task Validate_ConNuevoCostoNull_NoDeberiaValidarNuevoCosto()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NuevoCosto = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto));
    }

    #endregion

    #region Validación FechaMovimiento

    [Fact]
    public async Task Validate_ConFechaMovimientoFutura_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaMovimiento = DateTime.Now.AddDays(2); // Más de 1 día en el futuro

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.FechaMovimiento) &&
            e.ErrorMessage.Contains("La fecha del movimiento no puede ser futura"));
    }

    [Fact]
    public async Task Validate_ConFechaMovimientoMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaMovimiento = DateTime.Now.AddYears(-3); // Más de 2 años atrás

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.FechaMovimiento) &&
            e.ErrorMessage.Contains("La fecha del movimiento no puede ser anterior a 2 años"));
    }

    [Theory]
    [InlineData(-365)]  // 1 año atrás
    [InlineData(-30)]   // 1 mes atrás
    [InlineData(0)]     // Hoy
    [InlineData(1)]     // Mañana - hasta 1 día permitido
    public async Task Validate_ConFechaMovimientoValida_NoDeberiaRetornarErrorDeFechaMovimiento(int diasDesdeHoy)
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaMovimiento = DateTime.Now.AddDays(diasDesdeHoy);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarStockCommand.FechaMovimiento));
    }

    [Fact]
    public async Task Validate_ConFechaMovimientoNull_NoDeberiaValidarFechaMovimiento()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaMovimiento = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarStockCommand.FechaMovimiento));
    }

    #endregion

    #region Validaciones por Tipo de Movimiento - INGRESO

    [Theory]
    [InlineData("compra de ingredientes")]
    [InlineData("devolucion proveedor")]
    [InlineData("transferencia almacen")]
    [InlineData("ajuste inicial inventario")]
    [InlineData("recepcion mercancia")]
    public async Task Validate_ConIngresoYMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ingreso";
        command.Motivo = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para ingresos debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConIngresoYMotivoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ingreso";
        command.Motivo = "motivo generico sin palabras clave"; // Sin palabras clave válidas

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para ingresos debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConIngresoSinNuevoCosto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ingreso";
        command.NuevoCosto = null; // Sin costo para ingreso

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto) &&
            e.ErrorMessage.Contains("El costo es obligatorio para movimientos de ingreso"));
    }

    [Fact]
    public async Task Validate_ConIngresoSinReferenciaExterna_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ingreso";
        command.ReferenciaExterna = null; // Sin referencia externa

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ReferenciaExterna) &&
            e.ErrorMessage.Contains("La referencia externa es obligatoria para ingresos"));
    }

    #endregion

    #region Validaciones por Tipo de Movimiento - EGRESO

    [Theory]
    [InlineData("consumo cocina")]
    [InlineData("merma por vencimiento")]
    [InlineData("desperdicio proceso")]
    [InlineData("vencimiento producto")]
    [InlineData("transferencia sucursal")]
    [InlineData("venta directa")]
    public async Task Validate_ConEgresoYMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Egreso";
        command.Motivo = motivoValido;
        command.NuevoCosto = null; // No aplica para egresos
        command.ProveedorId = null; // No aplica para egresos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para egresos debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConEgresoYMotivoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Egreso";
        command.Motivo = "motivo generico sin palabras clave"; // Sin palabras clave válidas

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para egresos debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConEgresoConNuevoCosto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Egreso";
        command.NuevoCosto = 50m; // No debe especificarse para egresos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.NuevoCosto) &&
            e.ErrorMessage.Contains("No se puede especificar nuevo costo para movimientos de egreso"));
    }

    [Fact]
    public async Task Validate_ConEgresoConProveedor_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Egreso";
        command.ProveedorId = Guid.NewGuid(); // No debe especificarse para egresos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ProveedorId) &&
            e.ErrorMessage.Contains("No se puede especificar proveedor para movimientos de egreso"));
    }

    #endregion

    #region Validaciones por Tipo de Movimiento - AJUSTE

    [Theory]
    [InlineData("inventario fisico")]
    [InlineData("conteo semanal")]
    [InlineData("correccion diferencias")]
    [InlineData("auditoria mensual")]
    [InlineData("inventario fisico almacen")]
    public async Task Validate_ConAjusteYMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ajuste";
        command.Motivo = motivoValido;
        command.ProveedorId = null; // No aplica para ajustes

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para ajustes debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConAjusteYMotivoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ajuste";
        command.Motivo = "motivo generico sin palabras clave"; // Sin palabras clave válidas

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo para ajustes debe incluir palabras como"));
    }

    [Fact]
    public async Task Validate_ConAjusteSinReferenciaExterna_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ajuste";
        command.ReferenciaExterna = null; // Sin referencia externa

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ReferenciaExterna) &&
            e.ErrorMessage.Contains("La referencia externa es obligatoria para ajustes"));
    }

    [Fact]
    public async Task Validate_ConAjusteConProveedor_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ajuste";
        command.ProveedorId = Guid.NewGuid(); // No debe especificarse para ajustes

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ProveedorId) &&
            e.ErrorMessage.Contains("No se puede especificar proveedor para movimientos de ajuste"));
    }

    #endregion

    #region Validación ReferenciaExterna

    [Fact]
    public async Task Validate_ConReferenciaExternaMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ReferenciaExterna = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ReferenciaExterna) &&
            e.ErrorMessage.Contains("La referencia externa no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("FAC-2025-001")]
    [InlineData("OC-2025-12345")]
    [InlineData("INV-2025-Q1")]
    [InlineData("REF123")]
    public async Task Validate_ConReferenciaExternaValida_NoDeberiaRetornarErrorDeReferenciaExterna(string referenciaValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ReferenciaExterna = referenciaValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ReferenciaExterna) &&
            e.ErrorMessage.Contains("La referencia externa no puede exceder 100 caracteres"));
    }

    #endregion

    #region Validación ProveedorId

    [Fact]
    public async Task Validate_ConProveedorIdVacioSiSeEspecifica_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProveedorId = Guid.Empty; // GUID vacío si se especifica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El ID del proveedor no puede ser un GUID vacío"));
    }

    #endregion

    #region Validaciones Especiales - Merma

    [Fact]
    public async Task Validate_ConMermaTipoMovimientoIncorrecto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Ingreso"; // Debe ser Egreso para mermas
        command.Motivo = "merma por vencimiento";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.TipoMovimiento) &&
            e.ErrorMessage.Contains("Los movimientos de merma deben ser de tipo Egreso"));
    }

    [Fact]
    public async Task Validate_ConMermaCantidadExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = "Egreso";
        command.Motivo = "merma por vencimiento";
        command.Cantidad = 1500m; // Más de 1000 unidades

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            e.ErrorMessage.Contains("Las mermas superiores a 1000 unidades requieren autorización especial"));
    }

    #endregion

    #region Validaciones Movimientos Grandes

    [Theory]
    [InlineData("compra normal")]
    [InlineData("consumo cocina")]
    public async Task Validate_ConMovimientoGrandeNoInicial_DeberiaRetornarError(string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivo;
        command.Cantidad = 15000m; // Más de 10,000 unidades

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            e.ErrorMessage.Contains("Movimientos superiores a 10,000 unidades requieren validación adicional"));
    }

    [Theory]
    [InlineData("ajuste inicial inventario")]
    [InlineData("apertura nueva sucursal")]
    public async Task Validate_ConMovimientoGrandeInicial_NoDeberiaRetornarError(string motivoInicial)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoInicial;
        command.Cantidad = 15000m; // Más de 10,000 unidades pero es inicial

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarStockCommand.Cantidad) &&
            e.ErrorMessage.Contains("Movimientos superiores a 10,000 unidades requieren validación adicional"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandIngresoCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarStockCommand
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = "Ingreso",
            Motivo = "Compra de ingredientes frescos para la semana",
            UsuarioId = Guid.NewGuid(),
            Cantidad = 50m,
            NuevoCosto = 75.50m,
            FechaMovimiento = DateTime.Now.AddHours(-2),
            ReferenciaExterna = "FAC-2025-001234",
            ProveedorId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandEgresoCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarStockCommand
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = "Egreso",
            Motivo = "Consumo cocina para preparacion platos",
            UsuarioId = Guid.NewGuid(),
            Cantidad = 25m,
            NuevoCosto = null, // No aplica para egresos
            FechaMovimiento = DateTime.Now.AddHours(-1),
            ReferenciaExterna = "CONSUMO-2025-001",
            ProveedorId = null // No aplica para egresos
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandAjusteCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarStockCommand
        {
            IngredienteId = Guid.NewGuid(),
            TipoMovimiento = "Ajuste",
            Motivo = "Inventario fisico mensual enero 2025",
            UsuarioId = Guid.NewGuid(),
            Cantidad = 10m,
            NuevoCosto = 30m, // Opcional para ajustes
            FechaMovimiento = DateTime.Now,
            ReferenciaExterna = "INV-2025-01-001",
            ProveedorId = null // No aplica para ajustes
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
        var command = new ActualizarStockCommand
        {
            IngredienteId = Guid.Empty, // Error
            TipoMovimiento = "", // Error
            Motivo = "", // Error
            UsuarioId = Guid.Empty, // Error
            Cantidad = -10m, // Error
            NuevoCosto = -50m, // Error
            FechaMovimiento = DateTime.Now.AddDays(5), // Error
            ReferenciaExterna = new string('A', 101) // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(6);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Ingreso", "compra", 100, 50)]
    [InlineData("Egreso", "consumo", 50, null)]
    [InlineData("Ajuste", "inventario", 25, 30)]
    public async Task Validate_ConDiferentesEscenariosMovimiento_DeberiaSerValido(string tipo, string motivoBase, decimal cantidad, decimal? costo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TipoMovimiento = tipo;
        command.Motivo = $"{motivoBase} de ingredientes en almacen";
        command.Cantidad = cantidad;
        command.NuevoCosto = costo;
        
        if (tipo == "Egreso" || tipo == "Ajuste")
        {
            command.ProveedorId = null;
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 