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

namespace RestaurantePro.Core.Services
{
    public class ComandaService : IComandaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ComandaService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            var mesa = await _unitOfWork.Mesas.GetByIdAsync(comandaDto.MesaId);
            if (mesa == null)
                throw new NotFoundException($"Mesa con ID {comandaDto.MesaId} no encontrada");

            mesa.Estado = EstadoMesa.Ocupada;
            await _unitOfWork.Mesas.UpdateAsync(mesa);

            await _unitOfWork.Comandas.AddAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ComandaDto>(comanda);
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
                throw new NotFoundException($"Comanda con ID {comandaId} no encontrada");

            var detalle = _mapper.Map<ComandaDetalle>(detalleDto);
            comanda.Detalles.Add(detalle);

            await _unitOfWork.Comandas.UpdateAsync(comanda);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ComandaDto>(comanda);
        }
    }
} 