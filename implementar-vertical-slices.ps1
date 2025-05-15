# Script para implementar Vertical Slices en el módulo de Comandas

$moduloPath = "src\RestaurantePro.Domain\Operaciones\Comandas"

# Crear estructura de carpetas para Vertical Slices
Write-Host "Creando estructura de carpetas Vertical Slices..."

$verticalSlices = @(
    "CrearComanda",
    "ActualizarEstadoComanda",
    "AgregarProductoComanda",
    "CancelarComanda",
    "FinalizarComanda"
)

foreach ($slice in $verticalSlices) {
    $slicePath = Join-Path -Path $moduloPath -ChildPath $slice
    New-Item -Path $slicePath -ItemType Directory -Force
    Write-Host "Creada slice: $slicePath"
    
    # Crear subcarpetas para cada slice
    New-Item -Path "$slicePath\Domain" -ItemType Directory -Force
    New-Item -Path "$slicePath\Application" -ItemType Directory -Force
    New-Item -Path "$slicePath\Infrastructure" -ItemType Directory -Force
}

# Crear archivos de ejemplo para la slice de CrearComanda
Write-Host "Creando archivos de ejemplo para CrearComanda..."

# 1. Domain model
$comandaDomain = @"
using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Core.Base.Entities;

namespace RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Domain
{
    /// <summary>
    /// Modelo de dominio específico para la operación de crear comanda
    /// </summary>
    public class NuevaComandaModel
    {
        public int MesaId { get; }
        public int UsuarioId { get; }
        public List<NuevoProductoComanda> Productos { get; }
        public string Notas { get; }
        
        private NuevaComandaModel(int mesaId, int usuarioId, List<NuevoProductoComanda> productos, string notas)
        {
            MesaId = mesaId;
            UsuarioId = usuarioId;
            Productos = productos;
            Notas = notas;
        }
        
        public static NuevaComandaModel Crear(int mesaId, int usuarioId, List<NuevoProductoComanda> productos, string notas)
        {
            if (mesaId <= 0)
                throw new ArgumentException("Mesa inválida", nameof(mesaId));
                
            if (usuarioId <= 0)
                throw new ArgumentException("Usuario inválido", nameof(usuarioId));
                
            if (productos == null || productos.Count == 0)
                throw new ArgumentException("Debe incluir al menos un producto", nameof(productos));
                
            return new NuevaComandaModel(mesaId, usuarioId, productos, notas);
        }
    }
    
    public class NuevoProductoComanda
    {
        public int ProductoId { get; }
        public int Cantidad { get; }
        public string Notas { get; }
        public List<PersonalizacionProducto> Personalizaciones { get; }
        
        public NuevoProductoComanda(int productoId, int cantidad, string notas, List<PersonalizacionProducto> personalizaciones = null)
        {
            if (productoId <= 0)
                throw new ArgumentException("Producto inválido", nameof(productoId));
                
            if (cantidad <= 0)
                throw new ArgumentException("Cantidad debe ser mayor a cero", nameof(cantidad));
                
            ProductoId = productoId;
            Cantidad = cantidad;
            Notas = notas;
            Personalizaciones = personalizaciones ?? new List<PersonalizacionProducto>();
        }
    }
    
    public class PersonalizacionProducto
    {
        public string Tipo { get; }
        public string Valor { get; }
        
        public PersonalizacionProducto(string tipo, string valor)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo de personalización requerido", nameof(tipo));
                
            Tipo = tipo;
            Valor = valor;
        }
    }
}
"@
Set-Content -Path "$moduloPath\CrearComanda\Domain\NuevaComandaModel.cs" -Value $comandaDomain

# 2. Application command
$comandaCommand = @"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Domain;

namespace RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Application
{
    /// <summary>
    /// Command para crear una nueva comanda
    /// </summary>
    public class CrearComandaCommand
    {
        public int MesaId { get; }
        public int UsuarioId { get; }
        public List<ProductoComandaDto> Productos { get; }
        public string Notas { get; }
        
        public CrearComandaCommand(int mesaId, int usuarioId, List<ProductoComandaDto> productos, string notas)
        {
            MesaId = mesaId;
            UsuarioId = usuarioId;
            Productos = productos;
            Notas = notas;
        }
    }
    
    public class ProductoComandaDto
    {
        public int ProductoId { get; }
        public int Cantidad { get; }
        public string Notas { get; }
        public List<PersonalizacionDto> Personalizaciones { get; }
        
        public ProductoComandaDto(int productoId, int cantidad, string notas, List<PersonalizacionDto> personalizaciones = null)
        {
            ProductoId = productoId;
            Cantidad = cantidad;
            Notas = notas;
            Personalizaciones = personalizaciones ?? new List<PersonalizacionDto>();
        }
    }
    
    public class PersonalizacionDto
    {
        public string Tipo { get; }
        public string Valor { get; }
        
        public PersonalizacionDto(string tipo, string valor)
        {
            Tipo = tipo;
            Valor = valor;
        }
    }
    
    /// <summary>
    /// Resultado de la operación de crear comanda
    /// </summary>
    public class CrearComandaResult
    {
        public bool Exito { get; }
        public string NumeroComanda { get; }
        public int ComandaId { get; }
        public string Mensaje { get; }
        public List<string> Errores { get; }
        
        private CrearComandaResult(bool exito, string numeroComanda, int comandaId, string mensaje, List<string> errores = null)
        {
            Exito = exito;
            NumeroComanda = numeroComanda;
            ComandaId = comandaId;
            Mensaje = mensaje;
            Errores = errores ?? new List<string>();
        }
        
        public static CrearComandaResult Success(int comandaId, string numeroComanda, string mensaje = "Comanda creada correctamente")
        {
            return new CrearComandaResult(true, numeroComanda, comandaId, mensaje);
        }
        
        public static CrearComandaResult Failure(string mensaje, List<string> errores = null)
        {
            return new CrearComandaResult(false, null, 0, mensaje, errores);
        }
    }
    
    /// <summary>
    /// Handler para procesar el comando de crear comanda
    /// </summary>
    public class CrearComandaHandler
    {
        // Dependencias (interfaces a repositorios)
        private readonly IComandaRepository _comandaRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IProductoRepository _productoRepository;
        
        public CrearComandaHandler(
            IComandaRepository comandaRepository,
            IMesaRepository mesaRepository,
            IProductoRepository productoRepository)
        {
            _comandaRepository = comandaRepository;
            _mesaRepository = mesaRepository;
            _productoRepository = productoRepository;
        }
        
        public async Task<CrearComandaResult> Handle(CrearComandaCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validar disponibilidad de mesa
                var mesa = await _mesaRepository.GetByIdAsync(command.MesaId);
                if (mesa == null)
                    return CrearComandaResult.Failure("La mesa especificada no existe");
                    
                if (!mesa.EstaDisponible)
                    return CrearComandaResult.Failure("La mesa no está disponible");
                
                // 2. Transformar a modelo de dominio
                var productosComanda = new List<NuevoProductoComanda>();
                foreach (var prod in command.Productos)
                {
                    var producto = await _productoRepository.GetByIdAsync(prod.ProductoId);
                    if (producto == null)
                        return CrearComandaResult.Failure($"El producto {prod.ProductoId} no existe");
                        
                    if (!producto.Disponible)
                        return CrearComandaResult.Failure($"El producto {producto.Nombre} no está disponible");
                    
                    var personalizaciones = new List<PersonalizacionProducto>();
                    foreach (var pers in prod.Personalizaciones)
                    {
                        personalizaciones.Add(new PersonalizacionProducto(pers.Tipo, pers.Valor));
                    }
                    
                    productosComanda.Add(new NuevoProductoComanda(
                        prod.ProductoId, 
                        prod.Cantidad, 
                        prod.Notas, 
                        personalizaciones));
                }
                
                var nuevaComanda = NuevaComandaModel.Crear(
                    command.MesaId, 
                    command.UsuarioId, 
                    productosComanda, 
                    command.Notas);
                
                // 3. Generar número de comanda
                var numeroComanda = GenerarNumeroComanda();
                
                // 4. Crear comanda en repositorio
                var comandaId = await _comandaRepository.CrearNuevaComandaAsync(nuevaComanda, numeroComanda);
                
                // 5. Actualizar estado de mesa
                await _mesaRepository.ActualizarEstadoAsync(command.MesaId, ocupada: true);
                
                // 6. Retornar resultado
                return CrearComandaResult.Success(comandaId, numeroComanda);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return CrearComandaResult.Failure($"Error al crear comanda: {ex.Message}");
            }
        }
        
        private string GenerarNumeroComanda()
        {
            // Lógica para generar número único de comanda
            return $"CMD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }
    }
    
    // Interfaces necesarias (definidas aquí para simplificar el ejemplo)
    public interface IComandaRepository
    {
        Task<int> CrearNuevaComandaAsync(NuevaComandaModel nuevaComanda, string numeroComanda);
    }
    
    public interface IMesaRepository
    {
        Task<Mesa> GetByIdAsync(int mesaId);
        Task ActualizarEstadoAsync(int mesaId, bool ocupada);
    }
    
    public interface IProductoRepository
    {
        Task<Producto> GetByIdAsync(int productoId);
    }
    
    // Clases auxiliares para simplificar el ejemplo
    public class Mesa
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public bool EstaDisponible { get; set; }
    }
    
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Disponible { get; set; }
    }
}
"@
Set-Content -Path "$moduloPath\CrearComanda\Application\CrearComandaCommand.cs" -Value $comandaCommand

# 3. Infrastructure implementation
$comandaInfra = @"
using System;
using System.Threading.Tasks;
using RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Application;
using RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Domain;

namespace RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Infrastructure
{
    /// <summary>
    /// Implementación del repositorio para crear comandas
    /// </summary>
    public class ComandaRepository : IComandaRepository
    {
        // Conexión a base de datos y dependencias
        
        public async Task<int> CrearNuevaComandaAsync(NuevaComandaModel nuevaComanda, string numeroComanda)
        {
            // Implementación de persistencia
            // ...
            
            // Simulación para el ejemplo
            return new Random().Next(1, 1000);
        }
    }
}
"@
Set-Content -Path "$moduloPath\CrearComanda\Infrastructure\ComandaRepository.cs" -Value $comandaInfra

# Crear archivos de ejemplo para la slice de ActualizarEstadoComanda
Write-Host "Creando archivos de ejemplo para ActualizarEstadoComanda..."

# 1. Domain model
$estadoComandaDomain = @"
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
"@
Set-Content -Path "$moduloPath\ActualizarEstadoComanda\Domain\ActualizacionEstadoModel.cs" -Value $estadoComandaDomain

# Crear README específico para el módulo
$readmeVerticalSlices = @"
# Módulo Comandas - Vertical Slices

Este módulo implementa la arquitectura de Vertical Slices, organizando el código por funcionalidades o casos de uso en lugar de por capas técnicas.

## Estructura de carpetas

```
Comandas/
├── CrearComanda/                 # Funcionalidad de crear comanda
│   ├── Domain/                   # Modelos y reglas específicas
│   ├── Application/              # Comandos y handlers específicos
│   └── Infrastructure/           # Implementaciones específicas
│
├── ActualizarEstadoComanda/      # Funcionalidad de actualizar estado
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
├── AgregarProductoComanda/       # Funcionalidad de agregar productos
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
├── CancelarComanda/              # Funcionalidad de cancelar comanda
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
└── FinalizarComanda/             # Funcionalidad de finalizar comanda
    ├── Domain/
    ├── Application/
    └── Infrastructure/
```

## Beneficios de Vertical Slices

1. **Cohesión por funcionalidad**: Todos los componentes relacionados con una funcionalidad están juntos
2. **Independencia entre slices**: Cambios en una funcionalidad no afectan a otras
3. **Productividad**: Facilita trabajar en funcionalidades específicas sin entender todo el sistema
4. **Evolución independiente**: Cada funcionalidad puede evolucionar a su propio ritmo
5. **Comprensión**: Es más fácil entender casos de uso completos cuando están agrupados

## Patrones adicionales utilizados

- **Command-Query Responsibility Segregation (CQRS)**: Separación entre comandos (escritura) y consultas (lectura)
- **Modelos de dominio específicos**: Cada slice tiene sus propios modelos específicos de dominio
- **Result Pattern**: Retorno de resultados estandarizados para cada operación
- **Repository Pattern**: Abstracción de la persistencia de datos
"@
Set-Content -Path "$moduloPath\README.md" -Value $readmeVerticalSlices

Write-Host "Patrón Vertical Slices implementado correctamente en el módulo Comandas." 