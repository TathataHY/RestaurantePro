using System;
using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza una preparación diaria
    /// </summary>
    public class PreparacionDiariaActualizada : DomainEvent
    {
        public Guid PreparacionId { get; }
        public Guid ProductoId { get; }
        public int CantidadPreparada { get; }
        public int CantidadDisponible { get; }
        public Guid ChefId { get; }
        public DateTime FechaVencimiento { get; }

        public PreparacionDiariaActualizada(
            Guid preparacionId,
            Guid productoId,
            int cantidadPreparada,
            int cantidadDisponible,
            Guid chefId,
            DateTime fechaVencimiento)
        {
            PreparacionId = preparacionId;
            ProductoId = productoId;
            CantidadPreparada = cantidadPreparada;
            CantidadDisponible = cantidadDisponible;
            ChefId = chefId;
            FechaVencimiento = fechaVencimiento;
        }
    }
} 