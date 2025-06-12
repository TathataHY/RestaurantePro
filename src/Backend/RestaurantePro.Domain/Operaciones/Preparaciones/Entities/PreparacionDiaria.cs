using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Events;
using RestaurantePro.Domain.Core.Base.Testing;
using System;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Entities
{
    /// <summary>
    /// Representa una preparación diaria de un producto en el restaurante
    /// </summary>
    public class PreparacionDiaria : EntityBase
    {
        /// <summary>
        /// ID del producto que se preparó
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid ChefId { get; private set; }

        /// <summary>
        /// Cantidad inicial que se preparó
        /// </summary>
        public int CantidadPreparada { get; private set; }

        /// <summary>
        /// Cantidad disponible actualmente
        /// </summary>
        public int CantidadDisponible { get; private set; }

        /// <summary>
        /// Fecha de vencimiento de la preparación
        /// </summary>
        public DateTime FechaVencimiento { get; private set; }

        /// <summary>
        /// Observaciones adicionales sobre la preparación
        /// </summary>
        public string Observaciones { get; private set; }

        /// <summary>
        /// Fecha y hora en que se realizó la preparación
        /// </summary>
        public DateTime FechaPreparacion { get; private set; }

        /// <summary>
        /// Estado actual de la preparación
        /// </summary>
        public EstadoPreparacion Estado { get; private set; }

        /// <summary>
        /// Constructor sin parámetros requerido para EF Core
        /// </summary>
        public PreparacionDiaria()
        {
            Observaciones = null;
        }

        /// <summary>
        /// Método factory para crear una nueva preparación
        /// </summary>
        public static PreparacionDiaria Crear(
            Guid productoId,
            int cantidad,
            Guid chefId,
            DateTime fechaVencimiento,
            string observaciones = "",
            DateTime? fechaPreparacion = null)
        {
            if (productoId == Guid.Empty)
                throw new ArgumentException("El ID del producto no puede estar vacío", nameof(productoId));
                
            if (chefId == Guid.Empty)
                throw new ArgumentException("El ID del chef no puede estar vacío", nameof(chefId));
                
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            // Validamos que la fecha sea futura (solo en entorno de producción)
            if (!TestEnvironment.IsTestEnvironment && fechaVencimiento <= DateTime.Now)
                throw new ArgumentException("La fecha de vencimiento debe ser futura", nameof(fechaVencimiento));

            var preparacion = new PreparacionDiaria
            {
                ProductoId = productoId,
                ChefId = chefId,
                CantidadPreparada = cantidad,
                CantidadDisponible = cantidad,
                FechaVencimiento = fechaVencimiento,
                Observaciones = observaciones?.Trim() ?? string.Empty,
                FechaPreparacion = fechaPreparacion ?? DateTime.Now,
                Estado = EstadoPreparacion.Preparando
            };

            preparacion.AddDomainEvent(new PreparacionCreada(preparacion.Id, productoId, cantidad, chefId));
            return preparacion;
        }

        /// <summary>
        /// Marca la preparación como disponible para consumo
        /// </summary>
        public void MarcarComoDisponible()
        {
            if (Estado == EstadoPreparacion.Vencida)
                throw new InvalidOperationException("No se puede marcar como disponible una preparación vencida");

            Estado = EstadoPreparacion.Disponible;
            AddDomainEvent(new PreparacionDisponible(Id, ProductoId));
        }

        /// <summary>
        /// Marca la preparación como por vencer (próxima a su fecha de vencimiento)
        /// </summary>
        public void MarcarComoPorVencer()
        {
            if (Estado == EstadoPreparacion.Vencida)
                throw new InvalidOperationException("La preparación ya está vencida");

            if (Estado == EstadoPreparacion.Agotada)
                throw new InvalidOperationException("No se puede marcar como por vencer una preparación agotada");

            Estado = EstadoPreparacion.PorVencer;
            AddDomainEvent(new PreparacionPorVencer(Id, ProductoId, FechaVencimiento));
        }

        /// <summary>
        /// Marca la preparación como vencida (pasada su fecha de vencimiento)
        /// </summary>
        public void MarcarComoVencida()
        {
            if (Estado == EstadoPreparacion.Agotada)
                throw new InvalidOperationException("No se puede marcar como vencida una preparación agotada");

            Estado = EstadoPreparacion.Vencida;
            AddDomainEvent(new PreparacionVencida(Id, ProductoId, CantidadDisponible));
        }

        /// <summary>
        /// Consume una cantidad específica de la preparación
        /// </summary>
        public void ConsumirCantidad(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a consumir debe ser mayor a cero", nameof(cantidad));

            if (Estado == EstadoPreparacion.Vencida)
                throw new InvalidOperationException("No se puede consumir una preparación vencida");
                
            if (Estado == EstadoPreparacion.Agotada)
                throw new InvalidOperationException("No se puede consumir una preparación agotada");

            if (cantidad > CantidadDisponible)
                throw new InvalidOperationException($"No hay suficiente cantidad disponible. Solicitado: {cantidad}, Disponible: {CantidadDisponible}");

            CantidadDisponible -= cantidad;

            if (CantidadDisponible == 0)
            {
                Estado = EstadoPreparacion.Agotada;
                AddDomainEvent(new PreparacionAgotada(Id, ProductoId));
            }

            AddDomainEvent(new PreparacionConsumida(Id, ProductoId, cantidad, CantidadDisponible));
        }

        /// <summary>
        /// Agrega más cantidad a la preparación existente
        /// </summary>
        public void AgregarCantidad(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a agregar debe ser mayor a cero", nameof(cantidad));

            if (Estado == EstadoPreparacion.Vencida)
                throw new InvalidOperationException("No se puede agregar cantidad a una preparación vencida");

            int cantidadAnterior = CantidadDisponible;
            CantidadPreparada += cantidad;
            CantidadDisponible += cantidad;

            // Si estaba agotada y ahora tiene disponibilidad, cambiar estado
            if (Estado == EstadoPreparacion.Agotada && CantidadDisponible > 0)
            {
                Estado = EstadoPreparacion.Disponible;
            }

            AddDomainEvent(new CantidadAgregadaPreparacion(Id, ProductoId, cantidad, cantidadAnterior, CantidadDisponible));
        }

        /// <summary>
        /// Método para pruebas: establece el ID directamente
        /// </summary>
        public new void SetIdForTesting(Guid id)
        {
            Id = id;
        }
    }
}