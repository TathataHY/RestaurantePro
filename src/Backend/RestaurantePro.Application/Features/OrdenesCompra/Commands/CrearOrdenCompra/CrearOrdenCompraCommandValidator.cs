using FluentValidation;
using System;
using System.Linq;

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.CrearOrdenCompra
{
    public class CrearOrdenCompraCommandValidator : AbstractValidator<CrearOrdenCompraCommand>
    {
        public CrearOrdenCompraCommandValidator()
        {
            RuleFor(v => v.ProveedorId)
                .GreaterThan(0)
                .WithMessage("El ID del proveedor debe ser mayor a cero.");

            RuleFor(v => v.FechaEntregaEstimada)
                .Must(fecha => !fecha.HasValue || fecha.Value > DateTime.Now)
                .WithMessage("La fecha de entrega estimada debe ser posterior a la fecha actual.");

            RuleFor(v => v.Descuento)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("El descuento debe estar entre 0 y 100%.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder los 500 caracteres.");

            RuleFor(v => v.Detalles)
                .NotEmpty()
                .WithMessage("La orden de compra debe tener al menos un detalle.")
                .Must(detalles => detalles.Count <= 100)
                .WithMessage("La orden no puede tener más de 100 detalles.");

            RuleForEach(v => v.Detalles).SetValidator(new DetalleOrdenCompraValidator());
        }
    }

    public class DetalleOrdenCompraValidator : AbstractValidator<DetalleOrdenCompraDto>
    {
        public DetalleOrdenCompraValidator()
        {
            RuleFor(v => v.IngredienteId)
                .GreaterThan(0)
                .WithMessage("El ID del ingrediente debe ser mayor a cero.");

            RuleFor(v => v.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a cero.");

            RuleFor(v => v.UnidadMedida)
                .NotEmpty()
                .WithMessage("La unidad de medida es requerida.")
                .MaximumLength(20)
                .WithMessage("La unidad de medida no puede exceder los 20 caracteres.");

            RuleFor(v => v.PrecioUnitario)
                .GreaterThan(0)
                .WithMessage("El precio unitario debe ser mayor a cero.");

            RuleFor(v => v.PorcentajeImpuesto)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("El porcentaje de impuesto debe estar entre 0 y 100%.");

            RuleFor(v => v.PorcentajeDescuento)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("El porcentaje de descuento debe estar entre 0 y 100%.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(200)
                .WithMessage("Las observaciones no pueden exceder los 200 caracteres.");
        }
    }
} 