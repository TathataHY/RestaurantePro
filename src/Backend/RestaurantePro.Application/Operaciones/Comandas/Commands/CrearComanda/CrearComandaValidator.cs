namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

/// <summary>
/// Validador para CrearComandaCommand
/// Implementa validaciones específicas para el contexto de restaurante
/// </summary>
public class CrearComandaValidator : AbstractValidator<CrearComandaCommand>
{
    public CrearComandaValidator()
    {
        RuleFor(x => x.MesaId)
            .NotEmpty().WithMessage("El ID de la mesa es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la mesa no puede ser un GUID vacío");

        RuleFor(x => x.MeseroId)
            .NotEmpty().WithMessage("El ID del mesero es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del mesero no puede ser un GUID vacío");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("La comanda debe tener al menos un ítem")
            .NotEmpty().WithMessage("La comanda debe tener al menos un ítem");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        // Validaciones para productos iniciales (renombrado para coincidir con las pruebas)
        RuleForEach(x => x.Items)
            .SetValidator(new AgregarProductoValidator())
            .When(x => x.Items != null && x.Items.Any());

        // Validar que no se excedan los límites de productos por comanda
        RuleFor(x => x.Items)
            .Must(productos => productos.Count <= 50)
            .WithMessage("No se pueden agregar más de 50 productos diferentes en una comanda")
            .When(x => x.Items != null && x.Items.Any());

        // Validar que las cantidades totales no sean excesivas
        RuleFor(x => x.Items)
            .Must(productos => productos.Sum(p => p.Cantidad) <= 200)
            .WithMessage("La cantidad total de productos no puede exceder 200 unidades")
            .When(x => x.Items != null && x.Items.Any());
    }
}

/// <summary>
/// Validador para productos individuales en la comanda
/// </summary>
public class AgregarProductoValidator : AbstractValidator<AgregarProductoDto>
{
    public AgregarProductoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del producto no puede ser un GUID vacío");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("La cantidad no puede exceder 100 unidades");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0")
            .LessThanOrEqualTo(10000).WithMessage("El precio unitario no puede exceder $10,000");

        RuleFor(x => x.Observaciones)
            .MaximumLength(200).WithMessage("Las observaciones del producto no pueden exceder 200 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        // Validar personalizaciones
        RuleForEach(x => x.Personalizaciones)
            .SetValidator(new PersonalizacionValidator())
            .When(x => x.Personalizaciones != null && x.Personalizaciones.Any());

        RuleFor(x => x.Personalizaciones)
            .Must(personalizaciones => personalizaciones.Count <= 10)
            .WithMessage("No se pueden agregar más de 10 personalizaciones por producto")
            .When(x => x.Personalizaciones != null && x.Personalizaciones.Any());
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
            .NotEmpty().WithMessage("El ID del ingrediente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente no puede ser un GUID vacío");

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
            .When(x => !string.IsNullOrWhiteSpace(x.Detalles));
    }
} 