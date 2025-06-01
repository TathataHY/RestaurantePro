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
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de inicio no puede ser futura")
            .GreaterThanOrEqualTo(DateTime.Today.AddYears(-2))
            .WithMessage("La fecha de inicio no puede ser mayor a 2 años atrás");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de fin no puede ser futura")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.NivelDetalle)
            .NotEmpty()
            .WithMessage("El nivel de detalle es requerido")
            .Must(BeValidDetailLevel)
            .WithMessage($"El nivel de detalle debe ser uno de: {string.Join(", ", _nivelesDetalleValidos)}");

        // Validación del rango de fechas
        RuleFor(x => x)
            .Must(x => (x.FechaFin - x.FechaInicio).TotalDays <= 365)
            .WithMessage("El rango de fechas no puede ser mayor a 365 días")
            .When(x => x.FechaInicio != default && x.FechaFin != default);

        // Validaciones condicionales para análisis específicos
        When(x => x.SoloCriticos && x.SoloAlertaStock, () =>
        {
            RuleFor(x => x)
                .Must(x => false)
                .WithMessage("No se pueden activar ambos filtros 'SoloCriticos' y 'SoloAlertaStock' al mismo tiempo");
        });

        // Validación para análisis de tendencias
        When(x => x.IncluirTendencias, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 7)
                .WithMessage("Para incluir análisis de tendencias se requiere un mínimo de 7 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para análisis de recomendaciones
        When(x => x.IncluirRecomendaciones, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 3)
                .WithMessage("Para incluir recomendaciones se requiere un mínimo de 3 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para nivel de detalle específico
        When(x => x.NivelDetalle == "Críticos", () =>
        {
            RuleFor(x => x.SoloCriticos)
                .Equal(true)
                .WithMessage("Cuando el nivel de detalle es 'Críticos', debe activarse el filtro 'SoloCriticos'");
        });

        When(x => x.NivelDetalle == "Financiero", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 1)
                .WithMessage("Para análisis financiero se requiere al menos 1 día de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para análisis básico
        When(x => x.NivelDetalle == "Básico", () =>
        {
            RuleFor(x => x.IncluirTendencias)
                .Equal(false)
                .WithMessage("El análisis básico no incluye tendencias");

            RuleFor(x => x.IncluirRecomendaciones)
                .Equal(false)
                .WithMessage("El análisis básico no incluye recomendaciones");
        });
    }

    private bool BeValidDetailLevel(string nivelDetalle)
    {
        return _nivelesDetalleValidos.Contains(nivelDetalle);
    }
} 