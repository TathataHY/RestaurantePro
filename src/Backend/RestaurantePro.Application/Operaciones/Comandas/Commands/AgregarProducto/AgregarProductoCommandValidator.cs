using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProducto;

public class AgregarProductoCommandValidator : AbstractValidator<AgregarProductoCommand>
{
    public AgregarProductoCommandValidator(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (comandaId, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(comandaId, false, ct);
                if (comanda == null) return false;
                
                // Validar que esté en estado editable
                return comanda.Estado == Domain.Operaciones.Comandas.Enums.EstadoComanda.Creada || 
                       comanda.Estado == Domain.Operaciones.Comandas.Enums.EstadoComanda.EnProceso;
            }).WithMessage("La comanda no existe o no está en estado editable");

        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio")
            .MustAsync(async (productoId, ct) =>
            {
                var producto = await productoRepository.ObtenerPorIdAsync(productoId, ct);
                return producto != null && producto.EstaActivo;
            }).WithMessage("El producto no existe o no está activo");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero")
            .LessThanOrEqualTo(50).WithMessage("La cantidad no puede exceder 50 unidades");

        RuleFor(x => x.Observaciones)
            .MaximumLength(200).WithMessage("Las observaciones no pueden exceder los 200 caracteres");
    }
} 