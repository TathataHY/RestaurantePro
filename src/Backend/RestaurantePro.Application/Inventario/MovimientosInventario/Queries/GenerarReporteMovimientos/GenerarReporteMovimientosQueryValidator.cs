using FluentValidation;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.GenerarReporteMovimientos;

public class GenerarReporteMovimientosQueryValidator : AbstractValidator<GenerarReporteMovimientosQuery>
{
    public GenerarReporteMovimientosQueryValidator()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida");

        RuleFor(x => x.FechaInicio)
            .LessThanOrEqualTo(x => x.FechaFin)
            .WithMessage("La fecha de inicio debe ser menor o igual a la fecha de fin");

        RuleFor(x => x.FechaFin)
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.FechaInicio)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("La fecha de inicio no puede ser futura");

        RuleFor(x => x.FechaFin)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("La fecha de fin no puede ser futura");

        RuleFor(x => x.FechaFin)
            .Must((query, fechaFin) => (fechaFin - query.FechaInicio).Days <= 365)
            .WithMessage("El período del reporte no puede exceder 1 año");
    }
} 