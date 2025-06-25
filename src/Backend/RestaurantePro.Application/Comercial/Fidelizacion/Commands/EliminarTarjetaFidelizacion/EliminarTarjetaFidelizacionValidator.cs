using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;

public class EliminarTarjetaFidelizacionValidator : AbstractValidator<EliminarTarjetaFidelizacionCommand>
{
    public EliminarTarjetaFidelizacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");
    }
} 