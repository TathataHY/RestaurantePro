using FluentValidation;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Validators
{
    public class ComandaValidator : AbstractValidator<Comanda>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComandaValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.MesaId)
                .MustAsync(async (mesaId, _) => await ExisteMesa(mesaId))
                .WithMessage("La mesa especificada no existe");

            RuleFor(x => x.Detalles)
                .NotEmpty()
                .WithMessage("La comanda debe tener al menos un detalle")
                .Must(detalles => detalles.Count <= 50)
                .WithMessage("La comanda no puede tener más de 50 detalles");

            RuleForEach(x => x.Detalles)
                .ChildRules(detalle =>
                {
                    detalle.RuleFor(x => x.PlatoId)
                        .MustAsync(async (platoId, _) => await ExistePlato(platoId))
                        .WithMessage("El plato no existe")
                        .MustAsync(async (platoId, _) => await PlatoEstaDisponible(platoId))
                        .WithMessage("El plato no está disponible")
                        .WithName("Detalles");

                    detalle.RuleFor(x => x.Cantidad)
                        .GreaterThan(0)
                        .WithMessage("La cantidad debe ser mayor que 0")
                        .WithName("Detalles");
                });

            RuleForEach(x => x.Detalles)
                .MustAsync(async (detalle, _) => 
                    await HayStockSuficiente(detalle.PlatoId, detalle.Cantidad))
                .WithMessage("Stock insuficiente para el plato");
        }

        private async Task<bool> ExisteMesa(int mesaId)
        {
            var mesa = await _unitOfWork.Mesas.GetByIdAsync(mesaId);
            return mesa != null;
        }

        private async Task<bool> ExistePlato(int platoId)
        {
            var plato = await _unitOfWork.Platos.GetByIdAsync(platoId);
            return plato != null;
        }

        private async Task<bool> PlatoEstaDisponible(int platoId)
        {
            var plato = await _unitOfWork.Platos.GetByIdAsync(platoId);
            return plato != null && plato.Disponible;
        }

        private async Task<bool> HayStockSuficiente(int platoId, int cantidad)
        {
            var plato = await _unitOfWork.Platos.GetByIdAsync(platoId);
            return plato != null && plato.Stock >= cantidad;
        }
    }
} 