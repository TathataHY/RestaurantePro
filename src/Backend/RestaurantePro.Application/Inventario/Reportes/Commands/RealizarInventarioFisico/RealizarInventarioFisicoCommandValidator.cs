using FluentValidation;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.RealizarInventarioFisico;

public class RealizarInventarioFisicoCommandValidator : AbstractValidator<RealizarInventarioFisicoCommand>
{
    public RealizarInventarioFisicoCommandValidator()
    {
        RuleFor(x => x.FechaInventario)
            .NotEmpty().WithMessage("La fecha de inventario es obligatoria.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha de inventario no puede ser futura.");

        RuleFor(x => x.UsuarioId)
            .NotEqual(Guid.Empty).WithMessage("El usuario responsable es obligatorio.");

        RuleFor(x => x.Conteos)
            .NotNull().WithMessage("Debe enviar los conteos del inventario.")
            .Must(c => c != null && c.Count > 0).WithMessage("Debe incluir al menos un conteo.")
            .Must(SinIngredientesDuplicados).WithMessage("No puede haber ingredientes repetidos en el conteo.");

        RuleForEach(x => x.Conteos).SetValidator(new ConteoInventarioDtoValidator());
    }

    private bool SinIngredientesDuplicados(List<ConteoInventarioDto> conteos)
    {
        if (conteos == null) return true;
        return conteos.Select(c => c.IngredienteId).Distinct().Count() == conteos.Count;
    }
}

public class ConteoInventarioDtoValidator : AbstractValidator<ConteoInventarioDto>
{
    public ConteoInventarioDtoValidator()
    {
        RuleFor(x => x.IngredienteId)
            .NotEqual(Guid.Empty).WithMessage("El ID del ingrediente es obligatorio.");
        RuleFor(x => x.CantidadContada)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad contada no puede ser negativa.");
        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres.");
    }
} 