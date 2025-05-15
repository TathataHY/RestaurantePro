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
