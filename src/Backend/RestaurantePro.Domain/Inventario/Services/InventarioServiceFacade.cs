

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
            // Convertir string unidadMedida a enum UnidadMedida
            UnidadMedida unidadMedidaEnum;
            if (!Enum.TryParse(unidadMedida, true, out unidadMedidaEnum))
            {
                throw new ArgumentException($"Unidad de medida no válida: {unidadMedida}", nameof(unidadMedida));
            }
            
            // Generar código (usando las primeras letras del nombre y un timestamp)
            string codigo = $"{nombre.Substring(0, Math.Min(3, nombre.Length)).ToUpper()}-{DateTime.Now:yyyyMMdd}";
            
            // Crear el ingrediente
            var ingrediente = Ingrediente.Crear(
                nombre,
                codigo,
                descripcion,
                unidadMedidaEnum,
                stockMinimo,
                stockActual,
                rotacion,
                temporada);
                
            // Establecer el costo promedio si se proporciona
            if (costo > 0)
            {
                ingrediente.ActualizarCostoPromedio(costo);
            }
            
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
            
            // Preparar el motivo para el movimiento
            string motivo = string.IsNullOrEmpty(observacion) 
                ? $"Movimiento de {tipoMovimiento} - {referencia ?? "N/A"}" 
                : observacion;
            
            // Crear movimiento según el tipo
            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad debe ser mayor que cero");
            }
            
            // Crear el movimiento según el tipo
            switch (tipoMovimiento)
            {
                case TipoMovimientoInventario.Ingreso:
                    ingrediente.IncrementarStock(cantidad, motivo);
                    break;
                case TipoMovimientoInventario.Egreso:
                    ingrediente.DecrementarStock(cantidad, motivo);
                    break;
                case TipoMovimientoInventario.Ajuste:
                    if (cantidad > ingrediente.Stock)
                    {
                        // Incremento (ajuste positivo)
                        ingrediente.IncrementarStock(cantidad - ingrediente.Stock, $"Ajuste positivo - {motivo}");
                    }
                    else if (cantidad < ingrediente.Stock)
                    {
                        // Decremento (ajuste negativo)
                        ingrediente.DecrementarStock(ingrediente.Stock - cantidad, $"Ajuste negativo - {motivo}");
                    }
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
                observaciones,
                _dateTimeService.Now);
                
            // Establecer la fecha de entrega estimada
            ordenCompra.EstablecerFechaEntrega(fechaEntregaEstimada);
            
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
            ordenCompra.AgregarItem(ingredienteId, ingrediente.Nombre, cantidad, ingrediente.UnidadMedida);
            
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
                case EstadoOrdenCompra.Recibida:
                    ordenCompra.Recibir(_dateTimeService.Now, "Recibida desde servicio de inventario");
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
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return false;
            }
            
            // Verificar que esté en estado Enviada
            if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
            {
                throw new InvalidOperationException($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}");
            }
            
            // Marcar como recibida
            ordenCompra.Recibir(_dateTimeService.Now, observaciones);
            
            // Registrar entrada de stock para cada ítem
            foreach (var item in ordenCompra.Items)
            {
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                if (ingrediente != null)
                {
                    ingrediente.IncrementarStock(
                        item.Cantidad, 
                        $"Recepción de orden de compra #{ordenCompraId}");
                    
                    await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
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
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(ordenCompraId, cancellationToken);
            if (ordenCompra == null)
            {
                return false;
            }
            
            // Verificar que esté en estado Enviada
            if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
            {
                throw new InvalidOperationException($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}");
            }
            
            // Verificar que haya al menos un item para recibir
            if (itemsRecibidos == null || !itemsRecibidos.Any())
            {
                throw new ArgumentException("Debe especificar al menos un item para recibir", nameof(itemsRecibidos));
            }
            
            // Validar que todos los items especificados existen en la orden
            foreach (var itemId in itemsRecibidos.Keys)
            {
                if (!ordenCompra.Items.Any(i => i.Id == itemId))
                {
                    throw new ArgumentException($"El item con ID {itemId} no existe en esta orden", nameof(itemsRecibidos));
                }
            }
            
            // La orden se considera como recibida de forma parcial pero el estado actual será Recibida
            ordenCompra.Recibir(_dateTimeService.Now, $"{observaciones} (Recepción parcial)");
            
            // Registrar entrada de stock para los items recibidos
            foreach (var kvp in itemsRecibidos)
            {
                var itemId = kvp.Key;
                var cantidadRecibida = kvp.Value;
                
                var item = ordenCompra.Items.FirstOrDefault(i => i.Id == itemId);
                if (item != null && cantidadRecibida > 0)
                {
                    var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                    if (ingrediente != null)
                    {
                        ingrediente.IncrementarStock(
                            cantidadRecibida, 
                            $"Recepción parcial de orden de compra #{ordenCompraId}");
                        
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
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
            // Ejecutar política de stock bajo para obtener ingredientes priorizados
            var resultado = await _stockBajoPolicy.EjecutarAsync(cancellationToken);
            
            var ordenesCompra = new List<OrdenCompra>();
            
            // Agrupar ingredientes por proveedor
            var ingredientesPorProveedor = resultado.IngredientesPriorizados
                .Select(async ip => {
                    // Obtener el ingrediente completo con su proveedor
                    var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ip.IngredienteId, cancellationToken);
                    return new { Ingrediente = ingrediente, Prioridad = ip.Prioridad };
                })
                .Select(t => t.Result)
                .Where(t => t.Ingrediente != null && t.Ingrediente.ProveedorPrincipalId.HasValue)
                .GroupBy(t => t.Ingrediente.ProveedorPrincipalId.Value);
            
            // Generar una orden por cada proveedor
            foreach (var grupo in ingredientesPorProveedor)
            {
                var proveedorId = grupo.Key;
                
                // Verificar si existe el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                if (proveedor == null || !proveedor.Activo)
                    continue;
                    
                // Crear la orden
                var orden = OrdenCompra.Crear(
                    proveedorId,
                    $"Orden automática por stock bajo - {_dateTimeService.Now:dd/MM/yyyy}",
                    _dateTimeService.Now);
                    
                // Establecer fecha de entrega estimada (3 días después)
                orden.EstablecerFechaEntrega(_dateTimeService.Now.AddDays(3));
                
                // Agregar items a la orden
                foreach (var item in grupo.OrderByDescending(g => g.Prioridad))
                {
                    var ingrediente = item.Ingrediente;
                    
                    // Calcular cantidad a pedir
                    decimal cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
                    decimal cantidadPedir = Math.Max(1, Math.Ceiling(cantidadFaltante * 1.2m));
                    
                    // Agregar a la orden
                    orden.AgregarItem(
                        ingrediente.Id,
                        ingrediente.Nombre,
                        cantidadPedir,
                        ingrediente.UnidadMedida);
                }
                
                // Persistir la orden
                await _ordenCompraRepository.AgregarAsync(orden);
                ordenesCompra.Add(orden);
            }
            
            // Guardar todos los cambios
            await _ordenCompraRepository.GuardarCambiosAsync(cancellationToken);
            
            return ordenesCompra;
        }
        
        #endregion
    }
} 