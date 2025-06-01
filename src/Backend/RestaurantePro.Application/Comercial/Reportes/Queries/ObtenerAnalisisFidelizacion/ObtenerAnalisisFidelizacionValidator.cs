namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;

/// <summary>
/// Validator para ObtenerAnalisisFidelizacionQuery
/// </summary>
public class ObtenerAnalisisFidelizacionValidator : AbstractValidator<ObtenerAnalisisFidelizacionQuery>
{
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

        RuleFor(x => x.TipoAnalisis)
            .NotNull()
            .WithMessage("El tipo de análisis es requerido")
            .IsInEnum()
            .WithMessage("El tipo de análisis no es válido");

        // Validación del rango de fechas según el tipo de análisis
        RuleFor(x => x)
            .Must(x => ValidateDateRangeForAnalysisType(x.FechaInicio, x.FechaFin, x.TipoAnalisis))
            .WithMessage("El rango de fechas no es adecuado para el tipo de análisis seleccionado")
            .When(x => x.FechaInicio != default && x.FechaFin != default);

        // Validación para análisis completo
        When(x => x.TipoAnalisis == TipoAnalisis.Completo, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 90)
                .WithMessage("Para análisis completo se requiere un mínimo de 90 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para análisis predictivo
        When(x => x.TipoAnalisis == TipoAnalisis.Predictivo, () =>
        {
            RuleFor(x => x)
                .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 180)
                .WithMessage("Para análisis predictivo se requiere un mínimo de 180 días de datos")
                .When(x => x.FechaInicio != default && x.FechaFin != default);
        });

        // Validación para período personalizado (máximo 2 años)
        RuleFor(x => x)
            .Must(x => (x.FechaFin - x.FechaInicio).TotalDays <= 730)
            .WithMessage("El período de análisis no puede ser mayor a 2 años")
            .When(x => x.FechaInicio != default && x.FechaFin != default);

        // Validación para período mínimo (7 días)
        RuleFor(x => x)
            .Must(x => (x.FechaFin - x.FechaInicio).TotalDays >= 7)
            .WithMessage("Se requiere un mínimo de 7 días para el análisis")
            .When(x => x.FechaInicio != default && x.FechaFin != default);

        // Validación para clientes específicos
        RuleFor(x => x.ClientesEspecificos)
            .Must(clientes => clientes == null || clientes.Count <= 1000)
            .WithMessage("No se pueden analizar más de 1,000 clientes específicos")
            .When(x => x.ClientesEspecificos != null);
    }

    private static bool ValidateDateRangeForAnalysisType(DateTime fechaInicio, DateTime fechaFin, TipoAnalisis tipoAnalisis)
    {
        var dias = (fechaFin - fechaInicio).TotalDays;

        return tipoAnalisis switch
        {
            TipoAnalisis.Completo => dias >= 90,
            TipoAnalisis.Predictivo => dias >= 180,
            TipoAnalisis.Comparativo => dias >= 60,
            TipoAnalisis.ClientesEspecificos => dias >= 30,
            TipoAnalisis.Basico => dias >= 7,
            _ => dias >= 7
        };
    }
} 