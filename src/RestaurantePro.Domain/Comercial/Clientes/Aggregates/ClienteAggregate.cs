using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.DomainEvents;
using RestaurantePro.Domain.Core.Base.Entities;

namespace RestaurantePro.Domain.Comercial.Clientes.Aggregates
{
    /// <summary>
    /// Aggregate Root para Cliente.
    /// Garantiza la consistencia e integridad de todas las entidades
    /// y value objects relacionados con un cliente.
    /// </summary>
    public class ClienteAggregate : BaseEntity
    {
        // Value Objects
        public ClienteNombre Nombre { get; private set; }
        public string Email { get; private set; }
        public string Telefono { get; private set; }
        
        // Colección de entidades internas al agregado
        private readonly List<HistorialPuntos> _historialPuntos = new();
        public IReadOnlyCollection<HistorialPuntos> HistorialPuntos => _historialPuntos.AsReadOnly();

        // Estado del cliente
        public bool Activo { get; private set; }
        public int PuntosAcumulados { get; private set; }
        
        // Constructor para entidad existente
        private ClienteAggregate(int id, ClienteNombre nombre, string email, string telefono, bool activo, int puntosAcumulados)
        {
            Id = id;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
            Activo = activo;
            PuntosAcumulados = puntosAcumulados;
        }
        
        // Factory method
        public static ClienteAggregate Crear(ClienteNombre nombre, string email, string telefono)
        {
            var cliente = new ClienteAggregate(0, nombre, email, telefono, true, 0);
            
            // Lanzar evento de dominio
            cliente.AddDomainEvent(new ClienteCreadoEvent(cliente.Id, nombre.NombreCompleto));
            
            return cliente;
        }
        
        // Métodos de negocio
        public void AgregarPuntos(int puntos, string concepto)
        {
            if (puntos <= 0)
                throw new ArgumentException("Los puntos deben ser mayores a cero", nameof(puntos));
            
            if (!Activo)
                throw new InvalidOperationException("No se pueden agregar puntos a un cliente inactivo");
            
            // Crear movimiento
            var movimiento = new HistorialPuntos
            {
                ClienteId = Id,
                Puntos = puntos,
                Concepto = concepto,
                Fecha = DateTime.Now
            };
            
            // Aplicar al estado
            PuntosAcumulados += puntos;
            _historialPuntos.Add(movimiento);
            
            // Lanzar evento de dominio
            AddDomainEvent(new PuntosAgregadosEvent(Id, puntos, PuntosAcumulados));
        }
        
        public void DesactivarCliente()
        {
            if (!Activo)
                return;
            
            Activo = false;
            AddDomainEvent(new ClienteDesactivadoEvent(Id, Nombre.NombreCompleto));
        }
        
        // Métodos para agregar/eliminar eventos de dominio
        private readonly List<object> _domainEvents = new();
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
        
        public void AddDomainEvent(object domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
