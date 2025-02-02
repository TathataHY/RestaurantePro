using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.Services;
using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;

namespace RestaurantePro.Core.Queries
{
    public class GetPendientesComandasQuery
    {
    }

    public class GetPendientesComandasQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendientesComandasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ComandaDto>> Handle(GetPendientesComandasQuery query)
        {
            var comandas = await _unitOfWork.Comandas.GetPendientesAsync();
            return _mapper.Map<IEnumerable<ComandaDto>>(comandas);
        }
    }
} 