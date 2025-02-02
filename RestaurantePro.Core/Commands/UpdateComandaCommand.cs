using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Services;
using AutoMapper;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Exceptions;
using MediatR;
using System.Threading;

namespace RestaurantePro.Core.Commands
{
    public class UpdateComandaCommand : IRequest<ComandaDto>
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; }
        public EstadoComanda Estado { get; set; }
        public List<ComandaDetalleDto> Detalles { get; set; }
    }

    public class UpdateComandaCommandHandler : IRequestHandler<UpdateComandaCommand, ComandaDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateComandaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ComandaDto> Handle(UpdateComandaCommand request, CancellationToken cancellationToken)
        {
            var comanda = await _unitOfWork.Comandas.GetComandaWithDetallesAsync(request.Id);
            if (comanda == null)
            {
                throw new NotFoundException($"Comanda con ID {request.Id} no encontrada");
            }

            _mapper.Map(request, comanda);

            await _unitOfWork.Comandas.UpdateAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ComandaDto>(comanda);
        }
    }
} 