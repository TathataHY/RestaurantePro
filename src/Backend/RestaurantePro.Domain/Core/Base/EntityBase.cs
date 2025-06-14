using RestaurantePro.Domain.Core.Base.Events;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Core.Base
{
    /// <summary>
    /// Clase base para todas las entidades del dominio
    /// </summary>
    public abstract class EntityBase
    {
        private List<DomainEvent> _domainEvents = new List<DomainEvent>();

        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Fecha de última actualización del registro
        /// </summary>
        public DateTime? FechaActualizacion { get; protected set; }

        /// <summary>
        /// Indica si el registro ha sido eliminado lógicamente
        /// </summary>
        public bool EstaEliminado { get; protected set; }

        /// <summary>
        /// Eventos de dominio pendientes de publicación
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Obtiene el nombre del contexto de dominio al que pertenece esta entidad
        /// </summary>
        public virtual string DomainContext => GetType().Namespace?.Split('.')[2] ?? "Core";

        /// <summary>
        /// Constructor base que asigna un nuevo ID a la entidad
        /// </summary>
        protected EntityBase()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Agrega un nuevo evento de dominio
        /// </summary>
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Limpia todos los eventos de dominio pendientes
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// Extrae todos los eventos pendientes y los devuelve, dejando la lista vacía
        /// </summary>
        /// <returns>Lista de eventos pendientes</returns>
        public IReadOnlyCollection<DomainEvent> ExtractDomainEvents()
        {
            var events = _domainEvents.ToArray();
            _domainEvents.Clear();
            return events;
        }

        /// <summary>
        /// Marca la entidad como actualizada
        /// </summary>
        protected void MarkAsModified()
        {
            FechaActualizacion = DateTime.Now;
        }

        /// <summary>
        /// Marca la entidad como eliminada lógicamente
        /// </summary>
        public virtual void MarkAsDeleted()
        {
            EstaEliminado = true;
            MarkAsModified();
        }

        /// <summary>
        /// Establece la fecha de creación (solo para pruebas)
        /// </summary>
        public void SetFechaCreacionForTesting(DateTime fecha)
        {
            FechaCreacion = fecha;
        }

        /// <summary>
        /// Establece el ID (solo para pruebas)
        /// </summary>
        protected internal void SetIdForTesting(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Compara dos entidades por su identidad, no por sus propiedades
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not EntityBase other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id == default || other.Id == default)
                return false;

            return Id == other.Id;
        }

        /// <summary>
        /// Obtiene el hash code basado en la identidad
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode() * 41;
        }

        /// <summary>
        /// Compara dos entidades por su identidad
        /// </summary>
        public static bool operator ==(EntityBase? left, EntityBase? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compara dos entidades por su identidad
        /// </summary>
        public static bool operator !=(EntityBase? left, EntityBase? right)
        {
            return !(left == right);
        }
    }
}
