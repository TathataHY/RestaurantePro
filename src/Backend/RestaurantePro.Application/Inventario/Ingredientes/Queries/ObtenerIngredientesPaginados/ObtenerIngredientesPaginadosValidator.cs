namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;

/// <summary>
/// Validador para ObtenerIngredientesPaginadosQuery
/// Valida parámetros de paginación, filtros y ordenamiento
/// </summary>
public class ObtenerIngredientesPaginadosValidator : AbstractValidator<ObtenerIngredientesPaginadosQuery>
{
    public ObtenerIngredientesPaginadosValidator()
    {
        ConfigurarValidacionesPaginacion();
        ConfigurarValidacionesFiltros();
        ConfigurarValidacionesOrdenamiento();
        ConfigurarValidacionesRangos();
    }

    /// <summary>
    /// Configura validaciones para parámetros de paginación
    /// </summary>
    private void ConfigurarValidacionesPaginacion()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0")
            .LessThanOrEqualTo(10000).WithMessage("El número de página no puede exceder 10,000");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("El tamaño de página no puede exceder 100 elementos")
            .Must(BeValidPageSize).WithMessage("El tamaño de página debe ser uno de: 10, 20, 50, 100");
    }

    /// <summary>
    /// Configura validaciones para filtros de texto
    /// </summary>
    private void ConfigurarValidacionesFiltros()
    {
        RuleFor(x => x.FiltroTexto)
            .MaximumLength(100).WithMessage("El filtro de texto no puede exceder 100 caracteres")
            .Must(BeValidSearchText).WithMessage("El filtro de texto contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.FiltroTexto));

        RuleFor(x => x.FiltroRotacion)
            .Must(rotacion => new[] { "Baja", "Media", "Alta" }.Contains(rotacion, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La rotación debe ser: Baja, Media o Alta")
            .When(x => !string.IsNullOrWhiteSpace(x.FiltroRotacion));

        RuleFor(x => x.FiltroTemporada)
            .Must(temporada => new[] { "TodoElAño", "Primavera", "Verano", "Otoño", "Invierno" }.Contains(temporada, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La temporada debe ser: TodoElAño, Primavera, Verano, Otoño o Invierno")
            .When(x => !string.IsNullOrWhiteSpace(x.FiltroTemporada));

        RuleFor(x => x.FiltroUnidadMedida)
            .Must(unidad => new[] { "Gramos", "Kilogramos", "Mililitros", "Litros", "Unidad", "Docena", "Paquete" }.Contains(unidad, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La unidad de medida debe ser una de las unidades válidas del sistema")
            .When(x => !string.IsNullOrWhiteSpace(x.FiltroUnidadMedida));

        RuleFor(x => x.ProveedorPrincipalId)
            .NotEqual(Guid.Empty).WithMessage("El ID del proveedor no puede ser un GUID vacío")
            .When(x => x.ProveedorPrincipalId.HasValue);
    }

    /// <summary>
    /// Configura validaciones para parámetros de ordenamiento
    /// </summary>
    private void ConfigurarValidacionesOrdenamiento()
    {
        var camposOrdenValidos = new[] 
        { 
            "Nombre", "Codigo", "StockActual", "StockMinimo", "PorcentajeStock", 
            "ValorTotalStock", "CostoPromedio", "Rotacion", "Temporada", 
            "FechaCreacion", "UnidadMedida" 
        };

        RuleFor(x => x.OrdenarPor)
            .NotEmpty().WithMessage("El campo de ordenamiento es obligatorio")
            .Must(campo => camposOrdenValidos.Contains(campo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", camposOrdenValidos)}");

        RuleFor(x => x.DireccionOrden)
            .NotEmpty().WithMessage("La dirección de ordenamiento es obligatoria")
            .Must(direccion => new[] { "Asc", "Desc" }.Contains(direccion, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La dirección de ordenamiento debe ser: Asc o Desc");
    }

    /// <summary>
    /// Configura validaciones para filtros de rango
    /// </summary>
    private void ConfigurarValidacionesRangos()
    {
        // Validaciones para stock
        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo")
            .LessThan(1000000).WithMessage("El stock mínimo no puede exceder 999,999")
            .When(x => x.StockMinimo.HasValue);

        RuleFor(x => x.StockMaximo)
            .GreaterThan(0).WithMessage("El stock máximo debe ser mayor a 0")
            .LessThan(1000000).WithMessage("El stock máximo no puede exceder 999,999")
            .When(x => x.StockMaximo.HasValue);

        RuleFor(x => x)
            .Must(x => !x.StockMinimo.HasValue || !x.StockMaximo.HasValue || x.StockMinimo <= x.StockMaximo)
            .WithMessage("El stock mínimo no puede ser mayor al stock máximo")
            .When(x => x.StockMinimo.HasValue && x.StockMaximo.HasValue);

        // Validaciones para costo
        RuleFor(x => x.CostoMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El costo mínimo no puede ser negativo")
            .LessThan(1000000).WithMessage("El costo mínimo no puede exceder $999,999")
            .When(x => x.CostoMinimo.HasValue);

        RuleFor(x => x.CostoMaximo)
            .GreaterThan(0).WithMessage("El costo máximo debe ser mayor a 0")
            .LessThan(1000000).WithMessage("El costo máximo no puede exceder $999,999")
            .When(x => x.CostoMaximo.HasValue);

        RuleFor(x => x)
            .Must(x => !x.CostoMinimo.HasValue || !x.CostoMaximo.HasValue || x.CostoMinimo <= x.CostoMaximo)
            .WithMessage("El costo mínimo no puede ser mayor al costo máximo")
            .When(x => x.CostoMinimo.HasValue && x.CostoMaximo.HasValue);

        // Validaciones lógicas de filtros
        RuleFor(x => x)
            .Must(x => !(x.SoloConStock && x.StockMaximo == 0))
            .WithMessage("No se puede filtrar por 'Solo con stock' cuando el stock máximo es 0");

        RuleFor(x => x)
            .Must(x => !(x.SoloBajoStock && x.SoloConStock))
            .WithMessage("No se puede filtrar por 'Solo bajo stock' y 'Solo con stock' al mismo tiempo");
    }

    /// <summary>
    /// Valida que el tamaño de página sea uno de los valores permitidos
    /// </summary>
    private bool BeValidPageSize(int pageSize)
    {
        return new[] { 10, 20, 25, 50, 100 }.Contains(pageSize);
    }

    /// <summary>
    /// Valida que el texto de búsqueda no contenga caracteres peligrosos
    /// </summary>
    private bool BeValidSearchText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        // Permitir letras, números, espacios y algunos caracteres especiales básicos
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s\-\._()&]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(text, allowedChars);
    }
} 