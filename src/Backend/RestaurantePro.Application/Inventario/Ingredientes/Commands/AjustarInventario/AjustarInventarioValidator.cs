namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;

/// <summary>
/// Validador para el comando de ajuste de inventario
/// </summary>
public class AjustarInventarioValidator : AbstractValidator<AjustarInventarioCommand>
{
    public AjustarInventarioValidator()
    {
        // Validación del ID del ingrediente
        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es requerido");

        // Validación del tipo de movimiento
        RuleFor(x => x.TipoMovimiento)
            .IsInEnum()
            .WithMessage("El tipo de movimiento debe ser válido");

        // Validación de la cantidad
        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a cero")
            .LessThanOrEqualTo(999999)
            .WithMessage("La cantidad no puede exceder 999,999 unidades");

        // Validación del motivo
        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres")
            .Must(motivo => !string.IsNullOrWhiteSpace(motivo))
            .When(x => !string.IsNullOrEmpty(x.Motivo))
            .WithMessage("El motivo no puede estar vacío si se proporciona");

        // Validación del ID del usuario
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        // Validación condicional para movimientos de salida
        RuleFor(x => x)
            .Must(BeValidMovimientoSalida)
            .When(x => x.TipoMovimiento == TipoMovimientoInventario.Egreso ||
                      x.TipoMovimiento == TipoMovimientoInventario.Decremento ||
                      x.TipoMovimiento == TipoMovimientoInventario.Merma)
            .WithMessage("Los movimientos de salida requieren justificación en el motivo");

        // Validación de cantidad para mermas
        RuleFor(x => x.Cantidad)
            .LessThanOrEqualTo(1000)
            .When(x => x.TipoMovimiento == TipoMovimientoInventario.Merma)
            .WithMessage("Las mermas no pueden exceder 1,000 unidades por operación");
    }

    /// <summary>
    /// Valida que los movimientos de salida tengan motivo
    /// </summary>
    private bool BeValidMovimientoSalida(AjustarInventarioCommand command)
    {
        // Para movimientos de salida, el motivo es obligatorio y debe tener al menos 10 caracteres
        return !string.IsNullOrWhiteSpace(command.Motivo) && command.Motivo.Length >= 10;
    }
} 