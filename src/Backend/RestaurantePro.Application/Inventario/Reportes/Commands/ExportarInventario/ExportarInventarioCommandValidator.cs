using FluentValidation;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.ExportarInventario;

public class ExportarInventarioCommandValidator : AbstractValidator<ExportarInventarioCommand>
{
    public ExportarInventarioCommandValidator()
    {
        RuleFor(x => x.Formato)
            .NotEmpty().WithMessage("Debe especificar el formato de exportación.")
            .Must(f => f == "Excel" || f == "PDF" || f == "CSV").WithMessage("El formato de exportación debe ser Excel, PDF o CSV.");

        RuleFor(x => x.FechaDesde)
            .LessThanOrEqualTo(x => x.FechaHasta ?? DateTime.MaxValue)
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue)
            .WithMessage("La fecha desde no puede ser mayor que la fecha hasta.");
    }
} 