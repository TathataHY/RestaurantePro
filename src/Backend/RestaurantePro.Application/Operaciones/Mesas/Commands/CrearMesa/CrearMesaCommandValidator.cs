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

        RuleFor(x => x.Ubicacion)
            .NotEmpty().WithMessage("La ubicación es obligatoria.")
            .MaximumLength(30).WithMessage("La ubicación no debe superar los 30 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.Notas)
            .MaximumLength(500).WithMessage("Las notas no deben superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notas));

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(estado => estado == "Disponible" || estado == "FueraDeServicio")
            .WithMessage("El estado debe ser 'Disponible' o 'FueraDeServicio'.");
    }
} 