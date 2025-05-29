namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;

/// <summary>
/// Validador para ActualizarStockCommand
/// Implementa validaciones específicas por tipo de movimiento
/// </summary>
public class ActualizarStockValidator : AbstractValidator<ActualizarStockCommand>
{
    public ActualizarStockValidator()
    {
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesPorTipoMovimiento();
        ConfigurarValidacionesOpcionales();
        ConfigurarValidacionesNumericas();
    }

    /// <summary>
    /// Configura validaciones básicas obligatorias
    /// </summary>
    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(x => x.IngredienteId)
            .NotEmpty().WithMessage("El ID del ingrediente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente no puede ser un GUID vacío");

        RuleFor(x => x.TipoMovimiento)
            .NotEmpty().WithMessage("El tipo de movimiento es obligatorio")
            .Must(tipo => new[] { "Ingreso", "Egreso", "Ajuste" }.Contains(tipo, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo de movimiento debe ser: Ingreso, Egreso o Ajuste");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo del movimiento es obligatorio")
            .MaximumLength(200).WithMessage("El motivo no puede exceder 200 caracteres")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");
    }

    /// <summary>
    /// Configura validaciones numéricas
    /// </summary>
    private void ConfigurarValidacionesNumericas()
    {
        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThan(1000000).WithMessage("La cantidad no puede exceder 999,999 unidades")
            .Must(cantidad => cantidad.ToString("F3").Length <= 10).WithMessage("La cantidad tiene demasiados decimales");

        RuleFor(x => x.NuevoCosto)
            .GreaterThan(0).WithMessage("El nuevo costo debe ser mayor a 0")
            .LessThan(1000000).WithMessage("El nuevo costo no puede exceder $999,999")
            .When(x => x.NuevoCosto.HasValue);

        RuleFor(x => x.FechaMovimiento)
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La fecha del movimiento no puede ser futura")
            .GreaterThan(DateTime.Now.AddYears(-2)).WithMessage("La fecha del movimiento no puede ser anterior a 2 años")
            .When(x => x.FechaMovimiento.HasValue);
    }

    /// <summary>
    /// Configura validaciones específicas por tipo de movimiento
    /// </summary>
    private void ConfigurarValidacionesPorTipoMovimiento()
    {
        // Validaciones para INGRESO (compras, devoluciones)
        When(x => x.TipoMovimiento.Equals("Ingreso", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Motivo)
                .Must(motivo => new[] { "compra", "devolucion", "transferencia", "ajuste inicial", "recepcion" }
                    .Any(palabra => motivo.ToLower().Contains(palabra)))
                .WithMessage("El motivo para ingresos debe incluir palabras como: compra, devolución, transferencia, etc.");

            RuleFor(x => x.NuevoCosto)
                .NotNull().WithMessage("El costo es obligatorio para movimientos de ingreso")
                .GreaterThan(0).WithMessage("El costo debe ser mayor a 0 para ingresos");

            RuleFor(x => x.ReferenciaExterna)
                .NotEmpty().WithMessage("La referencia externa es obligatoria para ingresos (factura, orden de compra)")
                .MaximumLength(50).WithMessage("La referencia externa no puede exceder 50 caracteres");
        });

        // Validaciones para EGRESO (consumo, mermas, desperdicios)
        When(x => x.TipoMovimiento.Equals("Egreso", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Motivo)
                .Must(motivo => new[] { "consumo", "merma", "desperdicio", "vencimiento", "transferencia", "venta" }
                    .Any(palabra => motivo.ToLower().Contains(palabra)))
                .WithMessage("El motivo para egresos debe incluir palabras como: consumo, merma, desperdicio, etc.");

            // El nuevo costo no aplica para egresos
            RuleFor(x => x.NuevoCosto)
                .Null().WithMessage("No se puede especificar nuevo costo para movimientos de egreso");

            // El proveedor no aplica para egresos
            RuleFor(x => x.ProveedorId)
                .Null().WithMessage("No se puede especificar proveedor para movimientos de egreso");
        });

        // Validaciones para AJUSTE (inventarios físicos)
        When(x => x.TipoMovimiento.Equals("Ajuste", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Motivo)
                .Must(motivo => new[] { "inventario", "conteo", "correccion", "auditoria", "fisico" }
                    .Any(palabra => motivo.ToLower().Contains(palabra)))
                .WithMessage("El motivo para ajustes debe incluir palabras como: inventario, conteo, corrección, etc.");

            RuleFor(x => x.ReferenciaExterna)
                .NotEmpty().WithMessage("La referencia externa es obligatoria para ajustes (número de inventario)")
                .MaximumLength(50).WithMessage("La referencia externa no puede exceder 50 caracteres");

            // El proveedor no aplica para ajustes
            RuleFor(x => x.ProveedorId)
                .Null().WithMessage("No se puede especificar proveedor para movimientos de ajuste");
        });
    }

    /// <summary>
    /// Configura validaciones para campos opcionales
    /// </summary>
    private void ConfigurarValidacionesOpcionales()
    {
        RuleFor(x => x.ReferenciaExterna)
            .MaximumLength(100).WithMessage("La referencia externa no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenciaExterna));

        RuleFor(x => x.ProveedorId)
            .NotEqual(Guid.Empty).WithMessage("El ID del proveedor no puede ser un GUID vacío")
            .When(x => x.ProveedorId.HasValue);

        // Validaciones para casos especiales de merma
        When(x => x.Motivo.ToLower().Contains("merma"), () =>
        {
            RuleFor(x => x.TipoMovimiento)
                .Equal("Egreso", StringComparer.OrdinalIgnoreCase)
                .WithMessage("Los movimientos de merma deben ser de tipo Egreso");

            RuleFor(x => x.Cantidad)
                .LessThan(1000).WithMessage("Las mermas superiores a 1000 unidades requieren autorización especial");
        });

        // Validaciones para movimientos grandes (posibles errores)
        RuleFor(x => x.Cantidad)
            .LessThan(10000).WithMessage("Movimientos superiores a 10,000 unidades requieren validación adicional")
            .When(x => !x.Motivo.ToLower().Contains("inicial") && !x.Motivo.ToLower().Contains("apertura"));
    }
} 