# Script para implementar Bounded Contexts en el Core del sistema

$corePath = "src\RestaurantePro.Domain\Core"

# Crear estructura de carpetas para Bounded Contexts
Write-Host "Creando estructura de carpetas para Bounded Contexts..."

# 1. Crear SharedKernel
$sharedKernelPath = Join-Path -Path $corePath -ChildPath "SharedKernel"
New-Item -Path $sharedKernelPath -ItemType Directory -Force
Write-Host "Creado directorio: $sharedKernelPath"

# Crear subcarpetas en SharedKernel
New-Item -Path "$sharedKernelPath\ValueObjects" -ItemType Directory -Force
New-Item -Path "$sharedKernelPath\Interfaces" -ItemType Directory -Force
New-Item -Path "$sharedKernelPath\Services" -ItemType Directory -Force
New-Item -Path "$sharedKernelPath\Exceptions" -ItemType Directory -Force

# 2. Crear directorio BoundedContexts
$boundedContextsPath = Join-Path -Path $corePath -ChildPath "BoundedContexts"
New-Item -Path $boundedContextsPath -ItemType Directory -Force
Write-Host "Creado directorio: $boundedContextsPath"

# Crear algunos ejemplos de Bounded Contexts
New-Item -Path "$boundedContextsPath\Catalogo" -ItemType Directory -Force
New-Item -Path "$boundedContextsPath\Identidad" -ItemType Directory -Force
New-Item -Path "$boundedContextsPath\ContextMap" -ItemType Directory -Force

# Crear archivos de ejemplo para SharedKernel
Write-Host "Creando archivos de ejemplo para SharedKernel..."

# 1. Shared ValueObjects
$moneyValueObject = @"
using System;
using System.Globalization;

namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// ValueObject para representar dinero de forma consistente en todo el sistema
    /// </summary>
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        
        public static Money FromDecimal(decimal amount, string currency = "MXN")
        {
            // Validación de importe
            if (amount < 0)
                throw new ArgumentException("El importe no puede ser negativo", nameof(amount));
                
            // Validación de moneda
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("La moneda es requerida", nameof(currency));
                
            if (currency.Length != 3)
                throw new ArgumentException("El código de moneda debe tener 3 caracteres", nameof(currency));
            
            return new Money(
                Math.Round(amount, 2, MidpointRounding.AwayFromZero), 
                currency.ToUpperInvariant());
        }
        
        public static Money Zero(string currency = "MXN")
        {
            return FromDecimal(0, currency);
        }
        
        // Operaciones básicas
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return FromDecimal(Amount + other.Amount, Currency);
        }
        
        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return FromDecimal(Amount - other.Amount, Currency);
        }
        
        public Money Multiply(decimal factor)
        {
            return FromDecimal(Amount * factor, Currency);
        }
        
        // Validación de misma moneda
        private void EnsureSameCurrency(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            if (Currency != other.Currency)
                throw new InvalidOperationException($"No se pueden realizar operaciones entre monedas diferentes: {Currency} y {other.Currency}");
        }
        
        // Representación como cadena
        public override string ToString()
        {
            return Amount.ToString("C", CultureInfo.CurrentCulture) + " " + Currency;
        }
        
        // Igualdad y hash
        public override bool Equals(object obj)
        {
            if (obj is not Money other)
                return false;
                
            return Amount == other.Amount && Currency == other.Currency;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }
        
        // Operadores
        public static Money operator +(Money left, Money right)
        {
            return left.Add(right);
        }
        
        public static Money operator -(Money left, Money right)
        {
            return left.Subtract(right);
        }
        
        public static Money operator *(Money left, decimal right)
        {
            return left.Multiply(right);
        }
        
        public static bool operator ==(Money left, Money right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
                
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
                
            return left.Equals(right);
        }
        
        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }
    }
}
"@
Set-Content -Path "$sharedKernelPath\ValueObjects\Money.cs" -Value $moneyValueObject

# 2. Shared Interfaces
$unitOfWorkInterface = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz para implementar el patrón Unit of Work para transacciones
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deshace la transacción actual
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda todos los cambios en la base de datos
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Interfaz genérica para repositorios de entidades
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> spec);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(ISpecification<T> spec);
    }
    
    /// <summary>
    /// Interfaz para especificaciones genéricas
    /// </summary>
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
    }
    
    /// <summary>
    /// Interfaz para el servicio de correo
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    }
    
    /// <summary>
    /// Interfaz para el servicio de notificaciones
    /// </summary>
    public interface INotificationService
    {
        Task NotifyAsync(string userId, string message, string type = "info");
    }
}
"@
Set-Content -Path "$sharedKernelPath\Interfaces\IUnitOfWork.cs" -Value $unitOfWorkInterface

# Crear Context Mapping para documentar las relaciones
$contextMap = @"
using System.Collections.Generic;

namespace RestaurantePro.Domain.Core.BoundedContexts.ContextMap
{
    /// <summary>
    /// Enumera los tipos de relación entre bounded contexts
    /// </summary>
    public enum ContextRelationship
    {
        /// <summary>
        /// Un equipo upstream impone su modelo al equipo downstream
        /// </summary>
        ConformistUpstream,
        
        /// <summary>
        /// Un equipo downstream se adapta al modelo del equipo upstream
        /// </summary>
        ConformistDownstream,
        
        /// <summary>
        /// Relación de cooperación donde ambos equipos definen un modelo conjunto
        /// </summary>
        Partnership,
        
        /// <summary>
        /// Equipo downstream crea una capa anti-corrupción para traducir el modelo upstream
        /// </summary>
        AnticorruptionLayer,
        
        /// <summary>
        /// Dos contextos comparten un subconjunto del modelo de dominio
        /// </summary>
        SharedKernel,
        
        /// <summary>
        /// Un equipo proporciona servicio a otro equipo
        /// </summary>
        CustomerSupplier,
        
        /// <summary>
        /// Contextos separados sin comunicación directa
        /// </summary>
        SeparateWays
    }
    
    /// <summary>
    /// Define un Bounded Context en el sistema
    /// </summary>
    public class BoundedContext
    {
        public string Name { get; }
        public string Description { get; }
        public string ResponsibleTeam { get; }
        public List<string> MainEntities { get; }
        
        public BoundedContext(string name, string description, string responsibleTeam, List<string> mainEntities)
        {
            Name = name;
            Description = description;
            ResponsibleTeam = responsibleTeam;
            MainEntities = mainEntities;
        }
    }
    
    /// <summary>
    /// Define una relación entre dos Bounded Contexts
    /// </summary>
    public class ContextRelation
    {
        public BoundedContext Upstream { get; }
        public BoundedContext Downstream { get; }
        public ContextRelationship Relationship { get; }
        public string Description { get; }
        
        public ContextRelation(
            BoundedContext upstream, 
            BoundedContext downstream, 
            ContextRelationship relationship, 
            string description)
        {
            Upstream = upstream;
            Downstream = downstream;
            Relationship = relationship;
            Description = description;
        }
    }
    
    /// <summary>
    /// Mapa de contextos completo del sistema
    /// </summary>
    public static class RestauranteProContextMap
    {
        // Definición de Bounded Contexts
        public static readonly BoundedContext CatalogoContext = new BoundedContext(
            "Catálogo",
            "Gestión de productos, categorías e ingredientes",
            "Equipo Productos",
            new List<string> { "Producto", "Categoria", "Ingrediente" }
        );
        
        public static readonly BoundedContext OperacionesContext = new BoundedContext(
            "Operaciones",
            "Gestión de comandas, mesas y reservaciones",
            "Equipo Operaciones",
            new List<string> { "Comanda", "Mesa", "Reservacion" }
        );
        
        public static readonly BoundedContext InventarioContext = new BoundedContext(
            "Inventario",
            "Control de stock, movimientos y compras",
            "Equipo Inventario",
            new List<string> { "Inventario", "MovimientoInventario", "OrdenCompra" }
        );
        
        public static readonly BoundedContext ClientesContext = new BoundedContext(
            "Clientes",
            "Gestión de clientes, fidelización y promociones",
            "Equipo Comercial",
            new List<string> { "Cliente", "TarjetaFidelizacion", "Promocion" }
        );
        
        public static readonly BoundedContext ProveedoresContext = new BoundedContext(
            "Proveedores",
            "Gestión de proveedores",
            "Equipo Compras",
            new List<string> { "Proveedor", "ProveedorCategoria" }
        );
        
        public static readonly BoundedContext PagosContext = new BoundedContext(
            "Pagos",
            "Gestión de pagos y transacciones financieras",
            "Equipo Finanzas",
            new List<string> { "Pago", "Transaccion" }
        );
        
        public static readonly BoundedContext IdentidadContext = new BoundedContext(
            "Identidad",
            "Gestión de usuarios, roles y permisos",
            "Equipo Seguridad",
            new List<string> { "Usuario", "Rol", "Permiso" }
        );
        
        // Definición de relaciones entre contextos
        public static readonly List<ContextRelation> ContextRelations = new List<ContextRelation>
        {
            new ContextRelation(
                CatalogoContext,
                OperacionesContext,
                ContextRelationship.CustomerSupplier,
                "Operaciones consume productos del Catálogo para crear comandas"
            ),
            
            new ContextRelation(
                InventarioContext,
                CatalogoContext,
                ContextRelationship.ConformistDownstream,
                "Inventario se adapta al modelo de productos definido por Catálogo"
            ),
            
            new ContextRelation(
                OperacionesContext,
                PagosContext,
                ContextRelationship.CustomerSupplier,
                "Pagos procesa transacciones para las comandas de Operaciones"
            ),
            
            new ContextRelation(
                ProveedoresContext,
                InventarioContext,
                ContextRelationship.Partnership,
                "Colaboración para gestionar la compra y recepción de productos"
            ),
            
            new ContextRelation(
                ClientesContext,
                OperacionesContext,
                ContextRelationship.AnticorruptionLayer,
                "Clientes usa una capa de traducción para consumir datos de Operaciones"
            ),
            
            new ContextRelation(
                IdentidadContext,
                OperacionesContext,
                ContextRelationship.ConformistUpstream,
                "Identidad proporciona usuarios y roles a Operaciones"
            )
        };
    }
}
"@
Set-Content -Path "$boundedContextsPath\ContextMap\RestauranteProContextMap.cs" -Value $contextMap

# Crear archivo de configuración para documentar los elementos compartidos y los límites
$boundedContextConfig = @"
using System.Collections.Generic;

namespace RestaurantePro.Domain.Core.BoundedContexts
{
    /// <summary>
    /// Configuración y documentación de los elementos compartidos y límites entre contextos
    /// </summary>
    public static class BoundedContextsConfiguration
    {
        /// <summary>
        /// Elementos que son compartidos entre todos los bounded contexts (SharedKernel)
        /// </summary>
        public static readonly List<string> SharedKernelElements = new List<string>
        {
            // ValueObjects
            "Money",
            "Email",
            "PhoneNumber",
            "Address",
            
            // Interfaces
            "IRepository",
            "IUnitOfWork",
            "ISpecification",
            "IEmailService",
            "INotificationService",
            
            // Excepciones
            "DomainException",
            "ValidationException",
            "AuthorizationException",
            
            // Entidades base
            "BaseEntity",
            "AuditableEntity"
        };
        
        /// <summary>
        /// Describe cómo un bounded context traduce conceptos de otro bounded context
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, string>> TranslationMappings = 
            new Dictionary<string, Dictionary<string, string>>
        {
            // Cómo Inventario traduce conceptos de Catálogo
            ["Inventario-Catalogo"] = new Dictionary<string, string>
            {
                ["Producto"] = "ItemInventario",
                ["Ingrediente"] = "ComponenteInventario"
            },
            
            // Cómo Clientes traduce conceptos de Operaciones
            ["Clientes-Operaciones"] = new Dictionary<string, string>
            {
                ["Comanda"] = "CompraCliente",
                ["ComandaDetalle"] = "ItemCompra"
            }
        };
        
        /// <summary>
        /// Define los límites explícitos entre contextos - qué operaciones cruzan fronteras
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextBoundaries = 
            new Dictionary<string, List<string>>
        {
            // Integraciones permitidas entre Operaciones y Catálogo
            ["Operaciones-Catalogo"] = new List<string>
            {
                "ObtenerProductoPorId",
                "VerificarDisponibilidadProducto",
                "ObtenerPrecioProducto"
            },
            
            // Integraciones permitidas entre Operaciones e Inventario
            ["Operaciones-Inventario"] = new List<string>
            {
                "ReservarInventario",
                "ConfirmarConsumoInventario",
                "LiberarReservaInventario"
            },
            
            // Integraciones permitidas entre Operaciones y Pagos
            ["Operaciones-Pagos"] = new List<string>
            {
                "CrearPagoComanda",
                "ConsultarEstadoPago"
            }
        };
    }
}
"@
Set-Content -Path "$boundedContextsPath\BoundedContextsConfiguration.cs" -Value $boundedContextConfig

# Crear README específico para Bounded Contexts
$readmeBoundedContexts = @"
# Core - Bounded Contexts y SharedKernel

Esta parte del dominio implementa el concepto de Bounded Contexts (Contextos Delimitados) de Domain-Driven Design (DDD), facilitando la clara separación entre diferentes modelos de dominio y sus límites.

## Estructura de carpetas

```
Core/
├── SharedKernel/               # Elementos compartidos entre todos los contextos
│   ├── ValueObjects/           # ValueObjects genéricos (Money, Email, etc.)
│   ├── Interfaces/             # Interfaces compartidas (IRepository, etc.)
│   ├── Services/               # Servicios base compartidos
│   └── Exceptions/             # Excepciones comunes
│
├── BoundedContexts/            # Definición de contextos y sus límites
│   ├── Catalogo/               # Contexto de Catálogo de Productos
│   ├── Identidad/              # Contexto de Gestión de Identidad
│   └── ContextMap/             # Mapeo de relaciones entre contextos
```

## Conceptos implementados

### SharedKernel
Conjunto mínimo de conceptos, interfaces y entidades que son comunes a todos los Bounded Contexts del sistema. Estos elementos son utilizados sin traducción por todos los contextos.

### Bounded Contexts
Fronteras explícitas alrededor de un modelo de dominio, dentro de las cuales los términos y conceptos tienen un significado específico y consistente.

### Context Map
Documentación de las relaciones entre diferentes Bounded Contexts, incluidos patrones como:
- **Conformist**: Un contexto se adapta al modelo de otro
- **Anti-corruption Layer**: Capa de traducción entre contextos
- **Partnership**: Colaboración en la definición del modelo
- **Customer-Supplier**: Relación de consumo de servicios

## Beneficios

1. **Claridad conceptual**: Cada equipo puede trabajar con su propio modelo sin confusiones
2. **Flexibilidad**: Permite evolucionar partes del sistema de forma independiente
3. **Escalabilidad organizacional**: Diferentes equipos pueden trabajar en diferentes contextos
4. **Consistencia local**: Cada contexto mantiene consistencia dentro de sus límites
5. **Integridad global**: El Context Map documenta cómo se relacionan los diferentes contextos
"@
Set-Content -Path "$corePath\README.md" -Value $readmeBoundedContexts

Write-Host "Bounded Contexts implementados correctamente en el Core del sistema." 