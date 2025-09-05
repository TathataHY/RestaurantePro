namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

// NOTA: Se eliminó el validador duplicado de CrearComandaCommand para evitar colisión
// con CrearComandaCommandValidator (que realiza validaciones más completas e incluye
// comprobaciones contra repositorios/DB). Mantener solo los validadores de ítems.

/// <summary>
/// Validador para productos individuales en la comanda
/// </summary>
public class AgregarProductoValidator : AbstractValidator<AgregarProductoDto>
{
    public AgregarProductoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("La cantidad no puede exceder 100 unidades");

        RuleFor(x => x.Observaciones)
            .MaximumLength(200).WithMessage("Las observaciones del producto no pueden exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }
}

/// <summary>
/// Validador para personalizaciones de productos
/// </summary>
public class PersonalizacionValidator : AbstractValidator<PersonalizacionCreateDto>
{
    public PersonalizacionValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de personalización es obligatorio")
            .Must(tipo => new[] { "Extra", "Quitar", "Sustituir" }.Contains(tipo))
            .WithMessage("El tipo de personalización debe ser: Extra, Quitar o Sustituir");

        RuleFor(x => x.IngredienteId)
            .NotEmpty().WithMessage("El ID del ingrediente es obligatorio");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(10).WithMessage("La cantidad no puede exceder 10 unidades")
            .When(x => x.Tipo == "Extra" || x.Tipo == "Sustituir");

        RuleFor(x => x.PrecioAdicional)
            .GreaterThanOrEqualTo(0).WithMessage("El precio adicional no puede ser negativo")
            .LessThanOrEqualTo(1000).WithMessage("El precio adicional no puede exceder $1,000");

        RuleFor(x => x.IngredienteSustitucionId)
            .NotEmpty().WithMessage("El ingrediente de sustitución es obligatorio para personalizaciones de tipo Sustituir")
            .When(x => x.Tipo == "Sustituir");

        RuleFor(x => x.Detalles)
            .MaximumLength(100).WithMessage("Los detalles no pueden exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Detalles));
    }
} 