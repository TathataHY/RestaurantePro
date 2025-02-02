using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Exceptions;

namespace RestaurantePro.Core.Commands
{
    public class DeleteComandaCommand
    {
        public int Id { get; set; }
    }

    public class DeleteComandaCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteComandaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteComandaCommand command)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(command.Id);
            if (comanda == null)
            {
                throw new NotFoundException($"Comanda con ID {command.Id} no encontrada");
            }

            await _unitOfWork.Comandas.DeleteAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}