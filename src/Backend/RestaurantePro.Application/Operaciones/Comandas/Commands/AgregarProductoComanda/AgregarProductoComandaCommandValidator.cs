using System;
using FluentValidation;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProductoComanda
{
    /// <summary>
    /// Validador para el comando AgregarProductoComandaCommand
    /// </summary>
    public class AgregarProductoComandaCommandValidator : AbstractValidator<AgregarProductoComandaCommand>
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IProductoRepository _productoRepository;

        public AgregarProductoComandaCommandValidator(
            IComandaRepository comandaRepository,
            IProductoRepository productoRepository)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));

            RuleFor(v => v.ComandaId)
                .NotEmpty().WithMessage("El ID de la comanda es requerido.")
                .MustAsync(async (comandaId, cancellation) => 
                {
                    var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, false, cancellation);
                    return comanda != null;
                }).WithMessage("La comanda especificada no existe.");

            RuleFor(v => v.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es requerido.")
                .MustAsync(async (productoId, cancellation) => 
                {
                    var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellation);
                    return producto != null;
                }).WithMessage("El producto especificado no existe.");

            RuleFor(v => v.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.")
                .LessThanOrEqualTo(50).WithMessage("La cantidad no puede ser mayor a 50 unidades.");

            RuleFor(v => v.PrecioUnitario)
                .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero.")
                .LessThanOrEqualTo(100000).WithMessage("El precio unitario no puede ser mayor a 100,000.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(200).WithMessage("Las observaciones no pueden exceder los 200 caracteres.");
        }
    }
} 