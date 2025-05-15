using System;

namespace RestaurantePro.Domain.Operaciones.Comandas.ActualizarEstadoComanda.Domain
{
    /// <summary>
    /// Enumeración de estados posibles de una comanda
    /// </summary>
    public enum EstadoComanda
    {
        Pendiente,
        EnPreparacion,
        Lista,
        Entregada,
        Pagada,
        Cancelada
    }
    
    /// <summary>
    /// Modelo de dominio para actualización de estado
    /// </summary>
    public class ActualizacionEstadoModel
    {
        public int ComandaId { get; }
        public EstadoComanda NuevoEstado { get; }
        public int UsuarioId { get; }
        public string Notas { get; }
        
        private ActualizacionEstadoModel(int comandaId, EstadoComanda nuevoEstado, int usuarioId, string notas)
        {
            ComandaId = comandaId;
            NuevoEstado = nuevoEstado;
            UsuarioId = usuarioId;
            Notas = notas;
        }
        
        public static ActualizacionEstadoModel Crear(int comandaId, EstadoComanda nuevoEstado, int usuarioId, string notas)
        {
            if (comandaId <= 0)
                throw new ArgumentException("Comanda inválida", nameof(comandaId));
                
            if (usuarioId <= 0)
                throw new ArgumentException("Usuario inválido", nameof(usuarioId));
                
            return new ActualizacionEstadoModel(comandaId, nuevoEstado, usuarioId, notas);
        }
    }
}
