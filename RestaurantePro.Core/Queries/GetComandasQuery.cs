using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Queries
{
    public class GetComandasQuery
    {
        public EstadoComanda? Estado { get; set; }
    }

    public class GetComandasQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetComandasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ComandaDto>> Handle(GetComandasQuery query)
        {
            var comandas = await _unitOfWork.Comandas.GetAllAsync();

            if (query.Estado.HasValue)
            {
                comandas = comandas.Where(c => c.Estado == query.Estado.Value).ToList();
            }

            return comandas.Select(c => new ComandaDto
            {
                Id = c.Id,
                FechaHora = c.FechaHora,
                MesaId = c.MesaId,
                Estado = c.Estado,
                Detalles = c.Detalles.Select(d => new ComandaDetalleDto
                {
                    PlatoId = d.PlatoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            }).ToList();
        }
    }
}