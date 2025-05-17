using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.CrearComanda
{
    /// <summary>
    /// Manejador para el comando CrearComanda
    /// </summary>
    public class CrearComandaHandler : IRequestHandler<CrearComandaCommand, Result<Guid>>
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IMediator _mediator;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public CrearComandaHandler(
            IComandaRepository comandaRepository,
            IProductoRepository productoRepository,
            IMesaRepository mesaRepository,
            IMediator mediator)
        {
            _comandaRepository = comandaRepository;
            _productoRepository = productoRepository;
            _mesaRepository = mesaRepository;
            _mediator = mediator;
        }
        
        /// <summary>
        /// Maneja la ejecución del comando
        /// </summary>
        public async Task<Result<Guid>> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que la mesa exista y esté disponible
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId);
            if (mesa == null)
            {
                return Result.Failure<Guid>("La mesa especificada no existe");
            }
            
            if (!mesa.EstaDisponible)
            {
                return Result.Failure<Guid>("La mesa seleccionada no está disponible");
            }
            
            // 2. Validar productos
            if (request.Productos.Any())
            {
                foreach (var producto in request.Productos)
                {
                    var productoEntity = await _productoRepository.ObtenerPorIdAsync(producto.ProductoId);
                    if (productoEntity == null)
                    {
                        return Result.Failure<Guid>($"El producto con ID {producto.ProductoId} no existe");
                    }
                    
                    if (!productoEntity.EstaDisponible)
                    {
                        return Result.Failure<Guid>($"El producto {productoEntity.Nombre} no está disponible");
                    }
                }
            }
            
            // 3. Crear la comanda (usando la factory del dominio)
            var comanda = Comanda.Crear(
                request.MesaId,
                request.MeseroId,
                request.ClienteId,
                request.Observaciones);
                
            // 4. Agregar productos a la comanda
            foreach (var producto in request.Productos)
            {
                var productoEntity = await _productoRepository.ObtenerPorIdAsync(producto.ProductoId);
                comanda.AgregarProducto(
                    producto.ProductoId,
                    producto.Cantidad,
                    productoEntity.Precio,
                    producto.Observaciones);
            }
            
            // 5. Persistir la comanda
            await _comandaRepository.AgregarAsync(comanda);
            
            // 6. Cambiar estado de la mesa
            mesa.AsignarComanda(comanda.Id);
            await _mesaRepository.ActualizarAsync(mesa);
            
            // 7. Publicar eventos de dominio
            var domainEvents = comanda.DomainEvents.ToList();
            comanda.ClearDomainEvents();
            
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            
            // 8. Devolver el ID de la comanda creada
            return Result.Success(comanda.Id);
        }
    }
} 