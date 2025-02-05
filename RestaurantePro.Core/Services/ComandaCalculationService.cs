using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Services
{
    public interface IComandaCalculationService
    {
        Task<decimal> CalcularTotalComanda(int comandaId);
        Task<bool> ValidarStockDisponible(int platoId, int cantidad);
    }

    public class ComandaCalculationService : IComandaCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComandaCalculationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CalcularTotalComanda(int comandaId)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(comandaId);
            return comanda.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        }

        public async Task<bool> ValidarStockDisponible(int platoId, int cantidad)
        {
            var plato = await _unitOfWork.Platos.GetByIdAsync(platoId);
            return plato.Stock >= cantidad;
        }
    }
}