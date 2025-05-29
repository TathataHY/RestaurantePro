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
            .NotEmpty().WithMessage("El nombre del ingrediente es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-\.]+$").WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del ingrediente es obligatorio")
            .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres")
            .MinimumLength(2).WithMessage("El código debe tener al menos 2 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("El código solo puede contener letras mayúsculas, números, guiones y guiones bajos");

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
            .GreaterThanOrEqualTo(0).WithMessage("El stock inicial no puede ser negativo")
            .LessThan(1000000).WithMessage("El stock inicial no puede exceder 999,999 unidades");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo")
            .LessThan(100000).WithMessage("El stock mínimo no puede exceder 99,999 unidades");

        RuleFor(x => x.CostoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El costo inicial no puede ser negativo")
            .LessThan(1000000).WithMessage("El costo inicial no puede exceder $999,999");

        // El stock mínimo debe ser menor o igual al stock inicial
        RuleFor(x => x)
            .Must(x => x.StockMinimo <= x.StockInicial)
            .WithMessage("El stock mínimo no puede ser mayor al stock inicial")
            .When(x => x.StockInicial > 0);
    }

    /// <summary>
    /// Configura validaciones para enums
    /// </summary>
    private void ConfigurarValidacionesEnums()
    {
        var rotacionesValidas = new[] { "Baja", "Media", "Alta" };
        RuleFor(x => x.Rotacion)
            .Must(r => rotacionesValidas.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La rotación debe ser una de: {string.Join(", ", rotacionesValidas)}");

        var temporadasValidas = new[] { "TodoElAño", "Primavera", "Verano", "Otoño", "Invierno" };
        RuleFor(x => x.Temporada)
            .Must(t => temporadasValidas.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La temporada debe ser una de: {string.Join(", ", temporadasValidas)}");

        var unidadesValidas = new[] { "Gramos", "Kilogramos", "Mililitros", "Litros", "Unidad", "Docena", "Paquete" };
        RuleFor(x => x.UnidadMedida)
            .Must(u => unidadesValidas.Contains(u, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La unidad de medida debe ser una de: {string.Join(", ", unidadesValidas)}");
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