using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.EliminarMesa;

public class EliminarMesaCommandValidator : AbstractValidator<EliminarMesaCommand>
{
    public EliminarMesaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la mesa es obligatorio.")
            .NotEqual(Guid.Empty).WithMessage("El ID de la mesa no puede ser vacío.");
    }
} 