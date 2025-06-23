using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;

public class CrearMesaCommandValidator : AbstractValidator<CrearMesaCommand>
{
    public CrearMesaCommandValidator()
    {
        RuleFor(x => x.Numero)
            .GreaterThan(0).WithMessage("El número de mesa debe ser mayor a 0.");

        RuleFor(x => x.Capacidad)
            .GreaterThan(0).WithMessage("La capacidad debe ser mayor a 0.");

        RuleFor(x => x.Zona)
            .NotEmpty().WithMessage("La zona es obligatoria.")
            .MaximumLength(30).WithMessage("La zona no debe superar los 30 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
} 