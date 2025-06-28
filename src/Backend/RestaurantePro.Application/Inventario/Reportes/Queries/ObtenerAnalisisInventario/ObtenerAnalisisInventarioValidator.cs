using FluentValidation;

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
        RuleFor(x => x.Categorias)
            .Must(categorias => categorias == null || !categorias.Any(c => string.IsNullOrWhiteSpace(c)))
            .WithMessage("Las categorías no pueden estar vacías");

        RuleFor(x => x.FechaDesde)
            .LessThanOrEqualTo(x => x.FechaHasta)
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue)
            .WithMessage("La fecha desde debe ser menor o igual a la fecha hasta");

        RuleFor(x => x)
            .Must(x => !x.FechaDesde.HasValue || !x.FechaHasta.HasValue || (x.FechaHasta.Value - x.FechaDesde.Value).TotalDays <= 366)
            .WithMessage("El rango de fechas no puede ser mayor a 365 días")
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);

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

        // Validación para análisis de tendencias
        When(x => x.IncluirTendencias, () =>
        {
            RuleFor(x => x)
                .Must(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue && 
                          (x.FechaHasta.Value - x.FechaDesde.Value).TotalDays >= 0)
                .WithMessage("Para incluir análisis de tendencias se requiere un mínimo de 1 día de datos")
                .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);
        });

        // Validación para análisis de recomendaciones
        When(x => x.IncluirRecomendaciones, () =>
        {
            RuleFor(x => x)
                .Must(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue && 
                          (x.FechaHasta.Value - x.FechaDesde.Value).TotalDays >= 0)
                .WithMessage("Para incluir recomendaciones se requiere un mínimo de 1 día de datos")
                .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);
        });

        // Validación para nivel de detalle específico - Removidas las restricciones demasiado estrictas
        When(x => x.NivelDetalle == "Financiero", () =>
        {
            RuleFor(x => x)
                .Must(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue && 
                          (x.FechaHasta.Value - x.FechaDesde.Value).TotalDays >= 0)
                .WithMessage("Para análisis financiero se requiere al menos 1 día de datos")
                .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);
        });
    }

    private bool BeValidDetailLevel(string nivelDetalle)
    {
        return _nivelesDetalleValidos.Contains(nivelDetalle);
    }
} 