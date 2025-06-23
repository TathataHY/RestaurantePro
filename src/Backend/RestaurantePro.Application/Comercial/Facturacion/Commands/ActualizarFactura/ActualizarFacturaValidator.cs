using FluentValidation;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.ActualizarFactura;

public class ActualizarFacturaValidator : AbstractValidator<ActualizarFacturaCommand>
{
    public ActualizarFacturaValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la factura es obligatorio.");

        RuleFor(x => x.NombreCliente)
            .MaximumLength(100).WithMessage("El nombre del cliente no puede superar los 100 caracteres.");

        RuleFor(x => x.IdentificacionFiscal)
            .MaximumLength(50).WithMessage("La identificación fiscal no puede superar los 50 caracteres.");

        RuleFor(x => x.DireccionCliente)
            .MaximumLength(200).WithMessage("La dirección no puede superar los 200 caracteres.");

        RuleFor(x => x.EmailCliente)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailCliente))
            .WithMessage("El email no es válido.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.");

        RuleFor(x => x.DiasCredito)
            .GreaterThanOrEqualTo(0).When(x => x.DiasCredito.HasValue)
            .WithMessage("Los días de crédito no pueden ser negativos.");
    }
} 