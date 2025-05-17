using System;
using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Comandas.CrearComanda
{
    /// <summary>
    /// Validador para el comando CrearComanda
    /// </summary>
    public class CrearComandaValidator : AbstractValidator<CrearComandaCommand>
    {
        public CrearComandaValidator()
        {
            // Validaciones a nivel de comando
            RuleFor(x => x.MesaId)
                .NotEmpty()
                .WithMessage("El ID de la mesa es requerido");
                
            RuleFor(x => x.MeseroId)
                .NotEmpty()
                .WithMessage("El ID del mesero es requerido");
                
            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Observaciones))
                .WithMessage("Las observaciones no pueden exceder 500 caracteres");
                
            // Validaciones para los productos
            RuleForEach(x => x.Productos)
                .SetValidator(new ProductoComandaDtoValidator());
        }
    }
    
    /// <summary>
    /// Validador para los productos de la comanda
    /// </summary>
    public class ProductoComandaDtoValidator : AbstractValidator<ProductoComandaDto>
    {
        public ProductoComandaDtoValidator()
        {
            RuleFor(x => x.ProductoId)
                .NotEmpty()
                .WithMessage("El ID del producto es requerido");
                
            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor que cero");
                
            RuleFor(x => x.Observaciones)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.Observaciones))
                .WithMessage("Las observaciones del producto no pueden exceder 200 caracteres");
        }
    }
} 