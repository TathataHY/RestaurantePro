namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;

/// <summary>
/// Validador para ObtenerProveedoresPaginadosQuery
/// Valida parámetros de paginación, filtros y ordenamiento
/// </summary>
public class ObtenerProveedoresPaginadosValidator : AbstractValidator<ObtenerProveedoresPaginadosQuery>
{
    /// <summary>
    /// Campos válidos para ordenamiento
    /// </summary>
    private readonly string[] _camposOrdenValidos = 
    {
        "Nombre", "Ciudad", "Pais", "RFC", "DiasCredito", 
        "FechaRegistro", "Email", "NombreContacto"
    };

    public ObtenerProveedoresPaginadosValidator()
    {
        ConfigurarValidacionesPaginacion();
        ConfigurarValidacionesFiltros();
        ConfigurarValidacionesOrdenamiento();
        ConfigurarValidacionesNegocio();
    }

    /// <summary>
    /// Configura validaciones para parámetros de paginación
    /// </summary>
    private void ConfigurarValidacionesPaginacion()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0")
            .LessThanOrEqualTo(1000).WithMessage("El número de página no puede exceder 1000");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("El tamaño de página no puede exceder 100 elementos")
            .Must(BeValidPageSize).WithMessage("El tamaño de página debe ser uno de los valores permitidos: 5, 10, 15, 20, 25, 50, 100");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");
    }

    /// <summary>
    /// Configura validaciones para filtros de búsqueda
    /// </summary>
    private void ConfigurarValidacionesFiltros()
    {
        RuleFor(x => x.TerminoBusqueda)
            .MinimumLength(2).WithMessage("El término de búsqueda debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El término de búsqueda no puede exceder 100 caracteres")
            .Must(BeValidSearchTerm).WithMessage("El término de búsqueda contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.TerminoBusqueda));

        RuleFor(x => x.Ciudad)
            .MinimumLength(2).WithMessage("El nombre de la ciudad debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre de la ciudad no puede exceder 100 caracteres")
            .Must(BeValidCityName).WithMessage("El nombre de la ciudad contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.Ciudad));

        RuleFor(x => x.Pais)
            .MinimumLength(2).WithMessage("El nombre del país debe tener al menos 2 caracteres")
            .MaximumLength(50).WithMessage("El nombre del país no puede exceder 50 caracteres")
            .Must(BeValidCountryName).WithMessage("El nombre del país contiene caracteres no válidos")
            .When(x => !string.IsNullOrWhiteSpace(x.Pais));
    }

    /// <summary>
    /// Configura validaciones para filtros de días de crédito
    /// </summary>
    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(x => x.DiasCredito_Min)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de crédito mínimos no pueden ser negativos")
            .LessThanOrEqualTo(365).WithMessage("Los días de crédito mínimos no pueden exceder 365 días")
            .When(x => x.DiasCredito_Min.HasValue);

        RuleFor(x => x.DiasCredito_Max)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de crédito máximos no pueden ser negativos")
            .LessThanOrEqualTo(365).WithMessage("Los días de crédito máximos no pueden exceder 365 días")
            .When(x => x.DiasCredito_Max.HasValue);

        // Validar que el rango de días de crédito sea lógico
        RuleFor(x => x)
            .Must(x => !x.DiasCredito_Min.HasValue || !x.DiasCredito_Max.HasValue || 
                      x.DiasCredito_Min.Value <= x.DiasCredito_Max.Value)
            .WithMessage("El mínimo de días de crédito debe ser menor o igual al máximo")
            .When(x => x.DiasCredito_Min.HasValue && x.DiasCredito_Max.HasValue);
    }

    /// <summary>
    /// Configura validaciones para parámetros de ordenamiento
    /// </summary>
    private void ConfigurarValidacionesOrdenamiento()
    {
        RuleFor(x => x.CampoOrden)
            .NotEmpty().WithMessage("El campo de ordenamiento es obligatorio")
            .Must(BeValidOrderField).WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", _camposOrdenValidos)}");

        RuleFor(x => x.DireccionOrden)
            .NotEmpty().WithMessage("La dirección de ordenamiento es obligatoria")
            .Must(BeValidOrderDirection).WithMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'");
    }

    /// <summary>
    /// Valida que el tamaño de página sea uno de los valores permitidos
    /// </summary>
    private bool BeValidPageSize(int pageSize)
    {
        var validSizes = new[] { 5, 10, 15, 20, 25, 50, 100 };
        return validSizes.Contains(pageSize);
    }

    /// <summary>
    /// Valida que el término de búsqueda sea válido
    /// </summary>
    private bool BeValidSearchTerm(string? termino)
    {
        if (string.IsNullOrWhiteSpace(termino)) return true;
        
        // Permitir letras, números, espacios y algunos caracteres especiales
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s\.\-@_'""]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(termino, allowedChars);
    }

    /// <summary>
    /// Valida que el nombre de ciudad sea válido
    /// </summary>
    private bool BeValidCityName(string? ciudad)
    {
        if (string.IsNullOrWhiteSpace(ciudad)) return true;
        
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\.\-']+$";
        return System.Text.RegularExpressions.Regex.IsMatch(ciudad, allowedChars);
    }

    /// <summary>
    /// Valida que el nombre del país sea válido
    /// </summary>
    private bool BeValidCountryName(string? pais)
    {
        if (string.IsNullOrWhiteSpace(pais)) return true;
        
        var allowedChars = @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\.\-']+$";
        return System.Text.RegularExpressions.Regex.IsMatch(pais, allowedChars);
    }

    /// <summary>
    /// Valida que el campo de ordenamiento sea válido
    /// </summary>
    private bool BeValidOrderField(string campoOrden)
    {
        return _camposOrdenValidos.Contains(campoOrden, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida que la dirección de ordenamiento sea válida
    /// </summary>
    private bool BeValidOrderDirection(string direccionOrden)
    {
        return direccionOrden.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
               direccionOrden.Equals("desc", StringComparison.OrdinalIgnoreCase);
    }
} 