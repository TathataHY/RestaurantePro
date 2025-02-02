using FluentValidation;
using RestaurantePro.Core.Entities;

namespace RestaurantePro.Core.Validators
{
    public class ComandaValidator : AbstractValidator<Comanda>
    {
        public ComandaValidator()
        {
            RuleFor(x => x.MesaId).NotEmpty();
            RuleFor(x => x.FechaHora).NotEmpty();
            RuleFor(x => x.Estado).IsInEnum();
        }
    }
} 