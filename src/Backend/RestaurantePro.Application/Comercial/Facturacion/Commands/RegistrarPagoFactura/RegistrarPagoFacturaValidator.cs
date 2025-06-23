using FluentValidation;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;

public class RegistrarPagoFacturaValidator : AbstractValidator<RegistrarPagoFacturaCommand>
{
    public RegistrarPagoFacturaValidator()
    {
        RuleFor(x => x.FacturaId)
            .NotEmpty().WithMessage("El ID de la factura es obligatorio.");

        RuleFor(x => x.MetodoPago)
            .NotEmpty().WithMessage("El método de pago es obligatorio.")
            .MaximumLength(50).WithMessage("El método de pago no puede superar los 50 caracteres.");

        RuleFor(x => x.ReferenciaPago)
            .MaximumLength(100).WithMessage("La referencia del pago no puede superar los 100 caracteres.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("Las observaciones no pueden superar los 300 caracteres.");
    }
} 