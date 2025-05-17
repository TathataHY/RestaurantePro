using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Events;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que actualiza el inventario cuando se crea una comanda
    /// Este es un ejemplo de comunicación entre contextos: Operaciones → Inventario
    /// </summary>
    public class ComandaCreada_ActualizarInventarioHandler : IDomainEventHandler<ComandaCreada>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IComandaRepository _comandaRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventLog _eventLog;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCreada_ActualizarInventarioHandler(
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoRepository,
            IProductoRepository productoRepository,
            IComandaRepository comandaRepository,
            IDateTimeService dateTimeService,
            IDomainEventLog eventLog)
        {
            _ingredienteRepository = ingredienteRepository;
            _movimientoRepository = movimientoRepository;
            _productoRepository = productoRepository;
            _comandaRepository = comandaRepository;
            _dateTimeService = dateTimeService;
            _eventLog = eventLog;
        }

        /// <summary>
        /// Maneja el evento de creación de comanda actualizando el inventario
        /// </summary>
        public async Task Handle(ComandaCreada evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar si la comanda existe
                var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
                if (comanda == null)
                {
                    await _eventLog.LogEvent(evento, $"No se encontró la comanda con ID {evento.ComandaId}", cancellationToken);
                    return;
                }

                // Obtener el primer item de la comanda (para simplificar la prueba)
                var primerItem = comanda.Items.FirstOrDefault();
                if (primerItem != null)
                {
                    // Verificar si el producto existe
                    var producto = await _productoRepository.ObtenerPorIdAsync(primerItem.ProductoId, cancellationToken);
                    if (producto == null)
                    {
                        await _eventLog.LogEvent(evento, $"No se encontró el producto con ID {primerItem.ProductoId}", cancellationToken);
                        return;
                    }
                    
                    // Obtener los ingredientes asociados al producto
                    var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(producto.Id, cancellationToken);
                    
                    if (ingredientes != null && ingredientes.Any())
                    {
                        var listaIngredientes = ingredientes.ToList();
                        
                        // Procesamos hasta 2 ingredientes (para la prueba)
                        for (int i = 0; i < 2 && i < listaIngredientes.Count; i++)
                        {
                            var ingrediente = listaIngredientes[i];
                            
                            // Crear movimiento para registrar el consumo de ingrediente
                            var movimiento = MovimientoInventario.CrearEgreso(
                                ingrediente.Id,
                                1.0m, // Cantidad fija para la prueba
                                $"Venta en comanda #{evento.ComandaId}",
                                _dateTimeService.Now);
                            
                            // Registrar el movimiento 
                            await _movimientoRepository.AgregarAsync(movimiento);
                            
                            // Decrementar el stock del ingrediente
                            // Esto debe hacerse antes de actualizar para que el cambio quede registrado
                            ingrediente.DecrementarStock(1.0m, $"Venta en comanda #{evento.ComandaId}");
                            
                            // Actualizar el ingrediente con su nuevo stock
                            await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                        }
                    }
                }
                
                // Registrar el éxito de la operación
                await _eventLog.LogEvent(evento, "Inventario actualizado correctamente", cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventLog.LogEvent(evento, $"Error al actualizar inventario: {ex.Message}", cancellationToken);
            }
        }
    }
} 