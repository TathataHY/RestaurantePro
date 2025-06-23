using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ActualizarMesa;

public class ActualizarMesaCommandValidator : AbstractValidator<ActualizarMesaCommand>
{
    public ActualizarMesaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la mesa es requerido");

        RuleFor(x => x.Numero)
            .NotEmpty()
            .WithMessage("El número de mesa es requerido")
            .Must(numero => int.TryParse(numero, out int num) && num > 0)
            .WithMessage("El número de mesa debe ser un valor numérico mayor que cero");

        RuleFor(x => x.Capacidad)
            .GreaterThan(0)
            .WithMessage("La capacidad debe ser mayor que cero")
            .LessThanOrEqualTo(20)
            .WithMessage("La capacidad no puede ser mayor a 20 personas");

        RuleFor(x => x.Ubicacion)
            .NotEmpty()
            .WithMessage("La ubicación es requerida")
            .MaximumLength(100)
            .WithMessage("La ubicación no puede tener más de 100 caracteres");

        RuleFor(x => x.Tipo)
            .MaximumLength(50)
            .WithMessage("El tipo no puede tener más de 50 caracteres");
    }
} 