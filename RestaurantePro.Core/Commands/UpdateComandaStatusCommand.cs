using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.DTOs;
using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Common;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.Commands
{
    public class UpdateComandaStatusCommand : IRequest<Result<ComandaDto>>
    {
        public int ComandaId { get; set; }
        public EstadoComanda NewStatus { get; set; }
    }

    public class UpdateComandaStatusCommandHandler : IRequestHandler<UpdateComandaStatusCommand, Result<ComandaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ComandaStateManager _stateManager;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public async Task<Result<ComandaDto>> Handle(UpdateComandaStatusCommand request, CancellationToken cancellationToken)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(request.ComandaId);
            if (comanda == null)
                return Result<ComandaDto>.Failure($"Comanda {request.ComandaId} no encontrada");

            if (!_stateManager.IsValidTransition(comanda.Estado, request.NewStatus))
                return Result<ComandaDto>.Failure(_stateManager.GetTransitionError(comanda.Estado, request.NewStatus));

            var oldStatus = comanda.Estado;
            comanda.Estado = request.NewStatus;

            await _unitOfWork.Comandas.UpdateAsync(comanda);
            await _unitOfWork.CompleteAsync();
            
            var comandaDto = _mapper.Map<ComandaDto>(comanda);
            await _notificationService.NotifyComandaStatusChangedAsync(request.ComandaId, request.NewStatus);

            return Result<ComandaDto>.Success(comandaDto);
        }
    }
} 