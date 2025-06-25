using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.DesactivarTarjetaFidelizacion;

public class DesactivarTarjetaFidelizacionValidator : AbstractValidator<DesactivarTarjetaFidelizacionCommand>
{
    public DesactivarTarjetaFidelizacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");
    }
} 