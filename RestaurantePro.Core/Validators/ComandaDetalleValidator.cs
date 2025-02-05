using FluentValidation;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Validators
{
    public class ComandaDetalleValidator : AbstractValidator<ComandaDetalle>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComandaDetalleValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.PlatoId)
                .MustAsync(async (platoId, _) => await ExistePlato(platoId))
                .WithMessage("El plato no existe")
                .MustAsync(async (platoId, _) => await PlatoEstaDisponible(platoId))
                .WithMessage("El plato no está disponible");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor que 0");
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
    }
} 