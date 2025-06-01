namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;

/// <summary>
/// Validator para ObtenerAnalisisFidelizacionQuery
/// </summary>
public class ObtenerAnalisisFidelizacionValidator : AbstractValidator<ObtenerAnalisisFidelizacionQuery>
{
    private readonly List<string> _tiposAnalisisValidos = new() 
    { 
        "Completo", "RFM", "CLV", "Comportamiento", "Segmentacion", "Tendencias" 
    };

    private readonly List<string> _periodosAnalisisValidos = new() 
    { 
        "Semanal", "Mensual", "Trimestral", "Semestral", "Anual", "Personalizado" 
    };

    public ObtenerAnalisisFidelizacionValidator()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de inicio no puede ser futura")
            .GreaterThanOrEqualTo(DateTime.Today.AddYears(-5))
            .WithMessage("La fecha de inicio no puede ser mayor a 5 años atrás");

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

        RuleFor(x => x.TipoAnalisis)
            .NotEmpty()
            .WithMessage("El tipo de análisis es requerido")
            .Must(BeValidAnalysisType)
            .WithMessage($"El tipo de análisis debe ser uno de: {string.Join(", ", _tiposAnalisisValidos)}");

        RuleFor(x => x.PeriodoAnalisis)
            .NotEmpty()
            .WithMessage("El período de análisis es requerido")
            .Must(BeValidAnalysisPeriod)
            .WithMessage($"El período de análisis debe ser uno de: {string.Join(", ", _periodosAnalisisValidos)}");

        // Validación del rango de fechas según el tipo de análisis
        RuleFor(x => x)
            .Must(x => ValidateDateRangeForAnalysisType(x.FechaInicio, x.FechaFin, x.TipoAnalisis))
            .WithMessage("El rango de fechas no es adecuado para el tipo de análisis seleccionado")
            .When(x => x.FechaInicio != default && x.FechaFin != default && !string.IsNullOrEmpty(x.TipoAnalisis));

        // Validación para análisis RFM
        When(x => x.TipoAnalisis == "RFM" || x.TipoAnalisis == "Completo", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 90)
                .WithMessage("Para análisis RFM se requiere un mínimo de 90 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);

            RuleFor(x => x.IncluirSegmentacionRFM)
                .Equal(true)
                .WithMessage("Para análisis RFM debe incluirse la segmentación RFM");
        });

        // Validación para análisis CLV
        When(x => x.TipoAnalisis == "CLV" || x.TipoAnalisis == "Completo", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 180)
                .WithMessage("Para análisis CLV se requiere un mínimo de 180 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);

            RuleFor(x => x.IncluirCalculoCLV)
                .Equal(true)
                .WithMessage("Para análisis CLV debe incluirse el cálculo de CLV");
        });

        // Validación para análisis de comportamiento
        When(x => x.TipoAnalisis == "Comportamiento", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 30)
                .WithMessage("Para análisis de comportamiento se requiere un mínimo de 30 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);

            RuleFor(x => x.IncluirPatronesComportamiento)
                .Equal(true)
                .WithMessage("Para análisis de comportamiento deben incluirse los patrones de comportamiento");
        });

        // Validación para análisis de tendencias
        When(x => x.TipoAnalisis == "Tendencias", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 60)
                .WithMessage("Para análisis de tendencias se requiere un mínimo de 60 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);

            RuleFor(x => x.IncluirTendencias)
                .Equal(true)
                .WithMessage("Para análisis de tendencias debe incluirse el análisis de tendencias");
        });

        // Validación para período personalizado
        When(x => x.PeriodoAnalisis == "Personalizado", () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 7)
                .WithMessage("Para período personalizado se requiere un mínimo de 7 días")
                .When(x => x.FechaInicio != default && x.FechaFin != default);

            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays <= 730)
                .WithMessage("Para período personalizado el máximo es 2 años")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validaciones para filtros específicos
        When(x => x.SoloClientesActivos && x.SoloClientesInactivos, () =>
        {
            RuleFor(x => x)
                .Must(x => false)
                .WithMessage("No se pueden activar ambos filtros 'SoloClientesActivos' y 'SoloClientesInactivos' al mismo tiempo");
        });

        // Validación para umbral de valor mínimo
        RuleFor(x => x.UmbralValorMinimo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El umbral de valor mínimo debe ser mayor o igual a 0")
            .When(x => x.UmbralValorMinimo.HasValue);

        // Validación para límite de clientes
        RuleFor(x => x.LimiteClientes)
            .GreaterThan(0)
            .WithMessage("El límite de clientes debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("El límite de clientes no puede ser mayor a 10,000")
            .When(x => x.LimiteClientes.HasValue);
    }

    private bool BeValidAnalysisType(string tipoAnalisis)
    {
        return _tiposAnalisisValidos.Contains(tipoAnalisis);
    }

    private bool BeValidAnalysisPeriod(string periodoAnalisis)
    {
        return _periodosAnalisisValidos.Contains(periodoAnalisis);
    }

    private static bool ValidateDateRangeForAnalysisType(DateTime fechaInicio, DateTime fechaFin, string tipoAnalisis)
    {
        var dias = (fechaFin - fechaInicio).TotalDays;

        return tipoAnalisis switch
        {
            "RFM" or "Completo" => dias >= 90,
            "CLV" => dias >= 180,
            "Comportamiento" => dias >= 30,
            "Tendencias" => dias >= 60,
            "Segmentacion" => dias >= 30,
            _ => dias >= 7
        };
    }
} 