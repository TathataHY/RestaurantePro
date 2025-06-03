namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;

/// <summary>
/// Validator para ObtenerAnalisisInventarioQuery
/// </summary>
public class ObtenerAnalisisInventarioValidator : AbstractValidator<ObtenerAnalisisInventarioQuery>
{
    private readonly List<string> _nivelesDetalleValidos = new() 
    { 
        "Básico", "Completo", "Resumen", "Críticos", "Financiero" 
    };

    public ObtenerAnalisisInventarioValidator()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("La fecha de inicio no puede ser futura")
            .GreaterThanOrEqualTo(DateTime.Now.AddYears(-2))
            .WithMessage("La fecha de inicio no puede ser mayor a 2 años atrás");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("La fecha de fin no puede ser futura")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.NivelDetalle)
            .NotEmpty()
            .WithMessage("El nivel de detalle es requerido");

        When(x => !string.IsNullOrWhiteSpace(x.NivelDetalle), () =>
        {
            RuleFor(x => x.NivelDetalle)
                .Must(BeValidDetailLevel)
                .WithMessage($"El nivel de detalle debe ser uno de: {string.Join(", ", _nivelesDetalleValidos)}");
        });

        // Validación del rango de fechas
        RuleFor(x => x)
            .Must(x => (x.FechaFin - x.FechaInicio).TotalDays <= 366)
            .WithMessage("El rango de fechas no puede ser mayor a 365 días")
            .When(x => x.FechaInicio != default && x.FechaFin != default);

        // Validación para CategoriaId cuando se especifica
        RuleFor(x => x.CategoriaId)
            .NotEqual(Guid.Empty)
            .WithMessage("La categoría debe ser válida cuando se especifica")
            .When(x => x.CategoriaId.HasValue);

        // Validaciones condicionales para análisis específicos - Removidas las restricciones conflictivas
        // Los filtros SoloCriticos y SoloAlertaStock pueden coexistir para análisis completos

        // Validación para análisis de tendencias
        When(x => x.IncluirTendencias, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 0)
                .WithMessage("Para incluir análisis de tendencias se requiere un mínimo de 1 día de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para análisis de recomendaciones
        When(x => x.IncluirRecomendaciones, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 0)
                .WithMessage("Para incluir recomendaciones se requiere un mínimo de 1 día de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para nivel de detalle específico - Removidas las restricciones demasiado estrictas
        When(x => x.NivelDetalle == "Financiero", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 0)
                .WithMessage("Para análisis financiero se requiere al menos 1 día de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });
    }

    private bool BeValidDetailLevel(string nivelDetalle)
    {
        return _nivelesDetalleValidos.Contains(nivelDetalle);
    }
} 