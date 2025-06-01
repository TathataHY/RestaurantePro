using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Validador para el comando de crear tarjeta de fidelización
/// </summary>
public class CrearTarjetaFidelizacionValidator : AbstractValidator<CrearTarjetaFidelizacionCommand>
{
    public CrearTarjetaFidelizacionValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.TipoTarjeta)
            .IsInEnum()
            .WithMessage("El tipo de tarjeta no es válido");

        RuleFor(x => x.PuntosIniciales)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los puntos iniciales no pueden ser negativos");

        RuleFor(x => x.CodigoTarjeta)
            .NotEmpty()
            .Length(8, 20)
            .WithMessage("El código de tarjeta debe tener entre 8 y 20 caracteres");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder 500 caracteres");
    }
} 