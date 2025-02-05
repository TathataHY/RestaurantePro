using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Core.Common;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Specifications;
using RestaurantePro.Core.ValueObjects;

namespace RestaurantePro.Core.Services
{
    public class ComandaStateService
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IPlatoRepository _platoRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ComandaStateService> _logger;

        public ComandaStateService(
            IComandaRepository comandaRepository,
            IMesaRepository mesaRepository,
            IPlatoRepository platoRepository,
            INotificationService notificationService,
            ILogger<ComandaStateService> logger)
        {
            _comandaRepository = comandaRepository;
            _mesaRepository = mesaRepository;
            _platoRepository = platoRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result> UpdateComandaStateAsync(int comandaId, EstadoComanda newStatus)
        {
            var comanda = await _comandaRepository.GetByIdAsync(comandaId);
            if (comanda == null)
                return Result.Failure($"Comanda {comandaId} no encontrada");

            if (!IsValidStateTransition(comanda.Estado, newStatus))
                return Result.Failure($"Transición de estado inválida: {comanda.Estado} -> {newStatus}");

            var oldStatus = comanda.Estado;
            comanda.Estado = newStatus;

            await HandleStateChangeEffects(comanda, oldStatus, newStatus);
            await _comandaRepository.UpdateAsync(comanda);
            await _notificationService.NotifyComandaStatusChangedAsync(comandaId, newStatus);

            _logger.LogInformation($"Comanda {comandaId} cambió de estado: {oldStatus} -> {newStatus}");
            return Result.Success();
        }

        private bool IsValidStateTransition(EstadoComanda currentState, EstadoComanda newState)
        {
            return (currentState, newState) switch
            {
                (EstadoComanda.Pendiente, EstadoComanda.EnPreparacion) => true,
                (EstadoComanda.EnPreparacion, EstadoComanda.Lista) => true,
                (EstadoComanda.Lista, EstadoComanda.Cancelada) => true,
                _ => false
            };
        }

        private async Task HandleStateChangeEffects(Comanda comanda, EstadoComanda oldStatus, EstadoComanda newStatus)
        {
            if (newStatus == EstadoComanda.Cancelada)
            {
                // Actualizar estado de la mesa
                var mesa = await _mesaRepository.GetByIdAsync(comanda.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _mesaRepository.UpdateAsync(mesa);
                }

                // Calcular total final
                comanda.Total = comanda.Detalles.Sum(d => d.Subtotal);
            }
        }
    }
}