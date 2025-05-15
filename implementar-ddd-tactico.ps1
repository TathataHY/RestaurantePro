# Script para implementar patrones tácticos de DDD

$moduloPath = "src\RestaurantePro.Domain\Comercial\Clientes"

# Crear estructura de carpetas para patrones tácticos de DDD
Write-Host "Creando estructura de carpetas DDD..."

$carpetasDDD = @(
    "ValueObjects",
    "Aggregates",
    "DomainEvents",
    "Specifications",
    "Services"
)

foreach ($carpeta in $carpetasDDD) {
    $path = Join-Path -Path $moduloPath -ChildPath $carpeta
    New-Item -Path $path -ItemType Directory -Force
    Write-Host "Creada carpeta: $path"
}

# Crear archivos de ejemplo para cada patrón
Write-Host "Creando archivos de ejemplo para cada patrón..."

# 1. Value Object
$clienteNombreVO = @"
using System;

namespace RestaurantePro.Domain.Comercial.Clientes.ValueObjects
{
    /// <summary>
    /// Value Object que representa el nombre de un cliente.
    /// Es inmutable y encapsula reglas de validación.
    /// </summary>
    public class ClienteNombre
    {
        public string Nombre { get; }
        public string Apellido { get; }

        private ClienteNombre(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }

        public static ClienteNombre Crear(string nombre, string apellido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));
            
            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException("El apellido no puede estar vacío", nameof(apellido));
            
            return new ClienteNombre(nombre, apellido);
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        // Value Objects se comparan por valor, no por referencia
        public override bool Equals(object obj)
        {
            if (obj is not ClienteNombre other)
                return false;
            
            return Nombre == other.Nombre && Apellido == other.Apellido;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nombre, Apellido);
        }
    }
}
"@
Set-Content -Path "$moduloPath\ValueObjects\ClienteNombre.cs" -Value $clienteNombreVO

# 2. Aggregate
$clienteAggregate = @"
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
"@
Set-Content -Path "$moduloPath\Aggregates\ClienteAggregate.cs" -Value $clienteAggregate

# 3. Domain Event
$domainEvents = @"
using System;

namespace RestaurantePro.Domain.Comercial.Clientes.DomainEvents
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un cliente
    /// </summary>
    public class ClienteCreadoEvent
    {
        public int ClienteId { get; }
        public string NombreCompleto { get; }
        public DateTime FechaCreacion { get; }

        public ClienteCreadoEvent(int clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
            FechaCreacion = DateTime.Now;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a un cliente
    /// </summary>
    public class PuntosAgregadosEvent
    {
        public int ClienteId { get; }
        public int PuntosAgregados { get; }
        public int PuntosTotales { get; }
        public DateTime Fecha { get; }

        public PuntosAgregadosEvent(int clienteId, int puntosAgregados, int puntosTotales)
        {
            ClienteId = clienteId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
            Fecha = DateTime.Now;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se desactiva un cliente
    /// </summary>
    public class ClienteDesactivadoEvent
    {
        public int ClienteId { get; }
        public string NombreCompleto { get; }
        public DateTime Fecha { get; }

        public ClienteDesactivadoEvent(int clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
            Fecha = DateTime.Now;
        }
    }
}
"@
Set-Content -Path "$moduloPath\DomainEvents\ClienteEvents.cs" -Value $domainEvents

# 4. Specification
$specification = @"
using System;
using System.Linq.Expressions;
using RestaurantePro.Domain.Comercial.Clientes.Aggregates;

namespace RestaurantePro.Domain.Comercial.Clientes.Specifications
{
    /// <summary>
    /// Interface base para especificaciones
    /// </summary>
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        Expression<Func<T, bool>> ToExpression();
    }

    /// <summary>
    /// Especificación para clientes activos con más de cierta cantidad de puntos
    /// </summary>
    public class ClientePreferencialSpecification : ISpecification<ClienteAggregate>
    {
        private readonly int _puntosMinimos;

        public ClientePreferencialSpecification(int puntosMinimos)
        {
            _puntosMinimos = puntosMinimos;
        }

        public bool IsSatisfiedBy(ClienteAggregate cliente)
        {
            return cliente.Activo && cliente.PuntosAcumulados >= _puntosMinimos;
        }

        public Expression<Func<ClienteAggregate, bool>> ToExpression()
        {
            return cliente => cliente.Activo && cliente.PuntosAcumulados >= _puntosMinimos;
        }
    }

    /// <summary>
    /// Especificación para clientes elegibles para promociones especiales
    /// </summary>
    public class ClienteElegiblePromocionSpecification : ISpecification<ClienteAggregate>
    {
        private readonly DateTime _fechaLimite;

        public ClienteElegiblePromocionSpecification(int diasAntiguedad)
        {
            _fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);
        }

        public bool IsSatisfiedBy(ClienteAggregate cliente)
        {
            return cliente.Activo && cliente.FechaCreacion <= _fechaLimite;
        }

        public Expression<Func<ClienteAggregate, bool>> ToExpression()
        {
            return cliente => cliente.Activo && cliente.FechaCreacion <= _fechaLimite;
        }
    }
}
"@
Set-Content -Path "$moduloPath\Specifications\ClienteSpecifications.cs" -Value $specification

# 5. Domain Service
$domainService = @"
using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Comercial.Clientes.Aggregates;
using RestaurantePro.Domain.Comercial.Clientes.Specifications;

namespace RestaurantePro.Domain.Comercial.Clientes.Services
{
    /// <summary>
    /// Servicio de dominio para operaciones que involucran múltiples agregados
    /// o que no pertenecen naturalmente a ninguna entidad.
    /// </summary>
    public class ClienteFidelizacionService
    {
        /// <summary>
        /// Calcula nivel de fidelización basado en patrones de compra y puntos
        /// </summary>
        public string CalcularNivelFidelizacion(ClienteAggregate cliente, List<DateTime> fechasCompras, decimal totalCompras)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (!cliente.Activo)
                return "Inactivo";

            // Calcular frecuencia de compras
            var comprasPorMes = CalcularFrecuenciaCompras(fechasCompras);
            
            // Aplicar reglas de negocio
            if (cliente.PuntosAcumulados >= 1000 && comprasPorMes >= 4 && totalCompras >= 5000)
                return "Platinum";
            
            if (cliente.PuntosAcumulados >= 500 && comprasPorMes >= 2 && totalCompras >= 2000)
                return "Gold";
            
            if (cliente.PuntosAcumulados >= 200 && comprasPorMes >= 1)
                return "Silver";
            
            return "Standard";
        }

        /// <summary>
        /// Verifica si un cliente califica para promociones específicas basadas en especificaciones
        /// </summary>
        public IEnumerable<string> ObtenerPromocionesDisponibles(ClienteAggregate cliente)
        {
            var promociones = new List<string>();
            
            // Evaluar diferentes especificaciones
            var especClientePreferencial = new ClientePreferencialSpecification(500);
            if (especClientePreferencial.IsSatisfiedBy(cliente))
            {
                promociones.Add("Descuento Preferencial 10%");
            }
            
            var especClienteAntiguo = new ClienteElegiblePromocionSpecification(90);
            if (especClienteAntiguo.IsSatisfiedBy(cliente))
            {
                promociones.Add("Promoción Cliente Fiel");
            }
            
            // Reglas adicionales
            if (cliente.PuntosAcumulados >= 1000)
            {
                promociones.Add("Premio Milestone 1000 puntos");
            }
            
            return promociones;
        }
        
        /// <summary>
        /// Método auxiliar para calcular frecuencia de compras
        /// </summary>
        private double CalcularFrecuenciaCompras(List<DateTime> fechasCompras)
        {
            if (fechasCompras == null || fechasCompras.Count == 0)
                return 0;
            
            // Tomar las compras de los últimos 6 meses
            var fechaLimite = DateTime.Now.AddMonths(-6);
            var comprasRecientes = fechasCompras.FindAll(f => f >= fechaLimite);
            
            return (double)comprasRecientes.Count / 6;
        }
    }
}
"@
Set-Content -Path "$moduloPath\Services\ClienteFidelizacionService.cs" -Value $domainService

# Crear README específico para el módulo
$readmeClientes = @"
# Módulo Clientes

Este módulo implementa patrones tácticos de Domain-Driven Design (DDD) para gestionar toda la lógica relacionada con clientes y fidelización.

## Estructura de carpetas

- **Entities/**: Entidades tradicionales (Cliente, TarjetaFidelizacion, etc.)
- **ValueObjects/**: Objetos de valor inmutables (ClienteNombre)
- **Aggregates/**: Raíces de agregados que garantizan consistencia (ClienteAggregate)
- **DomainEvents/**: Eventos de dominio para comunicación entre módulos
- **Specifications/**: Encapsulación de reglas de negocio reutilizables
- **Services/**: Servicios que operan sobre varios agregados
- **Enums/**: Enumeraciones relacionadas con clientes
- **Interfaces/**: Interfaces para repositorios y servicios

## Patrones implementados

### ValueObject
Objetos inmutables identificados por el valor de sus atributos, no por identidad.
Ejemplo: \`ClienteNombre\` encapsula nombre y apellido con sus validaciones.

### Aggregate
Agrupación de entidades y objetos de valor que mantiene consistencia.
Ejemplo: \`ClienteAggregate\` controla todas las operaciones sobre un cliente.

### Domain Events
Notificaciones sobre cambios importantes en el dominio.
Ejemplos: \`ClienteCreadoEvent\`, \`PuntosAgregadosEvent\`

### Specification
Encapsula reglas de negocio reutilizables y combinables.
Ejemplo: \`ClientePreferencialSpecification\` determina si un cliente es preferencial.

### Domain Service
Operaciones de dominio que no pertenecen a una sola entidad.
Ejemplo: \`ClienteFidelizacionService\` calcula nivel de fidelización basado en múltiples factores.
"@
Set-Content -Path "$moduloPath\README.md" -Value $readmeClientes

Write-Host "Patrones tácticos de DDD implementados correctamente en el módulo Clientes." 