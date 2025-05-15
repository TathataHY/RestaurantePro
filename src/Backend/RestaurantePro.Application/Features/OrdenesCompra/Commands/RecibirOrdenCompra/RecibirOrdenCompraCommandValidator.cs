using FluentValidation;
using System;

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.RecibirOrdenCompra
{
    public class RecibirOrdenCompraCommandValidator : AbstractValidator<RecibirOrdenCompraCommand>
    {
        public RecibirOrdenCompraCommandValidator()
        {
            RuleFor(v => v.OrdenCompraId)
                .GreaterThan(0)
                .WithMessage("El ID de la orden de compra debe ser mayor a cero.");

            RuleFor(v => v.FechaRecepcion)
                .NotEmpty()
                .WithMessage("La fecha de recepción es requerida.")
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La fecha de recepción no puede ser futura.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder los 500 caracteres.");

            RuleFor(v => v.Detalles)
                .NotEmpty()
                .WithMessage("Debe especificar al menos un detalle para la recepción.");

            RuleForEach(v => v.Detalles).SetValidator(new DetalleRecepcionValidator());
        }
    }

    public class DetalleRecepcionValidator : AbstractValidator<DetalleRecepcionDto>
    {
        public DetalleRecepcionValidator()
        {
            RuleFor(v => v.DetalleOrdenCompraId)
                .GreaterThan(0)
                .WithMessage("El ID del detalle de la orden debe ser mayor a cero.");

            RuleFor(v => v.CantidadRecibida)
                .GreaterThan(0)
                .WithMessage("La cantidad recibida debe ser mayor a cero.");

            RuleFor(v => v.DescripcionInconformidad)
                .NotEmpty()
                .When(v => v.TieneInconformidad)
                .WithMessage("Debe especificar la descripción de la inconformidad.")
                .MaximumLength(200)
                .WithMessage("La descripción de la inconformidad no puede exceder los 200 caracteres.");
        }
    }
} 