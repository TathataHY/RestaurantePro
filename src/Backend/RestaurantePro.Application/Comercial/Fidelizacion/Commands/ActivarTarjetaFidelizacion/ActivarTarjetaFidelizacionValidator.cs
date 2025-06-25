using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActivarTarjetaFidelizacion;

public class ActivarTarjetaFidelizacionValidator : AbstractValidator<ActivarTarjetaFidelizacionCommand>
{
    public ActivarTarjetaFidelizacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");
    }
} 