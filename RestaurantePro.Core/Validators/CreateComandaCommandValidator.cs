using FluentValidation;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.Common;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Validators
{
    public class CreateComandaCommandValidator : AbstractValidator<CreateComandaCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateComandaCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.MesaId)
                .NotEmpty()
                .WithMessage("El ID de la mesa es requerido")
                .MustAsync(async (mesaId, _) =>
                {
                    var mesa = await _unitOfWork.Mesas.GetByIdAsync(mesaId);
                    return mesa != null && mesa.Estado == EstadoMesa.Disponible;
                })
                .WithMessage("La mesa no está disponible");

            RuleFor(x => x.FechaHora)
                .NotEmpty()
                .WithMessage("La fecha y hora son requeridas");

            RuleFor(x => x.Detalles)
                .NotEmpty()
                .WithMessage("La comanda debe tener al menos un detalle");

            RuleForEach(x => x.Detalles)
                .SetValidator(new ComandaDetalleCreateDtoValidator(_unitOfWork));
        }
    }
}