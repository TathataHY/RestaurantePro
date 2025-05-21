using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Inventario
    /// </summary>
    public class InventarioServiceFacade : IInventarioServiceFacade
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IStockBajoPolicy _stockBajoPolicy;
        private readonly IDateTimeService _dateTimeService;
        
        public InventarioServiceFacade(
            IIngredienteRepository ingredienteRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IStockBajoPolicy stockBajoPolicy,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _stockBajoPolicy = stockBajoPolicy ?? throw new ArgumentNullException(nameof(stockBajoPolicy));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        #region Ingredientes
        
        /// <inheritdoc />
        public async Task<Ingrediente> RegistrarIngredienteAsync(
            string nombre, 
            string descripcion, 
            string unidadMedida, 
            decimal stockMinimo, 
            decimal stockActual,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño,
            decimal costo = 0,
            CancellationToken cancellationToken = default)
        {
            // Crear el ingrediente
            var ingrediente = Ingrediente.Crear(
                nombre,
                descripcion,
                unidadMedida,
                stockMinimo,
                stockActual,
                rotacion,
                temporada,
                costo);
            
            // Persistir el ingrediente
            await _ingredienteRepository.AgregarAsync(ingrediente);
            await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return ingrediente;
        }
        
        /// <inheritdoc />
        public async Task<Ingrediente?> ActualizarStockIngredienteAsync(
            Guid ingredienteId, 
            decimal cantidad, 
            TipoMovimientoInventario tipoMovimiento, 
            string? referencia = null, 
            string? observacion = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
            if (ingrediente == null)
            {
                return null;
            }
            
            // Crear movimiento según el tipo
            if (cantidad > 0 && tipoMovimiento == TipoMovimientoInventario.Salida)
            {
                throw new InvalidOperationException("Para movimientos de salida, la cantidad debe ser negativa");
            }
            
            if (cantidad < 0 && tipoMovimiento == TipoMovimientoInventario.Entrada)
            {
                throw new InvalidOperationException("Para movimientos de entrada, la cantidad debe ser positiva");
            }
            
            // Crear el movimiento según el tipo
            MovimientoInventario movimiento;
            switch (tipoMovimiento)
            {
                case TipoMovimientoInventario.Entrada:
                    movimiento = ingrediente.RegistrarEntrada(cantidad, referencia, observacion);
                    break;
                case TipoMovimientoInventario.Salida:
                    // Para salidas, asegurar que la cantidad sea negativa
                    var cantidadSalida = cantidad > 0 ? -cantidad : cantidad;
                    movimiento = ingrediente.RegistrarSalida(Math.Abs(cantidadSalida), referencia, observacion);
                    break;
                case TipoMovimientoInventario.Ajuste:
                    movimiento = ingrediente.RegistrarAjuste(cantidad, referencia, observacion);
                    break;
                default:
                    throw new InvalidOperationException($"Tipo de movimiento no soportado: {tipoMovimiento}");
            }
            
            // Persistir cambios
            await _ingredienteRepository.ActualizarAsync(ingrediente);
            await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return ingrediente;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Ingrediente>> VerificarIngredientesStockBajoAsync(CancellationToken cancellationToken = default)
        {
            return await _ingredienteRepository.ObtenerIngredientesConStockBajoAsync(cancellationToken);
        }
        
        #endregion
        
        #region Órdenes de Compra
        
        /// <inheritdoc />
        public async Task<OrdenCompra> CrearOrdenCompraAsync(
            Guid proveedorId, 
            DateTime fechaEntregaEstimada, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Verificar que exista el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                throw new InvalidOperationException($"No se encontró el proveedor con ID {proveedorId}");
            }
            
            if (!proveedor.Activo)
            {
                throw new InvalidOperationException($"El proveedor con ID {proveedorId} no está activo");
            }
            
            // Crear la orden de compra
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                proveedor.Nombre,
                fechaEntregaEstimada,
                observaciones);
            
            // Persistir la orden
            await _ordenCompraRepository.AgregarAsync(ordenCompra);
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            
            return ordenCompra;
        }
        
        /// <inheritdoc />
        public async Task<OrdenCompra?> AgregarItemOrdenCompraAsync(
            Guid ordenCompraId, 
            Guid ingredienteId, 
            decimal cantidad, 
            decimal precioUnitario, 
            string observacion = "", 
            CancellationToken cancellationToken = default)
        {
            // Obtener la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return null;
            }
            
            // Verificar que la orden esté en estado borrador
            if (ordenCompra.Estado != EstadoOrdenCompra.Borrador)
            {
                throw new InvalidOperationException($"No se pueden agregar items a una orden que no esté en estado Borrador. Estado actual: {ordenCompra.Estado}");
            }
            
            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
            if (ingrediente == null)
            {
                throw new InvalidOperationException($"No se encontró el ingrediente con ID {ingredienteId}");
            }
            
            // Agregar el item
            ordenCompra.AgregarItem(ingredienteId, ingrediente.Nombre, cantidad, precioUnitario, observacion);
            
            // Persistir cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra);
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            
            return ordenCompra;
        }
        
        /// <inheritdoc />
        public async Task<bool> ActualizarEstadoOrdenCompraAsync(
            Guid ordenCompraId, 
            EstadoOrdenCompra nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return false;
            }
            
            // Actualizar estado según el tipo
            switch (nuevoEstado)
            {
                case EstadoOrdenCompra.Enviada:
                    ordenCompra.Enviar();
                    break;
                case EstadoOrdenCompra.Cancelada:
                    ordenCompra.Cancelar("Cancelada desde servicio de inventario");
                    break;
                case EstadoOrdenCompra.Completada:
                    ordenCompra.Completar();
                    break;
                default:
                    throw new InvalidOperationException($"No se puede actualizar al estado {nuevoEstado} directamente");
            }
            
            // Persistir cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra);
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> RecibirOrdenCompraCompletaAsync(
            Guid ordenCompraId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Obtener la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdConItemsAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return false;
            }
            
            // Verificar que la orden esté en estado Enviada
            if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
            {
                throw new InvalidOperationException($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}");
            }
            
            // Marcar como recibida
            ordenCompra.Recibir(observaciones);
            
            // Actualizar el stock de los ingredientes
            foreach (var item in ordenCompra.Items)
            {
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                if (ingrediente != null)
                {
                    ingrediente.RegistrarEntrada(
                        item.Cantidad,
                        $"Orden de compra #{ordenCompraId}",
                        $"Recepción de orden completa: {item.Observacion}");
                    
                    await _ingredienteRepository.ActualizarAsync(ingrediente);
                }
            }
            
            // Persistir cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra);
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> RecibirOrdenCompraParcialAsync(
            Guid ordenCompraId, 
            Dictionary<Guid, decimal> itemsRecibidos, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Obtener la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdConItemsAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return false;
            }
            
            // Verificar que la orden esté en estado Enviada
            if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
            {
                throw new InvalidOperationException($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}");
            }
            
            // Verificar que los items existan en la orden
            foreach (var itemId in itemsRecibidos.Keys)
            {
                if (!ordenCompra.ExisteItem(itemId))
                {
                    throw new InvalidOperationException($"No existe un item con ID {itemId} en la orden de compra");
                }
            }
            
            // Marcar como recibida parcialmente
            ordenCompra.RecibirParcialmente(itemsRecibidos, observaciones);
            
            // Actualizar el stock de los ingredientes recibidos
            foreach (var itemPair in itemsRecibidos)
            {
                var item = ordenCompra.ObtenerItem(itemPair.Key);
                if (item != null)
                {
                    var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                    if (ingrediente != null)
                    {
                        ingrediente.RegistrarEntrada(
                            itemPair.Value,
                            $"Orden de compra #{ordenCompraId}",
                            $"Recepción parcial: {item.Observacion}");
                        
                        await _ingredienteRepository.ActualizarAsync(ingrediente);
                    }
                }
            }
            
            // Persistir cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra);
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            await _ingredienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<OrdenCompra>> GenerarOrdenesCompraAutomaticasAsync(CancellationToken cancellationToken = default)
        {
            // Ejecutar la política de stock bajo
            var resultados = await _stockBajoPolicy.EjecutarAsync(cancellationToken);
            
            // Convertir las recomendaciones en órdenes de compra
            var ordenesGeneradas = new List<OrdenCompra>();
            
            foreach (var resultado in resultados)
            {
                // Agrupar por proveedor
                var itemsPorProveedor = new Dictionary<Guid, List<(Guid IngredienteId, string Nombre, decimal Cantidad, decimal PrecioUnitario)>>();
                
                foreach (var recomendacion in resultado.RecomendacionesCompra)
                {
                    if (!itemsPorProveedor.ContainsKey(recomendacion.ProveedorId))
                    {
                        itemsPorProveedor[recomendacion.ProveedorId] = new List<(Guid, string, decimal, decimal)>();
                    }
                    
                    itemsPorProveedor[recomendacion.ProveedorId].Add((
                        recomendacion.IngredienteId,
                        recomendacion.NombreIngrediente,
                        recomendacion.CantidadRecomendada,
                        recomendacion.PrecioUnitarioEstimado
                    ));
                }
                
                // Crear una orden por cada proveedor
                foreach (var proveedorId in itemsPorProveedor.Keys)
                {
                    // Verificar que el proveedor exista y esté activo
                    var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                    if (proveedor == null || !proveedor.Activo)
                    {
                        continue;
                    }
                    
                    // Crear la orden
                    var fechaEntrega = _dateTimeService.Now.AddDays(7); // Una semana por defecto
                    var orden = OrdenCompra.Crear(
                        proveedorId,
                        proveedor.Nombre,
                        fechaEntrega,
                        $"Orden automática generada por stock bajo: {resultado.MotivoGeneracion}");
                    
                    // Agregar items
                    foreach (var item in itemsPorProveedor[proveedorId])
                    {
                        orden.AgregarItem(
                            item.IngredienteId,
                            item.Nombre,
                            item.Cantidad,
                            item.PrecioUnitario,
                            "Generado automáticamente");
                    }
                    
                    // Persistir la orden
                    await _ordenCompraRepository.AgregarAsync(orden);
                    ordenesGeneradas.Add(orden);
                }
            }
            
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            
            return ordenesGeneradas;
        }
        
        #endregion
    }
} 