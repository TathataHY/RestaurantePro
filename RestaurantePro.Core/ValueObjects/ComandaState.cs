using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.ValueObjects
{
    public class ComandaState
    {
        public EstadoComanda Value { get; private set; }

        private ComandaState(EstadoComanda estado)
        {
            Value = estado;
        }

        public static ComandaState From(EstadoComanda estado)
        {
            return new ComandaState(estado);
        }

        public bool CanTransitionTo(EstadoComanda nuevoEstado)
        {
            return Value switch
            {
                EstadoComanda.Pendiente => nuevoEstado == EstadoComanda.EnPreparacion,
                EstadoComanda.EnPreparacion => nuevoEstado == EstadoComanda.Lista,
                EstadoComanda.Lista => nuevoEstado == EstadoComanda.Entregada,
                EstadoComanda.Entregada => nuevoEstado == EstadoComanda.Cancelada,
                _ => false
            };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
} 