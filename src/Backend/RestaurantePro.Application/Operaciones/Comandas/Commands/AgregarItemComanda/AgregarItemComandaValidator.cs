namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;

/// <summary>
/// Validador para AgregarItemComandaCommand
/// Aplica reglas de negocio para agregar items a comandas existentes
/// </summary>
public class AgregarItemComandaValidator : AbstractValidator<AgregarItemComandaCommand>
{
    public AgregarItemComandaValidator()
    {
        // Validación de ComandaId
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la comanda no puede ser un GUID vacío");

        // Validación de ProductoId
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del producto no puede ser un GUID vacío");

        // Validación de NombreProducto
        RuleFor(x => x.NombreProducto)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(200).WithMessage("El nombre del producto no puede exceder 200 caracteres")
            .Must(BeValidProductName).WithMessage("El nombre del producto contiene caracteres no válidos");

        // Validación de Cantidad
        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(50).WithMessage("La cantidad máxima por item es 50 unidades");

        // Validación de PrecioUnitario
        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0")
            .LessThanOrEqualTo(10000).WithMessage("El precio unitario no puede exceder $10,000");

        // Validación de Observaciones (opcional)
        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        // Validación de UsuarioId
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");

        // Validación de Personalizaciones
        RuleFor(x => x.Personalizaciones)
            .Must(HaveValidPersonalizationsCount).WithMessage("Máximo 10 personalizaciones por producto");

        // Validador anidado para cada personalización
        RuleForEach(x => x.Personalizaciones)
            .SetValidator(new PersonalizacionCreateDtoValidator())
            .When(x => x.Personalizaciones.Any());
    }

    /// <summary>
    /// Valida que el nombre del producto no contenga caracteres especiales peligrosos
    /// </summary>
    private static bool BeValidProductName(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return false;

        // No permitir caracteres especiales peligrosos
        var forbiddenChars = new[] { '<', '>', '"', '\'', '&', ';' };
        return !forbiddenChars.Any(nombre.Contains);
    }

    /// <summary>
    /// Valida que el número de personalizaciones esté dentro del límite permitido
    /// </summary>
    private static bool HaveValidPersonalizationsCount(List<PersonalizacionCreateDto> personalizaciones)
    {
        return personalizaciones.Count <= 10;
    }
}

/// <summary>
/// Validador para PersonalizacionCreateDto (reutilizado)
/// </summary>
public class PersonalizacionCreateDtoValidator : AbstractValidator<PersonalizacionCreateDto>
{
    public PersonalizacionCreateDtoValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de personalización es obligatorio")
            .Must(BeValidPersonalizationType).WithMessage("Tipo de personalización inválido. Valores válidos: Extra, Quitar, Sustituir");

        RuleFor(x => x.IngredienteId)
            .NotEmpty().WithMessage("El ID del ingrediente es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente no puede ser un GUID vacío");

        RuleFor(x => x.PrecioAdicional)
            .GreaterThanOrEqualTo(0).WithMessage("El precio adicional no puede ser negativo")
            .LessThanOrEqualTo(1000).WithMessage("El precio adicional no puede exceder $1,000");

        RuleFor(x => x.IngredienteSustitucionId)
            .NotEmpty().WithMessage("El ingrediente sustituto es obligatorio para personalizaciones de tipo 'Sustituir'")
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente sustituto no puede ser un GUID vacío")
            .When(x => x.Tipo == "Sustituir");

        RuleFor(x => x.IngredienteSustitucionId)
            .Empty().WithMessage("No se debe especificar ingrediente sustituto para personalizaciones que no son de tipo 'Sustituir'")
            .When(x => x.Tipo != "Sustituir");
    }

    private static bool BeValidPersonalizationType(string tipo)
    {
        var validTypes = new[] { "Extra", "Quitar", "Sustituir" };
        return validTypes.Contains(tipo);
    }
} 