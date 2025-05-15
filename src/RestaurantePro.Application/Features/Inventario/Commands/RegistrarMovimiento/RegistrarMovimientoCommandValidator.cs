using FluentValidation;
using System;

namespace RestaurantePro.Application.Features.Inventario.Commands.RegistrarMovimiento
{
    public class RegistrarMovimientoCommandValidator : AbstractValidator<RegistrarMovimientoCommand>
    {
        public RegistrarMovimientoCommandValidator()
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

            RuleFor(v => v.CostoUnitario)
                .GreaterThan(0)
                .When(v => v.CostoUnitario.HasValue)
                .WithMessage("El costo unitario debe ser mayor a cero.");

            RuleFor(v => v.Descripcion)
                .NotEmpty()
                .WithMessage("La descripción es requerida.")
                .MaximumLength(200)
                .WithMessage("La descripción no puede exceder los 200 caracteres.");

            RuleFor(v => v.Referencia)
                .MaximumLength(50)
                .WithMessage("La referencia no puede exceder los 50 caracteres.");

            RuleFor(v => v.Fecha)
                .LessThanOrEqualTo(DateTime.Now)
                .When(v => v.Fecha.HasValue)
                .WithMessage("La fecha no puede ser futura.");

            RuleFor(v => v.ComandaId)
                .GreaterThan(0)
                .When(v => v.ComandaId.HasValue)
                .WithMessage("El ID de la comanda debe ser mayor a cero.");

            RuleFor(v => v.OrdenCompraId)
                .GreaterThan(0)
                .When(v => v.OrdenCompraId.HasValue)
                .WithMessage("El ID de la orden de compra debe ser mayor a cero.");
        }
    }
} 