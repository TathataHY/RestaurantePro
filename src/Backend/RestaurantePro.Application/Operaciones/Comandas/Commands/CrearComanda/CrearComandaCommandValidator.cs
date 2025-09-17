using FluentValidation;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

public class CrearComandaCommandValidator : AbstractValidator<CrearComandaCommand>
{
    public CrearComandaCommandValidator(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IApplicationDbContext dbContext)
    {
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de comanda inválido");

        RuleFor(x => x.MeseroId)
            .NotEmpty().WithMessage("El mesero es obligatorio");

        When(x => x.Tipo == RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoComanda.Mesa, () =>
        {
            RuleFor(x => x.MesaId)
                .NotNull().WithMessage("La mesa es obligatoria para comandas de tipo Mesa")
                .NotEqual(Guid.Empty).WithMessage("La mesa es obligatoria para comandas de tipo Mesa")
                .MustAsync(async (mesaId, ct) =>
                {
                    if (mesaId == null || mesaId == Guid.Empty) return false;
                    return await dbContext.Mesas.FindAsync(new object[] { mesaId }, ct) != null;
                }).WithMessage("La mesa especificada no existe");
        });

        // Validaciones para comandas Delivery
        When(x => x.Tipo == RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoComanda.Delivery, () =>
        {
            RuleFor(x => x.NombreEntrega)
                .NotEmpty().WithMessage("El nombre de entrega es obligatorio para comandas Delivery")
                .MaximumLength(100).WithMessage("El nombre de entrega no puede exceder 100 caracteres");

            RuleFor(x => x.DireccionEntrega)
                .NotEmpty().WithMessage("La dirección de entrega es obligatoria para comandas Delivery")
                .MaximumLength(200).WithMessage("La dirección de entrega no puede exceder 200 caracteres");

            RuleFor(x => x.TelefonoEntrega)
                .NotEmpty().WithMessage("El teléfono de contacto es obligatorio para comandas Delivery")
                .MaximumLength(20).WithMessage("El teléfono de entrega no puede exceder 20 caracteres");
        });

        RuleFor(x => x.ClienteId)
            .MustAsync(async (clienteId, ct) =>
            {
                if (!clienteId.HasValue) return true;
                return await dbContext.Clientes.FindAsync(new object[] { clienteId.Value }, ct) != null;
            }).WithMessage("El cliente especificado no existe");

        RuleFor(x => x.ProductosIniciales)
            .NotNull().WithMessage("Debe agregar al menos un producto a la comanda")
            .Must(x => x.Any()).WithMessage("Debe agregar al menos un producto a la comanda")
            .Must(x => x.Select(p => p.ProductoId).Distinct().Count() == x.Count)
                .WithMessage("No se permiten productos duplicados en la comanda");

        RuleForEach(x => x.ProductosIniciales)
            .ChildRules(producto =>
            {
                producto.RuleFor(p => p.ProductoId)
                    .NotEmpty().WithMessage("El producto es obligatorio")
                    .MustAsync(async (productoId, ct) =>
                    {
                        return await productoRepository.ObtenerPorIdAsync(productoId, ct) != null;
                    }).WithMessage("El producto especificado no existe");

                producto.RuleFor(p => p.Cantidad)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");
            });
    }
} 