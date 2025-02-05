using System;
using System.Collections.Generic;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Services
{
    public class ComandaStateManager
    {
        private readonly Dictionary<EstadoComanda, EstadoComanda[]> _validTransitions;
        private readonly IUnitOfWork _unitOfWork;

        public ComandaStateManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _validTransitions = new Dictionary<EstadoComanda, EstadoComanda[]>
        {
            { EstadoComanda.Pendiente, new[] { EstadoComanda.EnPreparacion, EstadoComanda.Anulada } },
            { EstadoComanda.EnPreparacion, new[] { EstadoComanda.Lista, EstadoComanda.Anulada } },
            { EstadoComanda.Lista, new[] { EstadoComanda.Entregada } },
            { EstadoComanda.Entregada, new[] { EstadoComanda.Cancelada } }
        };
        }

        public bool IsValidTransition(EstadoComanda currentState, EstadoComanda newState)
        {
            return _validTransitions.ContainsKey(currentState) &&
                   _validTransitions[currentState].Contains(newState);
        }

        public async Task HandleStateEffects(Comanda comanda, EstadoComanda oldStatus, EstadoComanda newStatus)
        {
            if (newStatus == EstadoComanda.Cancelada)
            {
                var mesa = await _unitOfWork.Mesas.GetByIdAsync(comanda.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _unitOfWork.Mesas.UpdateAsync(mesa);
                }
            }
        }

        public string GetTransitionError(EstadoComanda currentState, EstadoComanda newState)
        {
            return $"No se puede cambiar el estado de {currentState} a {newState}";
        }
    }
}