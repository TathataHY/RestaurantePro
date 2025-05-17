using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Inventario.Dtos;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Inventario.Services
{
    public class InventarioComandaService : IInventarioComandaService
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public InventarioComandaService(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ActualizarInventarioPorComanda(int comandaId)
        {
            // Obtener la comanda con sus detalles e información de productos
            var comanda = await _context.Comandas
                .Include(c => c.DetallesComanda)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.IngredientesProducto)
                            .ThenInclude(ip => ip.Ingrediente)
                .FirstOrDefaultAsync(c => c.Id == comandaId);

            if (comanda == null)
            {
                throw new NotFoundException(nameof(Comanda), comandaId);
            }

            // Verificar que la comanda esté en estado apropiado (preparada o entregada)
            if (comanda.Estado != EstadoComanda.Preparada && comanda.Estado != EstadoComanda.Entregada)
            {
                throw new InvalidOperationException($"No se puede actualizar el inventario para una comanda en estado {comanda.Estado}");
            }

            // Diccionario para acumular las cantidades a descontar por ingrediente
            var ingredientesConsumo = new Dictionary<int, decimal>();

            // Procesar cada detalle de la comanda
            foreach (var detalle in comanda.DetallesComanda)
            {
                var producto = detalle.Producto;
                if (producto == null || producto.IngredientesProducto == null)
                {
                    continue;
                }

                // Para cada ingrediente del producto, calcular la cantidad a descontar
                foreach (var ingredienteProducto in producto.IngredientesProducto)
                {
                    // Cantidad de ingrediente = cantidad base * cantidad pedida
                    decimal cantidadTotal = ingredienteProducto.Cantidad * detalle.Cantidad;
                    
                    // Acumular la cantidad por ingrediente
                    if (ingredientesConsumo.ContainsKey(ingredienteProducto.IngredienteId))
                    {
                        ingredientesConsumo[ingredienteProducto.IngredienteId] += cantidadTotal;
                    }
                    else
                    {
                        ingredientesConsumo[ingredienteProducto.IngredienteId] = cantidadTotal;
                    }
                }
            }

            // Si no hay ingredientes para descontar, salir
            if (!ingredientesConsumo.Any())
            {
                return true;
            }

            // Cargar todos los ingredientes afectados de una vez
            var ingredientesIds = ingredientesConsumo.Keys.ToList();
            var inventarioItems = await _context.Inventarios
                .Where(i => ingredientesIds.Contains(i.IngredienteId))
                .ToListAsync();

            // Crear un diccionario para acceso rápido
            var inventarioPorIngrediente = inventarioItems.ToDictionary(i => i.IngredienteId, i => i);

            // Verificar stock y registrar los movimientos
            foreach (var kvp in ingredientesConsumo)
            {
                int ingredienteId = kvp.Key;
                decimal cantidadNecesaria = kvp.Value;

                // Verificar si hay inventario para este ingrediente
                if (!inventarioPorIngrediente.TryGetValue(ingredienteId, out var inventarioItem))
                {
                    // Si no hay registro de inventario, verificar si el ingrediente existe
                    var ingrediente = await _context.Ingredientes.FindAsync(ingredienteId);
                    if (ingrediente == null)
                    {
                        throw new NotFoundException(nameof(Ingrediente), ingredienteId);
                    }

                    // Crear un registro de inventario con cantidad 0
                    inventarioItem = new Domain.Entities.Inventario
                    {
                        IngredienteId = ingredienteId,
                        CantidadDisponible = 0,
                        UnidadMedida = ingrediente.UnidadMedida,
                        CostoUnitario = 0,
                        CantidadMinima = 0,
                        CantidadOptima = 0,
                        Ubicacion = "Almacén principal",
                        Estado = "Activo",
                        UltimaActualizacion = _dateTime.Now,
                        UltimoInventarioPor = _currentUserService.UserId,
                        Observaciones = "Registro creado automáticamente por comanda"
                    };
                    _context.Inventarios.Add(inventarioItem);
                    await _context.SaveChangesAsync(); // Guardar para obtener ID
                }

                // Verificar si hay suficiente stock
                if (inventarioItem.CantidadDisponible < cantidadNecesaria)
                {
                    // Inventario insuficiente, pero continuamos de todas formas
                    // Registramos una advertencia
                    var advertencia = new MovimientoInventario
                    {
                        InventarioId = inventarioItem.Id,
                        IngredienteId = ingredienteId,
                        TipoMovimiento = TipoMovimiento.Advertencia,
                        Cantidad = cantidadNecesaria - inventarioItem.CantidadDisponible,
                        CantidadAnterior = inventarioItem.CantidadDisponible,
                        CantidadNueva = 0,
                        UnidadMedida = inventarioItem.UnidadMedida,
                        CostoUnitario = inventarioItem.CostoUnitario,
                        CostoTotal = (cantidadNecesaria - inventarioItem.CantidadDisponible) * inventarioItem.CostoUnitario,
                        Descripcion = $"Advertencia: Stock insuficiente para comanda {comanda.NumeroComanda}",
                        Referencia = comanda.NumeroComanda,
                        Fecha = _dateTime.Now,
                        ComandaId = comandaId,
                        UsuarioId = _currentUserService.UserId
                    };
                    _context.MovimientosInventario.Add(advertencia);
                    
                    // Ajustamos la cantidad a consumir al máximo disponible
                    cantidadNecesaria = inventarioItem.CantidadDisponible;
                }

                // Si hay algo que consumir, registrar el movimiento
                if (cantidadNecesaria > 0)
                {
                    decimal cantidadAnterior = inventarioItem.CantidadDisponible;
                    
                    // Actualizar el inventario
                    inventarioItem.CantidadDisponible -= cantidadNecesaria;
                    inventarioItem.UltimaActualizacion = _dateTime.Now;
                    inventarioItem.UltimoInventarioPor = _currentUserService.UserId;

                    // Registrar el movimiento
                    var movimiento = new MovimientoInventario
                    {
                        InventarioId = inventarioItem.Id,
                        IngredienteId = ingredienteId,
                        TipoMovimiento = TipoMovimiento.Salida,
                        Cantidad = cantidadNecesaria,
                        CantidadAnterior = cantidadAnterior,
                        CantidadNueva = inventarioItem.CantidadDisponible,
                        UnidadMedida = inventarioItem.UnidadMedida,
                        CostoUnitario = inventarioItem.CostoUnitario,
                        CostoTotal = cantidadNecesaria * inventarioItem.CostoUnitario,
                        Descripcion = $"Consumo por comanda {comanda.NumeroComanda}",
                        Referencia = comanda.NumeroComanda,
                        Fecha = _dateTime.Now,
                        ComandaId = comandaId,
                        UsuarioId = _currentUserService.UserId
                    };
                    _context.MovimientosInventario.Add(movimiento);
                }
            }

            // Actualizar comanda como procesada para inventario
            comanda.InventarioProcesado = true;
            
            // Guardar todos los cambios
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<Dictionary<int, decimal>> VerificarDisponibilidadIngredientes(List<DetalleProductoDto> detallesProductos)
        {
            if (detallesProductos == null || !detallesProductos.Any())
            {
                return new Dictionary<int, decimal>();
            }

            // Obtener IDs de los productos
            var productosIds = detallesProductos.Select(d => d.ProductoId).ToList();
            
            // Obtener todos los ingredientes necesarios para los productos
            var ingredientesProducto = await _context.IngredientesProducto
                .Where(ip => productosIds.Contains(ip.ProductoId))
                .ToListAsync();

            // Calcular la cantidad necesaria por ingrediente
            var ingredientesNecesarios = new Dictionary<int, decimal>();
            
            foreach (var detalle in detallesProductos)
            {
                var ingredientesDelProducto = ingredientesProducto
                    .Where(ip => ip.ProductoId == detalle.ProductoId);
                
                foreach (var ingredienteProducto in ingredientesDelProducto)
                {
                    decimal cantidadNecesaria = ingredienteProducto.Cantidad * detalle.Cantidad;
                    
                    if (ingredientesNecesarios.ContainsKey(ingredienteProducto.IngredienteId))
                    {
                        ingredientesNecesarios[ingredienteProducto.IngredienteId] += cantidadNecesaria;
                    }
                    else
                    {
                        ingredientesNecesarios[ingredienteProducto.IngredienteId] = cantidadNecesaria;
                    }
                }
            }

            // Verificar disponibilidad en inventario
            var ingredientesIds = ingredientesNecesarios.Keys.ToList();
            var inventarioItems = await _context.Inventarios
                .Where(i => ingredientesIds.Contains(i.IngredienteId))
                .ToListAsync();

            // Calcular faltantes
            var faltantes = new Dictionary<int, decimal>();
            
            foreach (var ingredienteId in ingredientesNecesarios.Keys)
            {
                decimal cantidadNecesaria = ingredientesNecesarios[ingredienteId];
                var inventarioItem = inventarioItems.FirstOrDefault(i => i.IngredienteId == ingredienteId);
                
                // Si no hay registro de inventario o la cantidad es insuficiente
                if (inventarioItem == null || inventarioItem.CantidadDisponible < cantidadNecesaria)
                {
                    decimal disponible = inventarioItem?.CantidadDisponible ?? 0;
                    decimal faltante = cantidadNecesaria - disponible;
                    
                    if (faltante > 0)
                    {
                        faltantes[ingredienteId] = faltante;
                    }
                }
            }

            return faltantes;
        }
    }

    public interface IInventarioComandaService
    {
        Task<bool> ActualizarInventarioPorComanda(int comandaId);
        Task<Dictionary<int, decimal>> VerificarDisponibilidadIngredientes(List<DetalleProductoDto> detallesProductos);
    }

    public class DetalleProductoDto
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
    }
} 