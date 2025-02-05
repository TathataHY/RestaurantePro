using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Events;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Validators;
using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;
using FluentValidation;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Exceptions;
using RestaurantePro.Core.Common;

namespace RestaurantePro.Core.Handlers.CommandHandlers
{
    public class CreateComandaCommandHandler : IRequestHandler<CreateComandaCommand, Result<ComandaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IValidator<CreateComandaCommand> _validator;
        private readonly IMapper _mapper;

        public CreateComandaCommandHandler(
            IUnitOfWork unitOfWork, 
            IMediator mediator, 
            IValidator<CreateComandaCommand> validator, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<Result<ComandaDto>> Handle(CreateComandaCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return Result<ComandaDto>.Failure(validationResult.Errors.First().ErrorMessage);
            }

            var mesa = await _unitOfWork.Mesas.GetByIdAsync(command.MesaId);
            if (mesa == null)
                return Result<ComandaDto>.Failure($"Mesa con ID {command.MesaId} no encontrada");

            mesa.Estado = EstadoMesa.Ocupada;
            await _unitOfWork.Mesas.UpdateAsync(mesa);

            var comanda = command.ToEntity();
            
            await _unitOfWork.Comandas.AddAsync(comanda);
            await _unitOfWork.CompleteAsync();

            await _mediator.Publish(new ComandaCreatedEvent(comanda.Id, comanda.MesaId, comanda.FechaHora));
            
            return Result<ComandaDto>.Success(_mapper.Map<ComandaDto>(comanda));
        }
    }
} 