using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.Exceptions;
using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Enums;
using MediatR;
using RestaurantePro.Core.Events;
using RestaurantePro.Core.DTOs.Plato;
using RestaurantePro.Core.Validators;
using System.Linq;

namespace RestaurantePro.Core.Services
{
    public class ComandaService : IComandaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ComandaStateManager _stateManager;
        private readonly IMediator _mediator;
        private readonly IUserContext _userContext;
        private readonly ComandaValidator _validator;

        public ComandaService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ComandaStateManager stateManager, IMediator mediator, IUserContext userContext, ComandaValidator validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _stateManager = stateManager;
            _mediator = mediator;
            _userContext = userContext;
            _validator = validator;
        }

        public async Task<ComandaDto> GetByIdAsync(int id)
        {
            var comanda = await _unitOfWork.Comandas.GetComandaWithDetallesAsync(id);
            return _mapper.Map<ComandaDto>(comanda);
        }

        public async Task<IEnumerable<ComandaDto>> GetAllAsync()
        {
            var comandas = await _unitOfWork.Comandas.ListAllAsync();
            return _mapper.Map<IEnumerable<ComandaDto>>(comandas);
        }

        public async Task<ComandaDto> CreateAsync(ComandaCreateDto comandaDto)
        {
            var comanda = _mapper.Map<Comanda>(comandaDto);
            comanda.Estado = EstadoComanda.Pendiente;
            comanda.FechaHora = DateTime.UtcNow;

            var validationResult = await _validator.ValidateAsync(comanda);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            var mesa = await _unitOfWork.Mesas.GetByIdAsync(comandaDto.MesaId);
            if (mesa == null)
                throw new NotFoundException($"Mesa con ID {comandaDto.MesaId} no encontrada");

            mesa.Estado = EstadoMesa.Ocupada;
            await _unitOfWork.Mesas.UpdateAsync(mesa);

            await _unitOfWork.Comandas.AddAsync(comanda);
            await _unitOfWork.CompleteAsync();

            var comandaCreatedDto = _mapper.Map<ComandaDto>(comanda);
            await _notificationService.NotifyComandaCreatedAsync(comandaCreatedDto);
            
            return comandaCreatedDto;
        }

        public async Task<IEnumerable<ComandaDto>> GetPendientesAsync()
        {
            var comandas = await _unitOfWork.Comandas.GetPendientesAsync();
            return _mapper.Map<IEnumerable<ComandaDto>>(comandas);
        }

        public async Task<ComandaDto> UpdateAsync(int id, ComandaUpdateDto comandaDto)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(id);
            if (comanda == null)
                throw new NotFoundException($"Comanda con ID {id} no encontrada");

            _mapper.Map(comandaDto, comanda);
            await _unitOfWork.Comandas.UpdateAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ComandaDto>(comanda);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(id);
            if (comanda == null)
                throw new NotFoundException($"Comanda con ID {id} no encontrada");

            await _unitOfWork.Comandas.DeleteAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<ComandaDto> AddDetalleAsync(int comandaId, ComandaDetalleCreateDto detalleDto)
        {
            var comanda = await _unitOfWork.Comandas.GetComandaWithDetallesAsync(comandaId);
            if (comanda == null)
                throw new NotFoundException($"Comanda {comandaId} no encontrada");

            var detalle = _mapper.Map<ComandaDetalle>(detalleDto);
            comanda.Detalles.Add(detalle);

            await _mediator.Publish(new ComandaDetalleAgregadoEvent(
                comandaId,
                _mapper.Map<ComandaDetalleDto>(detalle)));

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ComandaDto>(comanda);
        }

        public async Task<ComandaDto> UpdateEstadoAsync(int id, EstadoComanda nuevoEstado)
        {
            var comanda = await _unitOfWork.Comandas.GetByIdAsync(id);
            if (comanda == null)
                throw new NotFoundException($"Comanda {id} no encontrada");

            var estadoAnterior = comanda.Estado;
            comanda.Estado = nuevoEstado;

            await _mediator.Publish(new ComandaEstadoCambiadoEvent(
                id,
                estadoAnterior,
                nuevoEstado,
                _userContext.CurrentUser));

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ComandaDto>(comanda);
        }

        private async Task HandleStateEffects(Comanda comanda, EstadoComanda estadoAnterior, EstadoComanda nuevoEstado)
        {
            if (nuevoEstado == EstadoComanda.Cancelada)
            {
                var mesa = await _unitOfWork.Mesas.GetByIdAsync(comanda.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _unitOfWork.Mesas.UpdateAsync(mesa);
                }
            }
        }
    }
} 