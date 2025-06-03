namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;

/// <summary>
/// Validador para CrearIngredienteCommand
/// Implementa validaciones de negocio para la creación de ingredientes
/// </summary>
public class CrearIngredienteValidator : AbstractValidator<CrearIngredienteCommand>
{
    public CrearIngredienteValidator()
    {
        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesNumericas();
        ConfigurarValidacionesEnums();
        ConfigurarValidacionesOpcionales();
    }

    /// <summary>
    /// Configura validaciones básicas de campos obligatorios
    /// </summary>
    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del ingrediente es obligatorio");

        When(x => !string.IsNullOrWhiteSpace(x.Nombre), () =>
        {
            RuleFor(x => x.Nombre)
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-\.]+$").WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");
        });

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del ingrediente es obligatorio");

        When(x => !string.IsNullOrWhiteSpace(x.Codigo), () =>
        {
            RuleFor(x => x.Codigo)
                .MinimumLength(2).WithMessage("El código debe tener al menos 2 caracteres")
                .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("El código solo puede contener letras mayúsculas, números, guiones y guiones bajos");
        });

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");
    }

    /// <summary>
    /// Configura validaciones numéricas para stocks y costos
    /// </summary>
    private void ConfigurarValidacionesNumericas()
    {
        RuleFor(x => x.StockInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El stock inicial debe ser mayor o igual a 0")
            .LessThan(1000000).WithMessage("El stock inicial no puede exceder 999,999 unidades");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo debe ser mayor o igual a 0")
            .LessThan(100000).WithMessage("El stock mínimo no puede exceder 99,999 unidades");

        RuleFor(x => x.CostoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El costo inicial debe ser mayor o igual a 0")
            .LessThanOrEqualTo(100000).WithMessage("El costo inicial no puede exceder $100,000");

        // El stock inicial debe ser mayor o igual al stock mínimo
        RuleFor(x => x.StockInicial)
            .Must((command, stockInicial) => stockInicial >= command.StockMinimo)
            .WithMessage("El stock inicial debe ser mayor o igual al stock mínimo")
            .When(x => x.StockMinimo >= 0 && x.StockInicial >= 0);
    }

    /// <summary>
    /// Configura validaciones para enums
    /// </summary>
    private void ConfigurarValidacionesEnums()
    {
        var rotacionesValidas = new[] { "Baja", "Media", "Alta" };
        RuleFor(x => x.Rotacion)
            .Must(r => rotacionesValidas.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La rotación debe ser una de: {string.Join(", ", rotacionesValidas)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Rotacion));

        var temporadasValidas = new[] { "TodoElAño", "Primavera", "Verano", "Otoño", "Invierno" };
        RuleFor(x => x.Temporada)
            .Must(t => temporadasValidas.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La temporada debe ser una de: {string.Join(", ", temporadasValidas)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Temporada));

        // Validación de unidades de medida - incluir tanto formas singulares como plurales
        var unidadesValidas = new[] { 
            "Unidad", "Unidades", 
            "Kilogramo", "Kilogramos", 
            "Gramo", "Gramos", 
            "Litro", "Litros", 
            "Mililitro", "Mililitros", 
            "Cucharada", "Cucharadas",
            "Cucharadita", "Cucharaditas",
            "Taza", "Tazas",
            "Paquete", "Paquetes",
            "Piezas", "Pieza"
        };
        RuleFor(x => x.UnidadMedida)
            .Must(u => unidadesValidas.Contains(u, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La unidad de medida no es válida");
    }

    /// <summary>
    /// Configura validaciones para campos opcionales
    /// </summary>
    private void ConfigurarValidacionesOpcionales()
    {
        RuleFor(x => x.ProveedorPrincipalId)
            .NotEqual(Guid.Empty).WithMessage("El ID del proveedor no puede ser un GUID vacío")
            .When(x => x.ProveedorPrincipalId.HasValue);

        RuleFor(x => x.MotivoStockInicial)
            .NotEmpty().WithMessage("El motivo del stock inicial es obligatorio")
            .MaximumLength(200).WithMessage("El motivo no puede exceder 200 caracteres");
    }
} 