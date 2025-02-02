using FluentValidation;
using RestaurantePro.Core.Commands;

namespace RestaurantePro.Core.Validators
{
    public class CreateComandaCommandValidator : AbstractValidator<CreateComandaCommand>

    {
        public CreateComandaCommandValidator()
        {
            RuleFor(x => x.MesaId)
                .NotEmpty()
                .WithMessage("El ID de la mesa es requerido");

            RuleFor(x => x.FechaHora)
                .NotEmpty()
                .WithMessage("La fecha y hora son requeridas");

            RuleFor(x => x.Detalles)
                .NotEmpty()
                .WithMessage("La comanda debe tener al menos un detalle");

            RuleForEach(x => x.Detalles)
                .SetValidator(new ComandaDetalleValidator());
        }
    }
}